using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    internal interface ILevel5DocomoCodeUnitConverter
    {
        EventData[] CreateEvents(CodeUnitSyntax codeUnit);
    }
}
