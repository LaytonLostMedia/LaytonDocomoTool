using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ButtonData
    {
        public string?[] ImageNames { get; set; }
        public int ImageX { get; set; }
        public int ImageY { get; set; }
        public int FlipMode { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int OrderY { get; set; }
        public int OrderX { get; set; }
        public string Result { get; set; }
    }
}
