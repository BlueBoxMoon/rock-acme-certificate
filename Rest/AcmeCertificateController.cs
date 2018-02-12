using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Rock;
using Rock.Rest;
using Rock.Rest.Filters;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    public class BBM_AcmeCertificateController : ApiControllerBase
    {
        #region API Methods

        /// <summary>
        /// Renew the certificate.
        /// </summary>
        /// <param name="certificateId">The certificate to be renewed.</param>
        /// <param name="csr">The custom CSR to use.</param>
        /// <returns>The result.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/BBM_AcmeCertificate/Renew/{certificateId}" )]
        public CertificateData RenewCertificate( int certificateId, string csr = null )
        {
            string errorMessage;
            CertificateData certificateData;

            if ( !string.IsNullOrWhiteSpace( csr ) )
            {
                string csrText = csr
                    .Replace( "-----BEGIN CERTIFICATE REQUEST-----", string.Empty )
                    .Replace( "-----END CERTIFICATE REQUEST-----", string.Empty );
                byte[] csrData = Convert.FromBase64String( csrText );
                certificateData = AcmeHelper.RenewCsrRequest( certificateId, csrData, out errorMessage );
            }
            else
            {
                //
                // Attempt to renew the certificate, new bindings will be created as needed.
                //
                certificateData = AcmeHelper.RenewCertificate( certificateId, out errorMessage );
            }

            if ( !string.IsNullOrWhiteSpace( errorMessage ) )
            {
                throw new Exception( errorMessage );
            }

            return certificateData;
        }

        /// <summary>
        /// Install the certificate data and configure IIS. If new bindings are added then
        /// this method will never actually return and the connection will be closed instead.
        /// After this happens a request to check if the certificate is configured should be
        /// made.
        /// </summary>
        /// <param name="certificate">The certificate data to be installed.</param>
        /// <returns>The result.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/BBM_AcmeCertificate/Install" )]
        public void InstallCertificate( [FromBody]CertificateData certificate  )
        {
            AcmeHelper.InstallCertificateData( certificate, true );
        }

        /// <summary>
        /// Verify that a certificate is correctly installed and configured in IIS.
        /// </summary>
        /// <param name="certificateId">The certificate Id number in question.</param>
        /// <param name="certificateHash">The Base64-encoded hash of the certificate to verify.</param>
        /// <returns>true if the certificate is fully installed and working.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/BBM_AcmeCertificate/Installed/{certificateId}" )]
        public bool GetCertificateInstalled( int certificateId, string certificateHash )
        {
            return AcmeHelper.VerifyCertificateBindings( certificateId, certificateHash );
        }

        /// <summary>
        /// Get the certificate hash as a base64 encoded string.
        /// </summary>
        /// <param name="certificateId">The certificate whose hash we want.</param>
        /// <returns>A base64 encoded string that represents the certificate hash.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/BBM_AcmeCertificate/Hash/{certificateId}")]
        public string GetCertificateHash( int certificateId )
        {
            var group = new Rock.Model.GroupService( new Rock.Data.RockContext() ).Get( certificateId );

            group.LoadAttributes();

            return group.GetAttributeValue( "CertificateHash" );
        }

        /// <summary>
        /// Deletes a certificate from the certificate store.
        /// </summary>
        /// <param name="certificateHash">The certificate hash to be deleted.</param>
        [Authenticate, Secured]
        [HttpDelete]
        [System.Web.Http.Route( "api/BBM_AcmeCertificate/Hash/{certificateHash}" )]
        public void DeleteCertificateHash( string certificateHash )
        {
            AcmeHelper.RemoveCertificate( Convert.FromBase64String( certificateHash ) );
        }

        #endregion
    }
}
