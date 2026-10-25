using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.Web.Cache;

using com.blueboxmoon.AcmeCertificate.ViewModels;

namespace com.blueboxmoon.AcmeCertificate.Blocks
{
    /// <summary>
    /// Lists the certificate configuration.
    /// </summary>
    [DisplayName( "Acme Certificates" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Lists the certificate configuration." )]
    [IconCssClass( "fa fa-certificate" )]
    [SupportedSiteTypes( Rock.Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the certificate details.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.DetailPage )]

    [TextField( "Redirect Override",
        Description = "If you enter a value here it will be used as the redirect URL for Acme Challenges to other sites instead of the automatically determined one.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.RedirectOverride )]

    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.ACME_CERTIFICATE_LIST )]
    [Rock.SystemGuid.BlockTypeGuid( SystemGuid.BlockType.ACME_CERTIFICATE_LIST_OBSIDIAN )]
    public class AcmeCertificateList : RockListBlockType<Group>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";

            public const string RedirectOverride = "RedirectOverride";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => RequestContext.ResolveRockUrl( "~/Plugins/com_blueboxmoon/AcmeCertificate/acmeCertificateList.obs" );

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var isAccountRegistered = !string.IsNullOrWhiteSpace( AcmeHelper.LoadAccountData().Key );
            var box = new ListBlockBox<AcmeCertificateListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = isAccountRegistered;
            box.IsDeleteEnabled = isAccountRegistered;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = new AcmeCertificateListOptionsBag
            {
                IsAccountRegistered = isAccountRegistered
            };
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "Id", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Group> GetListQueryable( RockContext rockContext )
        {
            var groupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.ACME_CERTIFICATES ).Id;

            return new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );
        }

        /// <inheritdoc/>
        protected override IQueryable<Group> GetOrderedListQueryable( IQueryable<Group> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( g => g.Name );
        }

        /// <inheritdoc/>
        protected override List<Group> GetListItems( IQueryable<Group> queryable, RockContext rockContext )
        {
            var groups = queryable.ToList();

            groups.ForEach( g => g.LoadAttributes( rockContext ) );

            return groups;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Group> GetGridBuilder()
        {
            return new GridBuilder<Group>()
                .WithBlock( this )
                .AddTextField( "idKey", g => g.IdKey )
                .AddTextField( "name", g => g.Name )
                .AddDateTimeField( "lastRenewed", g => g.GetAttributeValue( "LastRenewed" ).AsDateTime() )
                .AddDateTimeField( "expires", g => g.GetAttributeValue( "Expires" ).AsDateTime() )
                .AddTextField( "domains", g => string.Join( ", ", g.GetAttributeValues( "Domains" ) ) );
        }

        /// <summary>
        /// Rejects a request that is not a POST. State-changing block actions can
        /// otherwise be triggered by a cross-site GET link that carries the
        /// visitor's authentication cookie.
        /// </summary>
        /// <returns>A method-not-allowed result when the request is not a POST, otherwise <c>null</c>.</returns>
        private BlockActionResult RequirePostRequest()
        {
            return AcmeHelper.IsPostRequest()
                ? null
                : ActionStatusCode( System.Net.HttpStatusCode.MethodNotAllowed );
        }

        /// <summary>
        /// Rejects the request unless the current person has EDIT on this block.
        /// </summary>
        /// <returns>A forbidden result when the person lacks EDIT, otherwise <c>null</c>.</returns>
        private BlockActionResult RequireBlockEdit()
        {
            return BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson )
                ? null
                : ActionForbidden( "You are not authorized to make changes here." );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified certificate configuration.
        /// </summary>
        /// <param name="key">The identifier of the certificate to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var methodError = RequirePostRequest();
            if ( methodError != null )
            {
                return methodError;
            }

            var authError = RequireBlockEdit();
            if ( authError != null )
            {
                return authError;
            }

            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var group = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                // The key is client input, so confirm the group is actually a
                // certificate group before deleting it. Without this check a
                // caller could pass the id of any Rock group (a security role, a
                // family) and delete it.
                var acmeGroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.ACME_CERTIFICATES )?.Id;

                if ( group == null || group.GroupTypeId != acmeGroupTypeId )
                {
                    return ActionBadRequest( "Certificate not found." );
                }

                groupService.Delete( group );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion Block Actions
    }
}
