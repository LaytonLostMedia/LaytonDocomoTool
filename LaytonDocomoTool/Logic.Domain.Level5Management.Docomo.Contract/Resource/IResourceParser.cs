using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceParser
    {
        ResourceData Parse(Stream resourceStream);
        ResourceData Parse(ResourceEntryData[] entries);
    }
}
