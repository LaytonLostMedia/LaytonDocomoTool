using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceArrayData
    {
        public int[]? IntValues { get; set; }
        public short[]? ShortValues { get; set; }
        public byte[]? ByteValues { get; set; }
        public string[]? StringValues { get; set; }
    }
}
