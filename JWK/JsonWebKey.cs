using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.JWK
{
    /// <summary>
    /// Defines the basic structure of a JSON Web Key.
    /// </summary>
    public class JsonWebKey
    {
        /// <summary>
        /// The type of key this JWK references.
        /// </summary>
        [JsonProperty( "kty" )]
        public string KeyType { get; set; }

        /// <summary>
        /// The Modulus of the RSA key.
        /// </summary>
        [JsonProperty( "n" )]
        public string Modulus { get; set; }

        /// <summary>
        /// The Exponent of the RSA key.
        /// </summary>
        [JsonProperty( "e" )]
        public string Exponent { get; set; }

        /// <summary>
        /// Retrieve the JWK Thumbprint of this key.
        /// </summary>
        /// <returns>A byte array of the calculated thumbprint digest.</returns>
        public byte[] Thumbprint()
        {
            if ( KeyType == JWK.KeyType.RSA )
            {
                var hasher = new Org.BouncyCastle.Crypto.Digests.Sha256Digest();

                var JWK = new Dictionary<string, string>
                {
                    { "e", Exponent },
                    { "kty", KeyType },
                    { "n", Modulus }
                };

                var bytes = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( JWK, Formatting.None ) );
                hasher.BlockUpdate( bytes, 0, bytes.Length );

                var digest = new byte[hasher.GetDigestSize()];
                hasher.DoFinal( digest, 0 );

                return digest;
            }

            throw new NotSupportedException( "Unknown JWK key type." );
        }
    }
}
