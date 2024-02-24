using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class IfConditionData
    {
        public bool IsNegate { get; set; }
        public byte ComparisonType { get; set; }
        public short ComparisonValue { get; set; }
    }
}
