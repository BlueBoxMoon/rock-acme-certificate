using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 1, "1.6.3" )]
    class SystemData : ExtendedMigration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Acme Certificates",
                "Contains information on Acme SSL Certificates that are installed and will be renewed.",
                "Certificate", "Member", false, false, false, "fa fa-certificate", 999,
                null, 0, null, SystemGuid.GroupType.ACME_CERTIFICATES );
            int groupTypeId = ( int ) SqlScalar( "SELECT [Id] FROM [GroupType] WHERE [Guid] = @Guid", "@Guid", SystemGuid.GroupType.ACME_CERTIFICATES );

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.GroupType",
                Rock.SystemGuid.FieldType.TEXT, "Id", groupTypeId.ToString(),
                "Account", string.Empty, string.Empty, 0,
                string.Empty, SystemGuid.Attribute.ACCOUNT );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.TEXT, "Domains", string.Empty,
                0, string.Empty, SystemGuid.Attribute.DOMAINS );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.DATE_TIME, "Last Renewed", string.Empty,
                1, string.Empty, SystemGuid.Attribute.LAST_RENEWED );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.TEXT, "Certificate Hash", string.Empty,
                2, string.Empty, SystemGuid.Attribute.CERTIFICATE_HASH );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.TEXT, "Bindings", string.Empty,
                3, string.Empty, SystemGuid.Attribute.BINDINGS );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.TEXT, "Remove Old Certificate", string.Empty,
                4, string.Empty, SystemGuid.Attribute.REMOVE_OLD_CERTIFICATE );

            RockMigrationHelper.AddBlockType( "Acme Config",
                "Configures the Acme certification system.",
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeConfig.ascx",
                "Blue Box Moon > Acme Certificate",
                SystemGuid.BlockType.ACME_CONFIG );

            RockMigrationHelper.AddBlockType( "Acme Certificates",
                "Lists the certificate configuration.",
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeCertificates.ascx",
                "Blue Box Moon > Acme Certificate",
                SystemGuid.BlockType.ACME_CERTIFICATES );

            RockMigrationHelper.AddBlockType( "Acme Certificate Detail",
                "Configures a certificate.",
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeCertificateDetail.ascx",
                "Blue Box Moon > Acme Certificate",
                SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL );

            RockMigrationHelper.AddBlockType( "Acme Challenge",
                "Responds to challenges for the Acme certification system.",
                "~/Plugins/com_blueboxmoon/AcmeCertificate/AcmeChallenge.ascx",
                "Blue Box Moon > Acme Certificate",
                SystemGuid.BlockType.ACME_CHALLENGE );

            RockMigrationHelper.AddBlockTypeAttribute( SystemGuid.BlockType.ACME_CERTIFICATES,
                Rock.SystemGuid.FieldType.PAGE_REFERENCE,
                "Detail Page", "DetailPage", string.Empty, string.Empty,
                0, string.Empty, SystemGuid.Attribute.DETAIL_PAGE );

            RockMigrationHelper.AddPage( SystemGuid.Page.ROCK_INSTALLED_PLUGINS,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB",
                "Acme Certificates",
                string.Empty,
                SystemGuid.Page.ACME_CERTIFICATES,
                "fa fa-certificate" );

            RockMigrationHelper.AddPage( SystemGuid.Page.ACME_CERTIFICATES,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB",
                "Acme Certificate Detail",
                string.Empty,
                SystemGuid.Page.ACME_CERTIFICATE_DETAIL,
                "fa fa-certificate" );

            RockMigrationHelper.AddPage( SystemGuid.Page.ACME_CERTIFICATES,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB",
                "Acme Challenge",
                string.Empty,
                SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE, ".well-known/acme-challenge/{Token}" );

            RockMigrationHelper.AddBlock( SystemGuid.Page.ACME_CERTIFICATES,
                string.Empty, SystemGuid.BlockType.ACME_CONFIG,
                "Acme Config", "Main", string.Empty, string.Empty, 0, SystemGuid.Block.ACME_CONFIG );

            RockMigrationHelper.AddBlock( SystemGuid.Page.ACME_CERTIFICATES,
                string.Empty, SystemGuid.BlockType.ACME_CERTIFICATES,
                "Acme Certificates", "Main", string.Empty, string.Empty, 1, SystemGuid.Block.ACME_CERTIFICATES );

            RockMigrationHelper.AddBlock( SystemGuid.Page.ACME_CERTIFICATE_DETAIL,
                string.Empty, SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL,
                "Acme Certificate Details", "Main", string.Empty, string.Empty, 0, SystemGuid.Block.ACME_CERTIFICATE_DETAIL );

            RockMigrationHelper.AddBlock( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE,
                string.Empty, SystemGuid.BlockType.ACME_CHALLENGE,
                "Acme Challenge", "Main", string.Empty, string.Empty, 0, SystemGuid.Block.ACME_CHALLENGE );

            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.ACME_CERTIFICATES,
                SystemGuid.Attribute.DETAIL_PAGE, SystemGuid.Page.ACME_CERTIFICATE_DETAIL );
        }

        public override void Down()
        {
            RockMigrationHelper.DeletePage( SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE );
            RockMigrationHelper.DeletePage( SystemGuid.Page.ACME_CERTIFICATE_DETAIL );
            RockMigrationHelper.DeletePage( SystemGuid.Page.ACME_CERTIFICATES );

            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.ACME_CHALLENGE );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.ACME_CERTIFICATES );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.ACME_CONFIG );

            var guid = SqlScalar( "SELECT [Guid] FROM [Group] AS [G] LEFT JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId] WHERE [GT].[Guid] = @Guid", "@Guid", SystemGuid.GroupType.ACME_CERTIFICATES );
            while ( guid != null )
            {
                RockMigrationHelper.DeleteGroup( guid.ToString() );
                guid = SqlScalar( "SELECT [Guid] FROM [Group] AS [G] LEFT JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId] WHERE [GT].[Guid] = @Guid", "@Guid", SystemGuid.GroupType.ACME_CERTIFICATES );
            }

            RockMigrationHelper.DeleteGroupType( SystemGuid.GroupType.ACME_CERTIFICATES );
        }
    }
}
