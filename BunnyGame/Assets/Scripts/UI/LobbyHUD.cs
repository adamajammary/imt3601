using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
  * I might make this use the same UI generator as the settings menu eventually
  */
public class LobbyHUD : MonoBehaviour {

    //private NetworkPlayerSelect _manager;
    private NetworkManager _manager;

    private GameObject _panel1;
    private GameObject _serverCreationPanel;
    private GameObject _serverFindPanel;
    private GameObject _lobbyPanel;

    private List<MatchInfoSnapshot> _matchList;
    private MatchInfo _joinedMatch = null;
    private string _matchName = "";

    private bool _localhost;
    private bool _inRoom;
    private bool _isHost = false;
    private bool _waitingDrop = false;

    void Start() {
        //this._manager = GameObject.Find("LobbyManager").GetComponent<NetworkPlayerSelect>();
        this._manager = NetworkManager.singleton;
        this._panel1 = this.transform.GetChild(0).gameObject;
        this._serverCreationPanel = this.transform.GetChild(1).gameObject;
        this._serverFindPanel = this.transform.GetChild(2).gameObject;
        this._lobbyPanel = this.transform.GetChild(3).gameObject;

        this._panel1.SetActive(true);
        this._serverCreationPanel.SetActive(false);
        this._serverFindPanel.SetActive(false);
        this._lobbyPanel.SetActive(false);
    }

    /**
     * Panel 1
     * 
     **/

    public void SetLocalhost(bool val) {
        this._localhost = val;
    }

    public void onOpenServerCreation() {
        print("onOpenServerCreation");

        if (this._localhost) {
            // Localhost server creation
        } else {
            // Matchmaking server creation
            this._panel1.SetActive(false);
            this._serverCreationPanel.SetActive(true);
        }
    }

    public void onFindServer() {
        // Display screen for finding a server or entering a localhost ip
        this._panel1.SetActive(false);
        this._serverFindPanel.SetActive(true);

        this.disconnectMatch();
        this._manager.StartMatchMaker();

        _matchList = new List<MatchInfoSnapshot>();
        this._manager.matchMaker.ListMatches(0, 10, "", true, 0, 0, this.displayServers);
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
        print("onCreateServer");

        this._serverCreationPanel.SetActive(false);
        this._lobbyPanel.SetActive(true);

        if (this._localhost) {
            createLocalServer();
            return;
        }

        //this._manager.matchName = this._serverCreationPanel.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.GetComponent<Text>().text;
        this._matchName = this._serverCreationPanel.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.GetComponent<Text>().text;

        // Create a matchmaking server using the user-set params in the servercreation panel
        this.disconnectMatch();
        this._manager.StartMatchMaker();

        this._manager.matchMaker.CreateMatch(
            (this._matchName != "" ? this._matchName : "default"), this._manager.matchSize, true, "", "", "", 0, 0, 
            (b, s, m) => { this._manager.OnMatchCreate(b, s, m); this.onJoinLobby(m); }
        );

        print("onCreateServer::matchName2: " + _matchName);
        print("onCreateServer::matchSize2: " + this._manager.matchSize);

        this._isHost = true;
    }

    public void createLocalServer() {
        print("createLocalServer");

        // Create a local server using the user set parameters
    }

    public void onCancelServerCreation() {
        // Go back to panel1
        this._panel1.SetActive(true);
        this._serverCreationPanel.SetActive(false);

        this.disconnectMatch();
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

        for (int i = 2; i < this._serverFindPanel.transform.GetChild(1).childCount; i++) {
            this._serverFindPanel.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            Destroy(this._serverFindPanel.transform.GetChild(1).GetChild(i).gameObject);
        }

        GameObject template = _serverFindPanel.transform.GetChild(1).GetChild(0).gameObject;
        RectTransform rt;

        for (int i = 0; i < matchList.Count; i++) {
            print("matchList[" + i + "].name: " + matchList[i].name);

            string matchName = matchList[i].name;
            //NetworkID id = matchList[i].networkId;
            int currentSize = matchList[i].currentSize;
            int maxSize = matchList[i].maxSize;

            GameObject serverPanel = Instantiate(template);
            serverPanel.SetActive(true);
            serverPanel.transform.SetParent(_serverFindPanel.transform.GetChild(1));
            serverPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = string.Format("{0} [{1}/{2}]", matchName, currentSize, maxSize);

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

    public void onJoinServer(int serverIndex) {
        print("onJoinServer: " + serverIndex);

        this._serverFindPanel.SetActive(false);
        this._lobbyPanel.SetActive(true);

        this.disconnectMatch();
        this._manager.StartMatchMaker();

        //this._manager.matchName = this._matchList[serverIndex].name;
        //this._manager.matchSize = (uint)this._matchList[serverIndex].currentSize;

        this._matchName = this._matchList[serverIndex].name;

        this._manager.matchMaker.JoinMatch(
            this._matchList[serverIndex].networkId, "", "", "", 0, 0, 
            (b, s, m) => { this._manager.OnMatchJoined(b, s, m); this.onJoinLobby(m); }
        );

        print("onJoinServer::matchName1: " + this._matchList[serverIndex].name + " / " + this._manager.matchName);
    }

    public void onJoinLocalServer() {
        print("onJoinLocalServer");

        this.disconnectMatch();
    }

    public void onCancelFindServer() {
        this._panel1.SetActive(true);
        this._serverFindPanel.SetActive(false);

        // todo: reset whatever is in the localhost ip slot

        this.disconnectMatch();

        for (int i = 2; i < this._serverFindPanel.transform.GetChild(1).childCount; i++) {
            this._serverFindPanel.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            Destroy(this._serverFindPanel.transform.GetChild(1).GetChild(i).gameObject);
        }
    }

    /**
     * Game lobby
     * 
     **/

    public void onJoinLobby(MatchInfo match) {
        print("onJoinLobby1: " + _isHost);

        this._inRoom      = true;
        this._joinedMatch = match;

        print("onJoinLobby2: " + _isHost);
        if (NetworkClient.allClients.Count > 0) {
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_LOBBY_PLAYERS,    this.recieveNetworkMessage);
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_MATCH_DISCONNECT, this.recieveNetworkMessage);
        }
        print("onJoinLobby3: " + _isHost);

        this.onRefreshLobby();
        print("onJoinLobby4: " + _isHost);
    }

    public void onRefreshLobby() {
        StartCoroutine(this.initLobbyRoom(0.1f));
    }

    // NB! Lobby players are not available immediately, so we wait until they are completely initialized.
    private IEnumerator<WaitForSeconds> initLobbyRoom(float delayInSeconds) {
        float time = 0;

        while ((GameObject.FindGameObjectsWithTag("lobbyplayer").Length < 1) && (time < 10.0f)) {
            time += delayInSeconds;
            yield return new WaitForSeconds(delayInSeconds);
        }

        NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_LOBBY_UPDATE, new IntegerMessage());
    }

    // Update the list of names and ready-state of lobby players.
    private void updateRoom(LobbyPlayerMessage message) {
        if (!this._inRoom || (this._lobbyPanel == null))
            return;

        string        matchName;
        RectTransform rt;
        GameObject    title    = this._lobbyPanel.transform.GetChild(0).GetChild(0).gameObject;
        GameObject    template = this._lobbyPanel.transform.GetChild(0).GetChild(1).gameObject;

        if (title != null) {
            matchName = (this._matchName != "" ? this._matchName : "default");
            matchName = (matchName.Length < 11 ? matchName : matchName.Substring(0, 10) + "...");

            title.GetComponent<Text>().text = string.Format("Match: {0}", matchName);
        }

        for (int i = 2; i < this._lobbyPanel.transform.GetChild(0).childCount; i++) {
            this._lobbyPanel.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
            Destroy(this._lobbyPanel.transform.GetChild(0).GetChild(i).gameObject);
        }

        for (int i = 0; i < message.players.Length; i++) {
            GameObject listing = Instantiate(template);
            listing.transform.SetParent(template.transform.parent);
            listing.SetActive(true);

            rt = listing.GetComponent<RectTransform>();
            rt.offsetMax = new Vector2(192, (i + 1) * -45 - 20);
            rt.offsetMin = new Vector2(0,   (i + 2) * -45 - 20);

            listing.transform.GetChild(0).GetComponent<Text>().text = message.players[i].name;
            listing.transform.GetChild(1).GetComponent<Text>().text = (message.players[i].ready ? "Ready" : "Not ready");
        }
    }

    public void onReady() {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("lobbyplayer")) {
            NetworkLobbyPlayer lobbyPlayer = player.GetComponent<NetworkLobbyPlayer>();

            if (!lobbyPlayer.isLocalPlayer)
                continue;

            if (!lobbyPlayer.readyToBegin)
                lobbyPlayer.SendReadyToBeginMessage();
            else
                lobbyPlayer.SendNotReadyToBeginMessage();

            StartCoroutine(this.initLobbyRoom(0.0f));
        }
    }

    public void onLeaveLobby() {
        this._inRoom = false;
        this._lobbyPanel.SetActive(false);
        this._panel1.SetActive(true);

        this.disconnectMatch();
    }

    // Disconnects from the match making lobby.
    private void disconnectMatch() {
        print("disconnectMatch: " + _joinedMatch);

        if (this._joinedMatch != null) {
            if (this._isHost) {
                StartCoroutine(this.dropMatchAndWait(1.0f));
            } else {
                print("disconnectMatch::CLIENT");

                if ((this._manager.matchMaker != null) && (this._joinedMatch.networkId != NetworkID.Invalid)) {
                    this._manager.matchMaker.DropConnection(
                        this._joinedMatch.networkId,
                        this._joinedMatch.nodeId, 0, (s, e) => {
                            this._manager.OnDropConnection(s, e);
                            this._manager.StopClient();
                            this._manager.StopMatchMaker();
                        }
                    );
                }

                this._joinedMatch = null;
            }
        }
    }

    // The host will wait x seconds to allow all clients to disconnect before destroying the match.
    private IEnumerator<WaitForSeconds> dropMatchAndWait(float waitInSeconds) {
        print("disconnectMatch::HOST1");

        if (!this._waitingDrop) {
            if (NetworkClient.allClients.Count > 0)
                NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_MATCH_DROP, new IntegerMessage());

            if ((this._manager.matchMaker != null) && (this._joinedMatch != null) && (this._joinedMatch.networkId != NetworkID.Invalid))
                this._manager.matchMaker.DropConnection(this._joinedMatch.networkId, this._joinedMatch.nodeId, 0, (s, e) => this._manager.OnDropConnection(s, e));

            this._waitingDrop = true;
        }

        yield return new WaitForSeconds(waitInSeconds);

        print("disconnectMatch::HOST2");

        if ((this._manager.matchMaker != null) && (this._joinedMatch != null) && (this._joinedMatch.networkId != NetworkID.Invalid)) {
            this._manager.matchMaker.DestroyMatch(
                this._joinedMatch.networkId, 0, (s, e) => {
                    this._manager.OnDestroyMatch(s, e);
                    this._manager.StopHost();
                    this._manager.StopMatchMaker();
                }
            );
        }

        this._joinedMatch = null;
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_LOBBY_PLAYERS:
                this.updateRoom(message.ReadMessage<LobbyPlayerMessage>());
                break;
            case (short)NetworkMessageType.MSG_MATCH_DISCONNECT:
                this.onLeaveLobby();
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }
}
