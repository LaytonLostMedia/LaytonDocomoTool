using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta;

namespace Logic.Business.LaytonDocomoTool.DataClasses
{
    public sealed class MelodyTrackCombinedInstrumentEventData : MelodyTrackInstrumentEventData
    {
        public int Part { get; set; }
        public int Instrument { get; set; }
    }
}
