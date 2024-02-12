using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Docomo
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

        public EntryData[] ReadEntries(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input, true);

            var result = new List<EntryData>();

            var entryCount = 0;

            while (input.Position < input.Length)
            {
                byte[] identifierBytes = br.ReadBytes(3);
                int size = br.ReadInt32();

                string identifier = Encoding.ASCII.GetString(identifierBytes);

                switch (identifier)
                {
                    case "res":
                        EntryData[] resourceEntries = ReadResources(br);
                        result.AddRange(resourceEntries);
                        break;

                    case "obj":
                        var objName = $"{entryCount++:00}.obj";
                        Stream objStream = _streamFactory.CreateSubStream(input, input.Position, size);

                        var objEntry = new EntryData { Name = objName, Data = objStream };
                        result.Add(objEntry);

                        input.Position += size;
                        break;

                    case "ary":
                        var aryName = $"{entryCount++:00}.ary";
                        Stream aryStream = _streamFactory.CreateSubStream(input, input.Position, size);

                        var aryEntry = new EntryData { Name = aryName, Data = aryStream };
                        result.Add(aryEntry);

                        input.Position += size;
                        break;
                }
            }

            return result.ToArray();
        }

        private EntryData[] ReadResources(IBinaryReaderX reader)
        {
            int imgCount = reader.ReadInt32();

            var result = new EntryData[imgCount];

            for (var i = 0; i < imgCount; i++)
            {
                short nameLength = reader.ReadInt16();
                byte[] nameBytes = reader.ReadBytes(nameLength);

                int dataLength = reader.ReadInt32();

                string name = Encoding.ASCII.GetString(nameBytes);
                Stream dataStream = _streamFactory.CreateSubStream(reader.BaseStream, reader.BaseStream.Position, dataLength);

                result[i] = new EntryData { Name = name, Data = dataStream };

                reader.BaseStream.Position += dataLength;
            }

            return result;
        }
    }
}
