using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo.DataClasses
{
    public class Level5DocomoSyntaxToken
    {
        public Level5DocomoTokenKind Kind { get; }
        public string Text { get; }

        public int Position { get; }
        public int Line { get; }
        public int Column { get; }

        public Level5DocomoSyntaxToken(Level5DocomoTokenKind kind, int position, int line, int column, string? text = null)
        {
            Text = text ?? string.Empty;
            Kind = kind;
            Position = position;
            Line = line;
            Column = column;
        }
    }
}
