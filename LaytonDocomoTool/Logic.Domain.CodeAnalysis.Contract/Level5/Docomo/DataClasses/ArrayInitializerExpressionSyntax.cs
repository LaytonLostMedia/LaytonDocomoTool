using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class ArrayInitializerExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken BracketOpen { get; private set; }
        public CommaSeparatedSyntaxList<ExpressionSyntax>? Values { get; private set; }
        public SyntaxToken BracketClose { get; private set; }

        public override Location Location => new(BracketOpen.FullLocation.Position, BracketClose.FullLocation.EndPosition);

        public ArrayInitializerExpressionSyntax(SyntaxToken bracketOpen, CommaSeparatedSyntaxList<ExpressionSyntax>? values, SyntaxToken bracketClose)
        {
            bracketOpen.Parent = this;
            if (values != null)
                values.Parent = this;
            bracketClose.Parent = this;

            BracketOpen = bracketOpen;
            Values = values;
            BracketClose = bracketClose;
        }

        public void SetBracketOpen(SyntaxToken bracketOpen, bool updatePositions = true)
        {
            bracketOpen.Parent = this;
            BracketOpen = bracketOpen;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetValues(CommaSeparatedSyntaxList<ExpressionSyntax>? values, bool updatePositions = true)
        {
            if (values != null)
                values.Parent = this;
            values = values;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetBracketClose(SyntaxToken bracketClose, bool updatePositions = true)
        {
            bracketClose.Parent = this;
            BracketClose = bracketClose;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken bracketOpen = BracketOpen;
            SyntaxToken bracketClose = BracketClose;

            position = bracketOpen.UpdatePosition(position);
            if (Values != null)
                position = Values.UpdatePosition(position);
            position = bracketClose.UpdatePosition(position);

            BracketOpen = bracketOpen;
            BracketClose = bracketClose;

            return position;
        }
    }
}
