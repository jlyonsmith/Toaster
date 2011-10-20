<?xml version="1.0" encoding="unicodeFFFE" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" >
  <xsl:output method="text" encoding="utf-16"/>

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="/TestResult">
    <xsl:text>Total </xsl:text>
    <xsl:value-of select="@TestCount"/>
    <xsl:text>, Passed </xsl:text>
    <xsl:value-of select="@PassedTests"/>
    <xsl:text>, Failed </xsl:text>
    <xsl:value-of select="@FailedTests"/>
    <xsl:text>, Skipped </xsl:text>
    <xsl:value-of select="@SkippedTests"/>
    <xsl:apply-templates select="/TestResult/TestAssembly"/>
    <xsl:text disable-output-escaping='yes'>&#xA;&#xA;</xsl:text>
    <xsl:if test="/TestResult/TestAssembly//TestMethod[@Outcome='Failed']">
      <xsl:text>Failures:</xsl:text><xsl:text disable-output-escaping='yes'>&#xA;</xsl:text>
      <xsl:text disable-output-escaping="yes">&#xA;</xsl:text>
    </xsl:if>
    <xsl:apply-templates select="/TestResult/TestAssembly//TestMethod[@Outcome='Failed']"/>
  </xsl:template>

  <xsl:template match="TestAssembly">
    <xsl:text>, Execution Time </xsl:text>
    <xsl:value-of select="@ExecutionTime"/>
  </xsl:template>
    
  <xsl:template match="TestMethod">
    <xsl:value-of select="position()"/><xsl:text>) </xsl:text>
    <xsl:value-of select="@Name"/>
    <xsl:text> : </xsl:text>
    <xsl:value-of disable-output-escaping="yes" select="child::node()/Message"/>
    <xsl:text disable-output-escaping='yes'>&#xA;</xsl:text>
    <xsl:apply-templates select="Failure" />
  </xsl:template>

  <xsl:template match="Failure">
    <xsl:for-each select="child::node()/StackFrame">
      <xsl:sort select="position()" order="ascending" data-type="number" />
      <xsl:text>&#x20;&#x20;</xsl:text>
      <xsl:choose>
        <xsl:when test="@FileName = ''">
          <xsl:text></xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@FileName" /><xsl:text>(</xsl:text><xsl:value-of select="@Line" /><xsl:text>):&#x20;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:value-of disable-output-escaping="yes" select="@MethodName" /><xsl:text disable-output-escaping="yes">&#xA;</xsl:text>
    </xsl:for-each>
    <xsl:text disable-output-escaping="yes">&#xA;</xsl:text>
  </xsl:template>

</xsl:stylesheet>

  