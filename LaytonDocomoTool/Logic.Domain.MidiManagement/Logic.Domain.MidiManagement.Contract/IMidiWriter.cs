using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Domain.MidiManagement.Contract
{
    public interface IMidiWriter
    {
        void Write(MidiData midiData, Stream output);
        void Write(MidiChunkData[] chunks, Stream output);
    }
}
