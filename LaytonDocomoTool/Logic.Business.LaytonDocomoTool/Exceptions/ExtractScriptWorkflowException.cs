using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.Exceptions
{
    [Serializable]
    public class ExtractScriptWorkflowException:Exception
    {
        public ExtractScriptWorkflowException()
        {
        }

        public ExtractScriptWorkflowException(string message) : base(message)
        {
        }

        public ExtractScriptWorkflowException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ExtractScriptWorkflowException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
