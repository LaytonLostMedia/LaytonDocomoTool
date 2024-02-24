using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceData
    {
        public ResourceResEntryData[] ResourceFiles { get; set; }
        public ResourceObjectData[] Objects { get; set; }
        public ResourceArrayData ValueArrays { get; set; }
    }
}
