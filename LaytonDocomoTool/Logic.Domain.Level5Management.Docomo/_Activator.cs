﻿using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Script;

namespace Logic.Domain.Level5Management.Docomo
{
    public class Level5ManagementDocomoActivator : IComponentActivator
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

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(GameReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(TableReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ResourceReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScriptReader))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScriptParser))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScriptComposer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScriptWriter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ResourceWriter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(TableWriter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Level5ManagementDocomoConfiguration))]
        public void Register(ICoCoKernel kernel)
        {
            kernel.Register<IGameReader, GameReader>(ActivationScope.Unique);
            kernel.Register<ITableReader, TableReader>(ActivationScope.Unique);
            kernel.Register<IResourceReader, ResourceReader>(ActivationScope.Unique);

            kernel.Register<IResourceWriter, ResourceWriter>(ActivationScope.Unique);
            kernel.Register<ITableWriter, TableWriter>(ActivationScope.Unique);

            kernel.Register<IScriptReader, ScriptReader>(ActivationScope.Unique);
            kernel.Register<IScriptParser, ScriptParser>(ActivationScope.Unique);
            kernel.Register<IScriptComposer, ScriptComposer>(ActivationScope.Unique);
            kernel.Register<IScriptWriter, ScriptWriter>(ActivationScope.Unique);

            kernel.RegisterConfiguration<Level5ManagementDocomoConfiguration>();
        }

        public void AddMessageSubscriptions(IEventBroker broker)
        {
        }

        public void Configure(IConfigurator configurator)
        {
        }
    }
}
