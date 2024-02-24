using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectLineData : ResourceObjectData
    {
        public string BackgroundImageName { get; set; }

        public int[] LinePointPosX { get; set; }
        public int[] LinePointPosY { get; set; }
        public int[] OrderY { get; set; }
        public int[] OrderX { get; set; }

        public string[] Solution { get; set; }
        public string[] BlockPointCombination { get; set; }
        public string[] Unknown1 { get; set; }
    }
}
