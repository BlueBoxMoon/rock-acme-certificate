using Rock;
using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 3, "1.6.3" )]
    public class AddCertExpiresAttribute : ExtendedMigration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.DATE_TIME, "Expires", string.Empty,
                5, string.Empty, SystemGuid.Attribute.CERTIFICATE_EXPIRES );
        }

        public override void Down()
        {
        }
    }
}
