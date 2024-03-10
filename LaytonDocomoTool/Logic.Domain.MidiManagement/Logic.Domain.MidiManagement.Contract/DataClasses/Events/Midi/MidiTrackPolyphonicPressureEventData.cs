using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Events.Midi
{
    public sealed class MidiTrackPolyphonicPressureEventData : MidiTrackMidiEventData
    {
        public int Note { get; set; }
        public int Pressure { get; set; }
    }
}
