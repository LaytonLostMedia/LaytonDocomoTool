using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectSelectData : ResourceObjectData
    {
        public string BackgroundImageName { get; set; }

        public string[] SelectableImageNames { get; set; }
        public int[] Unknown1 { get; set; }
        public int[] Unknown2 { get; set; }
        public int[] UnselectedImagePosX { get; set; }
        public int[] UnselectedImagePosY { get; set; }
        public int[] SelectorFlipMode { get; set; }
        public int[] SelectorPosX { get; set; }
        public int[] SelectorPosY { get; set; }
        public int[] OrderY { get; set; }
        public int[] OrderX { get; set; }
        public int[] Unknown3 { get; set; }

        public string[] Solutions { get; set; }
    }
}
