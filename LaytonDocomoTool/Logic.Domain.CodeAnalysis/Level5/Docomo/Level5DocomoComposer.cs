using System.Text;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoComposer : ILevel5DocomoComposer
    {
        private readonly ILevel5DocomoSyntaxFactory _syntaxFactory;

        public Level5DocomoComposer(ILevel5DocomoSyntaxFactory syntaxFactory)
        {
            _syntaxFactory = syntaxFactory;
        }

        public string ComposeCodeUnit(CodeUnitSyntax codeUnit)
        {
            var sb = new StringBuilder();

            ComposeCodeUnit(codeUnit, sb);

            return sb.ToString();
        }

        private void ComposeCodeUnit(CodeUnitSyntax codeUnit, StringBuilder sb)
        {
            foreach (StatementSyntax statement in codeUnit.Statements)
                ComposeStatement(statement, sb);
        }

        private void ComposeStatement(StatementSyntax statement, StringBuilder sb)
        {
            switch (statement)
            {
                case FunctionInvocationStatementSyntax functionInvocation:
                    ComposeFunctionInvocationStatement(functionInvocation, sb);
                    break;

                case IfElseStatementSyntax ifElseStatement:
                    ComposeIfElseStatement(ifElseStatement, sb);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown statement {statement.GetType().Name}.");
            }
        }

        private void ComposeFunctionInvocationStatement(FunctionInvocationStatementSyntax functionInvocation, StringBuilder sb)
        {
            ComposeFunctionInvocationExpression(functionInvocation.FunctionInvocation, sb);
            ComposeSyntaxToken(functionInvocation.Semicolon, sb);
        }

        private void ComposeFunctionInvocationExpression(FunctionInvocationExpressionSyntax functionInvocation, StringBuilder sb)
        {
            ComposeNameSyntax(functionInvocation.Name, sb);
            ComposeFunctionInvocationParameters(functionInvocation.ParameterList, sb);
        }

        private void ComposeFunctionInvocationParameters(FunctionParametersSyntax functionParameters, StringBuilder sb)
        {
            ComposeSyntaxToken(functionParameters.ParenOpen, sb);
            ComposeCommaSeparatedList(functionParameters.Parameters, sb);
            ComposeSyntaxToken(functionParameters.ParenClose, sb);
        }

        private void ComposeIfElseStatement(IfElseStatementSyntax ifElseStatement, StringBuilder sb)
        {
            ComposeIfExpression(ifElseStatement.If, sb);
            ComposeBlockExpression(ifElseStatement.Block, sb);

            foreach (ElseStatementSyntax elseStatement in ifElseStatement.Else)
            {
                switch (elseStatement)
                {
                    case ElseIfStatementSyntax elseIfStatement:
                        ComposeElseIfStatement(elseIfStatement, sb);
                        break;

                    default:
                        ComposeElseStatement(elseStatement, sb);
                        break;
                }
            }
        }

        private void ComposeElseIfStatement(ElseIfStatementSyntax elseIfStatement, StringBuilder sb)
        {
            ComposeSyntaxToken(elseIfStatement.Else, sb);
            ComposeIfExpression(elseIfStatement.If, sb);
            ComposeBlockExpression(elseIfStatement.Block, sb);
        }

        private void ComposeElseStatement(ElseStatementSyntax elseStatement, StringBuilder sb)
        {
            ComposeSyntaxToken(elseStatement.Else, sb);
            ComposeBlockExpression(elseStatement.Block, sb);
        }

        private void ComposeIfExpression(IfExpressionSyntax ifExpression, StringBuilder sb)
        {
            ComposeSyntaxToken(ifExpression.If, sb);
            ComposeExpression(ifExpression.CompareExpression, sb);
        }

        private void ComposeBlockExpression(BlockExpressionSyntax block, StringBuilder sb)
        {
            ComposeSyntaxToken(block.CurlyOpen, sb);
            foreach (StatementSyntax statement in block.Statements)
                ComposeStatement(statement, sb);
            ComposeSyntaxToken(block.CurlyClose, sb);
        }

        private void ComposeCommaSeparatedList(CommaSeparatedSyntaxList<ExpressionSyntax> list, StringBuilder sb)
        {
            if (list.Elements.Count <= 0)
                return;

            for (var i = 0; i < list.Elements.Count - 1; i++)
            {
                ComposeExpression(list.Elements[i], sb);
                ComposeSyntaxToken(_syntaxFactory.Token(Level5DocomoTokenKind.Comma), sb);
            }

            ComposeExpression(list.Elements[^1], sb);
        }

        private void ComposeExpression(ExpressionSyntax expression, StringBuilder sb)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literalExpression:
                    ComposeLiteralExpression(literalExpression, sb);
                    break;

                case UnaryExpressionSyntax unaryExpression:
                    ComposeUnaryExpression(unaryExpression, sb);
                    break;

                case ArrayInitializerExpressionSyntax arrayInitializer:
                    ComposeArrayInitializerExpression(arrayInitializer, sb);
                    break;

                case NameSyntax nameExpression:
                    ComposeNameSyntax(nameExpression, sb);
                    break;

                case FunctionInvocationExpressionSyntax functionInvocation:
                    ComposeFunctionInvocationExpression(functionInvocation, sb);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}.");
            }
        }

        private void ComposeLiteralExpression(LiteralExpressionSyntax literal, StringBuilder sb)
        {
            ComposeSyntaxToken(literal.Literal, sb);
        }

        private void ComposeUnaryExpression(UnaryExpressionSyntax unary, StringBuilder sb)
        {
            ComposeSyntaxToken(unary.Operation, sb);
            ComposeExpression(unary.Expression, sb);
        }

        private void ComposeArrayInitializerExpression(ArrayInitializerExpressionSyntax arrayInitializer, StringBuilder sb)
        {
            ComposeSyntaxToken(arrayInitializer.BracketOpen, sb);
            if (arrayInitializer.Values != null)
                ComposeCommaSeparatedList(arrayInitializer.Values, sb);
            ComposeSyntaxToken(arrayInitializer.BracketClose, sb);
        }

        private void ComposeNameSyntax(NameSyntax name, StringBuilder sb)
        {
            switch (name)
            {
                case SimpleNameSyntax simpleName:
                    ComposeSimpleName(simpleName, sb);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown name expression {name.GetType().Name}.");
            }
        }

        private void ComposeSimpleName(SimpleNameSyntax simpleName, StringBuilder sb)
        {
            ComposeSyntaxToken(simpleName.Identifier, sb);
        }

        private void ComposeSyntaxToken(SyntaxToken token, StringBuilder sb)
        {
            if (token.LeadingTrivia.HasValue)
                sb.Append(token.LeadingTrivia.Value.Text);

            sb.Append(token.Text);

            if (token.TrailingTrivia.HasValue)
                sb.Append(token.TrailingTrivia.Value.Text);
        }
    }
}
