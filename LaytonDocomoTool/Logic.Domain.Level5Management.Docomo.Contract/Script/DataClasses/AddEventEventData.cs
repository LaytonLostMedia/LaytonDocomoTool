using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class AddEventEventData:EventData
    {
        public byte Value1 { get; set; }
        public short Value2 { get; set; }
        public byte Value3 { get; set; }
        public byte Value4 { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public string Text { get; set; }

        public byte? Value5 { get; set; }
        public byte? Value6 { get; set; }
    }
}
