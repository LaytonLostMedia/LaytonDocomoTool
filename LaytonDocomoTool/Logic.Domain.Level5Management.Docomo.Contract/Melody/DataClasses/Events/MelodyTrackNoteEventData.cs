using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events
{
    public sealed class MelodyTrackNoteEventData : MelodyTrackEventData
    {
        public int Channel { get; set; }
        public int Note { get; set; }
        public int Velocity { get; set; }
        public int OctaveShift { get; set; }
    }
}
