using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.CodeAnalysis.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoWhitespaceNormalizer : ILevel5DocomoWhitespaceNormalizer
    {
        public void NormalizeCodeUnit(CodeUnitSyntax codeUnit)
        {
            var ctx = new WhitespaceNormalizeContext();
            NormalizeCodeUnit(codeUnit, ctx);

            codeUnit.Update();
        }

        private void NormalizeCodeUnit(CodeUnitSyntax codeUnit, WhitespaceNormalizeContext ctx)
        {
            ctx.ShouldIndent = true;
            ctx.Indent = 0;

            foreach (StatementSyntax statement in codeUnit.Statements)
            {
                ctx.IsFirstElement = codeUnit.Statements[0] == statement;
                ctx.ShouldLineBreak = codeUnit.Statements[^1] != statement;

                NormalizeStatement(statement, ctx);
            }
        }

        private void NormalizeStatement(StatementSyntax statement, WhitespaceNormalizeContext ctx)
        {
            switch (statement)
            {
                case FunctionInvocationStatementSyntax functionInvocation:
                    NormalizeFunctionInvocationStatement(functionInvocation, ctx);
                    break;

                case IfElseStatementSyntax ifElseStatement:
                    NormalizeIfElseStatement(ifElseStatement, ctx);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown statement {statement.GetType().Name}.");
            }
        }

        private void NormalizeFunctionInvocationStatement(FunctionInvocationStatementSyntax functionInvocation, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken semicolonToken = functionInvocation.Semicolon.WithNoTrivia();

            if (ctx.ShouldLineBreak)
                semicolonToken = semicolonToken.WithTrailingTrivia("\r\n");

            ctx.ShouldLineBreak = false;
            NormalizeFunctionInvocationExpression(functionInvocation.FunctionInvocation, ctx);

            functionInvocation.SetSemicolon(semicolonToken, false);
        }

        private void NormalizeFunctionInvocationExpression(FunctionInvocationExpressionSyntax functionInvocation, WhitespaceNormalizeContext ctx)
        {
            NormalizeNameSyntax(functionInvocation.Name, ctx);
            NormalizeFunctionInvocationParameters(functionInvocation.ParameterList, ctx);
        }

        private void NormalizeFunctionInvocationParameters(FunctionParametersSyntax functionParameters, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken parenOpen = functionParameters.ParenOpen.WithNoTrivia();
            SyntaxToken parenClose = functionParameters.ParenClose.WithNoTrivia();

            if (ctx.ShouldLineBreak)
                parenClose = parenClose.WithTrailingTrivia("\r\n");

            ctx.ShouldLineBreak = false;
            NormalizeCommaSeparatedList(functionParameters.Parameters, ctx);

            functionParameters.SetParenOpen(parenOpen, false);
            functionParameters.SetParenClose(parenClose, false);
        }

        private void NormalizeIfElseStatement(IfElseStatementSyntax ifElseStatement, WhitespaceNormalizeContext ctx)
        {
            bool shouldLineBreak = ctx.ShouldLineBreak;

            ctx.ShouldLineBreak = true;
            NormalizeIfExpression(ifElseStatement.If, ctx);

            if (ifElseStatement.Else.Count <= 0)
                ctx.ShouldLineBreak = shouldLineBreak;

            NormalizeBlockExpression(ifElseStatement.Block, ctx);

            foreach (ElseStatementSyntax elseStatement in ifElseStatement.Else)
            {
                if (ifElseStatement.Else[^1] == elseStatement)
                    ctx.ShouldLineBreak = shouldLineBreak;

                switch (elseStatement)
                {
                    case ElseIfStatementSyntax elseIfStatement:
                        NormalizeElseIfStatement(elseIfStatement, ctx);
                        break;

                    default:
                        NormalizeElseStatement(elseStatement, ctx);
                        break;
                }
            }
        }

        private void NormalizeElseIfStatement(ElseIfStatementSyntax elseIfStatement, WhitespaceNormalizeContext ctx)
        {
            bool shouldLineBreak = ctx.ShouldLineBreak;

            SyntaxToken elseToken = elseIfStatement.Else.WithNoTrivia().WithTrailingTrivia(" ");

            if (ctx is { ShouldIndent: true, Indent: > 0 })
                elseToken = elseToken.WithLeadingTrivia(new string('\t', ctx.Indent));

            ctx.ShouldIndent = false;
            ctx.ShouldLineBreak = true;
            NormalizeIfExpression(elseIfStatement.If, ctx);

            ctx.ShouldIndent = true;
            ctx.ShouldLineBreak = shouldLineBreak;
            NormalizeBlockExpression(elseIfStatement.Block, ctx);

            elseIfStatement.SetElse(elseToken, false);
        }

        private void NormalizeElseStatement(ElseStatementSyntax elseStatement, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken elseToken = elseStatement.Else.WithNoTrivia().WithTrailingTrivia("\r\n");

            if (ctx is { ShouldIndent: true, Indent: > 0 })
                elseToken = elseToken.WithLeadingTrivia(new string('\t', ctx.Indent));

            NormalizeBlockExpression(elseStatement.Block, ctx);

            elseStatement.SetElse(elseToken, false);
        }

        private void NormalizeIfExpression(IfExpressionSyntax ifExpression, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken ifToken = ifExpression.If.WithNoTrivia().WithTrailingTrivia(" ");

            if (ctx is { ShouldIndent: true, Indent: > 0 })
                ifToken = ifToken.WithLeadingTrivia(new string('\t', ctx.Indent));

            ctx.ShouldIndent = false;
            ctx.IsFirstElement = true;
            ctx.ShouldLineBreak = true;
            NormalizeExpression(ifExpression.ConditionExpression, ctx);

            ifExpression.SetIf(ifToken, false);
        }

        private void NormalizeBlockExpression(BlockExpressionSyntax block, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken curlyOpen = block.CurlyOpen.WithNoTrivia().WithTrailingTrivia("\r\n");
            SyntaxToken curlyClose = block.CurlyClose.WithNoTrivia();

            if (ctx is { ShouldIndent: true, Indent: > 0 })
            {
                curlyOpen = curlyOpen.WithLeadingTrivia(new string('\t', ctx.Indent));
                curlyClose = curlyClose.WithLeadingTrivia(new string('\t', ctx.Indent));
            }
            if (ctx.ShouldLineBreak)
                curlyClose = curlyClose.WithTrailingTrivia("\r\n");

            ctx.ShouldIndent = true;
            ctx.Indent++;

            ctx.ShouldLineBreak = true;
            foreach (StatementSyntax statement in block.Statements)
            {
                ctx.IsFirstElement = block.Statements[0] == statement;
                NormalizeStatement(statement, ctx);
            }

            block.SetCurlyOpen(curlyOpen, false);
            block.SetCurlyClose(curlyClose, false);
        }

        private void NormalizeCommaSeparatedList(CommaSeparatedSyntaxList<ExpressionSyntax> list, WhitespaceNormalizeContext ctx)
        {
            foreach (ExpressionSyntax expression in list.Elements)
            {
                ctx.IsFirstElement = list.Elements[0] == expression;
                NormalizeExpression(expression, ctx);
            }
        }

        private void NormalizeExpression(ExpressionSyntax expression, WhitespaceNormalizeContext ctx)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literalExpression:
                    NormalizeLiteralExpression(literalExpression, ctx);
                    break;

                case UnaryExpressionSyntax unaryExpression:
                    NormalizeUnaryExpression(unaryExpression, ctx);
                    break;

                case LogicalExpressionSyntax logicalExpression:
                    NormalizeLogicalExpression(logicalExpression, ctx);
                    break;

                case BinaryExpressionSyntax binaryExpression:
                    NormalizeBinaryExpression(binaryExpression, ctx);
                    break;

                case ArrayInitializerExpressionSyntax arrayInitializer:
                    NormalizeArrayInitializerExpression(arrayInitializer, ctx);
                    break;

                case NameSyntax nameExpression:
                    NormalizeNameSyntax(nameExpression, ctx);
                    break;

                case FunctionInvocationExpressionSyntax functionInvocation:
                    NormalizeFunctionInvocationExpression(functionInvocation, ctx);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}.");
            }
        }

        private void NormalizeLiteralExpression(LiteralExpressionSyntax literal, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken newLiteral = literal.Literal.WithNoTrivia();

            if (!ctx.IsFirstElement)
                newLiteral = newLiteral.WithLeadingTrivia(" ");
            if (ctx.ShouldLineBreak)
                newLiteral = newLiteral.WithTrailingTrivia("\r\n");

            literal.SetLiteral(newLiteral);
        }

        private void NormalizeUnaryExpression(UnaryExpressionSyntax unary, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken operation = unary.Operation.WithNoTrivia().WithTrailingTrivia(" ");

            ctx.IsFirstElement = false;
            ctx.ShouldIndent = false;
            NormalizeExpression(unary.Expression, ctx);

            unary.SetOperation(operation);
        }

        private void NormalizeLogicalExpression(LogicalExpressionSyntax logical, WhitespaceNormalizeContext ctx)
        {
            bool shouldLineBreak = ctx.ShouldLineBreak;

            SyntaxToken operation = logical.Operation.WithLeadingTrivia(" ").WithTrailingTrivia(" ");

            ctx.IsFirstElement = false;
            ctx.ShouldIndent = false;
            ctx.ShouldLineBreak = false;
            NormalizeExpression(logical.Left, ctx);

            ctx.IsFirstElement = true;
            ctx.ShouldLineBreak = shouldLineBreak;
            NormalizeExpression(logical.Right, ctx);

            logical.SetOperation(operation);
        }

        private void NormalizeBinaryExpression(BinaryExpressionSyntax binary, WhitespaceNormalizeContext ctx)
        {
            bool shouldLineBreak = ctx.ShouldLineBreak;

            SyntaxToken operation = binary.Operation.WithLeadingTrivia(" ").WithTrailingTrivia(" ");

            ctx.IsFirstElement = false;
            ctx.ShouldIndent = false;
            ctx.ShouldLineBreak = false;
            NormalizeExpression(binary.Left, ctx);

            ctx.IsFirstElement = true;
            ctx.ShouldLineBreak = shouldLineBreak;
            NormalizeExpression(binary.Right, ctx);

            binary.SetOperation(operation);
        }

        private void NormalizeArrayInitializerExpression(ArrayInitializerExpressionSyntax arrayInitializer, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken bracketOpen = arrayInitializer.BracketOpen.WithNoTrivia();
            SyntaxToken bracketClose = arrayInitializer.BracketClose.WithNoTrivia();

            if (!ctx.IsFirstElement)
                bracketOpen = bracketOpen.WithLeadingTrivia(" ");

            if (arrayInitializer.Values != null)
            {
                ctx.ShouldLineBreak = false;
                NormalizeCommaSeparatedList(arrayInitializer.Values, ctx);
            }

            arrayInitializer.SetBracketOpen(bracketOpen, false);
            arrayInitializer.SetBracketClose(bracketClose, false);
        }

        private void NormalizeNameSyntax(NameSyntax name, WhitespaceNormalizeContext ctx)
        {
            switch (name)
            {
                case SimpleNameSyntax simpleName:
                    NormalizeSimpleName(simpleName, ctx);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown name expression {name.GetType().Name}.");
            }
        }

        private void NormalizeSimpleName(SimpleNameSyntax simpleName, WhitespaceNormalizeContext ctx)
        {
            SyntaxToken newIdentifier = simpleName.Identifier.WithNoTrivia();

            if (ctx is { ShouldIndent: true, Indent: > 0 })
                newIdentifier = newIdentifier.WithLeadingTrivia(new string('\t', ctx.Indent));
            //else if (ctx.IsFirstElement)
            //    newIdentifier = newIdentifier.WithTrailingTrivia(" ");

            simpleName.SetIdentifier(newIdentifier, false);
        }
    }
}
