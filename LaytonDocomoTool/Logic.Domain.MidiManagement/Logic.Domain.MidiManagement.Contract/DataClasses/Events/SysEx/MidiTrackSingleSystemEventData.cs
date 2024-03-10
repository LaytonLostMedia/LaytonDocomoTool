using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Events.SysEx
{
    public sealed class MidiTrackSingleSystemEventData : MidiTrackSysExEventData
    {
        public byte[] Message { get; set; }
    }
}
