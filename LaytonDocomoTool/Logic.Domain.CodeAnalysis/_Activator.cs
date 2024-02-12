using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysis.Contract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.CodeAnalysis.Level5.Docomo;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Domain.CodeAnalysis
{
    public class CodeAnalysisActivator : IComponentActivator
    {
        public void Activating()
        {
        }

        public void Activated()
        {
        }

        public void Deactivating()
        {
        }

        public void Deactivated()
        {
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoFactory))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoLexer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(TokenBuffer<LexerToken<Level5DocomoTokenKind>>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoParser))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoComposer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoSyntaxFactory))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoWhitespaceNormalizer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(BufferFactory))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(StringBuffer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CodeAnalysisConfiguration))]
        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<ITokenFactory<LexerToken<Level5DocomoTokenKind>>, Level5DocomoFactory>(ActivationScope.Unique);
            kernel.Register<ILexer<LexerToken<Level5DocomoTokenKind>>, Level5DocomoLexer>();
            kernel.Register<IBuffer<LexerToken<Level5DocomoTokenKind>>, TokenBuffer<LexerToken<Level5DocomoTokenKind>>>();

            kernel.Register<ILevel5DocomoParser, Level5DocomoParser>(ActivationScope.Unique);
            kernel.Register<ILevel5DocomoComposer, Level5DocomoComposer>(ActivationScope.Unique);

            kernel.Register<ILevel5DocomoSyntaxFactory, Level5DocomoSyntaxFactory>();

            kernel.Register<ILevel5DocomoWhitespaceNormalizer, Level5DocomoWhitespaceNormalizer>(ActivationScope.Unique);

            kernel.Register<IBufferFactory, BufferFactory>(ActivationScope.Unique);
            kernel.Register<IBuffer<int>, StringBuffer>();

            kernel.RegisterConfiguration<CodeAnalysisConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
