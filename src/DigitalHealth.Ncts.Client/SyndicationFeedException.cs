using System;
using System.Runtime.Serialization;

namespace DigitalHealth.Ncts.Client
{
    [Serializable]
    public class SyndicationFeedException : Exception
    {
        public SyndicationFeedException()
        {
        }

        public SyndicationFeedException(string message) : base(message)
        {
        }

        public SyndicationFeedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SyndicationFeedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
