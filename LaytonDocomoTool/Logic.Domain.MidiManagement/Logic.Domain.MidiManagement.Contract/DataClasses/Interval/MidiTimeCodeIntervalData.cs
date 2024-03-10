using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Interval
{
    public sealed class MidiTimeCodeIntervalData : MidiIntervalData
    {
        public int SubFrameResolution { get; set; }
        public int FramesPerSecond { get; set; }
    }
}
