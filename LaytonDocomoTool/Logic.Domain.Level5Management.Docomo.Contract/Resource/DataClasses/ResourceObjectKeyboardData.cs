using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses
{
    public class ResourceObjectKeyboardData : ResourceObjectData
    {
        public string ResourceName { get; set; }
        public string[] Solutions { get; set; }
        public int Digits { get; set; }
    }
}
