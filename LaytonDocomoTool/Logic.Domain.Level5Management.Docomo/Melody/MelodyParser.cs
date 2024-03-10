using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Melody;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta;

// Reference: https://github.com/logue/smfplayer.js/blob/d6bf8818488893c48beacb3dedc64c734a665319/src/mld.js

namespace Logic.Domain.Level5Management.Docomo.Melody
{
    internal class MelodyParser : IMelodyParser
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IMelodyReader _melodyReader;
        private readonly IStreamFactory _streamFactory;
        private readonly Encoding _sjis;

        public MelodyParser(IBinaryFactory binaryFactory, IStreamFactory streamFactory, IMelodyReader melodyReader)
        {
            _binaryFactory = binaryFactory;
            _melodyReader = melodyReader;
            _streamFactory = streamFactory;
            _sjis = Encoding.GetEncoding("Shift-JIS");
        }

        public MelodyData Parse(Stream inputStream)
        {
            MelodyChunkData[] chunks = _melodyReader.Read(inputStream);

            return Parse(chunks);
        }

        public MelodyData Parse(MelodyChunkData[] chunks)
        {
            var result = new MelodyData();

            foreach (MelodyChunkData chunk in chunks)
                result = ParseChunk(chunk);

            return result;
        }

        private MelodyData ParseChunk(MelodyChunkData chunk)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(chunk.Data, _sjis, true, ByteOrder.BigEndian);

            int trackOffset = reader.ReadInt16() + (int)chunk.Data.Position;

            int majorType = reader.ReadByte();
            int minorType = reader.ReadByte();
            int trackCount = reader.ReadByte();

            var result = new MelodyData
            {
                MajorType = majorType,
                MinorType = minorType,
                MetaData = ParseMetaData(reader, trackOffset)
            };

            var tracks = new List<MelodyTrackData>(trackCount);

            for (var i = 0; i < trackCount; i++)
            {
                string identifier = reader.ReadString(4);

                if (identifier != "trac")
                    throw new InvalidOperationException("Invalid track chunk.");

                int length = reader.ReadInt32();

                Stream trackStream = _streamFactory.CreateSubStream(chunk.Data, chunk.Data.Position, length);
                MelodyTrackData trackData = ParseTrack(result.MetaData, trackStream);

                chunk.Data.Position += length;

                tracks.Add(trackData);
            }

            result.Tracks = tracks.ToArray();

            return result;
        }

        private MelodyMetaData ParseMetaData(IBinaryReaderX reader, int endOffset)
        {
            var result = new MelodyMetaData();

            while (reader.BaseStream.Position < endOffset)
            {
                string identifier = reader.ReadString(4);
                int length = reader.ReadInt16();

                switch (identifier)
                {
                    case "titl":
                        result.Title = reader.ReadString(length);
                        break;

                    case "copy":
                        result.Copyright = reader.ReadString(length);
                        break;

                    case "vers":
                        result.Version = reader.ReadString(length);
                        break;

                    case "date":
                        result.Date = reader.ReadString(length);
                        break;

                    case "prot":
                        result.Protection = reader.ReadString(length);
                        break;

                    case "supt":
                        result.Support = reader.ReadString(length);
                        break;

                    case "sorc":
                        result.SorcValue = reader.ReadByte();
                        break;

                    case "note":
                        result.NoteValue = reader.ReadInt16();
                        break;

                    case "exst":
                        result.ExtraData = reader.ReadBytes(length);
                        break;

                    default:
                        reader.BaseStream.Position += length;
                        break;
                }
            }

            return result;
        }

        private MelodyTrackData ParseTrack(MelodyMetaData metaData, Stream trackStream)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(trackStream, _sjis, true, ByteOrder.BigEndian);

            var elements = new List<MelodyTrackElementData>();

            while (trackStream.Position < trackStream.Length)
            {
                int deltaTime = trackStream.ReadByte();
                int status = trackStream.ReadByte();

                MelodyTrackEventData eventData;

                if (status != 0xFF)
                {
                    eventData = ParseNoteEvent(metaData, reader, status);
                }
                else
                {
                    eventData = ParseMetaEvent(reader);
                }

                elements.Add(new MelodyTrackElementData
                {
                    DeltaTime = deltaTime,
                    Event = eventData
                });
            }

            return new MelodyTrackData
            {
                Elements = elements.ToArray()
            };
        }

        private MelodyTrackNoteEventData ParseNoteEvent(MelodyMetaData metaData, IBinaryReaderX reader, int status)
        {
            var noteEvent = new MelodyTrackNoteEventData
            {
                Voice = status >> 6,
                Key = status & 0x3F
            };

            if (metaData.NoteValue is 1)
            {
                int extendedStatus = reader.ReadByte();

                noteEvent.Velocity = extendedStatus >> 2;
                noteEvent.OctaveShift = extendedStatus & 0x3;
            }

            return noteEvent;
        }

        private MelodyTrackMetaEventData ParseMetaEvent(IBinaryReaderX reader)
        {
            int status = reader.ReadByte();

            int type = status >> 4;
            int subType = status & 0xF;

            switch (type)
            {
                case 11:
                    switch (subType)
                    {
                        case 0:
                            return new MelodyTrackMasterVolumeEventData
                            {
                                Volume = reader.ReadByte()
                            };

                        case 10:
                            int drumValue = reader.ReadByte();

                            return new MelodyTrackDrumScaleEventData
                            {
                                Channel = (drumValue >> 3) & 0x7,
                                Drum = drumValue & 0x1
                            };

                        default:
                            throw new InvalidOperationException($"Invalid meta system message type {subType:X2}.");
                    }

                case 12:
                    return new MelodyTrackSetTempoEventData
                    {
                        TimeBase = (status & 0x7) == 7 ? null : (int)Math.Pow(2, status & 0x7) * ((status & 0x8) == 0 ? 6 : 15),
                        Tempo = reader.ReadByte()
                    };

                case 13:
                    switch (subType)
                    {
                        case 0:
                            return new MelodyTrackPointEventData
                            {
                                Value = reader.ReadByte()
                            };

                        case 13:
                            int loopValue = reader.ReadByte();

                            return new MelodyTrackLoopEventData
                            {
                                Id = loopValue >> 6,
                                Count = (loopValue >> 2) & 0xF,
                                Point = loopValue & 0x3
                            };

                        case 14:
                            return new MelodyTrackNopEventData
                            {
                                Value = reader.ReadByte()
                            };

                        case 15:
                            return new MelodyTrackEndTrackEventData
                            {
                                Value = reader.ReadByte()
                            };

                        default:
                            throw new InvalidOperationException($"Invalid meta control message type {subType:X2}.");
                    }

                case 14:
                    int part = reader.ReadByte();

                    switch (subType)
                    {
                        case 0:
                            return new MelodyTrackInstrumentLowPartEventData
                            {
                                Part = part >> 6,
                                Instrument = part & 0x3F
                            };

                        case 1:
                            return new MelodyTrackInstrumentHighPartEventData
                            {
                                Part = part >> 6,
                                Instrument = part & 0x1
                            };

                        case 2:
                            return new MelodyTrackVolumeEventData
                            {
                                Part = part >> 6,
                                Volume = part & 0x3F
                            };

                        case 3:
                            return new MelodyTrackValanceEventData
                            {
                                Part = part >> 6,
                                Valance = part & 0x3F
                            };

                        case 4:
                            return new MelodyTrackPitchBendEventData
                            {
                                Part = part >> 6,
                                PitchBend = part & 0x3F
                            };

                        case 5:
                            return new MelodyTrackChannelAssignEventData
                            {
                                Part = part >> 6,
                                Channel = part & 0x3F
                            };

                        case 6:
                            return new MelodyTrackVolumeChangeEventData
                            {
                                Part = part >> 6,
                                Volume = ((part & 0x3F) << 26) >> 26
                            };

                        case 7:
                            return new MelodyTrackPitchBendRangeEventData
                            {
                                Part = part >> 6,
                                Range = part & 0x3F
                            };

                        case 9:
                            return new MelodyTrackMasterCoarseTuningEventData
                            {
                                Part = part >> 6,
                                Value = part & 0x3F
                            };

                        case 10:
                            return new MelodyTrackModulationEventData
                            {
                                Part = part >> 6,
                                Depth = part & 0x3F
                            };

                        default:
                            throw new InvalidOperationException($"Invalid meta instrument message type {subType:X2}.");
                    }

                case 15:
                    switch (subType)
                    {
                        case 0:
                            int editPart = reader.ReadByte();
                            byte[] modPart = reader.ReadBytes(4);
                            byte[] carrierPart = reader.ReadBytes(4);
                            int octaveSelect = reader.ReadByte();

                            return new MelodyTrackEditInstrumentEventData
                            {
                                Part = (editPart >> 4) & 0x3,
                                Modulator = new EditInstrumentModulator
                                {
                                    ML = modPart[0] >> 5,
                                    VIV = (modPart[0] >> 4) & 0x1,
                                    EG = (modPart[0] >> 3) & 0x1,
                                    SUS = (modPart[0] >> 2) & 0x1,
                                    RR = ((modPart[0] & 0x3) << 2) | (modPart[1] >> 6),
                                    DR = (modPart[1] >> 4) & 0xf,
                                    AR = ((modPart[1] & 0x3) << 2) | (modPart[2] >> 6),
                                    SL = (modPart[2] >> 4) & 0xf,
                                    TL = ((modPart[2] & 0x3) << 4) | (modPart[3] >> 4),
                                    WF = (modPart[3] >> 3) & 0x1,
                                    FB = modPart[3] & 0x7,
                                },
                                Carrier = new EditInstrumentCarrier
                                {
                                    ML = carrierPart[0] >> 5,
                                    VIV = (carrierPart[0] >> 4) & 0x1,
                                    EG = (carrierPart[0] >> 3) & 0x1,
                                    SUS = (carrierPart[0] >> 2) & 0x1,
                                    RR = ((carrierPart[0] & 0x3) << 2) | (carrierPart[1] >> 6),
                                    DR = (carrierPart[1] >> 4) & 0xf,
                                    AR = ((carrierPart[1] & 0x3) << 2) | (carrierPart[2] >> 6),
                                    SL = (carrierPart[2] >> 4) & 0xf,
                                    TL = ((carrierPart[2] & 0x3) << 4) | (carrierPart[3] >> 4),
                                    WF = (carrierPart[3] >> 3) & 0x1,
                                    FB = carrierPart[3] & 0x7,
                                },
                                OctaveSelect = octaveSelect
                            };

                        case 1:
                            if (reader.ReadByte() != 1)
                                throw new InvalidOperationException("Invalid vibrato value.");

                            return new MelodyTrackVibratoEventData
                            {
                                Part = (reader.ReadByte() >> 5) & 0x3,
                                Switch = reader.ReadByte() >> 6
                            };

                        case 15:
                            int deviceLength = (reader.ReadByte() << 8) | reader.ReadByte();

                            if (reader.ReadByte() != 0x11)
                                throw new InvalidOperationException("Invalid device specific command.");

                            return new MelodyTrackDeviceSpecificEventData
                            {
                                Data = reader.ReadBytes(deviceLength)
                            };

                        default:
                            throw new InvalidOperationException($"Invalid meta extended message type {subType:X2}.");
                    }

                default:
                    throw new InvalidOperationException($"Invalid meta type {type:X2}.");
            }
        }
    }
}
