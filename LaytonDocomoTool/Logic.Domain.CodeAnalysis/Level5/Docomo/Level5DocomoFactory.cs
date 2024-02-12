using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.DependencyInjection;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;

namespace Logic.Domain.CodeAnalysis.Level5.Docomo
{
    internal class Level5DocomoFactory: ITokenFactory<LexerToken<Level5DocomoTokenKind>>
    {
        private readonly ICoCoKernel _kernel;
        private readonly IBufferFactory _bufferFactory;

        public Level5DocomoFactory(ICoCoKernel kernel, IBufferFactory bufferFactory)
        {
            _kernel = kernel;
            _bufferFactory = bufferFactory;
        }

        public ILexer<LexerToken<Level5DocomoTokenKind>> CreateLexer(string text)
        {
            IBuffer<int> buffer = _bufferFactory.CreateStringBuffer(text);
            return _kernel.Get<ILexer<LexerToken<Level5DocomoTokenKind>>>(
                new ConstructorParameter("buffer", buffer));
        }

        public IBuffer<LexerToken<Level5DocomoTokenKind>> CreateTokenBuffer(ILexer<LexerToken<Level5DocomoTokenKind>> lexer)
        {
            return _kernel.Get<IBuffer<LexerToken<Level5DocomoTokenKind>>>(new ConstructorParameter("lexer", lexer));
        }
    }
}
