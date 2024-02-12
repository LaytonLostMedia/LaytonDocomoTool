using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script
{
    public interface IScriptWriter
    {
        void Write(EventEntryData[] events, Stream output);
    }
}
