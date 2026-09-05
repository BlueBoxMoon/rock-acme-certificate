namespace com.blueboxmoon.AcmeCertificate.JWK
{
    /// <summary>
    /// Defines the possible values for the JsonWebKey.KeyType property.
    /// </summary>
    public class KeyType
    {
        /// <summary>
        /// An RSA key.
        /// </summary>
        public const string RSA = "RSA";

        /// <summary>
        /// An ECDSA key type.
        /// </summary>
        public const string EllipticCurve = "EC";
    }
}
