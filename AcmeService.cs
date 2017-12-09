using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Rock;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;

namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// Provides an interface to an Acme service.
    /// </summary>
    public class AcmeService
    {
        #region Private Properties

        /// <summary>
        /// The Nonce that we will use for our next request. Do not use directly, use GetNonce().
        /// </summary>
        private string Nonce { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The resource directory provided by the Acme service.
        /// </summary>
        protected Rest.Directory Directory { get; private set; }

        /// <summary>
        /// The URL we used to retrieve the Directory. Useful if we need to
        /// generate a new Nonce for some reason.
        /// </summary>
        protected string DirectoryUrl { get; private set; }

        /// <summary>
        /// The JSON Web Key that identifies us. This is cached since we have to calculate it.
        /// </summary>
        protected JWK.JsonWebKey JWK
        {
            get
            {
                if ( _jwk == null )
                {
                    GenerateAccountKey();
                }

                return _jwk;
            }
            private set
            {
                _jwk = value;
            }
        }
        private JWK.JsonWebKey _jwk;

        /// <summary>
        /// The thumbprint of the JWK. This is also calculated so we cache it.
        /// </summary>
        protected string Thumbprint
        {
            get
            {
                if ( _thumbprint == null )
                {
                    GenerateAccountKey();
                }

                return _thumbprint;
            }
            private set
            {
                _thumbprint = value;
            }
        }
        private string _thumbprint;

        /// <summary>
        /// The RSA key pair that lets us identify ourselves to the server.
        /// </summary>
        public byte[] RSA
        {
            get
            {
                if ( _rsa == null )
                {
                    GenerateAccountKey();
                }

                return PrivateKeyInfoFactory.CreatePrivateKeyInfo( _rsa.Private ).ParsePrivateKey().GetEncoded();
            }
            protected set
            {
                var privKeyObj = ( Asn1Sequence ) Asn1Object.FromByteArray( value );
                var rsa = RsaPrivateKeyStructure.GetInstance( privKeyObj );

                AsymmetricKeyParameter pubKey = new RsaKeyParameters( false, rsa.Modulus, rsa.PublicExponent );
                AsymmetricKeyParameter privKey = new RsaPrivateCrtKeyParameters( rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient );

                _rsa = new AsymmetricCipherKeyPair( pubKey, privKey );
                
                JWK = new JWK.JsonWebKey
                {
                    KeyType = AcmeCertificate.JWK.KeyType.RSA,
                    Exponent = UrlBase64Encode( rsa.PublicExponent.ToByteArrayUnsigned() ),
                    Modulus = UrlBase64Encode( rsa.Modulus.ToByteArrayUnsigned() )
                };

                Thumbprint = UrlBase64Encode( JWK.Thumbprint() );
            }
        }
        private AsymmetricCipherKeyPair _rsa;

        #endregion

        #region Public Properties

        /// <summary>
        /// The TOS that the user must accept before creating an account. This should be
        /// passed to the Register method after the user has accepted them.
        /// </summary>
        public string TermsOfServiceUrl
        {
            get
            {
                return Directory?.Meta?.TermsOfService;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new LetsEncrypt instance for communicating with the LetsEncrypt API.
        /// </summary>
        /// <param name="staging">If true the staging URL will be used instead of production.</param>
        public AcmeService( bool staging = true )
        {
            DirectoryUrl = staging ? "https://acme-staging.api.letsencrypt.org/directory" : "https://acme-v01.api.letsencrypt.org/directory";

            var webRequest = ( HttpWebRequest ) WebRequest.Create( DirectoryUrl );

            Directory = ReadWebResponse<Rest.Directory>( webRequest, out WebHeaderCollection headers );
            if ( Directory == null || Directory.Meta == null )
            {
                throw new NotImplementedException( "TODO: Implement exception" );
            }

            Nonce = headers["Replay-Nonce"];
        }

        /// <summary>
        /// Initialize a new LetsEncrypt instance for communicating with the LetsEncrypt API.
        /// </summary>
        /// <param name="staging">If true the staging URL will be used instead of production.</param>
        public AcmeService( byte[] accountKey, bool staging = true )
            : this( staging )
        {
            RSA = accountKey;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generates a new account key..
        /// </summary>
        protected void GenerateAccountKey()
        {
            var generator = new RsaKeyPairGenerator();

            generator.Init( new KeyGenerationParameters( new SecureRandom(), 2048 ) );

            RSA = PrivateKeyInfoFactory.CreatePrivateKeyInfo( generator.GenerateKeyPair().Private ).ParsePrivateKey().GetEncoded();
        }

        /// <summary>
        /// Get a Replay-Nonce either from the last one we received (but haven't used) or generate a new one.
        /// </summary>
        /// <returns>A server supplied nonce.</returns>
        protected string GetNonce()
        {
            string nonce = Nonce;

            Nonce = null;

            if ( nonce == null )
            {
                var request = WebRequest.Create( DirectoryUrl );
                request.Timeout = 5000;
                request.Method = "HEAD";

                using ( var res = request.GetResponse() )
                {
                    return res.Headers["Replay-Nonce"];
                }
            }

            return nonce;
        }

        /// <summary>
        /// Construct a signed message for the given parameters.
        /// </summary>
        /// <param name="url">The url to send with the message.</param>
        /// <param name="nonce">The nonce used to prevent replay attacks.</param>
        /// <param name="payload">The payload data that is to be sent.</param>
        /// <returns>An instance of the <see cref="Rest.Message"/> that contains all the message data to be transmitted.</returns>
        protected Rest.Message ConstructMessage( string url, string nonce, object payload )
        {
            //
            // Construct the header with the one-time nonce.
            // Note: this is referred to as "protected".
            //
            var header = new
            {
                alg = "RS256",
                jwk = JWK,
                url = url,
                nonce = nonce
            };

            //
            // Base64 encode the payload and header.
            //
            var payload64 = UrlBase64Encode( JsonConvert.SerializeObject( payload, Formatting.None ) );
            var header64 = UrlBase64Encode( JsonConvert.SerializeObject( header, Formatting.None ) );

            //
            // Sign the data and get the Base64 encoded signature.
            //
            var signature64 = UrlBase64Encode( SignatureForText( header64 + "." + payload64, _rsa.Private ) );

            //
            // Construct the final message.
            //
            return new Rest.Message
            {
                Protected = header64,
                Payload = payload64,
                Signature = signature64
            };
        }

        #endregion

        #region Static Protected Methods

        /// <summary>
        /// Encode a string of text in a Url safe Base-64 encoding.
        /// </summary>
        /// <param name="plainText">The text to be encoded.</param>
        /// <returns>A base-64 encoded, Url safe, string.</returns>
        static protected string UrlBase64Encode( string plainText )
        {
            return UrlBase64Encode( Encoding.UTF8.GetBytes( plainText ) );
        }

        /// <summary>
        /// Encode an array of bytes in a Url safe Base-64 encoding.
        /// </summary>
        /// <param name="bytes">The data to be encoded.</param>
        /// <returns>A base-64 encoded, Url safe, string.</returns>
        static protected string UrlBase64Encode( byte[] bytes )
        {
            return Convert.ToBase64String( bytes ).Replace( "+", "-" ).Replace( "/", "_" ).Replace( "=", "" );
        }

        /// <summary>
        /// Calculate a SHA-256 digest (hash).
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>A byte array that contains the calculated hash value.</returns>
        static protected byte[] Sha256Digest( byte[] data )
        {
            var hasher = new Org.BouncyCastle.Crypto.Digests.Sha256Digest();

            hasher.BlockUpdate( data, 0, data.Length );

            var digest = new byte[hasher.GetDigestSize()];
            hasher.DoFinal( digest, 0 );

            return digest;
        }

        /// <summary>
        /// Safely reads the JSON formatted response from the web request.
        /// </summary>
        /// <typeparam name="T">The data type to be decoded from the response.</typeparam>
        /// <param name="request">The request object that is ready to have it's respons read.</param>
        /// <returns>The object that was read from the response stream.</returns>
        static protected T ReadWebResponse<T>( WebRequest request, out WebHeaderCollection headers )
        {
            using ( var response = ( HttpWebResponse ) request.GetResponse() )
            {
                headers = response.Headers;

                if ( typeof( T ) == typeof( byte[] ) )
                {
                    return ( T ) ( object ) response.GetResponseStream().ReadBytesToEnd();
                }

                using ( var reader = new StreamReader( response.GetResponseStream() ) )
                {
                    return JsonConvert.DeserializeObject<T>( reader.ReadToEnd() );
                }
            }
        }

        /// <summary>
        /// Generate a private key signature for a chunk of data.
        /// </summary>
        /// <param name="data">The data to be signed.</param>
        /// <returns>An array of bytes that will validate the data has not been modified.</returns>
        static protected byte[] SignatureForData( byte[] data, ICipherParameters privateKeyParameters )
        {
            var signer = SignerUtilities.GetSigner( "SHA-256withRSA" );

            signer.Init( true, privateKeyParameters );
            signer.BlockUpdate( data, 0, data.Length );

            return signer.GenerateSignature();
        }

        /// <summary>
        /// Generate a private key signature for a text string.
        /// </summary>
        /// <param name="data">The text data to be signed.</param>
        /// <returns>An array of bytes that will validate the data has not been modified.</returns>
        static protected byte[] SignatureForText( string data, ICipherParameters privateKeyParameters )
        {
            return SignatureForData( Encoding.UTF8.GetBytes( data ), privateKeyParameters );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs a simple GET of the url and decodes the data into the desired type.
        /// </summary>
        /// <typeparam name="T">The type of data expected to be returned.</typeparam>
        /// <param name="url">The url to request.</param>
        /// <exception cref="AcmeException">Thrown when an Acme error response is received from the server.</exception>
        /// <returns>An instance of T.</returns>
        public T GetRequest<T>( string url )
        {
            var webRequest = ( HttpWebRequest ) WebRequest.Create( url );
            webRequest.Timeout = 5000;

            try
            {
                return ReadWebResponse<T>( webRequest, out WebHeaderCollection headers );
            }
            catch ( WebException e )
            {
                //
                // If we got a response from the server, decode the JSON response data and return it.
                //
                if ( e.Response != null )
                {
                    using ( var reader = new StreamReader( e.Response.GetResponseStream() ) )
                    {
                        throw new AcmeException( JsonConvert.DeserializeObject<Rest.Error>( reader.ReadToEnd() ), e );
                    }
                }

                throw e;
            }
        }

        /// <summary>
        /// Send a message to the Acme server while expecting a specific data type to be returned.
        /// </summary>
        /// <typeparam name="T">The response data type that is expected.</typeparam>
        /// <param name="url">The url to post the payload data to.</param>
        /// <param name="payload">The inner payload data to be sent.</param>
        /// <param name="nonce">The nonce to use to authenticate the request.</param>
        /// <exception cref="AcmeException">Thrown when an Acme error response is received from the server.</exception>
        /// <returns>An instance of T</returns>
        public T SendMessage<T>( string url, object payload, string nonce )
        {
            return SendMessage<T>( url, payload, nonce, out WebHeaderCollection headers );
        }

        /// <summary>
        /// Send a message to the Acme server while expecting a specific data type to be returned.
        /// </summary>
        /// <typeparam name="T">The response data type that is expected.</typeparam>
        /// <param name="url">The url to post the payload data to.</param>
        /// <param name="payload">The inner payload data to be sent.</param>
        /// <param name="nonce">The nonce to use to authenticate the request.</param>
        /// <param name="headers">On return this will be populated with the response headers.</param>
        /// <exception cref="AcmeException">Thrown when an Acme error response is received from the server.</exception>
        /// <returns>An instance of T</returns>
        public T SendMessage<T>( string url, object payload, string nonce, out WebHeaderCollection headers )
        {
            var message = ConstructMessage( url, nonce, payload );

            //
            // Setup the web request.
            //
            var webRequest = ( HttpWebRequest ) WebRequest.Create( url );
            webRequest.Timeout = 5000;
            webRequest.Method = "POST";

            //
            // Write the POST data.
            //
            var data = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( message, Formatting.None ) );
            using ( var stream = webRequest.GetRequestStream() )
            {
                stream.Write( data, 0, data.Length );
                stream.Close();
            }

            try
            {
                var response = ReadWebResponse<T>( webRequest, out headers );

                Nonce = headers["Replay-Nonce"];

                return response;
            }
            catch ( WebException e )
            {
                //
                // If we got a response from the server, decode the JSON response data and return it.
                //
                if ( e.Response != null )
                {
                    Nonce = e.Response.Headers["Replay-Nonce"];

                    using ( var reader = new StreamReader( e.Response.GetResponseStream() ) )
                    {
                        throw new AcmeException( JsonConvert.DeserializeObject<Rest.Error>( reader.ReadToEnd() ), e );
                    }
                }

                throw e;
            }
        }

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="email">The e-mail address to be associated with this account.</param>
        /// <param name="terms">The url of the Terms Of Service the user has agreed to.</param>
        /// <returns>A <see cref="Rest.Account"/> instance that identifies the newly created account.</returns>
        public Rest.Account Register( string email, string terms )
        {
            var payload = new
            {
                resource = "new-reg",
                contact = new string[] { string.Format( "mailto:{0}", email ) },
                agreement = terms
            };

            return SendMessage<Rest.Account>( Directory.NewReg, payload, GetNonce() );
        }

        /// <summary>
        /// Requests authorization of a new domain name for use with our account.
        /// </summary>
        /// <param name="domain">The domain name to be authorized.</param>
        /// <returns>A <see cref="Rest.Authorization"/> instance that contains information on the authorization request.</returns>
        public Rest.Authorization AuthorizeDomain( string domain )
        {
            var payload = new
            {
                resource = "new-authz",
                identifier = new
                {
                    type = "dns",
                    value = domain
                }
            };

            return SendMessage<Rest.Authorization>( Directory.NewAuthz, payload, GetNonce() );
        }

        /// <summary>
        /// Send the challenge command to the server. This begins the process of checking
        /// the domain verification by the challenge method chosen.
        /// </summary>
        /// <param name="challenge">The challenge to use when authorizing the domain.</param>
        /// <returns>A new <see cref="Rest.Challenge"/> object that contains the status of this challenge.</returns>
        public Rest.Challenge Challenge( Rest.Challenge challenge )
        {
            var payload = new
            {
                resource = "challenge",
                type = challenge.Type,
                keyAuthorization = challenge.Token + "." + Thumbprint
            };

            return SendMessage<Rest.Challenge>( challenge.Uri, payload, GetNonce() );
        }

        /// <summary>
        /// Create a CSR and submit it to the Acme server for signing. Returns the certificate chain.
        /// </summary>
        /// <param name="domains">The list of domains that this certificate will be for. The first domain listed will be the CommonName.</param>
        /// <param name="keyPair">The RSA key pair for signing the certificate request, this is the key that will be used in conjunction with the certificate.</param>
        /// <returns>A tuple whose first value is the private key data and whose second value is a list of certificates. Everything is encoded in DER format, the first certificate is the signed certificate.</returns>
        public List<byte[]> GetCertificate( ICollection<string> domains, Pkcs10CertificationRequest csr )
        {
            var payload = new
            {
                resource = "new-cert",
                csr = UrlBase64Encode( csr.GetDerEncoded() )
            };

            var certificates = new List<X509Certificate>();
            var certParser = new X509CertificateParser();
            byte[] certData;

            //
            // Send the request and fetch the certificate data.
            //
            certData = SendMessage<byte[]>( Directory.NewCert, payload, GetNonce(), out WebHeaderCollection headers );
            certificates.Add( certParser.ReadCertificate( certData ) );

            //
            // Fetch all the certificates in the chain.
            //
            foreach ( var link in headers.GetValues( "Link" ) )
            {
                var match = System.Text.RegularExpressions.Regex.Match( link, "\\<(.*)\\>;rel=\"(.*)\"" );
                if ( match.Success && match.Groups[2].Value == "up" )
                {
                    certData = GetRequest<byte[]>( match.Groups[1].Value );
                    certificates.Add( certParser.ReadCertificate( certData ) );
                }
            }

            return certificates.Select( c => c.GetEncoded() ).ToList();
        }

        /// <summary>
        /// Validate all the domains by using the callback to handle the challenge responses.
        /// </summary>
        /// <param name="domains">A collection of domain names to validate.</param>
        /// <param name="prepareChallenge">The callback that will be called for each domain challenge to be processed.</param>
        public void ValidateDomains( ICollection<string> domains, Action<string, Rest.Challenge, string> prepareChallenge )
        {
            foreach ( var domain in domains )
            {
                var authorization = AuthorizeDomain( domain );
                var challenge = authorization.Challenges.FirstOrDefault( c => c.Type == "http-01" );

                if ( challenge == null )
                {
                    throw new KeyNotFoundException( "Could not find challenge of type 'http-01'." );
                }

                prepareChallenge( domain, challenge, challenge.Token + "." + Thumbprint );

                var challengeStatus = Challenge( challenge );

                var endDateTime = DateTime.Now.AddSeconds( 15 );
                int waitTime = 1000;

                do
                {
                    System.Threading.Thread.Sleep( waitTime );
                    waitTime += 1000;
                    challengeStatus = GetRequest<Rest.Challenge>( challengeStatus.Uri );

                    prepareChallenge( domain, challenge, null );
                } while ( DateTime.Now < endDateTime && challengeStatus.Status == "pending" );

                if ( challengeStatus.Status == "valid" )
                {
                    continue;
                }
                else if ( challengeStatus.Status == "pending" )
                {
                    throw new TimeoutException( string.Format( "Timeout while trying to validate domain {0}", domain ) );
                }
                else
                {
                    if ( challengeStatus.Error != null )
                    {
                        throw new Exception( challengeStatus.Error.Detail );
                    }
                    else
                    {
                        throw new Exception( string.Format( "Unknown error trying to validate domain {0}", domain ) );
                    }
                }
            }
        }

        /// <summary>
        /// Generate a new private key pair.
        /// </summary>
        /// <returns>The private key pair.</returns>
        static public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var generator = new RsaKeyPairGenerator();
            generator.Init( new KeyGenerationParameters( new SecureRandom(), 2048 ) );
            return generator.GenerateKeyPair();
        }

        /// <summary>
        /// Generate a private key and CSR for the specified domain list.
        /// </summary>
        /// <param name="keyPair">The private key pair to sign the CSR with.</param>
        /// <param name="domains">The domains to be encoded in the CSR.</param>
        /// <returns>A CSR object that is signed by the private key.</returns>
        static public Pkcs10CertificationRequest GenerateCSR( AsymmetricCipherKeyPair keyPair, ICollection<string> domains )
        {
            var sig = new Asn1SignatureFactory( "SHA256WITHRSA", keyPair.Private );
            var commonName = new X509Name( new DerObjectIdentifier[] { X509Name.CN }, new string[] { domains.First() } );

            //
            // Generate the list of subject alternative names.
            //
            List<GeneralName> names = new List<GeneralName>();
            foreach ( var domain in domains )
            {
                names.Add( new GeneralName( GeneralName.DnsName, domain ) );
            }
            var sanOctect = new DerOctetString( new GeneralNames( names.ToArray() ) );
            var sanSequence = new DerSequence( X509Extensions.SubjectAlternativeName, sanOctect );
            var extensionSet = new DerSet( new DerSequence( sanSequence ) );
            var attributes = new DerSet( new DerSequence( PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, extensionSet ) );

            //
            // Generate the CSR from all the data.
            //
            return new Pkcs10CertificationRequest( sig, commonName, keyPair.Public, attributes, keyPair.Private );
        }

        #endregion
    }
}
