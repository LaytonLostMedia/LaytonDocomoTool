using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectButtonData : ResourceObjectData
    {
        public string? BackgroundImageName { get; set; }
        public int Unknown { get; set; }
        public ButtonData[] Buttons { get; set; }
    }
}
