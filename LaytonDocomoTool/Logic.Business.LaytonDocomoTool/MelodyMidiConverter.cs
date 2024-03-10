using Logic.Business.LaytonDocomoTool.DataClasses;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta;
using Logic.Domain.MidiManagement.Contract.DataClasses;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.Meta;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.Midi;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.SysEx;
using Logic.Domain.MidiManagement.Contract.DataClasses.Interval;

// Reference: https://github.com/logue/smfplayer.js/blob/d6bf8818488893c48beacb3dedc64c734a665319/src/mld.js

namespace Logic.Business.LaytonDocomoTool
{
    internal class MelodyMidiConverter : IMelodyMidiConverter
    {
        public MidiData ConvertMelodyData(MelodyData melodyData)
        {
            var convertedEvents = new List<MelodyTrackConversionData>();
            for (var i = 0; i < melodyData.Tracks.Length; i++)
                AddConvertedTrackEvents(convertedEvents, melodyData.Tracks[i].Elements, i);

            MelodyTrackConversionData[] sortedEvents = convertedEvents.OrderBy(e => e.totalTime).ThenBy(e => e.id).ToArray();
            MidiTrackElementData[] trackElements = ConvertTrack(sortedEvents);

            return new MidiData
            {
                Header = new MidiHeaderData
                {
                    Format = 0,
                    TrackCount = 1,
                    Interval = new MidiMetricIntervalData
                    {
                        SubDivisionCount = 48
                    }
                },
                Tracks = new[]
                {
                    new MidiTrackData
                    {
                        Elements = trackElements
                    }
                }
            };
        }

        private void AddConvertedTrackEvents(IList<MelodyTrackConversionData> convertedEvents, MelodyTrackElementData[] elements, int trackIndex)
        {
            var totalTime = 0;
            var id = 0;

            for (var i = 0; i < elements.Length; i++)
            {
                totalTime += elements[i].DeltaTime;

                MelodyTrackEventData eventData;

                switch (elements[i].Event)
                {
                    case MelodyTrackNopEventData:
                        continue;

                    case MelodyTrackNoteEventData noteData:
                        convertedEvents.Add(new MelodyTrackConversionData
                        {
                            id = id++,
                            totalTime = totalTime,
                            channelBase = trackIndex * 4,
                            melodyEvent = noteData
                        });

                        eventData = new MelodyTrackNoteOffEventData
                        {
                            Voice = noteData.Voice,
                            Key = noteData.Key,
                            Velocity = noteData.Velocity,
                            OctaveShift = noteData.OctaveShift
                        };
                        break;

                    case MelodyTrackInstrumentHighPartEventData highInstrumentData:
                        if (elements[++i].Event is not MelodyTrackInstrumentLowPartEventData lowInstrumentData)
                            throw new InvalidOperationException("Invalid instrument value.");

                        eventData = new MelodyTrackCombinedInstrumentEventData
                        {
                            Part = lowInstrumentData.Part,
                            Instrument = (highInstrumentData.Instrument << 6) | lowInstrumentData.Instrument
                        };
                        break;

                    default:
                        eventData = elements[i].Event;
                        break;
                }

                convertedEvents.Add(new MelodyTrackConversionData
                {
                    id = id++,
                    totalTime = totalTime,
                    channelBase = trackIndex * 4,
                    melodyEvent = eventData
                });
            }
        }

        private MidiTrackElementData[] ConvertTrack(MelodyTrackConversionData[] unifiedData)
        {
            var result = new List<MidiTrackElementData>(unifiedData.Length);

            var channelTimes = new int[16];

            foreach (MelodyTrackConversionData element in unifiedData)
            {
                int channel = element.channelBase;
                MidiTrackEventData midiEvent;

                switch (element.melodyEvent)
                {
                    case MelodyTrackNoteEventData noteData:
                        int onKey = ApplyOctaveShift(noteData.Key + 45, noteData.OctaveShift);

                        channel += noteData.Voice;
                        midiEvent = new MidiTrackNoteOnEventData
                        {
                            Channel = channel,
                            Note = channel == 9 ? onKey - 10 : onKey,
                            Velocity = noteData.Velocity * 2,
                        };
                        break;

                    case MelodyTrackNoteOffEventData noteOffData:
                        int offKey = ApplyOctaveShift(noteOffData.Key + 45, noteOffData.OctaveShift);

                        channel += noteOffData.Voice;
                        midiEvent = new MidiTrackNoteOffEventData
                        {
                            Channel = channel,
                            Note = channel == 9 ? offKey - 10 : offKey,
                            Velocity = noteOffData.Velocity * 2,
                        };
                        break;

                    case MelodyTrackCombinedInstrumentEventData instrumentData:
                        channel += instrumentData.Part;
                        midiEvent = new MidiTrackProgramChangeEventData
                        {
                            Channel = channel,
                            Program = instrumentData.Instrument
                        };
                        break;

                    case MelodyTrackSetTempoEventData tempoData:
                        channel = 0;
                        midiEvent = new MidiTrackSetTempoEventData
                        {
                            Tempo = (int)(2880000000 / (tempoData.Tempo * tempoData.TimeBase))
                        };
                        break;

                    case MelodyTrackLoopEventData loopData:
                        channel = 0;
                        midiEvent = new MidiTrackMarkerEventData
                        {
                            Marker = $"LOOP_{(loopData.Point == 0 ? "START" : "END")}=ID:{loopData.Id},COUNT:{(loopData.Count == 0 ? -1 : loopData.Count)}"
                        };
                        break;

                    case MelodyTrackMasterVolumeEventData masterVolumeData:
                        channel = 0;
                        midiEvent = new MidiTrackSingleSystemEventData
                        {
                            Message = new byte[] { 0x7F, 0x7F, 0x04, 0x01, (byte)masterVolumeData.Volume, (byte)masterVolumeData.Volume, 0xF7 }
                        };
                        break;

                    case MelodyTrackModulationEventData modulationData:
                        channel += modulationData.Part;
                        midiEvent = new MidiTrackControllerEventData
                        {
                            Channel = channel,
                            Controller = 1,
                            Value = modulationData.Depth * 2
                        };
                        break;

                    case MelodyTrackVolumeEventData volumeData:
                        channel += volumeData.Part;
                        midiEvent = new MidiTrackControllerEventData
                        {
                            Channel = channel,
                            Controller = 7,
                            Value = volumeData.Volume * 2
                        };
                        break;

                    case MelodyTrackValanceEventData valanceData:
                        channel += valanceData.Part;
                        midiEvent = new MidiTrackControllerEventData
                        {
                            Channel = channel,
                            Controller = 10,
                            Value = (valanceData.Valance - 32) * 2 + 64
                        };
                        break;

                    case MelodyTrackPitchBendEventData pitchBendData:
                        channel += pitchBendData.Part;
                        midiEvent = new MidiTrackPitchBendEventData
                        {
                            Channel = channel,
                            PitchBend = (pitchBendData.PitchBend * 2) | ((pitchBendData.PitchBend * 2) << 7)
                        };
                        break;

                    case MelodyTrackPitchBendRangeEventData pitchBendRangeData:
                        channel += pitchBendRangeData.Part;
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = element.totalTime - channelTimes[channel],
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 100,
                                Value = 0
                            }
                        });
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = 0,
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 101,
                                Value = 0
                            }
                        });
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = 0,
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 6,
                                Value = pitchBendRangeData.Range * 2
                            }
                        });

                        channelTimes[channel] = element.totalTime;
                        continue;

                    case MelodyTrackMasterCoarseTuningEventData masterCoarseTuningData:
                        channel += masterCoarseTuningData.Part;
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = element.totalTime - channelTimes[channel],
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 100,
                                Value = 0
                            }
                        });
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = 0,
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 101,
                                Value = 2
                            }
                        });
                        result.Add(new MidiTrackElementData
                        {
                            DeltaTime = 0,
                            Event = new MidiTrackControllerEventData
                            {
                                Channel = channel,
                                Controller = 6,
                                Value = masterCoarseTuningData.Value * 2
                            }
                        });

                        channelTimes[channel] = element.totalTime;
                        continue;

                    default:
                        continue;
                }

                result.Add(new MidiTrackElementData
                {
                    DeltaTime = element.totalTime - channelTimes[channel],
                    Event = midiEvent
                });

                channelTimes[channel] = element.totalTime;
            }

            result.Add(new MidiTrackElementData
            {
                DeltaTime = 0,
                Event = new MidiTrackEndTrackEventData()
            });

            return result.ToArray();
        }

        private int ApplyOctaveShift(int key, int octaveShift)
        {
            int[] table = { 0, 12, -24, -12 };

            if (octaveShift is < 4 and >= 0)
                return key + table[octaveShift];

            throw new InvalidOperationException($"Invalid octave shift: {octaveShift}");
        }
    }
}
