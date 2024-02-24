using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectAnimeData : ResourceObjectData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string?[] ImageNames { get; set; }
        public int[] Values2 { get; set; }
        public int[] Values3 { get; set; }
    }
}
