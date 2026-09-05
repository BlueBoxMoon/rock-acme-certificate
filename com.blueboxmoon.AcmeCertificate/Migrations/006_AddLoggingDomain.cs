using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 6, "1.11.0" )]
    public class AddLoggingDomain : ExtendedMigration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                AcmeHelper.LoggingDomain,
                "Blue Box Moon Acme Certificate",
                SystemGuid.DefinedValue.LOGGING_DOMAIN_ACME_CERTIFICATES );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.LOGGING_DOMAIN_ACME_CERTIFICATES );
        }
    }
}
