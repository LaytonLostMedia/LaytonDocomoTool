using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    public interface IMelodyMidiConverter
    {
        MidiData ConvertMelodyData(MelodyData melodyData);
    }
}
