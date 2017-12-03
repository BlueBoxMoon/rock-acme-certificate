using Rock;
using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 2, "1.6.3" )]
    public class SetPageSecurity : ExtendedMigration
    {
        public override void Up()
        {
            RockMigrationHelper.AddSecurityAuthForPage( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE,
                0, Rock.Security.Authorization.VIEW, true,
                string.Empty, Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                System.Guid.NewGuid().ToString() );
        }

        public override void Down()
        {
        }
    }
}
