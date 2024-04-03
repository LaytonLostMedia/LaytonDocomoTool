using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events.Meta
{
    public sealed class MelodyTrackEditInstrumentEventData : MelodyTrackExtendedEventData
    {
        public int Channel { get; set; }
        public EditInstrumentModulator Modulator { get; set; }
        public EditInstrumentCarrier Carrier { get; set; }
        public int OctaveSelect { get; set; }
    }
}
