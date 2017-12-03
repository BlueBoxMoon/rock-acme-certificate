<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AcmeCertificateDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.AcmeCertificate.AcmeCertificateDetail" %>

<style>
    .domain-list .value-list-rows > .form-control-group {
        margin-bottom: 7.5px;
    }
</style>
<asp:UpdatePanel ID="upCertificate" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlCertificate" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> <asp:Literal ID="ltTitle" runat="server" /></h3>
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

                <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
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
    </ContentTemplate>
</asp:UpdatePanel>