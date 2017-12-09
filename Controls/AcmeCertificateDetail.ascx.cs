using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

using com.blueboxmoon.AcmeCertificate;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Certificate Detail" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Configures a certificate." )]
    public partial class AcmeCertificateDetail : RockBlock
    {
        #region Protected Properties

        /// <summary>
        /// Contains the information, via ViewState, for all the bindings that are configured
        /// for this certificate.
        /// </summary>
        protected List<BindingData> BindingsState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Load the custom view state data.
        /// </summary>
        /// <param name="savedState">The object that contains our view state.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            BindingsState = ( List<BindingData> ) ViewState["BindingsState"];
        }

        /// <summary>
        /// Handles the OnInit event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBindings.Actions.ShowAdd = true;
            gBindings.Actions.AddClick += gBindings_Add;
            lbDetailDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", "Certificate" );
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
                if ( PageParameter( "Id" ).AsInteger() != 0 )
                {
                    ShowDetail();
                }
                else
                {
                    ShowEdit();
                }
            }

            nbMessage.Text = string.Empty;
        }

        /// <summary>
        /// Save the custom view state data.
        /// </summary>
        /// <returns>The object that contains the view state.</returns>
        protected override object SaveViewState()
        {
            ViewState["BindingsState"] = BindingsState;

            return base.SaveViewState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show the read-only panel and fill in all fields.
        /// </summary>
        protected void ShowDetail()
        {
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( PageParameter( "Id" ).AsInteger() );

            group.LoadAttributes( rockContext );

            ltDetailTitle.Text = group.Name;
            ltRemoveOld.Text = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean().ToString();
            ltDetailDomains.Text = string.Join( "<br />", group.GetAttributeValue( "Domains" ).SplitDelimitedValues() );
            ltDetailLastRenewed.Text = group.GetAttributeValue( "LastRenewed" );
            ltDetailExpires.Text = group.GetAttributeValue( "Expires" );

            var bindings = group.GetAttributeValue( "Bindings" )
                .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( b => new BindingData( b ) )
                .Select( b => string.Format( "{0} {1}:{2}:{3}", b.Site, string.IsNullOrWhiteSpace( b.IPAddress ) ? "*" : b.IPAddress, b.Port, b.Domain ) );

            ltDetailBindings.Text = string.Join( "<br />", bindings );

            CheckIISState();

            pnlEdit.Visible = false;
            pnlDetail.Visible = true;
        }

        /// <summary>
        /// Show the edit panel and fill in all fields.
        /// </summary>
        protected void ShowEdit()
        {
            int groupId = PageParameter( "Id" ).AsInteger();
            var group = new GroupService( new RockContext() ).Get( groupId );

            ltEditTitle.Text = groupId != 0 ? "Edit Certificate" : "Add Certificate";
            BindingsState = new List<BindingData>();

            if ( group != null )
            {
                group.LoadAttributes();

                tbFriendlyName.Text = group.Name;
                vlDomains.Value = group.GetAttributeValue( "Domains" );
                cbRemoveOldCertificate.Checked = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false );

                var bindings = group.GetAttributeValue( "Bindings" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                BindingsState = bindings.Select( b => new BindingData( b ) ).ToList();
            }

            GridBind();

            nbIISError.Visible = false;
            pnlIISRedirectModuleWarning.Visible = false;
            pnlIISRedirectSiteWarning.Visible = false;

            pnlDetail.Visible = false;
            pnlEdit.Visible = true;
        }

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
        /// Bind the IIS Bindings grid to show the current list of configured bindings.
        /// </summary>
        protected void GridBind()
        {
            gBindings.DataSource = BindingsState;
            gBindings.DataBind();
        }

        /// <summary>
        /// Show the specified binding for editing.
        /// </summary>
        /// <param name="binding">The binding data to be edited or null to add a new binding.</param>
        protected void ShowBinding( BindingData binding )
        {
            ddlEditBindingSite.Items.Clear();
            ddlEditBindingSite.Items.Add( new ListItem() );
            try
            {
                AcmeHelper.GetSites().ToList().ForEach( s => ddlEditBindingSite.Items.Add( s ) );
            }
            catch { /* Intentionally left blank */ }

            ddlEditBindingIPAddress.Items.Clear();
            ddlEditBindingIPAddress.Items.Add( new ListItem() );
            try
            {
                AcmeHelper.GetIPv4Addresses().ToList().ForEach( a => ddlEditBindingIPAddress.Items.Add( a ) );
            }
            catch { /* Intentionally left blank */ }

            ddlEditBindingSite.SetValue( binding != null ? binding.Site : System.Web.Hosting.HostingEnvironment.SiteName );
            ddlEditBindingIPAddress.SetValue( binding != null ? binding.IPAddress : string.Empty );
            nbEditBindingPort.Text = binding != null ? binding.Port.ToString() : "443";
            tbEditBindingDomain.Text = binding != null ? binding.Domain : string.Empty;
        }

        #endregion

        #region Detail Event Methods

        /// <summary>
        /// Handles the Click event of the lbDetailEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailEdit_Click( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the Click event of the lbDetailRenew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailRenew_Click( object sender, EventArgs e )
        {
            pnlDetail.Visible = false;
            pnlRenew.Visible = true;
            pnlRenewOutput.Visible = false;
            pnlRenewInput.Visible = true;
            pnlRenewSuccess.Visible = false;
            nbRenewNotOffline.Visible = false;

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( PageParameter( "Id" ).AsInteger() );

            ltRenewTitle.Text = group.Name;
            tbRenewCSR.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the lbDetailCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbDetailDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( PageParameter( "Id" ).AsInteger() );
            groupService.Delete( group );

            rockContext.SaveChanges();

            NavigateToParentPage();
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

        #endregion

        #region Edit Event Methods

        /// <summary>
        /// Handles the Click event of the lbEditSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditSave_Click( object sender, EventArgs e )
        {
            //
            // Verify we have at least one domain name entered.
            //
            if ( vlDomains.Value.SplitDelimitedValues().Length == 0 )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Text = "You must enter at least one domain to vaidate.";

                return;
            }

            //
            // Verify we have at least one binding configured.
            // TODO: This should probably be removed so offline mode will work correctly.
            //
            if ( BindingsState.Count == 0 )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Text = "You must add at least one IIS binding.";

                return;
            }

            //
            // Load the existing data or create a new entry.
            //
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( PageParameter( "Id" ).AsInteger() );
            if ( group == null )
            {
                group = new Group();
                group.GroupTypeId = GroupTypeCache.Read( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;

                groupService.Add( group );
            }

            group.LoadAttributes( rockContext );

            //
            // Store the data.
            //
            group.Name = tbFriendlyName.Text;
            group.SetAttributeValue( "RemoveOldCertificate", cbRemoveOldCertificate.Checked.ToString() );
            group.SetAttributeValue( "Domains", vlDomains.Value );
            group.SetAttributeValue( "Bindings", string.Join( "|", BindingsState ) );

            //
            // Save all the information.
            //
            rockContext.WrapTransaction( () =>
             {
                 rockContext.SaveChanges();

                 group.SaveAttributeValues( rockContext );
             } );

            NavigateToCurrentPage( new Dictionary<string, string> { { "Id", group.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the lbEditCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditCancel_Click( object sender, EventArgs e )
        {
            if ( PageParameter( "Id" ).AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Handles the Add event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBindings_Add( object sender, EventArgs e )
        {
            mdEditBinding.SaveButtonText = "Add";
            mdEditBinding.Title = "Add Binding";

            hfEditBindingIndex.Value = string.Empty;
            ShowBinding( null );

            mdEditBinding.Show();
        }

        /// <summary>
        /// Handles the Delete event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            BindingsState.RemoveAt( e.RowIndex );

            GridBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            GridBind();
        }

        /// <summary>
        /// Handles the RowSelected event of the gBindings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBindings_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            mdEditBinding.SaveButtonText = "Save";
            mdEditBinding.Title = "Edit Binding";

            hfEditBindingIndex.Value = e.RowIndex.ToString();
            ShowBinding( BindingsState[e.RowIndex] );

            mdEditBinding.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditBinding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditBinding_SaveClick( object sender, EventArgs e )
        {
            BindingData binding;

            //
            // Edit existing binding or create a new one.
            //
            if ( !string.IsNullOrWhiteSpace( hfEditBindingIndex.Value ) )
            {
                binding = BindingsState[hfEditBindingIndex.ValueAsInt()];
            }
            else
            {
                binding = new BindingData();
                BindingsState.Add( binding );
            }

            //
            // Set all the binding information.
            //
            binding.Site = ddlEditBindingSite.SelectedValue;
            binding.IPAddress = ddlEditBindingIPAddress.SelectedValue;
            binding.Port = nbEditBindingPort.Text.AsInteger();
            binding.Domain = tbEditBindingDomain.Text;

            mdEditBinding.Hide();
            GridBind();
        }

        #endregion

        #region Renew Event Methods

        /// <summary>
        /// Handles the CheckChanged event of the cbRenewCustomCSR control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbRenewCustomCSR_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbRenewCustomCSR.Checked )
            {
                tbRenewCSR.Visible = AcmeHelper.LoadAccountData().OfflineMode;
                nbRenewNotOffline.Visible = !tbRenewCSR.Visible;
                lbRequestCertificate.Enabled = !nbRenewNotOffline.Visible;
            }
            else
            {
                tbRenewCSR.Visible = false;
                nbRenewNotOffline.Visible = false;
                lbRequestCertificate.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRequestCertificate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRequestCertificate_Click( object sender, EventArgs e )
        {
            try
            {
                string errorMessage;

                byte[] privateKeyData = null;
                List<byte[]> certificateData;

                if ( !string.IsNullOrWhiteSpace( tbRenewCSR.Text ) )
                {
                    string csrText = tbRenewCSR.Text
                        .Replace( "-----BEGIN CERTIFICATE REQUEST-----", string.Empty )
                        .Replace( "-----END CERTIFICATE REQUEST-----", string.Empty );
                    byte[] csrData = Convert.FromBase64String( csrText );
                    certificateData = AcmeHelper.RenewOfflineCertificate( PageParameter( "Id" ).AsInteger(), true, csrData, out errorMessage );
                }
                else
                {
                    //
                    // Attempt to renew the certificate, new bindings will be created as needed.
                    //
                    var tuple = AcmeHelper.RenewCertificate( PageParameter( "Id" ).AsInteger(), true, out errorMessage );
                    privateKeyData = tuple != null ? tuple.Item1 : null;
                    certificateData = tuple != null ? tuple.Item2 : null;
                }

                if ( certificateData == null )
                {
                    nbRenewError.Text = errorMessage;
                }
                else
                {
                    pnlRenewInput.Visible = false;
                    pnlRenewOutput.Visible = false;
                    pnlRenewSuccess.Visible = false;

                    if ( AcmeHelper.LoadAccountData().OfflineMode )
                    {
                        pnlRenewOutput.Visible = true;
                        pnlRenewOutputPEM.Visible = true;
                        pnlRenewOutputP12.Visible = false;

                        rblRenewDownloadType.Items.Clear();
                        rblRenewDownloadType.Items.Add( "PEM" );

                        //
                        // Prepare the PEM formatted certificates.
                        //
                        ltRenewPEM.Text = string.Empty;
                        if ( privateKeyData != null )
                        {
                            ltRenewPEM.Text = string.Format( "-----BEGIN RSA PRIVATE KEY-----\n{0}\n-----END RSA PRIVATE KEY-----\n\n",
                                Convert.ToBase64String( privateKeyData, Base64FormattingOptions.InsertLineBreaks ) ).ConvertCrLfToHtmlBr();
                        }

                        var certificates = certificateData
                            .Select( c => Convert.ToBase64String( c, Base64FormattingOptions.InsertLineBreaks ) )
                            .Select( c => string.Format( "-----BEGIN CERTIFICATE-----\n{0}\n-----END CERTIFICATE-----", c ) );
                        ltRenewPEM.Text += string.Join( "\n\n", certificates ).ConvertCrLfToHtmlBr();

                        if ( privateKeyData != null )
                        {
                            //
                            // Prepare the PKCS12 formatted certificate.
                            //
                            var password = System.Web.Security.Membership.GeneratePassword( 8, 1 );
                            var pkcs12 = AcmeHelper.GetPkcs12Certificate( password, privateKeyData, certificateData );

                            //
                            // Store the password protected PKCS12 data as a binary file.
                            //
                            var rockContext = new RockContext();
                            var outputBinaryFile = new BinaryFile
                            {
                                IsTemporary = true,
                                ContentStream = new System.IO.MemoryStream( pkcs12 ),
                                FileName = "Certificate.p12",
                                MimeType = "application/x-pkcs12",
                                BinaryFileTypeId = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id
                            };

                            new BinaryFileService( rockContext ).Add( outputBinaryFile );

                            rockContext.SaveChanges();

                            //
                            // Present a download link to the user.
                            //
                            ltRenewP12.Text = string.Format(
                                "Your certificate has been encrypted with the password <code>{0}</code> and is ready for <a href='/GetFile.ashx?guid={1}'>download</a>.",
                                password, outputBinaryFile.Guid );

                            rblRenewDownloadType.Items.Add( "P12" );
                        }

                        rblRenewDownloadType.SelectedValue = "PEM";
                    }
                    else
                    {
                        pnlRenewSuccess.Visible = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context );
                throw;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRenewCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenewCancel_Click( object sender, EventArgs e )
        {
            pnlRenew.Visible = false;
            pnlDetail.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbRenewDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenewDone_Click( object sender, EventArgs e )
        {
            pnlRenew.Visible = false;
            NavigateToCurrentPage( new Dictionary<string, string> { { "Id", PageParameter( "Id" ) } } );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRenewDownloadType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblRenewDownloadType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblRenewDownloadType.SelectedValue == "PEM" )
            {
                pnlRenewOutputPEM.Visible = true;
                pnlRenewOutputP12.Visible = false;
            }
            else
            {
                pnlRenewOutputPEM.Visible = false;
                pnlRenewOutputP12.Visible = true;
            }
        }

        #endregion
    }
}
