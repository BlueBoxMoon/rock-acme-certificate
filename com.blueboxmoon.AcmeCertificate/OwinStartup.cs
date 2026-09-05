using Owin;

using Rock.Utility;

namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// Handles tapping into the request pipeline.
    /// </summary>
    /// <seealso cref="Rock.Utility.IRockOwinStartup" />
    public class OwinStartup : IRockOwinStartup
    {
        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order to startup in.
        /// </value>
        public int StartupOrder => 0;

        /// <summary>
        /// Method that will be run at Rock Owin startup
        /// </summary>
        /// <param name="app"></param>
        public void OnStartup( IAppBuilder app )
        {
            app.Use( typeof( AcmeCertificateMiddleware ) );
        }
    }
}
