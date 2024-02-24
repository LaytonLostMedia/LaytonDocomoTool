using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Table
{
    internal class TableWriter : ITableWriter
    {
        private readonly ITableComposer _tableComposer;

        public TableWriter(ITableComposer tableComposer)
        {
            _tableComposer = tableComposer;
        }

        public void Write(TableData tableData, Stream dataOutput, Stream indexOutput)
        {
            TableEntryData[] entries = _tableComposer.Compose(tableData, dataOutput);

            Write(entries, indexOutput);
        }

        public void Write(TableEntryData[] entries, Stream indexOutput)
        {
            using var indexWriter = new StreamWriter(indexOutput, leaveOpen: true);

            indexWriter.WriteLine(entries.Length);

            foreach (TableEntryData entry in entries)
                indexWriter.WriteLine($"{entry.name},{entry.offset}");
        }
    }
}
