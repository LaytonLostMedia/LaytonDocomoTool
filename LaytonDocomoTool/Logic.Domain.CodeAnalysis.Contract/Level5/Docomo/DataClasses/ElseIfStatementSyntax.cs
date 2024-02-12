using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class ElseIfStatementSyntax : ElseStatementSyntax
    {
        public IfExpressionSyntax If { get; private set; }

        public ElseIfStatementSyntax(SyntaxToken elseToken, IfExpressionSyntax ifExpression, BlockExpressionSyntax blockExpression) : base(elseToken, blockExpression)
        {
            ifExpression.Parent = this;

            If = ifExpression;
        }

        public void SetIf(IfExpressionSyntax ifToken, bool updatePositions = true)
        {
            ifToken.Parent = this;
            If = ifToken;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken elseToken = Else;

            position = elseToken.UpdatePosition(position);
            position = If.UpdatePosition(position);
            position = Block.UpdatePosition(position);

            return position;
        }
    }
}
