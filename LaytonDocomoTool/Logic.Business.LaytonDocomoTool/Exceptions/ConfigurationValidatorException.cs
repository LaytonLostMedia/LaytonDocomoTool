using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.Exceptions
{
    [Serializable]
    public class ConfigurationValidatorException:Exception
    {
        public ConfigurationValidatorException()
        {
        }

        public ConfigurationValidatorException(string message) : base(message)
        {
        }

        public ConfigurationValidatorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ConfigurationValidatorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
