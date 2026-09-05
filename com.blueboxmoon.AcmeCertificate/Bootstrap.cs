using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace com.blueboxmoon.AcmeCertificate
{
    public class Bootstrap : IRockStartup
    {
        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder => 9999999;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            try
            {
                string bootstrapFilename = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/AcmeCertificate.bootstrap.json" );

                if ( !File.Exists( bootstrapFilename ) )
                {
                    return;
                }

                new System.Threading.Thread( Process ).Start( bootstrapFilename );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
            }
        }

        /// <summary>
        /// Processes certificate bootstrapping in a background thread. This is done as the server will
        /// not properly respond to requests while OnStartup() code is still running.
        /// </summary>
        /// <param name="filenameObject">The filename object.</param>
        /// <exception cref="Exception">Error getting certificate: { errorMessage }</exception>
        protected void Process( object filenameObject )
        {
            try
            {
                string json = File.ReadAllText( ( string ) filenameObject );
                var data = JsonConvert.DeserializeObject<BootstrapData>( json );

                //
                // Wait for Rock to settle.
                //
                using ( var webclient = new System.Net.WebClient() )
                {
                    webclient.DownloadString( $"http://{ data.Hostnames[0] }/.well-known/acme-challenge/ping" );
                }

                //
                // Create the account and certificate data.
                //
                var acme = CreateAccount( data );
                int certificateId = ConfigureCertificate( data );

                //
                // Renew the certificate.
                //
                var certificate = AcmeHelper.RenewCertificate( certificateId, out string errorMessage );
                if ( !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    throw new Exception( $"Error getting certificate: { errorMessage }" );
                }

                //
                // Install certificate.
                //
                AcmeHelper.InstallCertificateData( certificate, true );

                //
                // Cleanup.
                //
                File.Delete( ( string ) filenameObject );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
            }
        }

        /// <summary>
        /// Creates the account.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public AcmeService CreateAccount( BootstrapData data )
        {
            var acme = new AcmeService( data.TestMode );
            var account = acme.Register( data.Email );

            account.TestMode = data.TestMode;
            account.Key = Convert.ToBase64String( acme.RSA );

            AcmeHelper.SaveAccountData( account );

            return acme;
        }

        /// <summary>
        /// Configures the certificate.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public int ConfigureCertificate( BootstrapData data )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var group = new Group
            {
                GroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.ACME_CERTIFICATES ).Id
            };

            groupService.Add( group );

            group.LoadAttributes( rockContext );

            //
            // Store the data.
            //
            group.Name = string.IsNullOrWhiteSpace( data.FriendlyName ) ? "Default Certificate" : data.FriendlyName;
            group.SetAttributeValue( "RemoveOldCertificate", "True" );
            group.SetAttributeValue( "Domains", string.Join( ",", data.Hostnames ) );
            group.SetAttributeValue( "Bindings", string.Join( "|", data.Bindings.ToList() ) );

            //
            // Save all the information.
            //
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                group.SaveAttributeValues( rockContext );
            } );

            return group.Id;
        }
    }

    public class BootstrapData
    {
        public string FriendlyName { get; set; }

        public bool TestMode { get; set; }

        public string Email { get; set; }

        public string[] Hostnames { get; set; }

        public BindingData[] Bindings { get; set; }
    }
}
