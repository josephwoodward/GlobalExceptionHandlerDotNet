using System;
using System.Runtime.Serialization;

namespace NetCore3Demo
{
    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException()
        {
        }

        protected RecordNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RecordNotFoundException(string message) : base(message)
        {
        }

        public RecordNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}