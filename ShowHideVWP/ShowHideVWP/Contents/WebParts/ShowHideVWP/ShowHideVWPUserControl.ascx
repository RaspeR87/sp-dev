<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShowHideVWPUserControl.ascx.cs" Inherits="ShowHideVWP.Contents.WebParts.ShowHideVWP.ShowHideVWPUserControl" %>

<link rel="stylesheet" href="_layouts/15/ShowHideVWP/style/jquery-ui-1.12.0.css" />

<script type="text/javascript" src="_layouts/15/ShowHideVWP/script/jquery-2.2.0.min.js"></script>
<script type="text/javascript" src="_layouts/15/ShowHideVWP/script/jquery-ui-1.12.0.min.js"></script>
<script type="text/javascript" src="_layouts/15/ShowHideVWP/script/showhidevwp.js?v=20170821"></script>

<div class="showhidevwp-content" style="display:none;">
    <asp:Label ID="lbEmpty1" runat="server" Text="Empty 1:"></asp:Label><br />
    <asp:TextBox ID="tbEmpty1" runat="server"></asp:TextBox><br />
    <asp:Label ID="lbEmpty2" runat="server" Text="Empty 2:"></asp:Label><br />
    <asp:TextBox ID="tbEmpty2" runat="server"></asp:TextBox><br />
    <asp:Button ID="btnEmpty1" runat="server" Text="Submit" />&nbsp;&nbsp;<asp:Button ID="btnEmpty2" runat="server" Text="Clear" />
</div>