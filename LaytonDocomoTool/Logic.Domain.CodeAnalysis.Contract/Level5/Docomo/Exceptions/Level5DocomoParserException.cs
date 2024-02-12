using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.Exceptions
{
    [Serializable]
    public class Level5DocomoParserException : Exception
    {
        public Level5DocomoParserException()
        {
        }

        public Level5DocomoParserException(string message) : base(message)
        {
        }

        public Level5DocomoParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected Level5DocomoParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
