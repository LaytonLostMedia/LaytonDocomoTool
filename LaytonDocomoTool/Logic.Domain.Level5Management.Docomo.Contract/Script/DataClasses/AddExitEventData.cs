using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class AddExitEventData:EventData
    {
        public byte Id { get; set; }
        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public byte RoomId { get; set; }
    }
}
