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
                int groupId = PageParameter( "Id" ).AsInteger();

                ltTitle.Text = groupId != 0 ? "Edit Certificate" : "Add Certificate";

                BindingsState = new List<BindingData>();

                //
                // Attempt to load existing certificate data.
                //
                if ( groupId != 0 )
                {
                    var group = new GroupService( new RockContext() ).Get( groupId );

                    if ( group != null )
                    {
                        group.LoadAttributes();

                        tbFriendlyName.Text = group.Name;
                        vlDomains.Value = group.GetAttributeValue( "Domains" );
                        cbRemoveOldCertificate.Checked = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false );

                        var bindings = group.GetAttributeValue( "Bindings" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                        BindingsState = bindings.Select( b => new BindingData( b ) ).ToList();
                    }
                }

                GridBind();
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

        #region Event Methods

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
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

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
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
    }
}
