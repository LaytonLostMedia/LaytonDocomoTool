using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Business.LaytonDocomoTool.Contract.Exceptions
{
    [Serializable]
    public class LaytonDocomoExtractorManagementException : Exception
    {
        public LaytonDocomoExtractorManagementException()
        {
        }

        public LaytonDocomoExtractorManagementException(string message) : base(message)
        {
        }

        public LaytonDocomoExtractorManagementException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LaytonDocomoExtractorManagementException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
