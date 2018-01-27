using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using Rock;
using Rock.Data;
using Rock.Model;

using Microsoft.Web.Administration;
using Newtonsoft.Json;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Runtime.Caching;

namespace com.blueboxmoon.AcmeCertificate
{
    static public class AcmeHelper
    {
        #region Public IIS Methods

        /// <summary>
        /// Get the version number of the IIS installed on this system.
        /// </summary>
        /// <returns>A version that indicates the version of IIS installed.</returns>
        static public Version GetIISVersion()
        {
            using ( Microsoft.Win32.RegistryKey componentsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey( @"Software\Microsoft\InetStp", false ) )
            {
                if ( componentsKey != null )
                {
                    int majorVersion = ( int ) componentsKey.GetValue( "MajorVersion", -1 );
                    int minorVersion = ( int ) componentsKey.GetValue( "MinorVersion", -1 );

                    if ( majorVersion != -1 && minorVersion != -1 )
                        return new Version( majorVersion, minorVersion );
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to determine if the IIS Http Redirect module has been enabled on the server.
        /// </summary>
        /// <returns>True if the module is enabled, false if not, null if it could not be determined.</returns>
        static public bool IsHttpRedirectModuleEnabled()
        {
            var windows = Environment.GetFolderPath( Environment.SpecialFolder.System );

            return File.Exists( Path.Combine( Path.Combine( windows, "inetsrv" ), "redirect.dll" ) );
        }

        /// <summary>
        /// Attempts to enable the IIS HttpRedirect module for use in the system.
        /// </summary>
        /// <returns>True if the module was successfully enabled.</returns>
        static public bool EnableIISHttpRedirectModule()
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo( "dism.exe", "/Online /Enable-Feature /FeatureName:IIS-HttpRedirect /NoRestart" )
            };
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            if ( p.WaitForExit( 30000 ) )
            {
                return p.ExitCode == 0;
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified IIS site has been configured to redirect ACME
        /// challenge requests to the specified target URL.
        /// </summary>
        /// <param name="siteName">The IIS site name to check.</param>
        /// <param name="targetUrl">The target URL that must be used in the configuration.</param>
        /// <returns>True if the site has been properly configured.</returns>
        static public bool IsIISSiteRedirectEnabled( string siteName, string targetUrl )
        {
            var site = new ServerManager().Sites.Where( s => s.Name == siteName ).FirstOrDefault();

            if ( site == null )
            {
                return false;
            }

            var applicationRoot = site.Applications
                .Where( a => a.Path == "/" )
                .Single()
                .VirtualDirectories
                .Where( v => v.Path == "/" )
                .Single();

            var acmeChallenge = string.Format( "{0}\\.well-known\\acme-challenge", applicationRoot.PhysicalPath );

            if ( !Directory.Exists( acmeChallenge ) )
            {
                return false;
            }

            var webConfigPath = Path.Combine( acmeChallenge, "web.config" );

            if ( !File.Exists( webConfigPath ) )
            {
                return false;
            }

            var webConfig = new XmlDocument();
            webConfig.Load( webConfigPath );

            var node = webConfig.SelectSingleNode( "/configuration/system.webServer/httpRedirect" );
            if ( node == null )
            {
                return false;
            }

            var enabled = node.Attributes["enabled"];
            var destination = node.Attributes["destination"];

            return enabled != null && enabled.InnerText.AsBoolean() && destination != null && destination.InnerText == targetUrl;
        }

        /// <summary>
        /// Configure the specified IIS site name to redirect /.well-known/acme-challenge
        /// requests to the specified target path.
        /// </summary>
        /// <param name="siteName">The name of the IIS site to reconfigure.</param>
        /// <param name="targetUrl">The target URL to redirect ACME requests to.</param>
        static public void EnableIISSiteRedirect( string siteName, string targetUrl )
        {
            var site = new ServerManager().Sites.Where( s => s.Name == siteName ).FirstOrDefault();

            if ( site == null )
            {
                throw new KeyNotFoundException( "The specified site could not be found." );
            }

            //
            // Find the application root.
            //
            var applicationRoot = site.Applications
                .Where( a => a.Path == "/" )
                .Single()
                .VirtualDirectories
                .Where( v => v.Path == "/" )
                .Single();

            var wellKnown = Path.Combine( applicationRoot.PhysicalPath, ".well-known" );
            var acmeChallenge = Path.Combine( wellKnown, "acme-challenge" );

            //
            // Create directories if they don't exist.
            //
            if ( !Directory.Exists( wellKnown ) )
            {
                Directory.CreateDirectory( wellKnown );
            }
            if ( !Directory.Exists( acmeChallenge ) )
            {
                Directory.CreateDirectory( acmeChallenge );
            }

            //
            // Load an existing web.config if there is one.
            //
            var webConfigPath = Path.Combine( acmeChallenge, "web.config" );
            var webConfig = new XmlDocument();
            try
            {
                webConfig.Load( webConfigPath );
            }
            catch
            {
                webConfig = new XmlDocument();
                var dec = webConfig.CreateXmlDeclaration( "1.0", "UTF-8", null );
                webConfig.AppendChild( dec );
            }

            //
            // Generate the configuration section.
            //
            var configuration = webConfig.SelectSingleNode( "/configuration" );
            if ( configuration == null )
            {
                configuration = webConfig.CreateNode( XmlNodeType.Element, "configuration", string.Empty );
                webConfig.AppendChild( configuration );
            }

            //
            // Generate the system.webServer section.
            //
            var webServer = configuration.SelectSingleNode( "system.webServer" );
            if ( webServer == null )
            {
                webServer = webConfig.CreateNode( XmlNodeType.Element, "system.webServer", string.Empty );
                configuration.AppendChild( webServer );
            }

            //
            // Generate the httpRedirect node.
            //
            var httpRedirect = webServer.SelectSingleNode( "httpRedirect" );
            if ( httpRedirect == null )
            {
                httpRedirect = webConfig.CreateNode( XmlNodeType.Element, "httpRedirect", string.Empty );
                webServer.AppendChild( httpRedirect );
            }

            //
            // Re-configure the httpRedirect node to have our values.
            //
            httpRedirect.Attributes.RemoveAll();

            var enabled = webConfig.CreateAttribute( "enabled" );
            enabled.InnerText = "true";
            httpRedirect.Attributes.Append( enabled );

            var destination = webConfig.CreateAttribute( "destination" );
            destination.InnerText = targetUrl;
            httpRedirect.Attributes.Append( destination );

            webConfig.Save( webConfigPath );
        }

        /// <summary>
        /// Get a collection of all the site names on the IIS server.
        /// </summary>
        /// <returns>Collection of strings that identify all the site names.</returns>
        static public string[] GetSites()
        {
            return new ServerManager().Sites.Select( s => s.Name ).ToArray();
        }

        /// <summary>
        /// Get a collection of all IPv4 addresses on the IIS server.
        /// </summary>
        /// <returns>Collection of strings that identify all IPv4 addresses on the server.</returns>
        static public string[] GetIPv4Addresses()
        {
            List<string> addresses = new List<string>();

            foreach ( NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces() )
            {
                foreach ( var addr in netInterface.GetIPProperties().UnicastAddresses )
                {
                    if ( addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                    {
                        addresses.Add( addr.Address.ToString() );
                    }
                }
            }

            return addresses.ToArray();
        }

        #endregion

        #region Private IIS Methods

        /// <summary>
        /// Install a new certificate into the Local Machine store for use by IIS.
        /// </summary>
        /// <param name="friendlyName">The friendly name to set for the certificate.</param>
        /// <param name="privateKeyData">Private key data in DER format associated with the certificate.</param>
        /// <param name="certificateData">The certificate and chain certificates for this import. Primary certificate should be in position 0.</param>
        /// <returns>SHA-1 signature of the certificate.</returns>
        static private byte[] InstallCertificate( string friendlyName, byte[] privateKeyData, ICollection<byte[]> certificateData )
        {
            //
            // Compute the hash of the primary certificate.
            //
            byte[] certificateHash;
            using ( var hasher = System.Security.Cryptography.SHA1.Create() )
            {
                certificateHash = hasher.ComputeHash( certificateData.First() );
            }

            //
            // Open the certificate store.
            //
            var store = new X509Store( StoreName.My, StoreLocation.LocalMachine );
            store.Open( OpenFlags.ReadWrite );

            //
            // Do a final conversion of the certificate data into a PKCS12 blob and add it to the store.
            //
            var pkcs12 = GetPkcs12Certificate( null, privateKeyData, certificateData );
            var x509Flags = X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet;
            var finalCert = new X509Certificate2( pkcs12, string.Empty, x509Flags )
            {
                FriendlyName = friendlyName
            };

            store.Add( finalCert );

            return finalCert.GetCertHash();
        }

        /// <summary>
        /// Removes the certificate that matches the hash from the certificate store.
        /// </summary>
        /// <param name="certificateHash">The SHA-1 hash of the certificate to be removed.</param>
        /// <returns>true if the certificate was removed.</returns>
        static private bool RemoveCertificate( byte[] certificateHash )
        {
            //
            // Open the certificate store.
            //
            var store = new X509Store( StoreName.My, StoreLocation.LocalMachine );
            store.Open( OpenFlags.ReadWrite );

            foreach ( var certificate in store.Certificates )
            {
                if ( certificate.GetCertHash().SequenceEqual( certificateHash ) )
                {
                    store.Remove( certificate );

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Configures the binding information for the specified site name.
        /// </summary>
        /// <param name="siteName">The name of the IIS site to configure.</param>
        /// <param name="ipAddress">The IP address associated with this binding or null for all available.</param>
        /// <param name="port">The port number, usually 443.</param>
        /// <param name="hostname">The hostname associated with this binding or null for all available.</param>
        /// <param name="certificateHash">The SHA-1 hash of the certificate.</param>
        /// <param name="createBinding">True if a new binding should be created if it doesn't already exist.</param>
        /// <returns>true if the binding was created/updated, false if not.</returns>
        static private void ConfigureBindings( List<BindingData> bindings, byte[] certificateHash, bool createBinding = true )
        {
            //
            // This chunk of code will add a new HTTPS binding linked to the certificate.
            //
            var server = new ServerManager();

            foreach ( var bindingData in bindings )
            {
                string bindingInformation = string.Format( "{0}:{1}:{2}",
                    !string.IsNullOrWhiteSpace( bindingData.IPAddress ) ? bindingData.IPAddress : "*",
                    bindingData.Port,
                    bindingData.Domain ?? string.Empty );

                var site = server.Sites
                    .Where( s => s.Name.Equals( bindingData.Site, StringComparison.CurrentCultureIgnoreCase ) )
                    .FirstOrDefault();
                if ( site == null )
                {
                    throw new Exception( string.Format( "Could not configure binding for site '{0}', no such site exists.", bindingData.Site ) );
                }

                var binding = site.Bindings
                    .AsQueryable()
                    .Where( b => b.BindingInformation.Equals( bindingInformation, StringComparison.CurrentCultureIgnoreCase ) )
                    .FirstOrDefault();
                if ( binding == null )
                {
                    if ( !createBinding )
                    {
                        throw new Exception( string.Format( "Could add new binding for site '{0}' with details '{1}', not configured to create new bindings and no existing binding was bound.", bindingData.Site, bindingInformation ) );
                    }

                    binding = site.Bindings.Add( bindingInformation, certificateHash, Enum.GetName( typeof( StoreName ), StoreName.My ) );
                }
                else
                {
                    binding.CertificateHash = certificateHash;
                    binding.CertificateStoreName = Enum.GetName( typeof( StoreName ), StoreName.My );
                }

                if ( !string.IsNullOrWhiteSpace( binding.Host ) && GetIISVersion().Major >= 8 )
                {
                    binding.SetAttributeValue( "sslFlags", 1 );
                }
            }

            server.CommitChanges();
        }

        #endregion

        #region Account Methods

        /// <summary>
        /// Load the account data associated with this server.
        /// </summary>
        /// <returns>The AccountData object that contains the configuration for the Acme system.</returns>
        static public AccountData LoadAccountData()
        {
            try
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( SystemGuid.Attribute.ACCOUNT.AsGuid() );

                return JsonConvert.DeserializeObject<AccountData>( Rock.Security.Encryption.DecryptString( attribute.DefaultValue ) ) ?? new AccountData();
            }
            catch
            {
                return new AccountData();
            }
        }

        /// <summary>
        /// Saves the account data back to the Rock database.
        /// </summary>
        /// <param name="account">The AccountData that is to be saved to the database.</param>
        static public void SaveAccountData( AccountData account )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeId = Rock.Web.Cache.AttributeCache.Read( SystemGuid.Attribute.ACCOUNT.AsGuid() ).Id;

                var attribute = new AttributeService( rockContext ).Get( attributeId );
                attribute.DefaultValue = Rock.Security.Encryption.EncryptString( JsonConvert.SerializeObject( account ) );

                rockContext.SaveChanges();

                Rock.Web.Cache.AttributeCache.Flush( attributeId );
            }
        }

        #endregion

        #region Public Renewal Methods

        /// <summary>
        /// Convert DER encoded private key data and certificate data into a PKCS12 container.
        /// </summary>
        /// <param name="password">The password to encrypt the container with, or null for no encryption.</param>
        /// <param name="privateKeyData">The private key data that will be stored in the container.</param>
        /// <param name="certificateData">The list of certificates that will be stored in the container.</param>
        /// <returns>A PKCS12 byte array.</returns>
        static public byte[] GetPkcs12Certificate( string password, byte[] privateKeyData, ICollection<byte[]> certificateData )
        {
            var certPrivateKey = PrivateKeyFactory.CreateKey( privateKeyData );
            List<X509CertificateEntry> certificates = new List<X509CertificateEntry>();
            var x509Parser = new X509CertificateParser();
            var pkStore = new Pkcs12Store();

            //
            // Load all the certificates from the raw data.
            //
            foreach ( var certdata in certificateData )
            {
                certificates.Add( new X509CertificateEntry( x509Parser.ReadCertificate( certdata ) ) );
            }

            //
            // Set the primary certificate and key.
            //
            var keyEntry = new AsymmetricKeyEntry( certPrivateKey );
            pkStore.SetCertificateEntry( string.Empty, certificates[0] );
            pkStore.SetKeyEntry( string.Empty, new AsymmetricKeyEntry( certPrivateKey ), new[] { certificates[0] } );

            //
            // Add in any additional chain certificates.
            //
            for ( int i = 1; i < certificates.Count; i++ )
            {
                pkStore.SetCertificateEntry( i.ToString(), certificates[i] );
            }

            //
            // Do a final conversion of the certificate data into a PKCS12 blob and add it to the store.
            //
            using ( var ms = new MemoryStream() )
            {
                pkStore.Save( ms, password?.ToArray(), new SecureRandom() );

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Renews a certificate and installs it in IIS.
        /// </summary>
        /// <param name="groupId">The Id of the group that contains all the certificate information.</param>
        /// <param name="manualRun">If this is a manual run by the user, if true then bindings are initialized instead of only updated.</param>
        /// <param name="errorMessage">On output contains an error message if the certificate did not renew.</param>
        static public Tuple<byte[], List<byte[]>> RenewCertificate( int groupId, bool manualRun, out string errorMessage )
        {
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupId );
            var account = LoadAccountData();

            group.LoadAttributes( rockContext );

            //
            // Get the list of domains and the old certificate hash.
            //
            var domains = group.GetAttributeValues( "Domains" );
            byte[] oldCertificate;
            try
            {
                oldCertificate = Convert.FromBase64String( group.GetAttributeValue( "CertificateHash" ) );
            }
            catch
            {
                oldCertificate = null;
            }

            //
            // Get the certificate private key and cert data.
            //
            var keyPair = AcmeService.GenerateKeyPair();
            var privateKeyData = PrivateKeyInfoFactory.CreatePrivateKeyInfo( keyPair.Private ).ToAsn1Object().GetDerEncoded();
            var csr = AcmeService.GenerateCSR( keyPair, domains );
            var certs = RenewOfflineCertificate( groupId, manualRun, csr.ToAsn1Object().GetDerEncoded(), out errorMessage );

            //
            // If not offline mode, install the certificate and configure IIS.
            //
            if ( !account.OfflineMode )
            {
                //
                // Set the friendly certificate name.
                //
                var friendlyName = string.Format( "{0} {1}", domains[0], DateTime.Now.ToString() );

                //
                // Attempt to install the private key and certificates.
                //
                var certificateHash = InstallCertificate( friendlyName, privateKeyData, certs );

                //
                // Install or update all bindings for this certificate.
                //
                var bindings = group
                    .GetAttributeValue( "Bindings" )
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => new BindingData( s ) )
                    .ToList();

                ConfigureBindings( bindings, certificateHash, true );

                //
                // Everything worked, delete the old certificate it if we are configured to do so.
                //
                if ( group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false ) && oldCertificate != null )
                {
                    RemoveCertificate( oldCertificate );
                }
            }

            return string.IsNullOrWhiteSpace( errorMessage ) ? new Tuple<byte[], List<byte[]>>( privateKeyData, certs ) : null;
        }

        /// <summary>
        /// Renews a certificate and installs it in IIS.
        /// </summary>
        /// <param name="groupId">The Id of the group that contains all the certificate information.</param>
        /// <param name="manualRun">If this is a manual run by the user, if true then bindings are initialized instead of only updated.</param>
        /// <param name="errorMessage">On output contains an error message if the certificate did not renew.</param>
        static public List<byte[]> RenewOfflineCertificate( int groupId, bool manualRun, byte[] csrData, out string errorMessage )
        {
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupId );
            var cache = Rock.Web.Cache.RockMemoryCache.Default;
            var account = LoadAccountData();
            var acme = new AcmeService( Convert.FromBase64String( account.Key ), account.TestMode );

            errorMessage = string.Empty;

            group.LoadAttributes( rockContext );

            var domains = group.GetAttributeValues( "Domains" );
            var csr = new Pkcs10CertificationRequest( csrData );

            //
            // Attempt to validate all the domains.
            //
            acme.ValidateDomains( domains, ( domain, token, authorization ) =>
            {
                if ( authorization != null )
                {
                    cache.Add( string.Format( "com.blueboxmoon.AcmeChallenge.{0}", token.Token ), authorization, new CacheItemPolicy() );
                }
                else
                {
                    cache.Remove( string.Format( "com.blueboxmoon.AcmeChallenge.{0}", token.Token ) );
                }
            } );

            //
            // Get the certificate.
            //
            var certs = acme.GetCertificate( domains, csr );

            //
            // Set the date and time we last renewed and the friendly certificate name.
            //
            group.SetAttributeValue( "LastRenewed", DateTime.Now.ToString() );
            var friendlyName = string.Format( "{0} {1}", domains[0], DateTime.Now.ToString() );
            group.SetAttributeValue( "Expires", new X509CertificateParser().ReadCertificate( certs.First() ).NotAfter.ToLocalTime().ToString() );

            //
            // Compute the hash of the primary certificate.
            //
            byte[] certificateHash;
            using ( var hasher = System.Security.Cryptography.SHA1.Create() )
            {
                certificateHash = hasher.ComputeHash( certs.First() );
            }

            //
            // Do final updates on the certificate data and save.
            //
            group.SetAttributeValue( "CertificateHash", Convert.ToBase64String( certificateHash ) );
            group.SaveAttributeValues( rockContext );

            return string.IsNullOrWhiteSpace( errorMessage ) ? certs : null;
        }

        #endregion
    }
}
