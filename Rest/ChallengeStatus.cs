namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// Challenge Status Identifiers.
    /// </summary>
    public static class ChallengeStatus
    {
        /// <summary>
        /// The challenge is pending.
        /// </summary>
        public const string Pending = "pending";

        /// <summary>
        /// The challenge has been determined to be valid.
        /// </summary>
        public const string Valid = "valid";

        /// <summary>
        /// The challenge has been determined to be invalid.
        /// </summary>
        public const string Invalid = "invalid";
    }
}
