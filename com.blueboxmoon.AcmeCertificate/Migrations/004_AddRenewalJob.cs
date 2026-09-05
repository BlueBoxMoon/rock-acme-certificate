using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    [MigrationNumber( 4, "1.6.4" )]
    public class AddRenewalJob : ExtendedMigration
    {
        public override void Up()
        {
            //
            // Create Job 'Renew Certificates'.
            //
            Sql( @"
    INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,1
        ,'Renew Certificates'
        ,'Performs automated SSL certificate renewal.'
        ,'com.blueboxmoon.AcmeCertificate.Jobs.RenewAcmeCertificates'
        ,'0 20 1 1/1 * ? *'
        ,1
        ,@Guid
        )",
                "Guid", SystemGuid.ServiceJob.RENEW_CERTIFICATES );
        }

        public override void Down()
        {
            //
            // Delete Job 'Renew Certificates'.
            //
            Sql( "DELETE FROM [ServiceJob] WHERE [Guid] = @Guid",
                "Guid", SystemGuid.ServiceJob.RENEW_CERTIFICATES );
        }
    }
}
