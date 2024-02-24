using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoParser : Parser<Level5DocomoTokenKind>, ILevel5DocomoParser
    {
        protected override Level5DocomoTokenKind TriviaKind => Level5DocomoTokenKind.Trivia;

        public Level5DocomoParser(ITokenFactory<LexerToken<Level5DocomoTokenKind>> scriptFactory, ILevel5DocomoSyntaxFactory syntaxFactory) : base(scriptFactory, syntaxFactory)
        {
        }

        public CodeUnitSyntax ParseCodeUnit(string text)
        {
            IBuffer<LexerToken<Level5DocomoTokenKind>> buffer = CreateTokenBuffer(text);

            CodeUnitSyntax codeUnit = ParseCodeUnit(buffer);
            codeUnit.Update();

            return codeUnit;
        }

        private CodeUnitSyntax ParseCodeUnit(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            var methodDeclarations = ParseStatements(buffer);

            return new CodeUnitSyntax(methodDeclarations);
        }

        private IReadOnlyList<StatementSyntax> ParseStatements(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            var result = new List<StatementSyntax>();

            while (!HasTokenKind(buffer, Level5DocomoTokenKind.EndOfFile))
                result.Add(ParseStatement(buffer));

            return result;
        }

        private StatementSyntax ParseStatement(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            if (HasTokenKind(buffer, Level5DocomoTokenKind.Identifier))
                return ParseFunctionInvocationStatement(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.IfKeyword))
                return ParseIfElseStatement(buffer);

            throw CreateException(buffer, "Invalid statement.", Level5DocomoTokenKind.Identifier, Level5DocomoTokenKind.IfKeyword);
        }

        private FunctionInvocationStatementSyntax ParseFunctionInvocationStatement(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            FunctionInvocationExpressionSyntax functionInvocation = ParseFunctionInvocationExpression(buffer);
            SyntaxToken semicolon = ParseSemicolonToken(buffer);

            return new FunctionInvocationStatementSyntax(functionInvocation, semicolon);
        }

        private FunctionInvocationExpressionSyntax ParseFunctionInvocationExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            NameSyntax name = ParseName(buffer);
            FunctionParametersSyntax parameters = ParseFunctionParameters(buffer);

            return new FunctionInvocationExpressionSyntax(name, parameters);
        }

        private FunctionParametersSyntax ParseFunctionParameters(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken parenOpen = ParseParenOpenToken(buffer);
            CommaSeparatedSyntaxList<ExpressionSyntax> parameters = ParseCommaSeparatedExpressions(buffer);
            SyntaxToken parenClose = ParseParenCloseToken(buffer);

            return new FunctionParametersSyntax(parenOpen, parameters, parenClose);
        }

        private IfElseStatementSyntax ParseIfElseStatement(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            IfExpressionSyntax ifExpression = ParseIfExpression(buffer);
            BlockExpressionSyntax blockExpression = ParseBlockExpression(buffer);

            var elseStatements = new List<ElseStatementSyntax>();
            while (HasTokenKind(buffer, Level5DocomoTokenKind.ElseKeyword))
            {
                if (HasTokenKind(buffer, 1, Level5DocomoTokenKind.IfKeyword))
                {
                    elseStatements.Add(ParseElseIfStatement(buffer));
                    continue;
                }

                elseStatements.Add(ParseElseStatement(buffer));
            }

            return new IfElseStatementSyntax(ifExpression, blockExpression, elseStatements);
        }

        private ElseStatementSyntax ParseElseStatement(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken elseKeyword = ParseElseKeywordToken(buffer);
            BlockExpressionSyntax blockExpression = ParseBlockExpression(buffer);

            return new ElseStatementSyntax(elseKeyword, blockExpression);
        }

        private ElseIfStatementSyntax ParseElseIfStatement(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken elseKeyword = ParseElseKeywordToken(buffer);
            IfExpressionSyntax ifExpression = ParseIfExpression(buffer);
            BlockExpressionSyntax blockExpression = ParseBlockExpression(buffer);

            return new ElseIfStatementSyntax(elseKeyword, ifExpression, blockExpression);
        }

        private IfExpressionSyntax ParseIfExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken ifKeyword = ParseIfKeywordToken(buffer);
            ExpressionSyntax expression = ParseCompoundExpression(buffer);

            return new IfExpressionSyntax(ifKeyword, expression);
        }

        private BlockExpressionSyntax ParseBlockExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken curlyOpen = ParseCurlyOpenToken(buffer);

            var statements = new List<StatementSyntax>();
            while (!HasTokenKind(buffer, Level5DocomoTokenKind.CurlyClose))
                statements.Add(ParseStatement(buffer));

            SyntaxToken curlyClose = ParseCurlyCloseToken(buffer);

            return new BlockExpressionSyntax(curlyOpen, statements, curlyClose);
        }

        private CommaSeparatedSyntaxList<ExpressionSyntax> ParseCommaSeparatedExpressions(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            if (HasTokenKind(buffer, Level5DocomoTokenKind.ParenClose))
                return new CommaSeparatedSyntaxList<ExpressionSyntax>(null);

            var result = new List<ExpressionSyntax>();

            ExpressionSyntax expression = ParseExpression(buffer);
            result.Add(expression);

            while (HasTokenKind(buffer, Level5DocomoTokenKind.Comma))
            {
                SkipTokenKind(buffer, Level5DocomoTokenKind.Comma);

                result.Add(ParseExpression(buffer));
            }

            return new CommaSeparatedSyntaxList<ExpressionSyntax>(result.ToArray());
        }

        private ExpressionSyntax ParseCompoundExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            ExpressionSyntax leftExpression = ParseExpression(buffer);

            if (!HasTokenKind(buffer, Level5DocomoTokenKind.AndKeyword))
                return leftExpression;

            SyntaxToken andToken = ParseAndKeywordToken(buffer);
            ExpressionSyntax rightExpression = ParseCompoundExpression(buffer);

            return new BinaryExpressionSyntax(leftExpression, andToken, rightExpression);
        }

        private ExpressionSyntax ParseExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            if (HasTokenKind(buffer, Level5DocomoTokenKind.StringLiteral))
                return ParseStringLiteralExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.NumericLiteral))
                return ParseNumericLiteralExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.NotKeyword))
                return ParseUnaryNotExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.BracketOpen))
                return ParseArrayInitializerExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.TrueKeyword))
                return ParseTrueLiteralExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.FalseKeyword))
                return ParseFalseLiteralExpression(buffer);

            if (HasTokenKind(buffer, Level5DocomoTokenKind.Identifier))
            {
                if (HasTokenKind(buffer, 1, Level5DocomoTokenKind.ParenOpen))
                    return ParseFunctionInvocationExpression(buffer);

                return ParseName(buffer);
            }

            throw CreateException(buffer, "Invalid expression.", Level5DocomoTokenKind.StringLiteral, Level5DocomoTokenKind.NumericLiteral,
                Level5DocomoTokenKind.NotKeyword, Level5DocomoTokenKind.BracketOpen, Level5DocomoTokenKind.Identifier);
        }

        private LiteralExpressionSyntax ParseStringLiteralExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken literal = ParseStringLiteralToken(buffer);

            return new LiteralExpressionSyntax(literal);
        }

        private LiteralExpressionSyntax ParseNumericLiteralExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken literal = ParseNumericLiteralToken(buffer);

            return new LiteralExpressionSyntax(literal);
        }

        private LiteralExpressionSyntax ParseTrueLiteralExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken literal = ParseTrueKeywordToken(buffer);

            return new LiteralExpressionSyntax(literal);
        }

        private LiteralExpressionSyntax ParseFalseLiteralExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken literal = ParseFalseKeywordToken(buffer);

            return new LiteralExpressionSyntax(literal);
        }

        private UnaryExpressionSyntax ParseUnaryNotExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken notKeyword = ParseNotKeywordToken(buffer);
            ExpressionSyntax expression = ParseExpression(buffer);

            return new UnaryExpressionSyntax(notKeyword, expression);
        }

        private ArrayInitializerExpressionSyntax ParseArrayInitializerExpression(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken bracketOpen = ParseBracketOpenToken(buffer);
            CommaSeparatedSyntaxList<ExpressionSyntax> parameters = ParseCommaSeparatedExpressions(buffer);
            SyntaxToken bracketClose = ParseBracketCloseToken(buffer);

            return new ArrayInitializerExpressionSyntax(bracketOpen, parameters, bracketClose);
        }

        private NameSyntax ParseName(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            SyntaxToken identifier = ParseIdentifierToken(buffer);

            return new SimpleNameSyntax(identifier);
        }

        private SyntaxToken ParseParenOpenToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.ParenOpen);
        }

        private SyntaxToken ParseParenCloseToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.ParenClose);
        }

        private SyntaxToken ParseBracketOpenToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.BracketOpen);
        }

        private SyntaxToken ParseBracketCloseToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.BracketClose);
        }

        private SyntaxToken ParseCurlyOpenToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.CurlyOpen);
        }

        private SyntaxToken ParseCurlyCloseToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.CurlyClose);
        }

        private SyntaxToken ParseSemicolonToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.Semicolon);
        }

        private SyntaxToken ParseIdentifierToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.Identifier);
        }

        private SyntaxToken ParseStringLiteralToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.StringLiteral);
        }

        private SyntaxToken ParseNumericLiteralToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.NumericLiteral);
        }

        private SyntaxToken ParseIfKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.IfKeyword);
        }

        private SyntaxToken ParseElseKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.ElseKeyword);
        }

        private SyntaxToken ParseNotKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.NotKeyword);
        }

        private SyntaxToken ParseAndKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.AndKeyword);
        }

        private SyntaxToken ParseTrueKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.TrueKeyword);
        }

        private SyntaxToken ParseFalseKeywordToken(IBuffer<LexerToken<Level5DocomoTokenKind>> buffer)
        {
            return CreateToken(buffer, Level5DocomoTokenKind.FalseKeyword);
        }
    }
}
