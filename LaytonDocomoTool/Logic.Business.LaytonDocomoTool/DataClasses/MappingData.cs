using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.DataClasses
{
    internal class MappingData
    {
        public IDictionary<int, string> Bits { get; set; }
        public IDictionary<int, string> Stories { get; set; }
        public IDictionary<int, string> Speakers { get; set; }
    }
}
