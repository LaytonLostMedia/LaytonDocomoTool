﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses
{
    public abstract class BranchBlockEventData : BranchEventData
    {
        public EventData[] Events { get; set; }
    }
}
