using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.blueboxmoon.AcmeCertificate.Jobs
{
    /// <summary>
    /// Job to automatically renew Acme SSL Certificates.
    /// </summary>
    [IntegerField( "Renewal Period", "The number of days before a certificate expires to begin attempting to renew it.", true, 30, order: 0 )]
    [DisallowConcurrentExecution]
    public class RenewAcmeCertificates : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// </summary>
        public RenewAcmeCertificates()
        {
        }

        /// <summary>
        /// Job to automatically renew Acme SSL Certificates.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            var account = AcmeHelper.LoadAccountData();
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int? renewalPeriod = dataMap.GetString( "RenewalPeriod" ).AsIntegerOrNull();
            int renewalCount = 0;
            int skipCount = 0;
            var errorMessages = new List<string>();

            if ( !renewalPeriod.HasValue || renewalPeriod.Value < 1 )
            {
                throw new Exception( "Invalid setting for RenewalPeriod." );
            }

            if ( account.OfflineMode )
            {
                throw new Exception( "Job cannot operate when certificate account is configured for Offline mode." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupTypeId = GroupTypeCache.Read( SystemGuid.GroupType.ACME_CERTIFICATES ).Id;
                    var limitDate = RockDateTime.Now.AddDays( renewalPeriod.Value );

                    var groups = new GroupService( rockContext ).Queryable()
                        .Where( g => g.GroupTypeId == groupTypeId )
                        .ToList();

                    foreach ( var group in groups )
                    {
                        group.LoadAttributes( rockContext );

                        var expireDate = group.GetAttributeValue( "Expires" ).AsDateTime();

                        if ( !expireDate.HasValue || expireDate.Value < limitDate )
                        {
                            try
                            {
                                var certificateData = AcmeHelper.RenewCertificate( group.Id, out string errorMessage );

                                if ( certificateData == null )
                                {
                                    errorMessages.Add( errorMessage );
                                }
                                else
                                {
                                    AcmeHelper.InstallCertificateData( certificateData );
                                    renewalCount += 1;
                                }
                            }
                            catch ( System.Exception ex )
                            {
                                errorMessages.Add( ex.Message );
                            }
                        }
                        else
                        {
                            skipCount += 1;
                        }
                    }
                }

                context.Result = string.Format( "{0} {1} were renewed, {2} {3} were not due for renewal.",
                    renewalCount, "certificate".PluralizeIf( renewalCount != 0 ),
                    skipCount, "certificate".PluralizeIf( skipCount != 0 ) );
            }
            catch ( System.Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                throw;
            }
        }
    }
}