using System;

namespace DMS.InventoryManagement.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when an external service fails or returns unexpected results
    /// </summary>
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message) : base(message)
        {
        }

        public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
