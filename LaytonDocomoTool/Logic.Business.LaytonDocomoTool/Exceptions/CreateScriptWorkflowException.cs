using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.Exceptions
{
    [Serializable]
    public class CreateScriptWorkflowException:Exception
    {
        public CreateScriptWorkflowException()
        {
        }

        public CreateScriptWorkflowException(string message) : base(message)
        {
        }

        public CreateScriptWorkflowException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CreateScriptWorkflowException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
