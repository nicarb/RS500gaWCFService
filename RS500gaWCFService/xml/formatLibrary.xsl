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
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version='1.0'>

  <xsl:output method="html" />

  <xsl:strip-space elements="*" />

  <xsl:variable name="songs" select="document(/include/@file)/library/songlist" />
  <xsl:variable name="lists" select="document(/include/@file)/library/playlists" />

  <xsl:template match="/">
    <html>
    <head>
      <style type="text/css">
        body {
          background-color : #eee;
        }

        table {
          border-collapse : separate;
        }

        caption, th, td {
          border : 1px;
          border : inset 1px;
        }

        a:link { color : #672; }

        .dark { background-color : #bbb; }

        .light { background-color : #ddd; }

        .title {
          white-space : nowrap;
          background-color : #bc4;
          text-align : center;
          font-size : x-large;
          font-weight : bold;
          margin : 2px;
        }
      </style>
    </head>
    <body>
      <xsl:if test="/include/@songs='yes'">
        <table>
          <caption class="title">Library</caption>
          <tr class="light">
            <th>Track Name</th>
            <th>Artist</th>
            <th>Album</th>
            <th>Genre</th>
            <th>Grouping</th>
            <th>BPM</th>
            <th>Comments</th>
          </tr>
  
          <xsl:for-each select="$songs/song">
            <tr>
  
              <xsl:choose>
                <xsl:when test="position() mod 2 = 1">
                  <xsl:attribute name="class"> dark </xsl:attribute>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="class"> light </xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
  
              <td>
                <xsl:element name="a">
                  <xsl:attribute name="href">
                    itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?songTerm=<xsl:value-of select="Name" />&amp;artistTerm=<xsl:value-of select="Artist" />&amp;albumTerm=<xsl:value-of select="Album" />
                  </xsl:attribute>
                  <xsl:value-of select="Name" />
                </xsl:element>
              </td>
              <td>
                <xsl:element name="a">
                  <xsl:attribute name="href">
                    itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?artistTerm=<xsl:value-of select="Artist" />
                  </xsl:attribute>
                  <xsl:value-of select="Artist" />
                </xsl:element>
              </td>
              <td>
                <xsl:element name="a">
                  <xsl:attribute name="href">
                    itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?artistTerm=<xsl:value-of select="Artist" />&amp;albumTerm=<xsl:value-of select="Album" />
                  </xsl:attribute>
                  <xsl:value-of select="Album" />
                </xsl:element>
              </td>
              <td><xsl:value-of select="Genre" /></td>
              <td><xsl:value-of select="Grouping" /></td>
              <td><xsl:value-of select="BPM" /></td>
              <td><xsl:value-of select="Comments" /></td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>

      <xsl:for-each select="$lists/list">
        <xsl:if test="Name/text() != 'Library' and Name/text() != 'Music'">
          <xsl:call-template name="list" />
        </xsl:if>
      </xsl:for-each>
    </body>
    </html>
  </xsl:template>

  <xsl:template name="list">
    <table>
      <caption class="title">Playlist: <xsl:value-of select="Name" /></caption>
      <tr class="light">
        <th>Track Name</th>
        <th>Artist</th>
        <th>Album</th>
        <th>Genre</th>
        <th>Grouping</th>
        <th>BPM</th>
        <th>Comments</th>
      </tr>

      <xsl:for-each select="Track_ID">
        <tr>
          <xsl:choose>
            <xsl:when test="position() mod 2 = 1">
              <xsl:attribute name="class"> dark </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class"> light </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:variable name="trackID" select="." />
          <xsl:apply-templates select="$songs/song/Track_ID[text()=$trackID]" />
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template match="Track_ID">
    <td>
      <xsl:element name="a">
        <xsl:attribute name="href">
          itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?songTerm=<xsl:value-of select="following-sibling::Name" />&amp;artistTerm=<xsl:value-of select="following-sibling::Artist" />&amp;albumTerm=<xsl:value-of select="following-sibling::Album" />
        </xsl:attribute>
        <xsl:value-of select="following-sibling::Name" />
      </xsl:element>
    </td>
    <td>
      <xsl:element name="a">
        <xsl:attribute name="href">
          itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?artistTerm=<xsl:value-of select="following-sibling::Artist" />
        </xsl:attribute>
        <xsl:value-of select="following-sibling::Artist" />
      </xsl:element>
    </td>
    <td>
      <xsl:element name="a">
        <xsl:attribute name="href">
          itms://phobos.apple.com/WebObjects/MZSearch.woa/wa/advancedSearchResults?artistTerm=<xsl:value-of select="following-sibling::Artist" />&amp;albumTerm=<xsl:value-of select="following-sibling::Album" />
        </xsl:attribute>
        <xsl:value-of select="following-sibling::Album" />
      </xsl:element>
    </td>
    <td>
      <xsl:value-of select="following-sibling::Genre" />
    </td>
    <td>
      <xsl:value-of select="following-sibling::Grouping" />
    </td>
    <td>
      <xsl:value-of select="following-sibling::BPM" />
    </td>
    <td>
      <xsl:value-of select="following-sibling::Comments" />
    </td>
  </xsl:template>
</xsl:stylesheet>
