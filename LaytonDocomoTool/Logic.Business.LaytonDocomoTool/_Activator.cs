using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.LaytonDocomoTool.Contract;
using System.Diagnostics.CodeAnalysis;
using Logic.Business.LaytonDocomoTool.InternalContract;

namespace Logic.Business.LaytonDocomoTool
{
    public class LaytonDocomoExtractorActivator : IComponentActivator
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

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(LaytonDocomoToolManagement))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ConfigurationValidator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoEventDataConverter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5DocomoCodeUnitConverter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ExtractJarWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ExtractTableWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ExtractScriptWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ExtractResourceWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(InjectJarWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CreateScriptWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CreateTableWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CreateResourceWorkflow))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(EncodingProvider))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScriptParameterMapper))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LaytonDocomoExtractorConfiguration))]
        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<ILaytonDocomoToolManagement, LaytonDocomoToolManagement>(ActivationScope.Unique);
            kernel.Register<IConfigurationValidator, ConfigurationValidator>(ActivationScope.Unique);

            kernel.Register<ILevel5DocomoEventDataConverter, Level5DocomoEventDataConverter>(ActivationScope.Unique);
            kernel.Register<ILevel5DocomoCodeUnitConverter, Level5DocomoCodeUnitConverter>(ActivationScope.Unique);

            kernel.Register<IExtractJarWorkflow, ExtractJarWorkflow>(ActivationScope.Unique);
            kernel.Register<IExtractTableWorkflow, ExtractTableWorkflow>(ActivationScope.Unique);
            kernel.Register<IExtractScriptWorkflow, ExtractScriptWorkflow>(ActivationScope.Unique);
            kernel.Register<IExtractResourceWorkflow, ExtractResourceWorkflow>(ActivationScope.Unique);

            kernel.Register<ICreateScriptWorkflow, CreateScriptWorkflow>(ActivationScope.Unique);
            kernel.Register<ICreateTableWorkflow, CreateTableWorkflow>(ActivationScope.Unique);
            kernel.Register<IInjectJarWorkflow, InjectJarWorkflow>(ActivationScope.Unique);
            kernel.Register<ICreateResourceWorkflow, CreateResourceWorkflow>(ActivationScope.Unique);

            kernel.Register<IEncodingProvider, EncodingProvider>(ActivationScope.Unique);
            kernel.Register<IScriptParameterMapper, ScriptParameterMapper>(ActivationScope.Unique);

            kernel.RegisterConfiguration<LaytonDocomoExtractorConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
