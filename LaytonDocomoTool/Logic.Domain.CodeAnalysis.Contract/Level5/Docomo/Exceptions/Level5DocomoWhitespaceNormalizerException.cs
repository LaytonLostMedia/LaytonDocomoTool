using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.Exceptions
{
    [Serializable]
    public class Level5DocomoWhitespaceNormalizerException : Exception
    {
        public Level5DocomoWhitespaceNormalizerException()
        {
        }

        public Level5DocomoWhitespaceNormalizerException(string message) : base(message)
        {
        }

        public Level5DocomoWhitespaceNormalizerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected Level5DocomoWhitespaceNormalizerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
