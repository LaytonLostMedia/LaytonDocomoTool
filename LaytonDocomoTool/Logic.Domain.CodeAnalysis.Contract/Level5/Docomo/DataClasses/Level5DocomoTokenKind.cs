using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public enum Level5DocomoTokenKind
    {
        ParenOpen,
        ParenClose,
        BracketOpen,
        BracketClose,
        CurlyOpen,
        CurlyClose,

        Comma,
        Semicolon,
        EqualsEquals,
        NotEquals,
        SmallerThan,
        GreaterThan,
        SmallerEquals,
        GreaterEquals,

        Identifier,
        NumericLiteral,
        StringLiteral,

        TrueKeyword,
        FalseKeyword,
        IfKeyword,
        ElseKeyword,
        NotKeyword,
        AndKeyword,

        Trivia,

        EndOfFile
    }
}
