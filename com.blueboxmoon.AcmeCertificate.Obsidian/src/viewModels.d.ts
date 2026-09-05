// This code was auto-generated, any manual changes made will be lost.

export type AcmeCertificateDetailBag = {
    bindings?: BindingBag[] | null;

    domains?: string[] | null;

    expires?: string | null;

    idKey?: string | null;

    iisErrorMessage?: string | null;

    isNew: boolean;

    lastRenewed?: string | null;

    name?: string | null;

    offlineMode: boolean;

    parentPageUrl?: string | null;

    removeOldCertificate: boolean;

    showRedirectModuleWarning: boolean;

    showSiteRedirectWarning: boolean;

    siteRedirectNames?: string[] | null;

    targetRedirectUrl?: string | null;
};

export type AcmeCertificateEditDataBag = {
    bindings?: BindingBag[] | null;

    domains?: string[] | null;
};

export type AcmeCertificateListOptionsBag = {
    isAccountRegistered: boolean;
};

export type AcmeCertificateSaveBag = {
    bindings?: BindingBag[] | null;

    domains?: string[] | null;

    name?: string | null;

    removeOldCertificate: boolean;
};

export type AcmeConfigBag = {
    email?: string | null;

    isRegistered: boolean;

    offlineMode: boolean;

    testMode: boolean;
};

export type AcmeConfigRegistrationBag = {
    email?: string | null;

    hasExistingAccount: boolean;

    termsOfServiceUrl?: string | null;

    testMode: boolean;
};

export type BindingBag = {
    domain?: string | null;

    ipAddress?: string | null;

    port: number;

    site?: string | null;
};

export type BindingOptionsBag = {
    ipAddresses?: string[] | null;

    sites?: string[] | null;
};

export type CertificateDataBag = {
    certificates?: string[] | null;

    hash?: string | null;

    id: number;

    privateKey?: string | null;
};
