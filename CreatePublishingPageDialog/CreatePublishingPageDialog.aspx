<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@ Assembly Name="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%> 
<%@ Page Language="C#" DynamicMasterPageFile="~masterurl/default.master" CodeBehind="CreatePublishingPageDialog.aspx.cs" Inherits="Xnet.SP.Test.Layouts.Xnet.SP.Test.CreatePublishingPageDialog"       %> 
<%@ Import Namespace="Microsoft.SharePoint.WebControls" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Import Namespace="Microsoft.SharePoint" %> <%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="cms" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="LinksTable" src="/_controltemplates/15/LinksTable.ascx" %> <%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="/_controltemplates/15/InputFormSection.ascx" %> <%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="/_controltemplates/15/InputFormControl.ascx" %> <%@ Register TagPrefix="wssuc" TagName="LinkSection" src="/_controltemplates/15/LinkSection.ascx" %> <%@ Register TagPrefix="wssuc" TagName="ButtonSection" src="/_controltemplates/15/ButtonSection.ascx" %> <%@ Register TagPrefix="wssuc" TagName="ActionBar" src="/_controltemplates/15/ActionBar.ascx" %> <%@ Register TagPrefix="wssuc" TagName="ToolBar" src="/_controltemplates/15/ToolBar.ascx" %> <%@ Register TagPrefix="wssuc" TagName="ToolBarButton" src="/_controltemplates/15/ToolBarButton.ascx" %> <%@ Register TagPrefix="wssuc" TagName="Welcome" src="/_controltemplates/15/Welcome.ascx" %>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<SharePoint:EncodedLiteral runat="server" text="<%$Resources:wss,webpagecreation_new_pagetitle%>" EncodeMethod='HtmlEncode'/>
</asp:Content>
<asp:content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
	<SharePoint:StyleBlock runat="server">
		.infoTextComments{
			color: #717171;
			font-style:italic;
		}
	</SharePoint:StyleBlock>
<SharePoint:ScriptLink name="SP.Taxonomy.js" loadafterui="true" defer="true" localizable="false" runat="server"/>
<SharePoint:ScriptLink name="SP.Publishing.js" loadafterui="true" defer="true" localizable="false" runat="server"/>
<SharePoint:ScriptBlock runat="server">
function    Visascii(ch)
{
	return (!(ch.charCodeAt(0) & 0x80));
}
function Visspace(ch)
{
	return (ch.charCodeAt(0) == 32) || ((9 <= ch.charCodeAt(0)) && (ch.charCodeAt(0) <= 13));
}
function stripWS(str)
{
	var b = 0;
	var e = str.length;
	while (str.charAt(b) && (Visascii(str.charAt(b)) && Visspace(str.charAt(b))))
		b++;
	while ((b < e) && (Visascii(str.charAt(e-1)) && Visspace(str.charAt(e-1))))
		e = e-1;
	return ((b>=e)?"":str.substring(b, e ));
}
var L_NoFieldEmpty_TEXT = "<SharePoint:EncodedLiteral runat='server' text='<%$Resources:wss,common_nofieldempty_TEXT%>' EncodeMethod='EcmaScriptStringLiteralEncode'/>";
function CheckForEmptyField(text_orig,field_name)
{
	var text = stripWS(text_orig);
	if (text.length == 0)
	{
		alert(StBuildParam(L_NoFieldEmpty_TEXT, field_name));
		return false;
	}
	return (true);
}
function CheckForEmptyFieldNoAlert(text_orig)
{
	var text = stripWS(text_orig);
	if (text.length == 0)
	{
		return false;
	}
	return (true);
}
var L_WrongEmailName_TEXT = "<SharePoint:EncodedLiteral runat='server' text='<%$Resources:wss,common_wrongemailname_TEXT%>' EncodeMethod='EcmaScriptStringLiteralEncode'/>";
function CheckForAtSighInEmailName(text_orig,field_name)
{
	var text = stripWS(text_orig);
	if (!CheckForEmptyField(text_orig,field_name)) return false;
	var indexAt = 0;
	var countAt = 0;
	var countSpace = 0;
	var len = text.length;
	while(len)
	{
		len = len - 1;
		if (text.charAt(len) == '@')
		{
			indexAt = len;
			countAt++;
		}
		if (text.charAt(len) == ' ')
			countSpace ++;
	}
	if ((countAt == 0) ||
		(indexAt == 0) ||
		(indexAt == (text.length-1))
		)
	{
		alert(StBuildParam(L_WrongEmailName_TEXT, field_name));
		return false;
	}
	if (countSpace !=0 )
	{
		alert(L_TextWithoutSpaces1_TEXT + field_name);
		return false;
	}
	return (true);
}
	function _spBodyOnLoad()
	{
		UpdateUrl();
	}
	var labelHeight = 0;
	var labelWidth = 0;
	var g_lcid = 0;
	var g_isContextLoaded = false;
	var g_termStoreId = "";
	var g_termSetId = "";
	var g_parentTermId = "";
	var g_navProvider = "";
	var g_timerId = 0;
	function LoadTermContextInfo()
	{
		if( g_isContextLoaded == false )
		{
			var hiddenContextTermStoreIdClientId = "<%=hiddenContextTermStoreId.ClientID%>";
			var hiddenContextTermStoreId = document.getElementById(hiddenContextTermStoreIdClientId);
			var hiddenContextTermSetIdClientId = "<%=hiddenContextTermSetId.ClientID%>";
			var hiddenContextTermSetId = document.getElementById(hiddenContextTermSetIdClientId);
			var hiddenContextParentTermIdClientId = "<%=hiddenContextParentTermId.ClientID%>";
			var hiddenContextParentTermId = document.getElementById(hiddenContextParentTermIdClientId);
			var hiddenNavigationProviderClientId = "<%=hiddenNavigationProvider.ClientID%>";
			var hiddenNavigationProvider = document.getElementById(hiddenNavigationProviderClientId);
			var hiddenContextLcidClientId = "<%=hiddenContextLcid.ClientID%>";
			var hiddenContextLcid = document.getElementById(hiddenContextLcidClientId);
			g_termStoreId = GetInnerText(hiddenContextTermStoreId);
			g_termSetId = GetInnerText(hiddenContextTermSetId);
			g_parentTermId = GetInnerText(hiddenContextParentTermId);
			g_navProvider = GetInnerText(hiddenNavigationProvider);
			g_lcid = GetInnerText(hiddenContextLcid);
			g_isContextLoaded = true;
		}
	}
	function OnFriendlyUrlNameChanged()
	{
		g_timerId = 0;
		var nameInputTextBoxClientId = "<%=nameInput.ClientID%>";
		var nameInputTextBox = document.getElementById(nameInputTextBoxClientId);
		UpdateFriendlyUrlPreviewName(nameInputTextBox.value);
	}
	function UpdateFriendlyUrlPreviewName( suggestedFurlName )
	{
		if( suggestedFurlName != "" )
		{
			var clientContext = SP.ClientContext.get_current();
			var taxonomySession = SP.Taxonomy.TaxonomySession.getTaxonomySession(clientContext);
			var termStore = taxonomySession.get_termStores().getById(g_termStoreId);
			var termSet = termStore.getTermSet(g_termSetId);
			var parentTerm = null;
			if( g_parentTermId != "" )
			{
				parentTerm = termSet.getTerm(g_parentTermId);
			}
			var web = clientContext.get_web();
			var siteMapProviderName = g_navProvider;
			var parentNavTermSetItem = null;
			if( parentTerm != null )
			{
				parentNavTermSetItem = SP.Publishing.Navigation.NavigationTerm.getAsResolvedByWeb(clientContext, parentTerm, web, siteMapProviderName);
			}
			else
			{
				parentNavTermSetItem = SP.Publishing.Navigation.NavigationTermSet.getAsResolvedByWeb(clientContext, termSet, web, siteMapProviderName);
			}
			var tempNewTerm = parentNavTermSetItem.getAsEditable(taxonomySession).createTerm(suggestedFurlName, SP.Publishing.Navigation.NavigationLinkType.friendlyUrl, "35F06A4B-B988-4158-B9BD-A86A36E7A23A");
			clientContext.load(tempNewTerm);
			clientContext.load(tempNewTerm.get_friendlyUrlSegment());
			termStore.rollbackAll();
			clientContext.executeQueryAsync(
				function() {
					UpdatePreviewUrl(tempNewTerm.get_friendlyUrlSegment().get_value());
				}
			);
		}
		else
		{
			UpdatePreviewUrl(suggestedFurlName);
		}
	}
	function UpdateUrl()
	{
		LoadTermContextInfo();
		var hiddenPageUrlLabelExtensionClientId = "<%=hiddenPageUrlLabelExtension.ClientID%>";
		var hiddenPageUrlLabelExtension = document.getElementById(hiddenPageUrlLabelExtensionClientId);
		var pageNamePreviewUrlLabelClientId = "<%=pageNamePreviewUrlLabel.ClientID%>";
		var pageNamePreviewUrlLabel = document.getElementById(pageNamePreviewUrlLabelClientId);
		if( pageNamePreviewUrlLabel != null )
		{
			var nameInputTextBoxClientId = "<%=nameInput.ClientID%>";
			var nameInputTextBox = document.getElementById(nameInputTextBoxClientId);
			var allowSpaces = false;
			if( GetInnerText(hiddenPageUrlLabelExtension) != "" )
			{
				var suggestUrlValue = "";
				for (var i=0; i < nameInputTextBox.value.length; i++)
				{
					var currentChar = nameInputTextBox.value.charAt(i);
					if (IndexOfIllegalCharInUrlLeafName(currentChar) == -1 && !(currentChar == ' ' && allowSpaces == false) && currentChar != '.' && currentChar != '+')
					{
						suggestUrlValue += currentChar;
					}
					else if (currentChar == ' ' || currentChar == '+' || (currentChar == '.' && i > 0 && i < (nameInputTextBox.value.length - 1)))
					{
						if (Flighting.VariantConfiguration.IsExpFeatureClientEnabled(543))
						{
							suggestUrlValue += encodeURIComponent(currentChar);
						}
						else
						{
							suggestUrlValue += '-';
						}
					}
				}
				UpdatePreviewUrl( suggestUrlValue );
			}
			else
			{
				if( g_timerId != 0 )
				{
					window.clearTimeout(g_timerId);
				}
				g_timerId = window.setTimeout(OnFriendlyUrlNameChanged, 500);
			}
		}
	}
	function UpdatePreviewUrl( suggestUrlValue )
	{
		if( g_lcid != 0 && suggestUrlValue.length > 0)
		{
			var clientContext = SP.ClientContext.get_current();
			var result = SP.Utilities.Utility.getLowerCaseString(clientContext, suggestUrlValue, g_lcid);
			clientContext.executeQueryAsync(
	            function() {
		            UpdatePreviewUrlFinal(result.get_value());
	            }
			);
		}
		else
		{
			UpdatePreviewUrlFinal( suggestUrlValue );
		}
	}
	function UpdatePreviewUrlFinal( suggestUrlValue )
	{
		var pageNamePreviewUrlLabelClientId = "<%=pageNamePreviewUrlLabel.ClientID%>";
		var pageNamePreviewUrlLabel = document.getElementById(pageNamePreviewUrlLabelClientId);
		var pageUrlLabelClientId = "<%=pageUrlLabel.ClientID%>";
		var pageUrlLabel = document.getElementById(pageUrlLabelClientId);
		var hiddenPageUrlLabelExtensionClientId = "<%=hiddenPageUrlLabelExtension.ClientID%>";
		var hiddenPageUrlLabelExtension = document.getElementById(hiddenPageUrlLabelExtensionClientId);
		if (!UrlContainsIllegalStrings(suggestUrlValue))
		{
			setInnerText(pageNamePreviewUrlLabel, GetInnerText(pageUrlLabel) + "/" + suggestUrlValue.substring(0,123) + GetInnerText(hiddenPageUrlLabelExtension));
		}
		else if( suggestUrlValue.length == 0)
		{
			setInnerText(pageNamePreviewUrlLabel, GetInnerText(pageUrlLabel));
		}
		if(labelHeight != pageNamePreviewUrlLabel.offsetHeight || labelWidth != pageNamePreviewUrlLabel.offsetWidth)
		{
			labelHeight = pageNamePreviewUrlLabel.offsetHeight;
			labelWidth = pageNamePreviewUrlLabel.offsetWidth;
			var childDialog = window.top.g_childDialog;
			if(childDialog)
			{
				childDialog.autoSize();
			}
		}
	}
</SharePoint:ScriptBlock>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
	<asp:Panel id="mainPanel" runat="server">
		<div>
			<h3 class="ms-core-form-line">
			<asp:Label id="nameLabel" runat="server" AssociatedControlID="nameInput" />
			</h3>
		</div>
		<div class="ms-core-form-line">
			<asp:TextBox id="nameInput" class="ms-fullWidth" onkeyup="UpdateUrl()" runat="server" />
		</div>
        <div class="ms-core-form-line">
			<table border="0">
                <tr>
                    <td>
                        <span class="ms-metadata ms-floatLeft ms-displayInlineBlock ms-verticalAlignBaseline">
				            <SharePoint:EncodedLiteral runat="server" text="News Type" EncodeMethod='HtmlEncode'/>
				            &#160;
			            </span>
                    </td>
                    <td>
                        <asp:DropDownList ID="ddlNewsType" runat="server"></asp:DropDownList>
                    </td>
                </tr>
			</table>
		</div>
		<div class="ms-core-form-line ms-metadata ms-floatLeft ms-fullWidth">
			<span class="ms-metadata ms-floatLeft ms-displayInlineBlock ms-verticalAlignBaseline">
				<SharePoint:EncodedLiteral runat="server" text="<%$Resources:wss,objectUrlLocationLabel%>" EncodeMethod='HtmlEncode'/>
				&#160;
			</span>
			<asp:Label id="pageUrlLabel" style="display:none" runat="server" />
			<asp:Label id="pageNamePreviewUrlLabel" class="ms-floatLeft ms-displayInlineBlock ms-verticalAlignBaseline" runat="server" />
			<asp:Label id="hiddenPageUrlLabelExtension" style="display:none" runat="server" />
			<asp:Label id="hiddenContextTermStoreId" style="display:none" runat="server" />
			<asp:Label id="hiddenContextTermSetId" style="display:none" runat="server" />
			<asp:Label id="hiddenContextParentTermId" style="display:none" runat="server" />
			<asp:Label id="hiddenNavigationProvider" style="display:none" runat="server" />
			<asp:Label id="hiddenContextLcid" style="display:none" runat="server" />
		</div>
		<div class="ms-formvalidation">
			<wssawc:InputFormRequiredFieldValidator ControlToValidate="nameInput" MessagePrefix="<%$Resources:wss,page_name%>" Display="Dynamic" runat="server"/>
			<wssawc:UrlNameValidator id="urlNameValidator" ControlToValidate="nameInput" MessagePrefix="<%$Resources:wss,page_name%>" Display="Dynamic" runat="server"/>
			<cms:DocumentLibraryFileExistValidator id="fileExistValidator" ControlToValidate="nameInput" AppendFileExtension=".aspx" ErrorMessage="<%$Resources:cms,createpage_errormessage_pagename_already_exists%>"  ValidIfExists="false" Display="Dynamic" runat="server" />
		</div>
	</asp:Panel>
	<asp:Panel id="errorPanel" runat="server" Visible="false">
		<asp:Label id="errorMessageLabel" class="ms-error" runat="server" />
	</asp:Panel>
	<div class="ms-core-form-bottomButtonBox">
		<asp:button id="createButton"
			text="<%$Resources:wss,multipages_createbutton_text%>"
			accesskey="<%$Resources:wss,multipages_createbutton_accesskey%>"
			CssClass="ms-ButtonHeightWidth"
			onclick="CreateButton_Click" runat="server"/>
		<SharePoint:GoBackButton
			id="cancelButton"
			ControlMode="New"
			TemplateName="CancelCloseButtonNoTable"
			runat="server" />
	</div>
</asp:Content>
