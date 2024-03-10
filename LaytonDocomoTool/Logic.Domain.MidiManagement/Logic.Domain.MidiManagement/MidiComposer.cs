using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.MidiManagement.Contract;
using Logic.Domain.MidiManagement.Contract.DataClasses;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.Meta;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.Midi;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events.SysEx;
using Logic.Domain.MidiManagement.Contract.DataClasses.Interval;

namespace Logic.Domain.MidiManagement
{
    internal class MidiComposer : IMidiComposer
    {
        private readonly IBinaryFactory _binaryFactory;

        public MidiComposer(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public MidiChunkData[] Compose(MidiData midiData)
        {
            var result = new List<MidiChunkData>
            {
                ComposeHeader(midiData.Header)
            };

            foreach (MidiTrackData trackData in midiData.Tracks)
                result.Add(ComposeTrack(trackData));

            return result.ToArray();
        }

        private MidiChunkData ComposeHeader(MidiHeaderData headerData)
        {
            var headerStream = new MemoryStream();

            using IBinaryWriterX writer = _binaryFactory.CreateWriter(headerStream, true, ByteOrder.BigEndian);

            writer.Write((short)headerData.Format);
            writer.Write((short)headerData.TrackCount);

            switch (headerData.Interval)
            {
                case MidiMetricIntervalData metricInterval:
                    writer.Write((short)metricInterval.SubDivisionCount);
                    break;

                case MidiTimeCodeIntervalData timeCodeInterval:
                    writer.Write((byte)timeCodeInterval.FramesPerSecond);
                    writer.Write((byte)timeCodeInterval.SubFrameResolution);
                    break;
            }

            headerStream.Position = 0;

            return new MidiChunkData
            {
                Identifier = "MThd",
                Data = headerStream
            };
        }

        private MidiChunkData ComposeTrack(MidiTrackData trackData)
        {
            var trackStream = new MemoryStream();

            int previousEventType = -1;

            foreach (MidiTrackElementData element in trackData.Elements)
            {
                WriteVariableLengthValue(trackStream, element.DeltaTime);
                ComposeTrackEvent(trackStream, element.Event, ref previousEventType);
            }

            trackStream.Position = 0;

            return new MidiChunkData
            {
                Identifier = "MTrk",
                Data = trackStream
            };
        }

        private void ComposeTrackEvent(Stream output, MidiTrackEventData trackEvent, ref int previousEventType)
        {
            switch (trackEvent)
            {
                case MidiTrackMidiEventData midiEvent:
                    ComposeTrackMidiEvent(output, midiEvent, ref previousEventType);
                    break;

                case MidiTrackSysExEventData sysExEvent:
                    ComposeTrackSysExEvent(output, sysExEvent);
                    break;

                case MidiTrackMetaEventData metaEvent:
                    ComposeTrackMetaEvent(output, metaEvent);
                    break;
            }
        }

        private void ComposeTrackMidiEvent(Stream output, MidiTrackMidiEventData midiEvent, ref int previousStatus)
        {
            var status = (byte)(midiEvent.Channel & 0xF);

            switch (midiEvent)
            {
                case MidiTrackNoteOffEventData noteOffEvent:
                    status |= 0x80;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)noteOffEvent.Note);
                    output.WriteByte((byte)noteOffEvent.Velocity);
                    break;

                case MidiTrackNoteOnEventData noteOnEvent:
                    status |= 0x90;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)noteOnEvent.Note);
                    output.WriteByte((byte)noteOnEvent.Velocity);
                    break;

                case MidiTrackPolyphonicPressureEventData polyEvent:
                    status |= 0xA0;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)polyEvent.Note);
                    output.WriteByte((byte)polyEvent.Pressure);
                    break;

                case MidiTrackControllerEventData controllerEvent:
                    status |= 0xB0;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)controllerEvent.Controller);
                    output.WriteByte((byte)controllerEvent.Value);
                    break;

                case MidiTrackProgramChangeEventData programEvent:
                    status |= 0xC0;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)programEvent.Program);
                    break;

                case MidiTrackChannelPressureEventData pressureEvent:
                    status |= 0xD0;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)pressureEvent.Pressure);
                    break;

                case MidiTrackPitchBendEventData pitchBendEvent:
                    status |= 0xE0;
                    if (previousStatus != status)
                        output.WriteByte(status);
                    output.WriteByte((byte)(pitchBendEvent.PitchBend & 0x7F));
                    output.WriteByte((byte)((pitchBendEvent.PitchBend >> 7) & 0x7F));
                    break;

                default:
                    throw new InvalidOperationException($"Invalid midi event {midiEvent.GetType().Name}.");
            }

            previousStatus = status;
        }

        private void ComposeTrackSysExEvent(Stream output, MidiTrackSysExEventData sysExEvent)
        {
            switch (sysExEvent)
            {
                case MidiTrackSingleSystemEventData singleEvent:
                    output.WriteByte(0xF0);
                    WriteVariableLengthValue(output, singleEvent.Message.Length);
                    output.Write(singleEvent.Message);
                    break;

                case MidiTrackEscapeSystemEventData escapeEvent:
                    output.WriteByte(0xF7);
                    WriteVariableLengthValue(output, escapeEvent.Message.Length);
                    output.Write(escapeEvent.Message);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid system event {sysExEvent.GetType().Name}.");
            }
        }

        private void ComposeTrackMetaEvent(Stream output, MidiTrackMetaEventData metaEvent)
        {
            output.WriteByte(0xFF);

            switch (metaEvent)
            {
                case MidiTrackSequenceNumberEventData seqEvent:
                    output.WriteByte(0);
                    WriteVariableLengthValue(output, 2);

                    output.WriteByte((byte)(seqEvent.SequenceNumber >> 8));
                    output.WriteByte((byte)seqEvent.SequenceNumber);
                    break;

                case MidiTrackTextEventData trackEvent:
                    output.WriteByte(1);

                    byte[] trackBytes = Encoding.ASCII.GetBytes(trackEvent.Text);
                    WriteVariableLengthValue(output, trackBytes.Length);

                    output.Write(trackBytes);
                    break;

                case MidiTrackCopyrightEventData copyEvent:
                    output.WriteByte(2);

                    byte[] copyBytes = Encoding.ASCII.GetBytes(copyEvent.Copyright);
                    WriteVariableLengthValue(output, copyBytes.Length);

                    output.Write(copyBytes);
                    break;

                case MidiTrackNameEventData nameEvent:
                    output.WriteByte(3);

                    byte[] nameBytes = Encoding.ASCII.GetBytes(nameEvent.Name);
                    WriteVariableLengthValue(output, nameBytes.Length);

                    output.Write(nameBytes);
                    break;

                case MidiTrackInstrumentNameEventData instrumentNameEvent:
                    output.WriteByte(4);

                    byte[] instrumentBytes = Encoding.ASCII.GetBytes(instrumentNameEvent.InstrumentName);
                    WriteVariableLengthValue(output, instrumentBytes.Length);

                    output.Write(instrumentBytes);
                    break;

                case MidiTrackLyricEventData lyricEvent:
                    output.WriteByte(5);

                    byte[] lyricBytes = Encoding.ASCII.GetBytes(lyricEvent.Lyric);
                    WriteVariableLengthValue(output, lyricBytes.Length);

                    output.Write(lyricBytes);
                    break;

                case MidiTrackMarkerEventData markerEvent:
                    output.WriteByte(6);

                    byte[] markerBytes = Encoding.ASCII.GetBytes(markerEvent.Marker);
                    WriteVariableLengthValue(output, markerBytes.Length);

                    output.Write(markerBytes);
                    break;

                case MidiTrackCuePointEventData cueEvent:
                    output.WriteByte(7);

                    byte[] cueBytes = Encoding.ASCII.GetBytes(cueEvent.CuePoint);
                    WriteVariableLengthValue(output, cueBytes.Length);

                    output.Write(cueBytes);
                    break;

                case MidiTrackProgramNameEventData programNameEvent:
                    output.WriteByte(8);

                    byte[] programNameBytes = Encoding.ASCII.GetBytes(programNameEvent.ProgramName);
                    WriteVariableLengthValue(output, programNameBytes.Length);

                    output.Write(programNameBytes);
                    break;

                case MidiTrackDeviceNameEventData deviceNameEvent:
                    output.WriteByte(9);

                    byte[] deviceNameBytes = Encoding.ASCII.GetBytes(deviceNameEvent.DeviceName);
                    WriteVariableLengthValue(output, deviceNameBytes.Length);

                    output.Write(deviceNameBytes);
                    break;

                case MidiTrackChannelPrefixEventData prefixEvent:
                    output.WriteByte(0x20);
                    WriteVariableLengthValue(output, 1);
                    output.WriteByte((byte)prefixEvent.Channel);
                    break;

                case MidiTrackPortEventData portEvent:
                    output.WriteByte(0x21);
                    WriteVariableLengthValue(output, 1);
                    output.WriteByte((byte)portEvent.Port);
                    break;

                case MidiTrackEndTrackEventData:
                    output.WriteByte(0x2F);
                    WriteVariableLengthValue(output, 0);
                    break;

                case MidiTrackSetTempoEventData setTempoEvent:
                    output.WriteByte(0x51);
                    WriteVariableLengthValue(output, 3);
                    output.WriteByte((byte)(setTempoEvent.Tempo >> 16));
                    output.WriteByte((byte)(setTempoEvent.Tempo >> 8));
                    output.WriteByte((byte)setTempoEvent.Tempo);
                    break;

                case MidiTrackSmpteOffsetEventData smpteEvent:
                    output.WriteByte(0x54);
                    WriteVariableLengthValue(output, 5);
                    output.WriteByte((byte)smpteEvent.Hour);
                    output.WriteByte((byte)smpteEvent.Minute);
                    output.WriteByte((byte)smpteEvent.Second);
                    output.WriteByte((byte)smpteEvent.Frames);
                    output.WriteByte((byte)smpteEvent.FractionalFrames);
                    break;

                case MidiTrackTimeSignatureEventData timeSigEvent:
                    output.WriteByte(0x58);
                    WriteVariableLengthValue(output, 4);
                    output.WriteByte((byte)timeSigEvent.Numerator);
                    output.WriteByte((byte)timeSigEvent.Denominator);
                    output.WriteByte((byte)timeSigEvent.Clocks);
                    output.WriteByte((byte)timeSigEvent.NotesInQuarter);
                    break;

                case MidiTrackKeySignatureEventData keySigEvent:
                    output.WriteByte(0x59);
                    WriteVariableLengthValue(output, 2);
                    output.WriteByte((byte)keySigEvent.FlatSharpCount);
                    output.WriteByte((byte)(keySigEvent.IsMajor ? 0 : 1));
                    break;

                case MidiTrackSequencerSpecificEventData seqSpecificEvent:
                    output.WriteByte(0x7F);
                    WriteVariableLengthValue(output, seqSpecificEvent.Message.Length);
                    output.Write(seqSpecificEvent.Message);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid meta event {metaEvent.GetType().Name}.");
            }
        }

        private void WriteVariableLengthValue(Stream output, int value)
        {
            for (var i = 0; i < 4; i++)
            {
                var part = (byte)(value & 0x7F);
                value >>= 7;

                if (value == 0)
                {
                    output.WriteByte(part);
                    break;
                }

                output.WriteByte((byte)(part | 0x80));
            }
        }
    }
}
