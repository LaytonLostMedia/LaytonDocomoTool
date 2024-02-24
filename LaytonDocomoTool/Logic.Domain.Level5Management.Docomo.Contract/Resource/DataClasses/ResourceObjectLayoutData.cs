using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectLayoutData : ResourceObjectData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public LayoutResourcePosition[] ResourcePositions { get; set; }
        public LayoutResourceArea[] ResourceAreas { get; set; }
        public int[] Unknown1 { get; set; }
        public int[] Unknown2 { get; set; }
    }
}
