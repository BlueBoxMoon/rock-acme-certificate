using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    /// <summary>
    /// Removes the legacy WebForms "Acme Challenge" page, block, and route. The
    /// <c>.well-known/acme-challenge/</c> requests are now served by the OWIN
    /// <see cref="AcmeCertificateMiddleware"/>, which intercepts them ahead of the
    /// Rock page pipeline, so the page/block/route are redundant.
    /// </summary>
    [MigrationNumber( 8, "19.4" )]
    public class RemoveLegacyChallengePage : ExtendedMigration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.ACME_CHALLENGE );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.ACME_CHALLENGE );

            // Deleting the page also removes its blocks and the .well-known route.
            RockMigrationHelper.DeletePage( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE );
        }

        public override void Down()
        {
            RockMigrationHelper.AddBlockType( "Acme Challenge",
                "Responds to challenges for the Acme certification system.",
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeChallenge.ascx",
                "Blue Box Moon > Acme Certificate",
                SystemGuid.BlockType.ACME_CHALLENGE );

            RockMigrationHelper.AddPage( SystemGuid.Page.ACME_CERTIFICATES,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB",
                "Acme Challenge",
                string.Empty,
                SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE, ".well-known/acme-challenge/{Token}" );

            RockMigrationHelper.AddBlock( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE,
                string.Empty, SystemGuid.BlockType.ACME_CHALLENGE,
                "Acme Challenge", "Main", string.Empty, string.Empty, 0, SystemGuid.Block.ACME_CHALLENGE );
        }
    }
}
