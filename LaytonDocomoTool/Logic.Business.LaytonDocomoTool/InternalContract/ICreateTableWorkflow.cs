using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    internal interface ICreateTableWorkflow
    {
        void Work();
        void Work(Stream indexOutput, Stream dataOutput);
    }
}
