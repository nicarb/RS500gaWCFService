using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RS500gaWCFService.utils
{
    public class Utils
    {
        public static string removeIllegalChars(string xmlStr)
        {
            string retval = xmlStr;
            List<char> charsToSubstitute = new List<char>();
            charsToSubstitute.Add((char)0x19);
            charsToSubstitute.Add((char)0x1C);
            charsToSubstitute.Add((char)0x1D);

            foreach (char c in charsToSubstitute)
                retval = retval.Replace(Convert.ToString(c), string.Empty);

            return retval;
        }

        public static string encodeMD5(string plainStr)
        {
            string signature_hash = string.Empty;
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(plainStr);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            signature_hash = sb.ToString();

            return signature_hash;
        }

        public static string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            StringBuilder buffer = new StringBuilder(xml.Length);

            foreach (char c in xml)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public static bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }

        public static string getStringWithinLen(string inputStr, int strlen)
        {
            if (!string.IsNullOrEmpty(inputStr) && inputStr.Length > strlen)
            {
                return inputStr.Substring(0, strlen);
            }
            else
            {
                return inputStr;
            }
        }
    }
}