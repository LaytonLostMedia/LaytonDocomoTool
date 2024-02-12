using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract
{
    public interface IResourceWriter
    {
        void Write(EntryData[] entries, Stream input);
    }
}
