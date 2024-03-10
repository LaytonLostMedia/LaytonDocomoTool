using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses.Events;

namespace Logic.Business.LaytonDocomoTool.DataClasses
{
    [DebuggerDisplay("{id}, {totalTime}, {melodyEvent.GetType().Name}")]
    public struct MelodyTrackConversionData
    {
        public int id;
        public int totalTime;
        public int channelBase;
        public MelodyTrackEventData melodyEvent;
    }
}
