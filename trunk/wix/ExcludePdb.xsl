<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
            xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
            xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
            exclude-result-prefixes="msxsl" 
            xmlns:wix="http://schemas.microsoft.com/wix/2006/wix"
            xmlns:my="my:my">

  <xsl:output method="xml" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>