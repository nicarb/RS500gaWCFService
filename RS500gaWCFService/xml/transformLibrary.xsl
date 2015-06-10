<!--
* Copyright (c) 2007, Burnham H. Greeley
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of any organization nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY Burnham H. Greeley ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL Burnham H. Greeley BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" version="1.0" encoding="UTF-8" omit-xml-declaration="no" indent="yes" />

  <xsl:template match="/">
    <xsl:apply-templates select="document(/include/@file)/plist" />
  </xsl:template>

  <xsl:template match="plist">
    <library xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="iTunesLibrary.xsd">
    <xsl:text>&#10;  </xsl:text>
    <songlist>
      <xsl:apply-templates select="dict/dict/dict"/>
    <xsl:text>&#10;  </xsl:text>
    </songlist>

    <xsl:text>&#10;  </xsl:text>
    <playlists>
      <xsl:apply-templates select="dict/key[text()='Playlists']/following-sibling::array" />
    <xsl:text>&#10;  </xsl:text>
    </playlists>
    <xsl:text>&#10;</xsl:text>
    </library>
  </xsl:template>

  <xsl:template match="dict">
    <xsl:text>&#10;    </xsl:text>
    <song>
      <xsl:apply-templates select="key[text()='Track ID']" />
      <xsl:apply-templates select="key[text()='Name']" />
      <xsl:apply-templates select="key[text()='Artist']" />
      <xsl:apply-templates select="key[text()='Album']" />
      <xsl:apply-templates select="key[text()='Genre']" />
      <xsl:apply-templates select="key[text()='Grouping']" />
      <xsl:apply-templates select="key[text()='BPM']" />
      <xsl:apply-templates select="key[text()='Comments']" />
    <xsl:text>&#10;    </xsl:text>
    </song>
 </xsl:template>
  
  <xsl:template match="key">
      <xsl:text>&#10;      </xsl:text>
      <xsl:element name="{translate(text(), ' ', '_')}">
          <xsl:value-of select="following-sibling::node()" />
      </xsl:element>
  </xsl:template>

  <xsl:template match="array">
    <xsl:for-each select="dict/key[text()='Name']/following-sibling::string[1]">
      <xsl:text>&#10;    </xsl:text>
      <list>

      <xsl:text>&#10;      </xsl:text>
      <Name><xsl:value-of select="." /></Name>

      <xsl:for-each select="following-sibling::array/dict/integer">
        <xsl:text>&#10;      </xsl:text>
        <Track_ID><xsl:value-of select="." /></Track_ID>
      </xsl:for-each>
      <xsl:text>&#10;    </xsl:text>
      </list>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
