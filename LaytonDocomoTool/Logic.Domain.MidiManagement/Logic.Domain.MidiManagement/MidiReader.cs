using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.MidiManagement.Contract;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Domain.MidiManagement
{
    internal class MidiReader : IMidiReader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IStreamFactory _streamFactory;

        public MidiReader(IBinaryFactory binaryFactory, IStreamFactory streamFactory)
        {
            _binaryFactory = binaryFactory;
            _streamFactory = streamFactory;
        }

        public MidiChunkData[] Read(Stream input)
        {
            var result = new List<MidiChunkData>();

            using IBinaryReaderX reader = _binaryFactory.CreateReader(input, true, ByteOrder.BigEndian);

            while (input.Position < input.Length)
            {
                string identifier = reader.ReadString(4);
                int size = reader.ReadInt32();

                result.Add(new MidiChunkData
                {
                    Identifier = identifier,
                    Data = _streamFactory.CreateSubStream(input, input.Position, size)
                });

                input.Position += size;
            }

            return result.ToArray();
        }
    }
}
