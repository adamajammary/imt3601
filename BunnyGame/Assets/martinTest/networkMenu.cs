using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class networkMenu : MonoBehaviour {
    public static networkMenu Instance { set; get; }

    public GameObject mainMenu;
    public GameObject connectMenu;
    public GameObject hostMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;
	
    private void Start() {
        Instance = this;
        connectMenu.SetActive(false);
        hostMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void mainMenuConnect() {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }

    public void mainMenuHost() {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);

        try {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.init();
        }catch(Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void hostMenuConnect() {
        string hostAdress = "127.0.0.1";
        try {
            client c = Instantiate(clientPrefab).GetComponent<client>();
            c.connectToServer(hostAdress, 6321);
            connectMenu.SetActive(false);
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void connectMenuConnect() {
        string hostAdress = GameObject.Find("IP").GetComponent<InputField>().text;
        if (hostAdress == "")
            hostAdress = "127.0.0.1";

        try {
            client c = Instantiate(clientPrefab).GetComponent<client>();
            c.connectToServer(hostAdress, 6321);
            connectMenu.SetActive(false);
        }catch(Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void connectMenuBack() {
        connectMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void hostMenuBack() {
        hostMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}
