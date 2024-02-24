using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Resource
{
    internal class ResourceReader : IResourceReader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IStreamFactory _streamFactory;

        public ResourceReader(IBinaryFactory binaryFactory, IStreamFactory streamFactory)
        {
            _binaryFactory = binaryFactory;
            _streamFactory = streamFactory;
        }

        public ResourceEntryData[] Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input, true);

            var result = new List<ResourceEntryData>();

            while (input.Position < input.Length)
            {
                string identifier = br.ReadString(3);

                int size = br.ReadInt32();
                Stream dataStream = _streamFactory.CreateSubStream(input, input.Position, size);

                input.Position += size;

                result.Add(new ResourceEntryData
                {
                    Identifier = identifier,
                    Data = dataStream
                });
            }

            return result.ToArray();
        }
    }
}
