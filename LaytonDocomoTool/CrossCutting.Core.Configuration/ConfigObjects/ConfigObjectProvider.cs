using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.Configuration.DataClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrossCutting.Core.Configuration.ConfigObjects
{
    public class ConfigObjectProvider : IConfigObjectProvider
    {
        private readonly IConfigurator _configurator;
        private readonly IDictionary<Type, object> _configObjects;

        public ConfigObjectProvider(IConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            _configObjects = new ConcurrentDictionary<Type, object>();
        }

        public TConfig Get<TConfig>()
        {
            Type configType = typeof(TConfig);

            ValidateType(configType);

            object configObj = GetConfigObject(configType);
            return (TConfig)configObj;
        }

        public object Get(Type configType)
        {
            if (configType == null) throw new ArgumentNullException(nameof(configType));

            ValidateType(configType);

            object configObj = GetConfigObject(configType);
            return configObj;
        }

        private void ValidateType(Type type)
        {
            PropertyInfo[] properties = type.GetProperties();

            bool allAreSettable = properties.All(p => p.SetMethod != null);
            bool allHaveAttributes = properties.All(p => p.GetCustomAttributes<ConfigMapAttribute>().Any());

            if (!allHaveAttributes || !allAreSettable)
            {
                throw new InvalidOperationException("Requested type can only consist of settable properties with ConfigMapAttribute.");
            }
        }

        private object GetConfigObject(Type configType)
        {
            if (!_configObjects.TryGetValue(configType, out object? obj))
                _configObjects[configType] = obj = CreateConfigObject(configType);

            return obj;
        }

        private object CreateConfigObject(Type configType)
        {
            object configInstance = Activator.CreateInstance(configType);

            foreach (PropertyInfo property in configType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<ConfigMapAttribute>();
                if (attribute == null)
                    continue;

                foreach (string key in attribute.Keys)
                {
                    if (!_configurator.Contains(attribute.Category, key))
                        continue;

                    var value = _configurator.Get<object>(attribute.Category, key);

                    try
                    {
                        property.SetValue(configInstance, value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return configInstance;
        }
    }
}