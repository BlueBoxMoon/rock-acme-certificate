﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AcmeCertificateDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.AcmeCertificate.AcmeCertificateDetail" %>

<style>
    .domain-list .value-list-rows > .form-control-group {
        margin-bottom: 7.5px;
    }
</style>
<asp:UpdatePanel ID="upCertificate" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetail" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> <asp:Literal Id="ltDetailTitle" runat="server" /></h3>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="ltRemoveOld" runat="server" Label="Remove Old Certificate" />
                    </div>

                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="ltDetailDomains" runat="server" Label="Domains" />
                        <Rock:RockLiteral ID="ltDetailBindings" runat="server" Label="Bindings" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockLiteral ID="ltDetailLastRenewed" runat="server" Label="Last Renewed" />
                        <Rock:RockLiteral ID="ltDetailExpires" runat="server" Label="Expires On" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbDetailEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbDetailEdit_Click" />
                    <asp:LinkButton ID="lbDetailRenew" runat="server" CssClass="btn btn-success margin-l-sm" Text="Renew" OnClick="lbDetailRenew_Click" />
                    <asp:LinkButton ID="lbDetailCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbDetailCancel_Click" />
                    <asp:LinkButton ID="lbDetailDelete" runat="server" CssClass="btn btn-link" Text="Delete" OnClick="lbDetailDelete_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> <asp:Literal ID="ltEditTitle" runat="server" /></h3>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vSummary" runat="server" CssClass="alert alert-warning" />
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbFriendlyName" runat="server" Label="Friendly Name" Help="A friendly name that you will recognize when working with this certificate." Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbRemoveOldCertificate" runat="server" Label="Remove Old Certificate" Help="After renewing the certificate, should the old certificate be removed from the certificate store?" />
                    </div>
                </div>

                <Rock:ValueList ID="vlDomains" runat="server" Label="Domains" FormGroupCssClass="domain-list" Help="Enter all the domains you want to be associated with this certificate." />

                <Rock:RockControlWrapper ID="rcwDomains" runat="server" Label="Bindings" Help="Enter all the IIS bindings that you want to be updated to match this certificate.">
                    <Rock:Grid ID="gBindings" runat="server" CssClass="margin-b-md" RowItemText="Binding" AllowPaging="false" OnGridRebind="gBindings_GridRebind" OnRowSelected="gBindings_RowSelected">
                        <Columns>
                            <Rock:RockBoundField HeaderText="Site" DataField="Site" />
                            <Rock:RockBoundField HeaderText="IP Address" DataField="IPAddress" />
                            <Rock:RockBoundField HeaderText="Port" DataField="Port" />
                            <Rock:RockBoundField HeaderText="Domain" DataField="Domain" />
                            <Rock:DeleteField OnClick="gBindings_Delete" />
                        </Columns>
                    </Rock:Grid>
                </Rock:RockControlWrapper>

                <asp:LinkButton ID="lbEditSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbEditSave_Click" />
                <asp:LinkButton ID="lbEditCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbEditCancel_Click" CausesValidation="false" />
            </div>

            <Rock:ModalDialog ID="mdEditBinding" runat="server" ValidationGroup="EditBinding" OnSaveClick="mdEditBinding_SaveClick">
                <Content>
                    <asp:ValidationSummary ID="vEditBindingSummary" runat="server" CssClass="alert alert-warning" ValidationGroup="EditBinding" />

                    <asp:HiddenField ID="hfEditBindingIndex" runat="server" />

                    <Rock:RockDropDownList ID="ddlEditBindingSite" runat="server" Label="Site" Required="true" ValidationGroup="EditBinding" />

                    <Rock:RockDropDownList ID="ddlEditBindingIPAddress" runat="server" Label="IP Address" Help="If you select an IP address here then only this address will match this binding." ValidationGroup="EditBinding" />

                    <Rock:NumberBox ID="nbEditBindingPort" runat="server" Label="Port" ValidationGroup="EditBinding" MinimumValue="0" MaximumValue="65535" NumberType="Integer" />

                    <Rock:RockTextBox ID="tbEditBindingDomain" runat="server" Label="Domain" Help="If you enter a value here then only this domain name will match this binding." ValidationGroup="EditBinding" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:Panel ID="pnlRenew" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> Renew <asp:Literal ID="ltRenewTitle" runat="server" /></h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlRenewInput" runat="server">
                    <Rock:NotificationBox ID="nbRenewError" runat="server" NotificationBoxType="Danger" />

                    <Rock:RockCheckBox ID="cbRenewCustomCSR" runat="server" Label="Custom CSR" Help="If you have a CSR already that you want to use you can provide it. Leave this off to have one automatically generated." OnCheckedChanged="cbRenewCustomCSR_CheckedChanged" AutoPostBack="true" CausesValidation="false" />

                    <Rock:NotificationBox ID="nbRenewNotOffline" runat="server" NotificationBoxType="Warning" Visible="false">
                        In order to use a custom CSR you must be configured for Offline mode.
                    </Rock:NotificationBox>

                    <Rock:RockTextBox ID="tbRenewCSR" runat="server" TextMode="MultiLine" Rows="6" Label="CSR" Visible="false" />

                    <div class="actions">
                        <asp:LinkButton ID="lbRequestCertificate" runat="server" CssClass="btn btn-primary" Text="Request Certificate" OnClick="lbRequestCertificate_Click" />
                        <asp:LinkButton ID="lbRenewCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbRenewCancel_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlRenewOutput" runat="server">
                    <div class="alert alert-info">
                        You are operating in offline mode so your existing certificate and IIS settings have not been changed.
                        You can download your certificate below.
                    </div>

                    <Rock:RockRadioButtonList ID="rblRenewDownloadType" runat="server" Label="Certificate Format" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblRenewDownloadType_SelectedIndexChanged" AutoPostBack="true" />

                    <asp:Panel ID="pnlRenewOutputPEM" runat="server">
                        <code><asp:Literal ID="ltRenewPEM" runat="server" /></code>
                    </asp:Panel>

                    <asp:Panel ID="pnlRenewOutputP12" runat="server">
                        <asp:Literal ID="ltRenewP12" runat="server" />
                    </asp:Panel>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbRenewDone" runat="server" CssClass="btn btn-primary" Text="Done" OnClick="lbRenewDone_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlRenewSuccess" runat="server">
                    <div class="alert alert-success">
                        Certificate was renewed.
                    </div>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbRenewSuccessDone" runat="server" CssClass="btn btn-primary" Text="Done" OnClick="lbRenewDone_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>