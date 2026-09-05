/**
 * Browser bus message published by the Acme Config block whenever the account
 * registration state changes, so sibling blocks (e.g. the certificate list) can
 * react without a page refresh.
 */
export const ACCOUNT_CHANGED_MESSAGE = "com.blueboxmoon.AcmeCertificate.accountChanged";

/** The data published with the {@link ACCOUNT_CHANGED_MESSAGE}. */
export type AccountChangedData = {
    /** Whether an Acme account is currently registered. */
    isRegistered: boolean;
};
