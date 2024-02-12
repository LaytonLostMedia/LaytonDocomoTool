using CrossCutting.Core.Contract.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Business.LaytonDocomoTool.Contract.Exceptions;

namespace Logic.Business.LaytonDocomoTool.Contract
{
    [MapException(typeof(LaytonDocomoExtractorManagementException))]
    public interface ILaytonDocomoToolManagement
    {
        int Execute();
    }
}
