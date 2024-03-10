using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses
{
    [DebuggerDisplay("{DeltaTime}, {Event.GetType().Name}")]
    public class MidiTrackElementData
    {
        public int DeltaTime { get; set; }
        public MidiTrackEventData Event { get; set; }
    }
}
