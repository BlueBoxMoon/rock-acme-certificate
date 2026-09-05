using System.Threading.Tasks;

using Microsoft.Owin;

using Rock;

namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// Owin Middleware to handle processing of Acme Challenges.
    /// </summary>
    /// <seealso cref="Microsoft.Owin.OwinMiddleware" />
    public class AcmeCertificateMiddleware : OwinMiddleware
    {
        private bool? _disabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcmeCertificateMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        public AcmeCertificateMiddleware( OwinMiddleware next )
            : base( next )
        {
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context">The request context.</param>
        public override async Task Invoke( IOwinContext context )
        {
            if ( !_disabled.HasValue )
            {
                _disabled = Rock.Web.Cache.GlobalAttributesCache.Value( "AcmeCertificateDisableOwin" ).AsBoolean();
            }

            var path = context.Request.Uri.AbsolutePath;

            if ( !_disabled.Value && path.StartsWith( "/.well-known/acme-challenge/" ) )
            {
                var token = path.Substring( 28 );

                var authorization = AcmeHelper.GetAuthorizationForToken( token );

                if ( string.IsNullOrEmpty( authorization ) )
                {
                    Rock.Logging.RockLogger.Log.Information( AcmeHelper.LoggingDomain, $"Received challenge request for unknown token '{token}'" );

                    context.Response.StatusCode = 404;
                    context.Response.Headers.Set( "Content-Type", "text-plain" );
                    context.Response.Write( "Unknown Challenge" );
                }
                else
                {
                    Rock.Logging.RockLogger.Log.Information( AcmeHelper.LoggingDomain, $"Received challenge request for token '{token}' and responding with '{authorization}'." );

                    context.Response.StatusCode = 200;
                    context.Response.Headers.Set( "Content-Type", "text-plain" );
                    context.Response.Write( authorization );
                }
            }
            else
            {
                await Next.Invoke( context );
            }
        }
    }
}
