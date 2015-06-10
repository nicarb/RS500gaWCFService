using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Resources;
using RS500gaWCFServiceTest.LoadPlaylistDataService;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;

namespace RS500gaWCFServiceTest
{
    [TestClass]
    public class UnitTest1
    {
        //static LoadPlaylistDataServiceClient client;


        [TestMethod]
        public void LondonCalling_getSongLyrics_TestMethod()
        {

            // Use the 'client' variable to call operations on the service.
            //LoadPlaylistDataServiceClient client = new LoadPlaylistDataServiceClient();

            //client.getSongLyricsAsync("London Calling", "Clash");
            // Always close the client.

            //client.
        }

        [TestMethod]
        public void RS500gaWCF_SendPlaylist_ALL_TestMethod()
        {

            // Use the 'client' variable to call operations on the service.
            LoadPlaylistDataServiceClient client = new LoadPlaylistDataServiceClient();

            //var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string xmldocsPath = @"D:\Development\VisualStudio\Projects\Tesi\RS500gaWCFService\RS500gaWCFService\Test\RS500gaWCFServiceTest\RS500gaWCFServiceTest\xml";
            //string xmldocsPath = @"C:\Users\Nicarb\Documents\Visual Studio 2013\Projects\xml";
            string xmlFileName = "Rolling Stones 500 Greatest Albums_v3.xml";
            string xmlFilepath = Path.Combine(xmldocsPath, xmlFileName);
            List<string> tracksSlotList = new List<string>();

            tracksSlotList.Add("RS500GA_rk001-050");
            tracksSlotList.Add("RS500GA_rk051-100");
            tracksSlotList.Add("RS500GA_rk101-150");
            tracksSlotList.Add("RS500GA_rk151-200");
            tracksSlotList.Add("RS500GA_rk201-250");
            tracksSlotList.Add("RS500GA_rk251-300");
            tracksSlotList.Add("RS500GA_rk301-350");
            tracksSlotList.Add("RS500GA_rk351-400");
            tracksSlotList.Add("RS500GA_rk401-450");
            tracksSlotList.Add("RS500GA_rk451-500");
            //tracksSlotList.Add("track52");
            //xmlContent = rl.GetString("track52");

            try
            {

                string xmlContent;

                ResourceLoader rl = new ResourceLoader("TestResources");
                Task<string> retval;
                Task waitEnd;
                string playlistTitle = "Rolling Stones 500 Greates Albums";
                foreach (string str in tracksSlotList)
                {
                    xmlContent = rl.GetString(str);
                    //foreach (KeyValuePair<string, string> entry in Utils.TRANSLATE_RU_EN)
                    //{
                    //    xmlContent.Replace(entry.Key, entry.Value);
                    //}
                    retval = client.sendLibraryAsync(xmlContent, playlistTitle);

                    //retval.Wait();
                    waitEnd = client.CloseAsync();
                    waitEnd.Wait(210000);
                    //string result = retval.Result;
                    //Assert.AreEqual(result, "OK");
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }

        }

        [TestMethod]
        public void RS500gaWCF_SendPlaylist_SLOTS_TestMethod()
        {

            // Use the 'client' variable to call operations on the service.
            LoadPlaylistDataServiceClient client = new LoadPlaylistDataServiceClient();

            //var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string xmldocsPath = @"D:\Development\VisualStudio\Projects\Tesi\RS500gaWCFService\RS500gaWCFService\Test\RS500gaWCFServiceTest\RS500gaWCFServiceTest\xml";
            //string xmldocsPath = @"C:\Users\Nicarb\Documents\Visual Studio 2013\Projects\xml";
            string xmlFileName = "Rolling Stones 500 Greatest Albums_v3.xml";
            string xmlFilepath = Path.Combine(xmldocsPath, xmlFileName);
            List<string> tracksSlotList = new List<string>();

            //tracksSlotList.Add("RS500GA_rk001-050");
            //tracksSlotList.Add("RS500GA_rk051-100");
            //tracksSlotList.Add("RS500GA_rk101-150");
            //tracksSlotList.Add("RS500GA_rk151-200");
            //tracksSlotList.Add("RS500GA_rk201-250");
            //tracksSlotList.Add("RS500GA_rk251-300");
            //tracksSlotList.Add("RS500GA_rk301-350");
            //tracksSlotList.Add("RS500GA_rk351-400");
            //tracksSlotList.Add("RS500GA_rk401-450");
            //tracksSlotList.Add("RS500GA_rk451-500");
            //tracksSlotList.Add("track52");
            //xmlContent = rl.GetString("track52");

            try
            {

                string xmlContent;

                ResourceLoader rl = new ResourceLoader("TestResources");
                Task<string> retval;
                Task waitEnd;
                string playlistTitle = "Rolling Stones 500 Greates Albums";
                string str = string.Empty;
                //str = "RS500GA_rk001-050";
                //str = "RS500GA_rk051-100";
                //str = "RS500GA_rk101-150";
                //str = "RS500GA_rk151-200";
                //str = "RS500GA_rk201-250";
                //str = "RS500GA_rk251-300";
                //str = "RS500GA_rk301-350";
                //str = "RS500GA_rk351-400";
                //str = "RS500GA_rk401-450";
                str = "RS500GA_rk451-500";
                xmlContent = rl.GetString(str);
                //foreach (KeyValuePair<string, string> entry in Utils.TRANSLATE_RU_EN)
                //{
                //    xmlContent.Replace(entry.Key, entry.Value);
                //}
                retval = client.sendLibraryAsync(xmlContent, playlistTitle);

                //retval.Wait();
                waitEnd = client.CloseAsync();
                waitEnd.Wait(210000);
                //string result = retval.Result;
                //Assert.AreEqual(result, "OK");
            }
            catch (Exception ex)
            {
                //throw ex;
            }

        }




        [TestMethod]
        public void RS500gaWCF_SendPlaylist_ALL_fromfile_TestMethod()
        {

            // Use the 'client' variable to call operations on the service.
            LoadPlaylistDataServiceClient client = new LoadPlaylistDataServiceClient();

            //var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string xmldocsPath = @"D:\Development\VisualStudio\Projects\Tesi\RS500gaWCFService\RS500gaWCFService\Test\RS500gaWCFServiceTest\RS500gaWCFServiceTest\xml";
            //string xmldocsPath = @"C:\Users\Nicarb\Documents\Visual Studio 2013\Projects\xml";
            string xmlFileName = "xml/Rolling Stones 500 Greatest Albums_v3.xml";
            //string xmlFilepath = Path.Combine(xmldocsPath, xmlFileName);
            //List<string> tracksSlotList = new List<string>();
            //tracksSlotList.Add("RS500GA_rk001-050");
            //tracksSlotList.Add("RS500GA_rk051-100");
            //tracksSlotList.Add("RS500GA_rk101-150");
            //tracksSlotList.Add("RS500GA_rk151-200");
            //tracksSlotList.Add("RS500GA_rk201-250");
            //tracksSlotList.Add("RS500GA_rk251-300");
            //tracksSlotList.Add("RS500GA_rk301-350");
            //tracksSlotList.Add("RS500GA_rk351-400");
            //tracksSlotList.Add("RS500GA_rk401-450");
            //tracksSlotList.Add("RS500GA_rk451-500");
            //xmlContent = rl.GetString("");
            try
            {
                string xmlContent = string.Empty;

                Task<string> retval;
                Task waitEnd;
                string playlistTitle = "Rolling Stones 500 Greates Albums";
                using (XmlReader reader = XmlReader.Create(xmlFileName))
                {
                    StringBuilder sb = new StringBuilder();

                    while (reader.Read())
                        sb.AppendLine(reader.ReadOuterXml());
                    xmlContent = sb.ToString();
                    retval = client.sendPlaylistAsync(xmlContent, playlistTitle);
                    //retval.Wait(1000000);
                   
                    //string result = retval.Result;
                    //Assert.AreEqual(result, "OK");

                }
                //foreach (KeyValuePair<string, string> entry in Utils.TRANSLATE_RU_EN)
                //{
                //    xmlContent.Replace(entry.Key, entry.Value);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [TestMethod]
        public void RS500gaWCF_SendPlaylist_Test_TestMethod()
        {

            // Use the 'client' variable to call operations on the service.
            LoadPlaylistDataServiceClient client = new LoadPlaylistDataServiceClient();

            //var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string xmldocsPath = @"D:\Development\VisualStudio\Projects\Tesi\RS500gaWCFService\RS500gaWCFService\Test\RS500gaWCFServiceTest\RS500gaWCFServiceTest\xml";
            //string xmldocsPath = @"C:\Users\Nicarb\Documents\Visual Studio 2013\Projects\xml";
            string xmlFileName = "Rolling Stones 500 Greatest Albums_v3.xml";
            string xmlFilepath = Path.Combine(xmldocsPath, xmlFileName);

            try
            {

                string xmlContent;

                ResourceLoader rl = new ResourceLoader("Resources");
                Task<string> retval;
                string playlistTitle = "RejectedTracks";
                xmlContent = rl.GetString("Track-ID-0");
                foreach (KeyValuePair<string, string> entry in Utils.TRANSLATE_RU_EN)
                {
                    xmlContent.Replace(entry.Key, entry.Value);
                }
                retval = client.sendPlaylistAsync(xmlContent, playlistTitle);
                retval.Wait();
                string result = retval.Result;
                Assert.AreEqual(result, "OK");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


    }
}
