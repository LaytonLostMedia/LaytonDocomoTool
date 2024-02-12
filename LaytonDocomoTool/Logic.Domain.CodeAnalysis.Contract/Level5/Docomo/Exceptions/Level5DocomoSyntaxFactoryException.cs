using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.Exceptions
{
    [Serializable]
    public class Level5DocomoSyntaxFactoryException : Exception
    {
        public Level5DocomoSyntaxFactoryException()
        {
        }

        public Level5DocomoSyntaxFactoryException(string message) : base(message)
        {
        }

        public Level5DocomoSyntaxFactoryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected Level5DocomoSyntaxFactoryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
