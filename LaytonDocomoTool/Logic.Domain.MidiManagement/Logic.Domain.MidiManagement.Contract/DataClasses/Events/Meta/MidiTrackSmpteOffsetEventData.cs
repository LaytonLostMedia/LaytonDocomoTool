using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Events.Meta
{
    public sealed class MidiTrackSmpteOffsetEventData : MidiTrackMetaEventData
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Frames { get; set; }
        public int FractionalFrames { get; set; }
    }
}
