using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Script
{
    internal class ScriptReader : IScriptReader
    {
        private readonly IBinaryFactory _binaryFactory;

        public ScriptReader(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public EventEntryData[] Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input, true);

            var result = new List<EventEntryData>();

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte[] identifierBytes = br.ReadBytes(10);
                short dataSize = br.ReadInt16();
                byte[] data = br.ReadBytes(dataSize);

                var entry = new EventEntryData
                {
                    identifier = Encoding.ASCII.GetString(identifierBytes),
                    dataSize = dataSize,
                    data = data
                };

                result.Add(entry);
            }

            return result.ToArray();
        }
    }
}
