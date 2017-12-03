<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AcmeCertificates.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.AcmeCertificate.AcmeCertificates" %>

<asp:UpdatePanel ID="upCertificates" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbIISError" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlIISRedirectModuleWarning" runat="server" CssClass="alert alert-warning" Visible="false">
            <div>
                You do not currently have the Http Redirect module installed in IIS. In order to generate certificates
                for non-Rock domains we need to enable this module.
            </div>

            <div class="margin-t-md">
                Please note that doing this may restart your web sites.
            </div>

            <div class="margin-t-md">
                <asp:LinkButton ID="lbEnableRedirectModule" runat="server" CssClass="btn btn-warning" Text="Enable Redirect Module" OnClick="lbEnableRedirectModule_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlIISRedirectSiteWarning" runat="server" Cssclass="alert alert-warning" Visible="false">
            <asp:HiddenField ID="hfEnableSiteRedirects" runat="server" />

            <div>
                The following sites need redirect rules setup to forward Acme Challenge requests to your Rock site. We
                do this by creating a <code>.well-known/acme-challenge</code> directory in each site and then redirecting
                requests to files in those directories to your Rock site at <code><asp:Literal ID="ltTargetRedirect" runat="server" /></code> for verification.
            </div>

            <div class="margin-t-md">
                <ul>
                    <asp:Literal ID="ltEnableSiteRedirects" runat="server" />
                </ul>
            </div>

            <div class="margin-t-md">
                <asp:LinkButton ID="lbEnableSiteRedirects" runat="server" CssClass="btn btn-warning" Text="Enable Site Redirects" OnClick="lbEnableSiteRedirects_Click" />
            </div>
        </asp:Panel>

        <Rock:NotificationBox ID="nbRenewStatus" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDownloadCertificate" runat="server" CssClass="margin-b-md" Visible="false">
            <asp:HiddenField ID="hfCertificate" runat="server" />

            <div class="alert alert-success">
                Certificate was renewed. Because you are operating in offline mode no changes have been made to IIS
                or the certificate store. Enter a password below to prepare the certificate for download and then
                you may manually configure IIS to use the certificate.
            </div>

            <Rock:RockTextBox ID="tbCertificatePassword" runat="server" Label="Password" TextMode="Password" ValidationGroup="DownloadCertificate" Required="true" />
            
            <div class="actions">
                <asp:LinkButton ID="lbPrepareCertificate" runat="server" CssClass="btn btn-primary" Text="Prepare Certificate" OnClick="lbPrepareCertificate_Click" ValidationGroup="DownloadCertificate" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCertificates" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> Certificates</h3>
            </div>

            <Rock:Grid ID="gCertificates" runat="server" OnGridRebind="gCertificates_GridRebind" OnRowSelected="gCertificates_RowSelected" DataKeyNames="Id" RowItemText="Certificate">
                <Columns>
                    <Rock:RockBoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                    <Rock:DateTimeField HeaderText="Last Renewed" DataField="LastRenewed" SortExpression="LastRenewed" HeaderStyle-HorizontalAlign="Right" />
                    <Rock:DateTimeField HeaderText="Expires" DataField="Expires" SortExpression="Expires" HeaderStyle-HorizontalAlign="Right" />
                    <Rock:RockBoundField HeaderText="Domains" DataField="Domains" HtmlEncode="false" />
                    <Rock:LinkButtonField HeaderText="Renew" HeaderStyle-HorizontalAlign="Center" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-rss'></i>" OnClick="gCertificates_Renew" />
                    <Rock:DeleteField OnClick="gCertificates_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>