using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public class PlaySoundEventData : EventData
    {
        public byte Id { get; set; }
        public string FileName { get; set; }
        public bool Value2 { get; set; }
    }
}
