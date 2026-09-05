using System;

namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// An Acme protocol error as provided by the server.
    /// </summary>
    public class AcmeException : Exception
    {
        /// <summary>
        /// The type of error that occurred. This is not user-friendly.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The error code status, this should be equal to the HTTP response code.
        /// </summary>
        public int Status { get; private set; }

        /// <summary>
        /// Construct a new exception from the data provided by the server.
        /// </summary>
        /// <param name="error">The Error object returned by the server.</param>
        public AcmeException( Rest.Error error )
            : base( error.Detail )
        {
            Type = error.Type;
            Status = error.Status;
        }

        /// <summary>
        /// Construct a new exception from the data provided by the server.
        /// </summary>
        /// <param name="error">The Error object returned by the server.</param>
        /// <param name="innerException">The inner web exception that caused this error.</param>
        public AcmeException( Rest.Error error, Exception innerException )
            : base( error.Detail, innerException )
        {
            Type = error.Type;
            Status = error.Status;
        }
    }
}
