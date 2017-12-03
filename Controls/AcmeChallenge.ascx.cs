using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Challenge" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Responds to challenges for the Acme certification system." )]
    public partial class AcmeChallenge : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "Token" ) ) )
                {
                    var cache = Rock.Web.Cache.RockMemoryCache.Default;

                    Response.Clear();
                    Response.Write( cache[string.Format( "com.blueboxmoon.AcmeChallenge.{0}", PageParameter( "Token" ) )] );
                    Response.Flush();
                    Response.SuppressContent = true;
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}
