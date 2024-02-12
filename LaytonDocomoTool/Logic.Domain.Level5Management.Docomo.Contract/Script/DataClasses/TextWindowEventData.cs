using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class TextWindowEventData : EventData
    {
        public byte Value1 { get; set; }
        public byte PersonId { get; set; }
        public string Text { get; set; }
    }
}
