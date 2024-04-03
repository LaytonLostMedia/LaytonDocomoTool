using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta
{
    public sealed class MelodyTrackModulationEventData : MelodyTrackInstrumentEventData
    {
        public int Channel { get; set; }
        public int Depth { get; set; }
    }
}
