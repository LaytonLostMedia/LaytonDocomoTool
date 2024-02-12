using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses
{
    public class FunctionInvocationStatementSyntax : StatementSyntax
    {
        public FunctionInvocationExpressionSyntax FunctionInvocation { get; private set; }
        public SyntaxToken Semicolon { get; private set; }

        public override Location Location => new(FunctionInvocation.Location.Position, Semicolon.FullLocation.EndPosition);

        public FunctionInvocationStatementSyntax(FunctionInvocationExpressionSyntax functionInvocation, SyntaxToken semicolon)
        {
            functionInvocation.Parent = this;
            semicolon.Parent = this;

            FunctionInvocation = functionInvocation;
            Semicolon = semicolon;
        }

        public void SetSemicolon(SyntaxToken semicolon, bool updatePositions = true)
        {
            semicolon.Parent = this;
            Semicolon = semicolon;

            if (updatePositions)
                Root.UpdatePosition();
        }

        internal override int UpdatePosition(int position = 0)
        {
            SyntaxToken semicolon = Semicolon;

            position = FunctionInvocation.UpdatePosition(position);
            position = semicolon.UpdatePosition(position);

            Semicolon = semicolon;

            return position;
        }
    }
}
