using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/**
  * I'll probably make this use the same UI generator as the settings menu eventually
  */
[AddComponentMenu("Network/NetworkManagerHUD")]
[RequireComponent(typeof(NetworkManager))]
public class LobbyHUD : MonoBehaviour {

    public GameObject targetCanvas;
    private GameObject _panel1;
    private GameObject _serverCreationPanel;
    private GameObject _serverFindPanel;
    private GameObject _lobbyPanel;

    private NetworkManager _manager;

    private bool _localhost;

	// Use this for initialization
	void Awake () {
        this._manager             = GetComponent<NetworkManager>();
        this._panel1              = targetCanvas.transform.GetChild(0).gameObject;
        this._serverCreationPanel = targetCanvas.transform.GetChild(1).gameObject;
        this._serverFindPanel     = targetCanvas.transform.GetChild(2).gameObject;
        this._lobbyPanel          = targetCanvas.transform.GetChild(3).gameObject;

    }

    void Start() {

        this._panel1.SetActive(true);
        this._serverCreationPanel.SetActive(false);
        this._serverFindPanel.SetActive(false);
        this._lobbyPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
		
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
        this._manager.matchMaker.CreateMatch(this._manager.matchName, this._manager.matchSize, true, "", "", "", 0, 0, this._manager.OnMatchCreate);
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

    public void onJoinServer()
    {

    }

    public void onJoinLocalServer()
    {

    }

    public void onCancelFindServer() {
        this._panel1.SetActive(true);
        this._serverFindPanel.SetActive(false);
        // reset whatever is in the  localhost ip slot
    }
    


    /**
     * Game lobby
     * 
     **/

    public void onLeaveLobby()
    {

    }


}
