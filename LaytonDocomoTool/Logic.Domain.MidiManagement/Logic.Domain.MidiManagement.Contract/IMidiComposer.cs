using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Domain.MidiManagement.Contract
{
    public interface IMidiComposer
    {
        MidiChunkData[] Compose(MidiData midiData);
    }
}
