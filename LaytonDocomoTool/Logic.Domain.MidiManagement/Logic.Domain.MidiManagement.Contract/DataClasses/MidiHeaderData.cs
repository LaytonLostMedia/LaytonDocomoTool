using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Interval;

namespace Logic.Domain.MidiManagement.Contract.DataClasses
{
    public class MidiHeaderData
    {
        public int Format { get; set; }
        public int TrackCount { get; set; }
        public MidiIntervalData Interval { get; set; }
    }
}
