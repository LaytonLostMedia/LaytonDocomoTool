using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.Exceptions
{
    [Serializable]
    public class Level5DocomoComposeException : Exception
    {
        public Level5DocomoComposeException()
        {
        }

        public Level5DocomoComposeException(string message) : base(message)
        {
        }

        public Level5DocomoComposeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected Level5DocomoComposeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
