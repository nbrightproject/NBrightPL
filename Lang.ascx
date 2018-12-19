<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightPL.Lang" AutoEventWireup="false" ViewStateMode="Disabled" Codebehind="Lang.ascx.cs" %>
<asp:Literal ID="litCommonHeaderTemplate" runat="server" />
<asp:DropDownList ID="selectCulture" runat="server" AutoPostBack="true" CssClass="NormalTextBox" ViewStateMode="Enabled"/>
<asp:Repeater ID="rptLanguages" runat="server">
    <ItemTemplate><asp:Literal ID="litItemTemplate" runat="server" /></ItemTemplate>
    <AlternatingItemTemplate><asp:Literal ID="litItemTemplate" runat="server" /></AlternatingItemTemplate>
    <HeaderTemplate><asp:Literal ID="litItemTemplate" runat="server" /></HeaderTemplate>
    <SeparatorTemplate><asp:Literal ID="litItemTemplate" runat="server" /></SeparatorTemplate>
    <FooterTemplate><asp:Literal ID="litItemTemplate" runat="server" /></FooterTemplate>
</asp:Repeater>
<asp:Literal ID="litCommonFooterTemplate" runat="server"/>