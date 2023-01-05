<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema"
xmlns:d="http://schemas.microsoft.com/sharepoint/dsp"
version="1.0"
exclude-result-prefixes="xsl msxsl ddwrt"
xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime"
xmlns:asp="http://schemas.microsoft.com/ASPNET/20"
xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:SharePoint="Microsoft.SharePoint.WebControls"
xmlns:ddwrt2="urn:frontpage:internal">

  <xsl:template match="FieldRef[@FieldType='XnetSPMJU_FTFileUploader']" mode="URL_body" >
    <xsl:param name="thisNode" select="."></xsl:param>
    <xsl:variable name="propWindow" select="./@*[name()=current()/@NewWindow]"></xsl:variable>
    <xsl:variable name="url" select="substring-before($thisNode/@*[name()=current()/@Name],',')"></xsl:variable>
    <xsl:variable name="desc" select="$thisNode/@*[name()=concat(current()/@Name, '.desc')]"></xsl:variable>
    <xsl:choose>
      <xsl:when test="$propWindow='No'">
        <a href="{$url}" target="_self">
          <xsl:value-of select="$desc"></xsl:value-of>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <a href="{$url}" target="_blank">
          <xsl:value-of select="$desc"></xsl:value-of>
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>

