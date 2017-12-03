using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.blueboxmoon.AcmeCertificate;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Certificates" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Lists the certificate configuration." )]

    [LinkedPage( "Detail Page", order: 0 )]
    [TextField( "Redirect Override", "If you enter a value here it will be used as the redirect URL for Acme Challenges to other sites instead of the automatically determined one.", false, order: 1 )]
    public partial class AcmeCertificates : RockBlock, ISecondaryBlock
    {
        #region Base Control Method Overrides

        /// <summary>
        /// Handles the OnInit event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCertificates.Actions.ShowAdd = true;
            gCertificates.Actions.AddClick += gCertificates_AddClick;
        }

        /// <summary>
        /// Handles the OnLoad event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                var account = AcmeHelper.LoadAccountData();

                pnlCertificates.Visible = !string.IsNullOrWhiteSpace( account.Key );

                if ( !string.IsNullOrWhiteSpace( account.Key ) )
                {
                    CheckIISState();
                }

                BindGrid();
            }
            else
            {
                nbIISError.Text = string.Empty;
                nbRenewStatus.Text = string.Empty;
                pnlDownloadCertificate.Visible = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs various checks on IIS to ensure it is configured correctly for the certificates that
        /// have been setup for processing.
        /// </summary>
        protected void CheckIISState()
        {
            var rockContext = new RockContext();
            var targetUrl = GetRedirectUrl();
            var groupTypeId = GroupTypeCache.Read( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;
            var bindings = new List<BindingData>();

            var groups = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );

            var siteNames = new List<string>();

            //
            // Determine if we have any certificates that edit bindings of a site other than the Rock site.
            //
            foreach ( var group in groups )
            {
                var currentSiteName = System.Web.Hosting.HostingEnvironment.SiteName;

                group.LoadAttributes( rockContext );

                bindings.AddRange( group.GetAttributeValue( "Bindings" )
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( b => new BindingData( b ) )
                    .Where( b => b.Site != currentSiteName ) );

                siteNames.AddRange( bindings.Where( b => !AcmeHelper.IsIISSiteRedirectEnabled( b.Site, targetUrl ) ).Select( b => b.Site ) );
            }

            //
            // If we have non-Rock sites to configure, ensure that the Http Redirect module has been
            // installed in IIS.
            //
            if ( bindings.Any() && !AcmeHelper.IsHttpRedirectModuleEnabled() )
            {
                pnlIISRedirectModuleWarning.Visible = true;
                pnlIISRedirectSiteWarning.Visible = false;
            }
            else
            {
                pnlIISRedirectModuleWarning.Visible = false;
            }

            //
            // If the redirect module has been installed but we have sites that need to be
            // configured, then present a notice about those sites.
            //
            if ( siteNames.Any() && !pnlIISRedirectModuleWarning.Visible )
            {
                siteNames = siteNames.Distinct().ToList();

                hfEnableSiteRedirects.Value = siteNames.AsDelimited( "," );
                ltEnableSiteRedirects.Text = "<li>" + siteNames.AsDelimited( "</li><li>" ) + "</li>";

                ltTargetRedirect.Text = targetUrl;
                pnlIISRedirectSiteWarning.Visible = true;
            }
            else
            {
                pnlIISRedirectSiteWarning.Visible = false;
            }
        }

        /// <summary>
        /// Gets a URL that should be used in configuring the Site Redirects to Rock.
        /// </summary>
        /// <returns>A string representing the URL to be used for site redirects.</returns>
        protected string GetRedirectUrl()
        {
            var url = GetAttributeValue( "RedirectOverride" );

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = string.Format( "{0}.well-known/acme-challenge/", GlobalAttributesCache.Value( "PublicApplicationRoot" ) );
            }

            return url;
        }

        /// <summary>
        /// Bind the grid of certificates that are configured in the system.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();

            var groupTypeId = GroupTypeCache.Read( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;

            var groups = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );


            if ( gCertificates.SortProperty != null )
            {
                groups = groups.Sort( gCertificates.SortProperty );
            }
            else
            {
                groups = groups.OrderBy( g => g.Name );
            }


            var groupList = groups.ToList();
            groupList.ForEach( g => g.LoadAttributes( rockContext ) );
            var data = groupList.Select( g => new
            {
                Id = g.Id,
                Name = g.Name,
                LastRenewed = g.GetAttributeValue( "LastRenewed" ),
                Expires = g.GetAttributeValue( "Expires" ),
                Domains = string.Join( "<br />", g.GetAttributeValues( "Domains" ) )
            } );

            gCertificates.DataSource = data.ToList();
            gCertificates.DataBind();
        }

        /// <summary>
        /// Called when another block on the page requests secondary blocks to hide or become visible.
        /// </summary>
        /// <param name="visible">True if this block should become visible.</param>
        public void SetVisible( bool visible )
        {
            var account = AcmeHelper.LoadAccountData();

            pnlCertificates.Visible = visible && !string.IsNullOrWhiteSpace( account.Key );
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Handles the GridRebind event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCertificates_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", 0 );
        }

        /// <summary>
        /// Handles the Delete event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( e.RowKeyId );
            groupService.Delete( group );

            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Renew event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_Renew( object sender, RowEventArgs e )
        {
            try
            {
                string errorMessage;

                //
                // Attempt to renew the certificate, new bindings will be created as needed.
                //
                var tuple = AcmeHelper.RenewCertificate( e.RowKeyId, true, out errorMessage );

                if ( tuple == null )
                {
                    nbRenewStatus.NotificationBoxType = NotificationBoxType.Danger;
                    nbRenewStatus.Text = errorMessage;
                }
                else
                {
                    if ( AcmeHelper.LoadAccountData().OfflineMode )
                    {
                        string certData = Convert.ToBase64String( tuple.Item1 ) + "|" +
                            string.Join( "|", tuple.Item2.Select( c => Convert.ToBase64String( c ) ) );

                        hfCertificate.Value = Rock.Security.Encryption.EncryptString( certData );
                        pnlDownloadCertificate.Visible = true;
                    }
                    else
                    {
                        nbRenewStatus.NotificationBoxType = NotificationBoxType.Success;
                        nbRenewStatus.Text = "Certificate was renewed.";
                    }
                }

                BindGrid();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context );
                throw;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEnableRedirectModule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEnableRedirectModule_Click( object sender, EventArgs e )
        {
            if ( !AcmeHelper.EnableIISHttpRedirectModule() )
            {
                nbIISError.Text = "Failed to enable the IIS Http Redirect module. Rock may not have enough permissions to perform this task. Please manually enable the IIS Http Redirect module.";
            }
            else
            {
                NavigateToCurrentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEnableSiteRedirects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEnableSiteRedirects_Click( object sender, EventArgs e )
        {
            var siteNames = hfEnableSiteRedirects.Value.Split( ',' );
            var targetUrl = GetRedirectUrl();

            var errors = new List<string>();

            //
            // For each site that was detected as not properly configured, try to configure it.
            //
            foreach ( var siteName in siteNames )
            {
                try
                {
                    AcmeHelper.EnableIISSiteRedirect( siteName, targetUrl );
                }
                catch ( Exception ex )
                {
                    errors.Add( ex.Message );
                }
            }

            if ( errors.Any() )
            {
                nbIISError.Text = string.Format( "Failed to enable the redirect on one or more sites. This may be due to insufficient permissions to make modifications to the other sites. <ul><li>{0}</li></ul>",
                    string.Join( "</li><li>", errors ) );
            }

            CheckIISState();
        }

        /// <summary>
        /// Handles the Click event of the lbPrepareCertificate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrepareCertificate_Click( object sender, EventArgs e )
        {
            //
            // Decode the data in the hidden field into the raw certificate data.
            //
            var certData = Rock.Security.Encryption.DecryptString( hfCertificate.Value )
                .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( v => Convert.FromBase64String( v ) )
                .ToList();

            var pkcs12 = AcmeHelper.GetPkcs12Certificate( tbCertificatePassword.Text,
                certData.First(), certData.Skip( 1 ).ToList() );

            //
            // Store the password protected PKCS12 data as a binary file.
            //
            var rockContext = new RockContext();
            var outputBinaryFile = new BinaryFile
            {
                IsTemporary = true,
                ContentStream = new System.IO.MemoryStream( pkcs12 ),
                FileName = "Certfificate.p12",
                MimeType = "application/x-pkcs12",
                BinaryFileTypeId = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id
            };

            new BinaryFileService( rockContext ).Add( outputBinaryFile );

            rockContext.SaveChanges();

            //
            // Present a download link to the user.
            //
            pnlDownloadCertificate.Visible = false;
            nbRenewStatus.Text = string.Format( "Your <a href='/GetFile.ashx?guid={0}'>certificate</a> is ready for download.", outputBinaryFile.Guid );
            nbRenewStatus.NotificationBoxType = NotificationBoxType.Success;
        }

        #endregion
    }
}
