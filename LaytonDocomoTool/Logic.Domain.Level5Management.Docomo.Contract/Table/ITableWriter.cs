using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Table
{
    public interface ITableWriter
    {
        void Write(TableData tableData, Stream dataOutput, Stream indexOutput);
        void Write(TableEntryData[] entries, Stream indexOutput);
    }
}
