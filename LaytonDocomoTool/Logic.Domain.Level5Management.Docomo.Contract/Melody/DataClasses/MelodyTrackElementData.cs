using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses
{
    [DebuggerDisplay("{DeltaTime}, {Event.GetType().Name}")]
    public class MelodyTrackElementData
    {
        public int DeltaTime { get; set; }
        public MelodyTrackEventData Event { get; set; }
    }
}
