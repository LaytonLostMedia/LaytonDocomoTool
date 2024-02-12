using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class ElseStatementSyntax : StatementSyntax
    {
        public SyntaxToken Else { get; private set; }
        public BlockExpressionSyntax Block { get; private set; }

        public override Location Location => new(Else.FullLocation.Position, Block.Location.EndPosition);

        public ElseStatementSyntax(SyntaxToken elseToken, BlockExpressionSyntax blockExpression)
        {
            elseToken.Parent = this;
            blockExpression.Parent = this;

            Else = elseToken;
            Block = blockExpression;
        }

        public void SetElse(SyntaxToken elseToken, bool updatePositions = true)
        {
            elseToken.Parent = this;
            Else = elseToken;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetBlockExpression(BlockExpressionSyntax block, bool updatePositions = true)
        {
            block.Parent = this;
            Block = block;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken elseToken = Else;

            position = elseToken.UpdatePosition(position);
            Block.UpdatePosition(position);

            Else = elseToken;

            return position;
        }
    }
}
