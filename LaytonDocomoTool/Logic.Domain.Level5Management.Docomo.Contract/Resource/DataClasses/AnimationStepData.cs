using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class AnimationStepData
    {
        public AnimationStepPartData[] Parts { get; set; }
        public int EndFrame { get; set; }
    }
}
