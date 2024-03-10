using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Events.Meta
{
    public sealed class MidiTrackCopyrightEventData : MidiTrackMetaEventData
    {
        public string Copyright { get; set; }
    }
}
