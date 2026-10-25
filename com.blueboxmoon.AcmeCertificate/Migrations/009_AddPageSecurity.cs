using System.Collections.Generic;

using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    /// <summary>
    /// Locks the Acme Certificates page down to Rock Administrators. Before this the
    /// page shipped with no security of its own, so it inherited VIEW from the Admin
    /// Tools page, which also grants Communication and Web Administrators access. The
    /// block actions now enforce their own EDIT or ADMINISTRATE checks, and this
    /// migration makes the page itself admin-only so lower privileged staff cannot
    /// view the certificates, the ACME account, or the IIS bindings. The child Acme
    /// Certificate Detail page inherits these rules through its parent page, and every
    /// block on both pages inherits them through its page.
    /// </summary>
    [MigrationNumber( 9, "19.4" )]
    public class AddPageSecurity : ExtendedMigration
    {
        /// <summary>
        /// Auth record guids this migration owns, so Up() and Down() only ever manage
        /// their own rows and never touch rules an administrator added.
        /// </summary>
        private const string VIEW_ALLOW = "CAFC0CA2-3E73-42E1-8C4A-5AA45C392245";
        private const string VIEW_DENY = "06DD3071-1F8A-4A38-9F7C-B1033E23260D";
        private const string EDIT_ALLOW = "0374B298-A90C-4201-BDF1-92E4C7D5ADB9";
        private const string EDIT_DENY = "836DEB3D-6F45-4CF3-9A53-8F54DD00CFDD";
        private const string ADMIN_ALLOW = "E738030D-09B3-4482-A116-78644A752D34";
        private const string ADMIN_DENY = "B45DE479-5F31-42B9-8177-B4B5D987F37C";

        public override void Up()
        {
            AddDefaultPageSecurity( Rock.Security.Authorization.VIEW, VIEW_ALLOW, VIEW_DENY );
            AddDefaultPageSecurity( Rock.Security.Authorization.EDIT, EDIT_ALLOW, EDIT_DENY );
            AddDefaultPageSecurity( Rock.Security.Authorization.ADMINISTRATE, ADMIN_ALLOW, ADMIN_DENY );
        }

        public override void Down()
        {
            // Remove only the rows this migration added.
            RockMigrationHelper.DeleteSecurityAuth( VIEW_ALLOW );
            RockMigrationHelper.DeleteSecurityAuth( VIEW_DENY );
            RockMigrationHelper.DeleteSecurityAuth( EDIT_ALLOW );
            RockMigrationHelper.DeleteSecurityAuth( EDIT_DENY );
            RockMigrationHelper.DeleteSecurityAuth( ADMIN_ALLOW );
            RockMigrationHelper.DeleteSecurityAuth( ADMIN_DENY );
        }

        /// <summary>
        /// Adds the default "Rock Administrators only" rules for one action, but only
        /// when the page has no explicit rule for that action yet. If an administrator
        /// already configured this action on the page, it is left exactly as-is.
        /// </summary>
        /// <param name="action">The security action (VIEW, EDIT, or ADMINISTRATE).</param>
        /// <param name="allowGuid">The guid to assign to the Rock Administration allow rule.</param>
        /// <param name="denyGuid">The guid to assign to the All Users deny rule.</param>
        private void AddDefaultPageSecurity( string action, string allowGuid, string denyGuid )
        {
            var existingCount = SqlScalar( @"
SELECT COUNT(*)
FROM [Auth] AS [a]
INNER JOIN [EntityType] AS [et] ON [et].[Id] = [a].[EntityTypeId] AND [et].[Name] = 'Rock.Model.Page'
INNER JOIN [Page] AS [p] ON [p].[Id] = [a].[EntityId] AND [p].[Guid] = @PageGuid
WHERE [a].[Action] = @Action",
                new Dictionary<string, object>
                {
                    ["PageGuid"] = SystemGuid.Page.ACME_CERTIFICATES,
                    ["Action"] = action
                } );

            if ( existingCount != null && ( int ) existingCount > 0 )
            {
                return;
            }

            // First matching rule wins, ordered by [Order], and "All Users" matches
            // everyone, so the Rock Administration allow is at Order 0 and the All
            // Users deny is at Order 1.
            RockMigrationHelper.AddSecurityAuthForPage( SystemGuid.Page.ACME_CERTIFICATES,
                0, action, true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None, allowGuid );

            RockMigrationHelper.AddSecurityAuthForPage( SystemGuid.Page.ACME_CERTIFICATES,
                1, action, false, null,
                ( int ) Rock.Model.SpecialRole.AllUsers, denyGuid );
        }
    }
}
