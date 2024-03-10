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
    internal class MidiParser : IMidiParser
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IMidiReader _reader;

        public MidiParser(IBinaryFactory binaryFactory, IMidiReader reader)
        {
            _binaryFactory = binaryFactory;
            _reader = reader;
        }

        public MidiData Parse(Stream input)
        {
            MidiChunkData[] chunks = _reader.Read(input);

            return Parse(chunks);
        }

        public MidiData Parse(MidiChunkData[] chunks)
        {
            var result = new MidiData();
            var tracks = new List<MidiTrackData>();

            foreach (MidiChunkData chunk in chunks)
            {
                switch (chunk.Identifier)
                {
                    case "MThd":
                        result.Header = ParseHeader(chunk.Data);
                        break;

                    case "MTrk":
                        tracks.Add(ParseTrack(chunk.Data));
                        break;
                }
            }

            result.Tracks = tracks.ToArray();

            return result;
        }

        private MidiHeaderData ParseHeader(Stream dataStream)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(dataStream, true, ByteOrder.BigEndian);

            short format = reader.ReadInt16();
            short trackCount = reader.ReadInt16();
            short tickData = reader.ReadInt16();

            MidiIntervalData intervalType;
            if ((tickData & 0x8000) == 0)
            {
                intervalType = new MidiMetricIntervalData
                {
                    SubDivisionCount = tickData
                };
            }
            else
            {
                intervalType = new MidiTimeCodeIntervalData
                {
                    SubFrameResolution = tickData & 0xFF,
                    FramesPerSecond = Math.Abs((sbyte)(tickData >> 8))
                };
            }

            return new MidiHeaderData
            {
                Format = format,
                TrackCount = trackCount,
                Interval = intervalType
            };
        }

        private MidiTrackData ParseTrack(Stream dataStream)
        {
            var elements = new List<MidiTrackElementData>();

            int previousStatus = -1;

            while (dataStream.Position < dataStream.Length)
            {
                int deltaTime = ReadVariableLengthInt(dataStream);
                MidiTrackEventData trackEvent = ParseTrackEvent(dataStream, ref previousStatus);

                elements.Add(new MidiTrackElementData
                {
                    DeltaTime = deltaTime,
                    Event = trackEvent
                });
            }

            return new MidiTrackData
            {
                Elements = elements.ToArray()
            };
        }

        private MidiTrackEventData ParseTrackEvent(Stream dataStream, ref int previousStatus)
        {
            int status = dataStream.ReadByte();

            if (status < 0x80)
            {
                dataStream.Position--;
                return ParseTrackMidiEvent(dataStream, previousStatus);
            }

            previousStatus = status;

            if (status is >= 0x80 and <= 0xEF)
                return ParseTrackMidiEvent(dataStream, status);

            if (status is 0xF0 or 0xF7)
                return ParseTrackSysExEvent(dataStream, status);

            if (status is 0xFF)
                return ParseTrackMetaEvent(dataStream);

            throw new InvalidOperationException($"Invalid event status 0x{status:X2}.");
        }

        private MidiTrackMidiEventData ParseTrackMidiEvent(Stream dataStream, int status)
        {
            int channel = status & 0xF;
            int eventType = (status >> 4) & 0xF;

            switch (eventType)
            {
                case 8:
                    return new MidiTrackNoteOffEventData
                    {
                        Channel = channel,
                        Note = dataStream.ReadByte(),
                        Velocity = dataStream.ReadByte()
                    };

                case 9:
                    return new MidiTrackNoteOnEventData
                    {
                        Channel = channel,
                        Note = dataStream.ReadByte(),
                        Velocity = dataStream.ReadByte()
                    };

                case 10:
                    return new MidiTrackPolyphonicPressureEventData
                    {
                        Channel = channel,
                        Note = dataStream.ReadByte(),
                        Pressure = dataStream.ReadByte()
                    };

                case 11:
                    return new MidiTrackControllerEventData
                    {
                        Channel = channel,
                        Controller = dataStream.ReadByte(),
                        Value = dataStream.ReadByte()
                    };

                case 12:
                    return new MidiTrackProgramChangeEventData
                    {
                        Channel = channel,
                        Program = dataStream.ReadByte()
                    };

                case 13:
                    return new MidiTrackChannelPressureEventData
                    {
                        Channel = channel,
                        Pressure = dataStream.ReadByte()
                    };

                case 14:
                    return new MidiTrackPitchBendEventData
                    {
                        Channel = channel,
                        PitchBend = (dataStream.ReadByte() & 0x7F) | ((dataStream.ReadByte() & 0x7F) << 7)
                    };

                default:
                    throw new InvalidOperationException($"Invalid status byte {status:X2}.");
            }
        }

        private MidiTrackSysExEventData ParseTrackSysExEvent(Stream dataStream, int status)
        {
            if (status == 0xF0)
            {
                int length = ReadVariableLengthInt(dataStream);

                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackSingleSystemEventData
                {
                    Message = messageBytes
                };
            }

            if (status == 0xF7)
            {
                int length = ReadVariableLengthInt(dataStream);

                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackEscapeSystemEventData
                {
                    Message = messageBytes
                };
            }

            throw new InvalidOperationException($"Invalid status byte {status:X2}.");
        }

        private MidiTrackMetaEventData ParseTrackMetaEvent(Stream dataStream)
        {
            int type = dataStream.ReadByte();
            int length = ReadVariableLengthInt(dataStream);

            if (type == 0)
            {
                return new MidiTrackSequenceNumberEventData
                {
                    SequenceNumber = (dataStream.ReadByte() << 8) | dataStream.ReadByte()
                };
            }

            if (type == 1)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackTextEventData
                {
                    Text = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 2)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackCopyrightEventData
                {
                    Copyright = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 3)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackNameEventData
                {
                    Name = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 4)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackInstrumentNameEventData
                {
                    InstrumentName = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 5)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackLyricEventData
                {
                    Lyric = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 6)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackMarkerEventData
                {
                    Marker = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 7)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackCuePointEventData
                {
                    CuePoint = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 8)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackProgramNameEventData
                {
                    ProgramName = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 9)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackDeviceNameEventData
                {
                    DeviceName = Encoding.ASCII.GetString(messageBytes)
                };
            }

            if (type == 0x20)
            {
                return new MidiTrackChannelPrefixEventData
                {
                    Channel = dataStream.ReadByte()
                };
            }

            if (type == 0x21)
            {
                return new MidiTrackPortEventData
                {
                    Port = dataStream.ReadByte()
                };
            }

            if (type == 0x2F)
                return new MidiTrackEndTrackEventData();

            if (type == 0x51)
            {
                return new MidiTrackSetTempoEventData
                {
                    Tempo = (dataStream.ReadByte() << 16) | (dataStream.ReadByte() << 8) | dataStream.ReadByte()
                };
            }

            if (type == 0x54)
            {
                return new MidiTrackSmpteOffsetEventData
                {
                    Hour = dataStream.ReadByte(),
                    Minute = dataStream.ReadByte(),
                    Second = dataStream.ReadByte(),
                    Frames = dataStream.ReadByte(),
                    FractionalFrames = dataStream.ReadByte()
                };
            }

            if (type == 0x58)
            {
                return new MidiTrackTimeSignatureEventData
                {
                    Numerator = dataStream.ReadByte(),
                    Denominator = dataStream.ReadByte(),
                    Clocks = dataStream.ReadByte(),
                    NotesInQuarter = dataStream.ReadByte()
                };
            }

            if (type == 0x59)
            {
                return new MidiTrackKeySignatureEventData
                {
                    FlatSharpCount = dataStream.ReadByte(),
                    IsMajor = dataStream.ReadByte() == 0
                };
            }

            if (type == 0x7F)
            {
                var messageBytes = new byte[length];
                _ = dataStream.Read(messageBytes);

                return new MidiTrackSequencerSpecificEventData
                {
                    Message = messageBytes
                };
            }

            throw new InvalidOperationException($"Invalid meta type {type:X2}.");
        }

        private int ReadVariableLengthInt(Stream dataStream)
        {
            var result = 0;

            for (var i = 0; i < 4; i++)
            {
                int value = dataStream.ReadByte();
                result |= (value & 0x7F) << (i * 7);

                if ((value & 0x80) == 0)
                    break;
            }

            return result;
        }
    }
}
