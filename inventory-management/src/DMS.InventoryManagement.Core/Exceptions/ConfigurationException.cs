using System;

namespace DMS.InventoryManagement.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an issue with configuration settings
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException() : base("A configuration error occurred.")
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
