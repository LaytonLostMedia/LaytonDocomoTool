using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.DataClasses
{
    public class TableData
    {
        public EntryData[] Melodies { get; set; }
        public EntryData[] Images { get; set; }
        public EntryData[] Animations { get; set; }
        public EntryData[] Resources { get; set; }
        public EntryData[] Scripts { get; set; }
        public EntryData[] Texts { get; set; }
    }
}
