using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Table
{
    internal class TableParser : ITableParser
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ITableReader _tableReader;

        public TableParser(IStreamFactory streamFactory, ITableReader tableReader)
        {
            _streamFactory = streamFactory;
            _tableReader = tableReader;
        }

        public TableData Parse(Stream indexStream, Stream dataStream)
        {
            TableEntryData[] entries = _tableReader.Read(indexStream);

            return Parse(entries, dataStream);
        }

        public TableData Parse(TableEntryData[] entries, Stream dataStream)
        {
            var melodies = new List<EntryData>();
            var images = new List<EntryData>();
            var animations = new List<EntryData>();
            var resources = new List<EntryData>();
            var scripts = new List<EntryData>();
            var texts = new List<EntryData>();

            for (var i = 0; i < entries.Length; i++)
            {
                TableEntryData entry = entries[i];

                long endOffset = i + 1 >= entries.Length ? dataStream.Length : entries[i + 1].offset;
                long entrySize = endOffset - entry.offset;

                Stream entryStream = _streamFactory.CreateSubStream(dataStream, entry.offset, entrySize);
                var entryData = new EntryData { Name = entry.name, Data = entryStream };

                if (entry.name.EndsWith(".mld"))
                {
                    melodies.Add(entryData);
                    continue;
                }

                if (entry.name.EndsWith(".jpg"))
                {
                    images.Add(entryData);
                    continue;
                }

                if (entry.name.EndsWith(".gif"))
                {
                    animations.Add(entryData);
                    continue;
                }

                if (entry.name is "map.dat" or "memo.dat" or "monooki.dat" or "target.dat")
                {
                    texts.Add(entryData);
                    continue;
                }

                if (entry.name.StartsWith("event") || entry.name.StartsWith("room"))
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
    }
}
