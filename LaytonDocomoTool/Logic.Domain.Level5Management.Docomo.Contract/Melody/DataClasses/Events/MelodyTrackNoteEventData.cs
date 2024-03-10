using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events
{
    public sealed class MelodyTrackNoteEventData : MelodyTrackEventData
    {
        public int Voice { get; set; }
        public int Key { get; set; }
        public int Velocity { get; set; }
        public int OctaveShift { get; set; }
    }
}
