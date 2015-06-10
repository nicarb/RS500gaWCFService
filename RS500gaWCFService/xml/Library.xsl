<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" indent="yes"/>



  <!-- Stylesheet for tracks in Apple iTunes library export file -->



  <xsl:template match="plist">

    <html>

      <head>

        <title>Playlist</title>

        <style type="text/css">

          body {font-family:sans-serif}

          h1 {text-align:center}

          table {margin-left:auto;margin-right:auto;}

          thead {background-color:black;color:white}

          td,th {padding: 7px 7px 7px 7px;font-size:15px}

        </style>

      </head>

      <body>

        <h1>Playlist</h1>

        <xsl:apply-templates select="dict"/>

      </body>

    </html>

  </xsl:template>



  <xsl:template match="dict">

    <table border="1" rules="all">

      <thead>

        <tr>

          <th>Track ID</th>

          <th>Name</th>

          <th>Artist</th>

          <th>Album</th>

          <th>Genre</th>

          <th>Kind</th>

          <th>Size</th>

          <th>Total Time</th>

          <th>Disc Number</th>

          <th>Disc Count</th>

          <th>Track Number</th>

          <th>Track Count</th>

          <th>Year</th>

          <th>Date Modified</th>

          <th>Date Added</th>

          <th>Bit Rate</th>

          <th>Sample Rate</th>

          <th>Play Count</th>

          <th>Play Date</th>

          <th>Play Date UTC</th>

          <th>Normalization</th>

          <th>Location</th>

          <th>File Folder Count</th>

          <th>Library Folder Count</th>

        </tr>

      </thead>

      <xsl:apply-templates select="dict/dict"/>

    </table>

  </xsl:template>



  <xsl:template match="dict/dict">

    <tr>

      <td style="text-align:center"><xsl:value-of select="key[.='Track ID']/

following-sibling::integer"/></td>

      <td>
        <xsl:value-of select="key[.='Name']/following-sibling::string"/>
      </td>

      <td>
        <xsl:value-of select="key[.='Artist']/following-sibling::string"/>
      </td>

      <td>
        <xsl:value-of select="key[.='Album']/following-sibling::string"/>
      </td>

      <td>
        <xsl:value-of select="key[.='Genre']/following-sibling::string"/>
      </td>

      <td><xsl:value-of select="key[.='Kind']/following-sibling::string"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Size']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Total Time']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Disc Number']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Disc Count']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Track Number']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Track Count']/

following-sibling::integer"/></td>

      <td style="text-align:center">
        <xsl:value-of select="key[.='Year']/

following-sibling::integer"/>
      </td>

      <td><xsl:value-of select="substring(key[.='Date Modified']/

following-sibling::date, 1, 10)"/></td>

      <td>
        <xsl:value-of select="substring(key[.='Date Added']/

following-sibling::date, 1, 10)"/>
      </td>

      <td style="text-align:center"><xsl:value-of select="key[.='Bit Rate']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Sample Rate']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Play Count']/

following-sibling::integer"/></td>

      <td><xsl:value-of select="key[.='Play Date']/

following-sibling::integer"/>

</td>

      <td><xsl:value-of select="substring(key[.='Play Date UTC']/

following-sibling::date, 1, 10)"/></td>

      <td><xsl:value-of select="key[.='Normalization']/

following-sibling::integer"/></td>

      <td><xsl:value-of select="key[.='Location']/

following-sibling::string"/>

</td>

      <td style="text-align:center"><xsl:value-of select="key[.='File Folder Count']/

following-sibling::integer"/></td>

      <td style="text-align:center"><xsl:value-of select="key[.='Library Folder Count']/

following-sibling::integer"/></td>

    </tr>

  </xsl:template>



</xsl:stylesheet>
