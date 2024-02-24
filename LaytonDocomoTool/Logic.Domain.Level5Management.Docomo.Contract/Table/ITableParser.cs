using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Table
{
    public interface ITableParser
    {
        TableData Parse(Stream indexStream, Stream dataStream);
        TableData Parse(TableEntryData[] entries, Stream dataStream);
    }
}
