using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class BlockExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken CurlyOpen { get; private set; }
        public IReadOnlyList<StatementSyntax> Statements { get; private set; }
        public SyntaxToken CurlyClose { get; private set; }

        public override Location Location => new(CurlyOpen.FullLocation.Position, CurlyClose.FullLocation.EndPosition);

        public BlockExpressionSyntax(SyntaxToken curlyOpen, IReadOnlyList<StatementSyntax>? statements, SyntaxToken curlyClose)
        {
            CurlyOpen = curlyOpen;
            Statements = statements ?? Array.Empty<StatementSyntax>();
            CurlyClose = curlyClose;

            curlyOpen.Parent = this;
            curlyClose.Parent = this;

            foreach (var statement in Statements)
                statement.Parent = this;
        }

        public void SetCurlyOpen(SyntaxToken curlyOpen, bool updatePositions = true)
        {
            curlyOpen.Parent = this;
            CurlyOpen = curlyOpen;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetStatements(IReadOnlyList<StatementSyntax>? statements, bool updatePositions = true)
        {
            Statements = statements ?? Array.Empty<StatementSyntax>();
            foreach (StatementSyntax statement in Statements)
                statement.Parent = this;

            if (updatePositions)
                Root.UpdatePosition();
        }

        public void SetCurlyClose(SyntaxToken curlyClose, bool updatePositions = true)
        {
            curlyClose.Parent = this;
            CurlyClose = curlyClose;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken openParen = CurlyOpen;
            SyntaxToken closeParen = CurlyClose;

            position = openParen.UpdatePosition(position);
            foreach (var statement in Statements)
                position = statement.UpdatePosition(position);
            position = closeParen.UpdatePosition(position);

            CurlyOpen = openParen;
            CurlyClose = closeParen;

            return position;
        }
    }
}
