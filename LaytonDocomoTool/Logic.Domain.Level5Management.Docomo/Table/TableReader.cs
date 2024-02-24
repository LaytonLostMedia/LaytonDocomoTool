using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Table
{
    internal class TableReader : ITableReader
    {
        public TableEntryData[] Read(Stream input)
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

                result[i] = new TableEntryData { name = entryInfos[0], offset = entryOffset };
            }

            return result;
        }
    }
}
