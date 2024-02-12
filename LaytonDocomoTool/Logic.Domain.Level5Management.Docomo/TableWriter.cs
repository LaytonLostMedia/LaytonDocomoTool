using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Docomo
{
    internal class TableWriter : ITableWriter
    {
        public void Write(TableData table, Stream indexStream, Stream dataStream)
        {
            using var indexWriter = new StreamWriter(indexStream, leaveOpen: true);

            int entryCount = table.Images.Length + table.Animations.Length + table.Melodies.Length +
                             table.Texts.Length + table.Scripts.Length + table.Resources.Length;

            indexWriter.WriteLine(entryCount);

            EntryData[] entries = GetEntries(table);
            WriteEntries(entries, indexWriter, dataStream);
        }

        private void WriteEntries(EntryData[] entries, StreamWriter indexWriter, Stream dataStream)
        {
            foreach (EntryData entry in entries)
            {
                indexWriter.WriteLine($"{entry.Name},{dataStream.Position}");

                entry.Data.CopyTo(dataStream);
            }
        }

        //private EntryData[] GetEntriesNew(TableData table)
        //{
        //    var path = @"D:\Users\Kirito\Desktop\Layton_Motdm\data\ch1\old\0.dat";
        //    var lines = File.ReadAllLines(path).Skip(1);

        //    string[] origOrder = lines.Select(x => x.Split(',')[0]).ToArray();

        //    var result = new List<EntryData>();

        //    var entries = GetEntries(table);
        //    foreach (string order in origOrder)
        //        result.Add(entries.FirstOrDefault(e => e.Name == order));

        //    return result.ToArray();
        //}

        private EntryData[] GetEntries(TableData table)
        {
            return table.Images.Concat(table.Animations).Concat(table.Melodies).Concat(table.Texts)
                .Concat(table.Scripts).Concat(table.Resources).ToArray();
        }
    }
}
