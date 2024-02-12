using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class AddObjectEventData : EventData
    {
        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public byte Value3 { get; set; }
        public short Value4 { get; set; }
        public short Value5 { get; set; }
        public byte Value6 { get; set; }
    }
}
