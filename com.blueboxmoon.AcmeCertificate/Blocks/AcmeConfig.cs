using System;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Blocks;
using Rock.Model;

using com.blueboxmoon.AcmeCertificate.ViewModels;

namespace com.blueboxmoon.AcmeCertificate.Blocks
{
    /// <summary>
    /// Configures the Acme certification system account.
    /// </summary>
    [DisplayName( "Acme Config" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Configures the Acme certification system." )]
    [IconCssClass( "fa fa-user-secret" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.ACME_CONFIG )]
    [Rock.SystemGuid.BlockTypeGuid( SystemGuid.BlockType.ACME_CONFIG_OBSIDIAN )]
    public class AcmeConfig : RockBlockType
    {
        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => RequestContext.ResolveRockUrl( "~/Plugins/com_blueboxmoon/AcmeCertificate/acmeConfig.obs" );

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetConfigBag();
        }

        /// <summary>
        /// Builds the read-only account state for display.
        /// </summary>
        /// <returns>An <see cref="AcmeConfigBag"/> describing the current account.</returns>
        private AcmeConfigBag GetConfigBag()
        {
            var account = AcmeHelper.LoadAccountData();

            return new AcmeConfigBag
            {
                Email = account.Email,
                TestMode = account.TestMode,
                OfflineMode = account.OfflineMode,
                IsRegistered = !string.IsNullOrWhiteSpace( account.Key )
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the information needed to display the registration form.
        /// </summary>
        /// <returns>An <see cref="AcmeConfigRegistrationBag"/> including the terms of service URL.</returns>
        [BlockAction]
        public BlockActionResult PrepareRegistration()
        {
            var account = AcmeHelper.LoadAccountData();
            var acme = new AcmeService( false );

            return ActionOk( new AcmeConfigRegistrationBag
            {
                Email = account.Email,
                TestMode = account.TestMode,
                HasExistingAccount = !string.IsNullOrWhiteSpace( account.Email ),
                TermsOfServiceUrl = acme.TermsOfServiceUrl
            } );
        }

        /// <summary>
        /// Registers a new account with the certificate provider.
        /// </summary>
        /// <param name="email">The e-mail address to register.</param>
        /// <param name="testMode">Whether to use the testing server.</param>
        /// <param name="agreeToTerms">Whether the individual agreed to the terms of service.</param>
        /// <returns>The updated account state.</returns>
        [BlockAction]
        public BlockActionResult Register( string email, bool testMode, bool agreeToTerms )
        {
            if ( !agreeToTerms )
            {
                return ActionBadRequest( "You must read and agree to the terms of service." );
            }

            var acme = new AcmeService( testMode );
            var account = acme.Register( email );

            account.TestMode = testMode;
            account.OfflineMode = false;
            account.Key = Convert.ToBase64String( acme.RSA );

            AcmeHelper.SaveAccountData( account );

            return ActionOk( GetConfigBag() );
        }

        /// <summary>
        /// Saves changes to the existing account.
        /// </summary>
        /// <param name="offlineMode">Whether the account should operate in offline mode.</param>
        /// <returns>The updated account state.</returns>
        [BlockAction]
        public BlockActionResult SaveAccount( bool offlineMode )
        {
            var account = AcmeHelper.LoadAccountData();

            account.OfflineMode = offlineMode;

            AcmeHelper.SaveAccountData( account );

            return ActionOk( GetConfigBag() );
        }

        #endregion Block Actions
    }
}
