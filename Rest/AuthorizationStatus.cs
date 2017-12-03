namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// Authorization Status Identifiers.
    /// </summary>
    public static class AuthorizationStatus
    {
        /// <summary>
        /// The status of the authorization is unknown.
        /// </summary>
        public const string Unknown = "unknown";

        /// <summary>
        /// The authorization is pending and has not been acted upon yet.
        /// </summary>
        public const string Pending = "pending";

        /// <summary>
        /// The authorization is currently processing.
        /// </summary>
        public const string Processing = "Processing";

        /// <summary>
        /// The authorization has been determined to be valid.
        /// </summary>
        public const string Valid = "valid";

        /// <summary>
        /// The authorization has been determiend to be invalid.
        /// </summary>
        public const string Invalid = "invalid";

        /// <summary>
        /// The authorization has been revoked by the server.
        /// </summary>
        public const string Revoked = "revoked";

        /// <summary>
        /// The authorization has been cancelled by the user.
        /// </summary>
        public const string Deactivated = "deactivated";
    }
}
