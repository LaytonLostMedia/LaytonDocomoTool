using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectSwitchData : ResourceObjectData
    {
        public string BackgroundImageName { get; set; }

        public string[][] ImageNames { get; set; }
        public int[] ImagePosX { get; set; }
        public int[] ImagePosY { get; set; }
        public int[] SelectorFlipMode { get; set; }
        public int[] SelectorPosX { get; set; }
        public int[] SelectorPosY { get; set; }
        public int[] OrderY { get; set; }
        public int[] OrderX { get; set; }
        public int[] Solution { get; set; }
    }
}
