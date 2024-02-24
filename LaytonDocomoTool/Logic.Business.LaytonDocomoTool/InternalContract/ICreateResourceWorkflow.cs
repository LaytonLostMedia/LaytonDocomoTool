using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    public interface ICreateResourceWorkflow
    {
        void Work();
        void Work(string resourceDirectory, Stream output);
    }
}
