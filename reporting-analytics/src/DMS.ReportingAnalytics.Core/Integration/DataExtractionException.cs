namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Exception thrown when data extraction from a module fails.
/// </summary>
public class DataExtractionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataExtractionException"/> class.
    /// </summary>
    public DataExtractionException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataExtractionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DataExtractionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataExtractionException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DataExtractionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
