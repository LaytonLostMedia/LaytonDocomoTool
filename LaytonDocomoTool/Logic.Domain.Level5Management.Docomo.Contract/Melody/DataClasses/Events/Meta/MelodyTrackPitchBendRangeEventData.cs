using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta
{
    public sealed class MelodyTrackPitchBendRangeEventData : MelodyTrackInstrumentEventData
    {
        public int Channel { get; set; }
        public int Range { get; set; }
    }
}
