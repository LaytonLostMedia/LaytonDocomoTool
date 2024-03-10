using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.MidiManagement.Contract.DataClasses
{
    public class MidiData
    {
        public MidiHeaderData Header { get; set; }
        public MidiTrackData[] Tracks { get; set; }
    }
}
