using System.Collections.Generic;

using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 5, "1.11.0" )]
    public class EnableGlobalRoute : ExtendedMigration
    {
        public override void Up()
        {
            Sql( @"UPDATE PR
SET PR.[IsGlobal] = 1
FROM [PageRoute] AS PR
INNER JOIN [Page] AS P ON P.[Id] = PR.[PageId]
WHERE P.[Guid] = @PageGuid AND PR.[Route] = @Route",
                new Dictionary<string, object>
                {
                    ["PageGuid"] = SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE,
                    ["Route"] = ".well-known/acme-challenge/{Token}"
                } );
        }

        public override void Down()
        {
            Sql( @"UPDATE PR
SET PR.[IsGlobal] = 0
FROM [PageRoute] AS PR
INNER JOIN [Page] AS P ON P.[Id] = PR.[PageId]
WHERE P.[Guid] = @PageGuid AND PR.[Route] = @Route",
                new Dictionary<string, object>
                {
                    ["PageGuid"] = SystemGuid.Page.ACME_CERTIFICATE_CHALLENGE,
                    ["Route"] = ".well-known/acme-challenge/{Token}"
                } );
        }
    }
}
