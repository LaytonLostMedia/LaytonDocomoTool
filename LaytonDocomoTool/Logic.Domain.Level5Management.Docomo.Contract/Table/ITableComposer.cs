using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Table
{
    public interface ITableComposer
    {
        TableEntryData[] Compose(TableData tableData, Stream output);
    }
}
