using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Business.LaytonDocomoTool.Exceptions;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    [MapException(typeof(ExtractScriptWorkflowException))]
    public interface IExtractScriptWorkflow
    {
        void Work();
    }
}
