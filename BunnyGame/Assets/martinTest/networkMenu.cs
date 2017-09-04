using System;
using UnityEngine;
using UnityEngine.UI;

public class networkMenu : MonoBehaviour {
    public static networkMenu Instance { set; get; }

    public GameObject mainMenu;
    public GameObject connectMenu;
    public GameObject hostMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;
	
    // Initializes the start menu
    private void Start() {
        Instance = this;
        connectMenu.SetActive(false);
        hostMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // Takes player to connect menu
    public void mainMenuConnect() {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }

    // Creates and instance of a server that other players can connect to.
    public void mainMenuHost() {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);

        try {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.init();
            client c = Instantiate(clientPrefab).GetComponent<client>();
            string hostAdress = "127.0.0.1";
            c.connectToServer("host", hostAdress, 6321);
        } catch(Exception e) {
            Debug.Log(e.Message);
        }
    }

    // Launches the server and starts game.
    public void hostMenuConnect() {        
        try {
            hostMenu.SetActive(false);
            FindObjectOfType<Server>().startGame();
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    // Connects to server specified by IP in text field.
    public void connectMenuConnect() {
        string hostAdress = GameObject.Find("IP").GetComponent<InputField>().text;
        if (hostAdress == "")
            hostAdress = "127.0.0.1";

        string name = GameObject.Find("Name").GetComponent<InputField>().text;
        if (name == "")
            name = "client";

        try {
            client c = Instantiate(clientPrefab).GetComponent<client>();
            c.connectToServer(name, hostAdress, 6321);
            connectMenu.SetActive(false);
        }catch(Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void connectMenuBack() {
        connectMenu.SetActive(false);
        mainMenu.SetActive(true);

        client[] cl = FindObjectsOfType<client>();
        foreach (client c in cl)
            Destroy(c.gameObject);
    }

    // Cancels server
    public void hostMenuBack() {
        hostMenu.SetActive(false);
        mainMenu.SetActive(true);

        // Host canceled, remove all servers and clients
        Server[] sl = FindObjectsOfType<Server>();
        foreach (Server s in sl)
            Destroy(s.gameObject);

        client[] cl = FindObjectsOfType<client>();
        foreach (client c in cl)
            Destroy(c.gameObject);
    }
}
