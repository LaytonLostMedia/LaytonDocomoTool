using System.Text;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoLexer : Lexer<Level5DocomoTokenKind>
    {
        private readonly StringBuilder _sb;

        public Level5DocomoLexer(IBuffer<int> buffer) : base(buffer)
        {
            _sb = new StringBuilder();
        }

        public override LexerToken<Level5DocomoTokenKind> Read()
        {
            if (!TryPeekChar(out char character))
                return new(Level5DocomoTokenKind.EndOfFile, Position, Line, Column);

            switch (character)
            {
                case ',':
                    return new(Level5DocomoTokenKind.Comma, Position, Line, Column, $"{ReadChar()}");
                case ';':
                    return new(Level5DocomoTokenKind.Semicolon, Position, Line, Column, $"{ReadChar()}");

                case '(':
                    return new(Level5DocomoTokenKind.ParenOpen, Position, Line, Column, $"{ReadChar()}");
                case ')':
                    return new(Level5DocomoTokenKind.ParenClose, Position, Line, Column, $"{ReadChar()}");
                case '{':
                    return new(Level5DocomoTokenKind.CurlyOpen, Position, Line, Column, $"{ReadChar()}");
                case '}':
                    return new(Level5DocomoTokenKind.CurlyClose, Position, Line, Column, $"{ReadChar()}");
                case '[':
                    return new(Level5DocomoTokenKind.BracketOpen, Position, Line, Column, $"{ReadChar()}");
                case ']':
                    return new(Level5DocomoTokenKind.BracketClose, Position, Line, Column, $"{ReadChar()}");

                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return ReadTriviaAndComments();

                case '"':
                    return ReadStringLiteral();

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumericLiteral();

                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                case '@':
                    return ReadIdentifierOrKeyword();
            }

            throw CreateException("Invalid character.");
        }

        private LexerToken<Level5DocomoTokenKind> ReadTriviaAndComments()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case '/':
                        if (IsPeekedChar(1, '/'))
                        {
                            _sb.Append(ReadChar());
                            _sb.Append(ReadChar());

                            while (!IsPeekedChar('\n'))
                                _sb.Append(ReadChar());

                            continue;
                        }

                        if (IsPeekedChar(1, '*'))
                        {
                            _sb.Append(ReadChar());
                            _sb.Append(ReadChar());

                            while (!IsPeekedChar('*') || !IsPeekedChar(1, '/'))
                                _sb.Append(ReadChar());

                            _sb.Append(ReadChar());
                            _sb.Append(ReadChar());

                            continue;
                        }

                        break;

                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        _sb.Append(ReadChar());
                        continue;
                }

                break;
            }

            return new(Level5DocomoTokenKind.Trivia, position, line, column, _sb.ToString());
        }

        private LexerToken<Level5DocomoTokenKind> ReadStringLiteral()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            if (!IsPeekedChar('"'))
                throw CreateException("Invalid string literal start.", "\"");

            _sb.Append(ReadChar());

            while (!IsPeekedChar('"'))
            {
                if (IsPeekedChar('\\'))
                    _sb.Append(ReadChar());

                _sb.Append(ReadChar());
            }

            if (IsEndOfInput)
                throw CreateException("Invalid string literal end.", "\"");

            _sb.Append(ReadChar());

            return new(Level5DocomoTokenKind.StringLiteral, position, line, column, _sb.ToString());
        }

        private LexerToken<Level5DocomoTokenKind> ReadNumericLiteral()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            var isHex = false;

            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case '0':
                        if (!IsPeekedChar(1, 'x'))
                            goto case '1';

                        if (_sb.Length != 0)
                            throw CreateException($"Invalid hex identifier in numeric literal {character} in numeric literal.");

                        _sb.Append(ReadChar());
                        _sb.Append(ReadChar());

                        isHex = true;
                        continue;

                    case '-':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        _sb.Append(ReadChar());
                        continue;

                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                        if (!isHex)
                            throw CreateException("Invalid character in numeric literal.");

                        _sb.Append(ReadChar());
                        continue;
                }

                break;
            }

            return new(Level5DocomoTokenKind.NumericLiteral, position, line, column, _sb.ToString());
        }

        private LexerToken<Level5DocomoTokenKind> ReadIdentifierOrKeyword()
        {
            int position = Position;
            int line = Line;
            int column = Column;

            _sb.Clear();

            var firstChar = true;
            while (TryPeekChar(out char character))
            {
                switch (character)
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                        firstChar = false;

                        _sb.Append(ReadChar());
                        continue;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (firstChar)
                            throw CreateException("Invalid identifier starting with numbers.");

                        firstChar = false;

                        _sb.Append(ReadChar());
                        continue;
                }

                if (firstChar)
                    throw CreateException("Invalid identifier.");

                break;
            }

            var finalValue = _sb.ToString();
            switch (finalValue)
            {
                case "true":
                    return new(Level5DocomoTokenKind.TrueKeyword, position, line, column, finalValue);

                case "false":
                    return new(Level5DocomoTokenKind.FalseKeyword, position, line, column, finalValue);

                case "if":
                    return new(Level5DocomoTokenKind.IfKeyword, position, line, column, finalValue);

                case "else":
                    return new(Level5DocomoTokenKind.ElseKeyword, position, line, column, finalValue);

                case "not":
                    return new(Level5DocomoTokenKind.NotKeyword, position, line, column, finalValue);

                default:
                    return new(Level5DocomoTokenKind.Identifier, position, line, column, finalValue);
            }
        }
    }
}
