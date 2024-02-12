using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script
{
    public interface IScriptParser
    {
        EventData[] Parse(Stream input);
        EventData[] Parse(EventEntryData[] entries);
    }
}
