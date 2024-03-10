using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Melody;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Melody
{
    internal class MelodyReader : IMelodyReader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IStreamFactory _streamFactory;

        public MelodyReader(IBinaryFactory binaryFactory, IStreamFactory streamFactory)
        {
            _binaryFactory = binaryFactory;
            _streamFactory = streamFactory;
        }

        public MelodyChunkData[] Read(Stream inputStream)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(inputStream, true, ByteOrder.BigEndian);

            string identifier = reader.ReadString(4);
            int length = reader.ReadInt32();

            return new[]
            {
                new MelodyChunkData
                {
                    Identifier = identifier,
                    Data = _streamFactory.CreateSubStream(inputStream,inputStream.Position,length)
                }
            };
        }
    }
}
