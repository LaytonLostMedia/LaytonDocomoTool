using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class FunctionParametersSyntax : SyntaxNode
    {
        public SyntaxToken ParenOpen { get; private set; }
        public CommaSeparatedSyntaxList<ExpressionSyntax> Parameters { get; private set; }
        public SyntaxToken ParenClose { get; private set; }

        public override Location Location => new(ParenOpen.FullLocation.Position, ParenClose.FullLocation.EndPosition);

        public FunctionParametersSyntax(SyntaxToken parenOpen, CommaSeparatedSyntaxList<ExpressionSyntax>? parameters, SyntaxToken parenClose)
        {
            parenOpen.Parent = this;
            if (parameters != null)
                parameters.Parent = this;
            parenClose.Parent = this;

            ParenOpen = parenOpen;
            Parameters = parameters ?? new CommaSeparatedSyntaxList<ExpressionSyntax>(null);
            ParenClose = parenClose;
        }

        public void SetParenOpen(SyntaxToken parenOpen, bool updatePositions = true)
        {
            parenOpen.Parent = this;
            ParenOpen = parenOpen;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetParenClose(SyntaxToken parenClose, bool updatePositions = true)
        {
            parenClose.Parent = this;
            ParenClose = parenClose;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken parenOpen = ParenOpen;
            SyntaxToken parenClose = ParenClose;

            position = parenOpen.UpdatePosition(position);
            position = Parameters.UpdatePosition(position);
            position = parenClose.UpdatePosition(position);

            ParenOpen = parenOpen;
            ParenClose = parenClose;

            return position;
        }
    }
}
