using Logic.Business.LaytonDocomoTool.Contract.DataClasses;
using Logic.Domain.DocomoManagement.Contract;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Docomo
{
    internal class GameReader : IGameReader
    {
        private readonly IJarReader _jarReader;
        private readonly ITableReader _tableReader;

        public GameReader(IJarReader jarReader, ITableReader tableReader)
        {
            _jarReader = jarReader;
            _tableReader = tableReader;
        }

        public GameData Read(Stream input)
        {
            JarArchiveEntry[] entries = _jarReader.Read(input);

            Stream tableIndexStream = GetTableIndexStream(entries);
            Stream tableDataStream = GetTableDataStream(entries);

            TableData tableData = _tableReader.Read(tableIndexStream, tableDataStream);

            return new GameData
            {
                Table = tableData
            };
        }

        private Stream GetTableIndexStream(JarArchiveEntry[] entries)
        {
            JarArchiveEntry? tblDataEntry = entries.FirstOrDefault(e => e.Path == "tbl/0.dat");
            if (tblDataEntry == null)
                throw new InvalidOperationException("Could not find tbl/0.dat");

            return tblDataEntry.Data;
        }

        private Stream GetTableDataStream(JarArchiveEntry[] entries)
        {
            JarArchiveEntry? tblDataEntry = entries.FirstOrDefault(e => e.Path == "tbl/1.dat");
            if (tblDataEntry == null)
                throw new InvalidOperationException("Could not find tbl/1.dat");

            return tblDataEntry.Data;
        }
    }
}
