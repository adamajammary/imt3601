using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Network/NetworkManagerHUD")]
[RequireComponent(typeof(NetworkManager))]
public class LobbyHUD : MonoBehaviour {

    public GameObject targetCanvas;

    private NetworkManager _manager;

    private bool _localhost;

	// Use this for initialization
	void Awake () {
        _manager = GetComponent<NetworkManager>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    /**
     * PANEL 1
     * 
     **/

    public void SetLocalhost(bool val) {
        _localhost = val;
    }

    public void onOpenServerCreation() {
        if (_localhost) {
            // Localhost server creation
        }
        else {
            // Matchmaking server creation
        }
    }

    public void onFindServer() {
        // Display screen for finding a server or entering a localhost ip
    }

    /**
     * Server Creation panel
     * 
     **/

    public void onCreateServer() {
        // Create a server using the user-set preferences in the servercreation panel
    }

    public void onCreateLocalServer() {

    }

    public void onCancelServerCreation() {
        // Go back to panel1
    }
    
}
