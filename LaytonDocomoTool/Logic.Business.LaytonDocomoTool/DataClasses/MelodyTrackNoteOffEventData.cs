using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events;

namespace Logic.Business.LaytonDocomoTool.DataClasses
{
    public class MelodyTrackNoteOffEventData : MelodyTrackEventData
    {
        public int Voice { get; set; }
        public int Key { get; set; }
        public int Velocity { get; set; }
        public int OctaveShift { get; set; }
    }
}
