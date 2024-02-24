using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Table
{
    internal class TableComposer : ITableComposer
    {
        public TableEntryData[] Compose(TableData tableData, Stream dataOutput)
        {
            EntryData[] entries = GetEntries(tableData);

            var result = new TableEntryData[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                long offset = dataOutput.Position;

                entries[i].Data.CopyTo(dataOutput);

                result[i] = new TableEntryData
                {
                    name = entries[i].Name,
                    offset = (int)offset
                };
            }

            return result;
        }

        private EntryData[] GetEntries(TableData table)
        {
            return table.Images
                .Concat(table.Animations)
                .Concat(table.Melodies)
                .Concat(table.Texts)
                .Concat(table.Scripts)
                .Concat(table.Resources)
                .ToArray();
        }
    }
}
