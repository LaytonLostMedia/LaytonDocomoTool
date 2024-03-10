using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta
{
    public sealed class MelodyTrackDrumScaleEventData : MelodyTrackSystemEventData
    {
        public int Channel { get; set; }
        public int Drum { get; set; }
    }
}
