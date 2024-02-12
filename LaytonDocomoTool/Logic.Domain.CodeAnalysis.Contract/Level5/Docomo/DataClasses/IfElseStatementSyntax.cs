using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class IfElseStatementSyntax : StatementSyntax
    {
        public IfExpressionSyntax If { get; private set; }
        public BlockExpressionSyntax Block { get; private set; }
        public IReadOnlyList<ElseStatementSyntax> Else { get; private set; }

        public override Location Location => new(If.Location.Position, Else.Count <= 0 ? Block.Location.EndPosition : Else[^1].Location.EndPosition);

        public IfElseStatementSyntax(IfExpressionSyntax ifExpression, BlockExpressionSyntax block, IReadOnlyList<ElseStatementSyntax> elseStatements)
        {
            ifExpression.Parent = this;
            block.Parent = this;
            foreach (ElseStatementSyntax elseStatement in elseStatements)
                elseStatement.Parent = this;

            If = ifExpression;
            Block = block;
            Else = elseStatements;
        }

        public void SetIf(IfExpressionSyntax ifExpression, bool updatePositions = true)
        {
            ifExpression.Parent = this;
            If = ifExpression;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetBlock(BlockExpressionSyntax block, bool updatePositions = true)
        {
            block.Parent = this;
            Block = block;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetElse(IReadOnlyList<ElseStatementSyntax> elseStatements, bool updatePositions = true)
        {
            foreach (ElseStatementSyntax elseStatement in elseStatements)
                elseStatement.Parent = this;

            Else = elseStatements;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            position=If.UpdatePosition(position);
            position = Block.UpdatePosition(position);
            foreach (ElseStatementSyntax elseStatement in Else)
                position = elseStatement.UpdatePosition(position);

            return position;
        }
    }
}
