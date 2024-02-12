using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.DataClasses;

namespace Logic.Domain.Level5Management.Docomo
{
    internal class TableReader : ITableReader
    {
        private readonly IStreamFactory _streamFactory;

        public TableReader(IStreamFactory streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public TableData Read(Stream indexStream, Stream dataStream)
        {
            TableEntryData[] entries = ReadEntries(indexStream, dataStream.Length);

            var melodies = new List<EntryData>();
            var images = new List<EntryData>();
            var animations = new List<EntryData>();
            var resources = new List<EntryData>();
            var scripts = new List<EntryData>();
            var texts = new List<EntryData>();

            foreach (TableEntryData entry in entries)
            {
                Stream entryStream = _streamFactory.CreateSubStream(dataStream, entry.Offset, entry.Size);
                var entryData = new EntryData { Name = entry.Name, Data = entryStream };

                if (entry.Name.EndsWith(".mld"))
                {
                    melodies.Add(entryData);
                    continue;
                }

                if (entry.Name.EndsWith(".jpg"))
                {
                    images.Add(entryData);
                    continue;
                }

                if (entry.Name.EndsWith(".gif"))
                {
                    animations.Add(entryData);
                    continue;
                }

                if (entry.Name is "map.dat" or "memo.dat" or "monooki.dat" or "target.dat")
                {
                    texts.Add(entryData);
                    continue;
                }

                if (entry.Name.StartsWith("event") || entry.Name.StartsWith("room"))
                {
                    scripts.Add(entryData);
                    continue;
                }

                resources.Add(entryData);
            }

            return new TableData
            {
                Images = images.ToArray(),
                Animations = animations.ToArray(),
                Melodies = melodies.ToArray(),
                Texts = texts.ToArray(),
                Scripts = scripts.ToArray(),
                Resources = resources.ToArray()
            };
        }

        private TableEntryData[] ReadEntries(Stream input, long dataSize)
        {
            using var reader = new StreamReader(input);

            string? countLine = reader.ReadLine();
            if (!int.TryParse(countLine, out int entryCount))
                throw new InvalidOperationException("Invalid entry count at line 1.");

            var result = new TableEntryData[entryCount];

            for (var i = 0; i < entryCount; i++)
            {
                string? entryLine = reader.ReadLine();
                if (string.IsNullOrEmpty(entryLine))
                    throw new InvalidOperationException($"Invalid entry info at line {i + 1}.");

                string[] entryInfos = entryLine.Split(',');
                if (entryInfos.Length != 2)
                    throw new InvalidOperationException($"Invalid entry info at line {i + 1}.");

                if (!int.TryParse(entryInfos[1], out int entryOffset))
                    throw new InvalidOperationException($"Invalid entry size at line {i + 1}.");

                result[i] = new TableEntryData { Name = entryInfos[0], Offset = entryOffset, Size = 0 };
                if (i - 1 >= 0)
                    result[i - 1].Size = entryOffset - result[i - 1].Offset;
            }

            result[^1].Size = (int)(dataSize - result[^1].Offset);

            return result;
        }
    }
}
