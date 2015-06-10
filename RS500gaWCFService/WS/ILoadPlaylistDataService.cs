using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace RS500gaWCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ILoadPlaylistDataService" in both code and config file together.
    [ServiceContract]
    public interface ILoadPlaylistDataService
    {
        [OperationContract]
        string sendLibrary(string xmlcontent, string libraryTitle);

        [OperationContract]
        string getSongLyrics(string song, string artist);

        [OperationContract]
        string sendPlaylist(string xmlcontent, string playlistTitle);

        [OperationContract]
        string generatePlaylist(string xmlcontent);

    }
}
