using System.Diagnostics.CodeAnalysis;
using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.MidiManagement.Contract;

namespace Logic.Domain.MidiManagement
{
    public class MidiManagementActivator : IComponentActivator
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

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(MidiReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(MidiParser))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(MidiComposer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(MidiWriter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(MidiManagementConfiguration))]
        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<IMidiReader, MidiReader>(ActivationScope.Unique);
            kernel.Register<IMidiParser, MidiParser>(ActivationScope.Unique);
            kernel.Register<IMidiComposer, MidiComposer>(ActivationScope.Unique);
            kernel.Register<IMidiWriter, MidiWriter>(ActivationScope.Unique);

            kernel.RegisterConfiguration<MidiManagementConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
