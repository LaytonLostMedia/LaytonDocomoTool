using System.Diagnostics.CodeAnalysis;
using CrossCutting.Core.Bootstrapping;
using CrossCutting.Core.Configuration;
using CrossCutting.Core.Configuration.CommandLine;
using CrossCutting.Core.Configuration.ConfigObjects;
using CrossCutting.Core.Configuration.File;
using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Logging;
using CrossCutting.Core.Contract.Scheduling;
using CrossCutting.Core.Contract.Serialization;
using CrossCutting.Core.DI.AutofacAdapter;
using CrossCutting.Core.EventBrokerage;
using CrossCutting.Core.Logging.NLogAdapter;
using CrossCutting.Core.Scheduling.QuartzAdapter;
using CrossCutting.Core.Serialization.JsonAdapter;
using Logic.Business.LaytonDocomoTool;
using Logic.Domain.CodeAnalysis;
using Logic.Domain.DocomoManagement;
using Logic.Domain.Kuriimu2.KomponentAdapter;
using Logic.Domain.Level5Management.Docomo;

namespace Mappings.LaytonDocomoTool
{
    public class KernelInitializer : IKernelInitializer
    {
        private IKernelContainer _kernelContainer;

        public IKernelContainer CreateKernelContainer()
        {
            if (_kernelContainer == null)
            {
                _kernelContainer = new KernelContainer();
            }
            return _kernelContainer;
        }

        public void Initialize()
        {
            RegisterCoreComponents(_kernelContainer.Kernel);
            ActivateComponents(_kernelContainer.Kernel);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Scheduler))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(JsonSerializer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Logger))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ConfigObjectProvider))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Configurator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CommandLineConfigurationRepository))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(FileConfigurationRepository))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(EventBroker))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Bootstrapper))]
        private void RegisterCoreComponents(ICoCoKernel kernel)
        {
            kernel.Register<IBootstrapper, Bootstrapper>(ActivationScope.Unique);
            kernel.Register<IEventBroker, EventBroker>(ActivationScope.Unique);
            kernel.Register<IConfigurationRepository, FileConfigurationRepository>();
            kernel.Register<IConfigurationRepository, CommandLineConfigurationRepository>();
            kernel.Register<IConfigurator, Configurator>(ActivationScope.Unique);
            kernel.Register<IConfigObjectProvider, ConfigObjectProvider>(ActivationScope.Unique);
            kernel.Register<ILogger, Logger>(ActivationScope.Unique);
            kernel.Register<ISerializer, JsonSerializer>();
            kernel.Register<IScheduler, Scheduler>(ActivationScope.Unique);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(LaytonDocomoExtractorActivator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Kuriimu2KomponentActivator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(DocomoManagementActivator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5ManagementDocomoActivator))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(CodeAnalysisActivator))]
        private void ActivateComponents(ICoCoKernel kernel)
        {
            // TODO: Add own components
            kernel.RegisterComponent<LaytonDocomoExtractorActivator>();
            kernel.RegisterComponent<Kuriimu2KomponentActivator>();
            kernel.RegisterComponent<DocomoManagementActivator>();
            kernel.RegisterComponent<Level5ManagementDocomoActivator>();
            kernel.RegisterComponent<CodeAnalysisActivator>();
        }
    }
}