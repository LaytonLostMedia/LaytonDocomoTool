using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoSyntaxFactory : ILevel5DocomoSyntaxFactory
    {
        public SyntaxToken Create(string text, Level5DocomoTokenKind kind, SyntaxTokenTrivia? leadingTrivia = null, SyntaxTokenTrivia? trailingTrivia = null)
        {
            return new SyntaxToken(text, (int)kind, leadingTrivia, trailingTrivia);
        }

        public SyntaxToken Token(Level5DocomoTokenKind kind)
        {
            switch (kind)
            {
                case Level5DocomoTokenKind.ParenOpen: return new("(", (int)kind);
                case Level5DocomoTokenKind.ParenClose: return new(")", (int)kind);
                case Level5DocomoTokenKind.BracketOpen: return new("[", (int)kind);
                case Level5DocomoTokenKind.BracketClose: return new("]", (int)kind);
                case Level5DocomoTokenKind.CurlyOpen: return new("{", (int)kind);
                case Level5DocomoTokenKind.CurlyClose: return new("}", (int)kind);
                case Level5DocomoTokenKind.Comma: return new(",", (int)kind);
                case Level5DocomoTokenKind.Semicolon: return new(";", (int)kind);
                case Level5DocomoTokenKind.TrueKeyword: return new("true", (int)kind);
                case Level5DocomoTokenKind.FalseKeyword: return new("false", (int)kind);
                case Level5DocomoTokenKind.IfKeyword: return new("if", (int)kind);
                case Level5DocomoTokenKind.ElseKeyword: return new("else", (int)kind);
                case Level5DocomoTokenKind.NotKeyword: return new("not", (int)kind);
                default: throw new InvalidOperationException($"Cannot create simple token from kind {kind}. Use other methods instead.");
            }
        }

        public SyntaxToken Identifier(string text)
        {
            return new(text, (int)Level5DocomoTokenKind.Identifier);
        }

        public SyntaxToken StringLiteral(string text)
        {
            return new($"\"{text}\"", (int)Level5DocomoTokenKind.StringLiteral);
        }

        public SyntaxToken NumericLiteral(int value)
        {
            return new($"{value}", (int)Level5DocomoTokenKind.NumericLiteral);
        }
    }
}
