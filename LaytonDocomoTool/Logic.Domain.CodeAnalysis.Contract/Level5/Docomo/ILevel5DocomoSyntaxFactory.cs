using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo
{
    public interface ILevel5DocomoSyntaxFactory : ISyntaxFactory<Level5DocomoTokenKind>
    {
        SyntaxToken Identifier(string text);
        SyntaxToken StringLiteral(string text);
        SyntaxToken NumericLiteral(int value);
    }
}
