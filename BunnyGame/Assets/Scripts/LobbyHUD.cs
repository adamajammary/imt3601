using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
  * I might make this use the same UI generator as the settings menu eventually
  */
public class LobbyHUD : MonoBehaviour {

    private NetworkManager _manager;

    private GameObject _panel1;
    private GameObject _serverCreationPanel;
    private GameObject _serverFindPanel;
    private GameObject _lobbyPanel;

    private List<MatchInfoSnapshot> _matchList;

    private bool _localhost;
    private bool _inRoom;


    void Start() {
        this._manager             = GameObject.Find("LobbyManager").GetComponent<NetworkPlayerSelect>();
        this._panel1              = this.transform.GetChild(0).gameObject;
        this._serverCreationPanel = this.transform.GetChild(1).gameObject;
        this._serverFindPanel     = this.transform.GetChild(2).gameObject;
        this._lobbyPanel          = this.transform.GetChild(3).gameObject;

        this._panel1.SetActive(true);
        this._serverCreationPanel.SetActive(false);
        this._serverFindPanel.SetActive(false);
        this._lobbyPanel.SetActive(false);
    }

    private void Update()
    {
    }

    /**
     * Panel 1
     * 
     **/

    public void SetLocalhost(bool val) {
        this._localhost = val;
    }

    public void onOpenServerCreation() {
        if (this._localhost) {
            // Localhost server creation
        }
        else {
            // Matchmaking server creation
            this._panel1.SetActive(false);
            this._serverCreationPanel.SetActive(true);
        }
    }

    public void onFindServer() {
        // Display screen for finding a server or entering a localhost ip
        this._panel1.SetActive(false);
        this._serverFindPanel.SetActive(true);
        this._manager.StartMatchMaker();

        _matchList = new List<MatchInfoSnapshot>();
        this._manager.matchMaker.ListMatches(0, 10, "", true, 0, 0, displayServers);
    }

    public void onBackToStart() {
        SceneManager.LoadScene("StartScreen");
    }

    /**
     * Server Creation panel
     * 
     **/
    
    // Create a server using the user-set parameters
    public void onCreateServer() {
        this._serverCreationPanel.SetActive(false);
        this._lobbyPanel.SetActive(true);

        if (this._localhost) {
            createLocalServer();
            return;
        }
        // Create a matchmaking server using the user-set params in the servercreation panel
        this._manager.StartMatchMaker();
        this._manager.matchName = _serverCreationPanel.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.GetComponent<Text>().text;
        this._manager.matchMaker.CreateMatch(this._manager.matchName, this._manager.matchSize, true, "", "", "", 0, 0, 
            (bool b, string s, MatchInfo mi) => { this._manager.OnMatchCreate(b,s, mi); onJoinLobby(); });
    }

    public void createLocalServer() {
        // Create a local server using the user set parameters
    }

    public void onCancelServerCreation() {
        // Go back to panel1
        this._panel1.SetActive(true);
        this._serverCreationPanel.SetActive(false);
        this._manager.StopMatchMaker();
    }

    /**
     * Server Find panel
     * 
     **/

    public void displayServers(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        this._manager.OnMatchList(success, extendedInfo, matchList);
        
        // TODO: Display error message if not a success

        this._matchList = matchList;

        GameObject template = _serverFindPanel.transform.GetChild(1).GetChild(0).gameObject;

        RectTransform rt;
        for (int i = 0; i < matchList.Count; i++) {
            string servername = matchList[i].name;
            NetworkID id = matchList[i].networkId;
            int currentSize = matchList[i].currentSize;
            int maxSize = matchList[i].maxSize;

            GameObject serverPanel = Instantiate(template);
            serverPanel.SetActive(true);
            serverPanel.transform.SetParent(_serverFindPanel.transform.GetChild(1));
            serverPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = servername;
            rt = serverPanel.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(0, (i + 1) * -35);
            rt.offsetMax = new Vector2(0, i * -35);
            int idx = i;
            serverPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener( () => onJoinServer(idx) );
        }

        rt = _serverFindPanel.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(-200, -100 - 17.5f * (matchList.Count - 1));
        rt.offsetMax = new Vector2(200, 100 + 17.5f * (matchList.Count - 1));
    }

    public void onJoinServer(int serverIndex)
    {
        this._serverFindPanel.SetActive(false);
        this._lobbyPanel.SetActive(true);

        this._manager.matchName = this._matchList[serverIndex].name;
        this._manager.matchSize = (uint)this._matchList[serverIndex].currentSize;
        _manager.matchMaker.JoinMatch(this._matchList[serverIndex].networkId, "", "", "", 0, 0, 
            (bool b, string s, MatchInfo mi) => { this._manager.OnMatchJoined(b, s, mi); onJoinLobby(); });
    }


    public void onJoinLocalServer() {
        this._manager.StopMatchMaker();


    }

    public void onCancelFindServer() {
        this._panel1.SetActive(true);
        this._serverFindPanel.SetActive(false);
        // reset whatever is in the localhost ip slot
        this._manager.StopMatchMaker();

        for(int i = 2; i < this._serverFindPanel.transform.GetChild(1).childCount; i++)
            Destroy(this._serverFindPanel.transform.GetChild(1).GetChild(i).gameObject);
    }
    


    /**
     * Game lobby
     * 
     **/

    public void onJoinLobby() {
        _inRoom = true;
        StartCoroutine(updateRoom());
    }

    //TODO : Don't redraw everythin every time, but only when there is something new
    private IEnumerator updateRoom(){
        //List<NetworkLobbyPlayer> players = new List<NetworkLobbyPlayer>();
        GameObject template = this._lobbyPanel.transform.GetChild(0).GetChild(1).gameObject;
        RectTransform rt;
        while (_inRoom) {
            int index = 0;

            if (this._lobbyPanel != null) {
                for (int i = 2; i < this._lobbyPanel.transform.GetChild(0).childCount; i++)
                    Destroy(this._lobbyPanel.transform.GetChild(0).GetChild(i).gameObject);

                foreach(GameObject player in GameObject.FindGameObjectsWithTag("lobbyplayer")) {
                    //players.Add(player.GetComponent<NetworkLobbyPlayer>());
                    GameObject listing = Instantiate(template);
                    listing.transform.SetParent(template.transform.parent);
                    listing.SetActive(true);
                    rt = listing.GetComponent<RectTransform>();
                    rt.offsetMax = new Vector2(192, (index + 1) * -45 - 20);
                    rt.offsetMin = new Vector2(0, (index + 2) * -45 - 20);

                    listing.transform.GetChild(0).GetComponent<Text>().text = "Player [" + player.GetComponent<NetworkLobbyPlayer>().netId + "]";
                    listing.transform.GetChild(1).GetComponent<Text>().text = (player.GetComponent<NetworkLobbyPlayer>().readyToBegin ? "Ready" : "Not ready");

                    ++index;
                }
            }
            yield return null;
        }
    }

    public void onReady() {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("lobbyplayer")) {
            NetworkLobbyPlayer lobbyPlayer = player.GetComponent<NetworkLobbyPlayer>();
            if (lobbyPlayer.isLocalPlayer) {
                if (!lobbyPlayer.readyToBegin)
                    lobbyPlayer.SendReadyToBeginMessage();
                else
                    lobbyPlayer.SendNotReadyToBeginMessage();
                break;
            }
        }
        

    }


    public void onLeaveLobby() {
        _inRoom = false;
        this._lobbyPanel.SetActive(false);
        this._panel1.SetActive(true);
    }


}
