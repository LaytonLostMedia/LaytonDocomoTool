using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.Exceptions
{
    [Serializable]
    public class ExtractJarWorkflowException:Exception
    {
        public ExtractJarWorkflowException()
        {
        }

        public ExtractJarWorkflowException(string message) : base(message)
        {
        }

        public ExtractJarWorkflowException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ExtractJarWorkflowException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
