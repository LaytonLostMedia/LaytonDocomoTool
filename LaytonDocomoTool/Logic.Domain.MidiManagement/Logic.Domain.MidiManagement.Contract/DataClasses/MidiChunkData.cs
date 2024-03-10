using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.MidiManagement.Contract.DataClasses
{
    public class MidiChunkData
    {
        public string Identifier { get; set; }
        public Stream Data { get; set; }
    }
}
