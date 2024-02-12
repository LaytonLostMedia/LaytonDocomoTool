using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class ChoiceEventData : EventData
    {
        public string Name { get; set; }
        public string[] Choices { get; set; }
        public byte Value1 { get; set; }
    }
}
