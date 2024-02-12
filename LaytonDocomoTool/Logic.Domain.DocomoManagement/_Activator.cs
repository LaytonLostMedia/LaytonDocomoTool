using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.DocomoManagement.Contract;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Domain.DocomoManagement
{
    public class DocomoManagementActivator : IComponentActivator
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

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(JarReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(JarWriter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(DocomoManagementConfiguration))]
        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<IJarReader, JarReader>(ActivationScope.Unique);
            kernel.Register<IJarWriter, JarWriter>(ActivationScope.Unique);

            kernel.RegisterConfiguration<DocomoManagementConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
