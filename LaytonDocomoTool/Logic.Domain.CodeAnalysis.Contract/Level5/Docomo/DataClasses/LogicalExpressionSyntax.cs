using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class LogicalExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; private set; }
        public SyntaxToken Operation { get; private set; }
        public ExpressionSyntax Right { get; private set; }

        public override Location Location => new(Left.Location.Position, Right.Location.EndPosition);

        public LogicalExpressionSyntax(ExpressionSyntax left, SyntaxToken operation, ExpressionSyntax right)
        {
            left.Parent = this;
            operation.Parent = this;
            right.Parent = this;

            Left = left;
            Operation = operation;
            Right = right;
        }

        public void SetLeft(ExpressionSyntax left, bool updatePositions = true)
        {
            left.Parent = this;
            Left = left;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetOperation(SyntaxToken operation, bool updatePositions = true)
        {
            operation.Parent = this;
            Operation = operation;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetRight(ExpressionSyntax right, bool updatePositions = true)
        {
            right.Parent = this;
            Right = right;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken logical = Operation;

            position = Left.UpdatePosition(position);
            position = logical.UpdatePosition(position);
            position = Right.UpdatePosition(position);

            Operation = logical;

            return position;
        }
    }
}
