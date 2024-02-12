using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script
{
    public interface IScriptReader
    {
        EventEntryData[] Read(Stream input);
    }
}
