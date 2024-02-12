using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class AddBgObjEventData : EventData
    {
        public byte Id { get; set; }
        public short Value1 { get; set; }
        public short Value2 { get; set; }
    }
}
