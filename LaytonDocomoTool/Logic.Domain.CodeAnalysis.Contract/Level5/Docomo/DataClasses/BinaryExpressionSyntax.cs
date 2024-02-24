using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; private set; }
        public SyntaxToken Operation { get; private set; }
        public ExpressionSyntax Right { get; private set; }

        public override Location Location => new(Left.Location.Position, Right.Location.EndPosition);

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operation, ExpressionSyntax right)
        {
            left.Parent = this;
            operation.Parent = this;
            right.Parent = this;

            Left = left;
            Operation = operation;
            Right = right;
        }

        public void SetOperation(SyntaxToken operation, bool updatePositions = true)
        {
            operation.Parent = this;
            Operation = operation;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken operation = Operation;

            position = Left.UpdatePosition(position);
            position = operation.UpdatePosition(position);
            position = Right.UpdatePosition(position);

            Operation = operation;

            return position;
        }
    }
}
