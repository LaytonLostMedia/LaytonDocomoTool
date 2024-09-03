using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class AddEventEventData:EventData
    {
        public byte EventType { get; set; }
        public short SpeakerId { get; set; }
        public byte RankX { get; set; }
        public byte RankY { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public string Text { get; set; }

        public byte? Value5 { get; set; }
        public byte? Value6 { get; set; }
    }
}
