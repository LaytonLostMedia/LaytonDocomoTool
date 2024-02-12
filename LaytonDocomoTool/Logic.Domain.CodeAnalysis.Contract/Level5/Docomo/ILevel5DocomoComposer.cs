using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.Exceptions;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo
{
    [MapException(typeof(Level5DocomoComposeException))]
    public interface ILevel5DocomoComposer
    {
        string ComposeCodeUnit(CodeUnitSyntax codeUnit);
    }
}
