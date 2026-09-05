using System;

using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 7, "19.4" )]
    public class ConvertBlocksToObsidian : ExtendedMigration
    {
        public override void Up()
        {
            // Acme Config block conversion.
            RockMigrationHelper.AddOrUpdateEntityType(
                "com.blueboxmoon.AcmeCertificate.Blocks.AcmeConfig",
                SystemGuid.EntityType.ACME_CONFIG,
                false,
                false );

            ConvertWebFormsBlockToObsidian(
                new Guid( SystemGuid.BlockType.ACME_CONFIG ),
                new Guid( SystemGuid.BlockType.ACME_CONFIG_OBSIDIAN ),
                new Guid( SystemGuid.EntityType.ACME_CONFIG ) );

            // Acme Certificates list block conversion.
            RockMigrationHelper.AddOrUpdateEntityType(
                "com.blueboxmoon.AcmeCertificate.Blocks.AcmeCertificateList",
                SystemGuid.EntityType.ACME_CERTIFICATE_LIST,
                false,
                false );

            ConvertWebFormsBlockToObsidian(
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATES ),
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_LIST_OBSIDIAN ),
                new Guid( SystemGuid.EntityType.ACME_CERTIFICATE_LIST ) );

            // Acme Certificate Detail block conversion.
            RockMigrationHelper.AddOrUpdateEntityType(
                "com.blueboxmoon.AcmeCertificate.Blocks.AcmeCertificateDetail",
                SystemGuid.EntityType.ACME_CERTIFICATE_DETAIL,
                false,
                false );

            ConvertWebFormsBlockToObsidian(
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL ),
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL_OBSIDIAN ),
                new Guid( SystemGuid.EntityType.ACME_CERTIFICATE_DETAIL ) );
        }

        public override void Down()
        {
            ConvertObsidianBlockToWebForms(
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL_OBSIDIAN ),
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL ),
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeCertificateDetail.ascx" );

            ConvertObsidianBlockToWebForms(
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATE_LIST_OBSIDIAN ),
                new Guid( SystemGuid.BlockType.ACME_CERTIFICATES ),
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeCertificates.ascx" );

            ConvertObsidianBlockToWebForms(
                new Guid( SystemGuid.BlockType.ACME_CONFIG_OBSIDIAN ),
                new Guid( SystemGuid.BlockType.ACME_CONFIG ),
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeConfig.ascx" );
        }
    }
}
