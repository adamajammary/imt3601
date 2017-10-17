using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class EscMenu : NetworkBehaviour {

    public Canvas escMenu;
    public Button resumeButton;
    public Button settings;
    public Button exitToMenu;
    public Button exitToDesktop;
    private NetworkPlayerSelect manager;

    private bool isPressed = false;

    // Use this for initialization
    void Start()
    {
        manager = FindObjectOfType<NetworkPlayerSelect>();
        escMenu       = escMenu.GetComponent<Canvas>();
        resumeButton  = resumeButton.GetComponent<Button>();
        settings      = settings.GetComponent<Button>();
        exitToMenu    = exitToMenu.GetComponent<Button>();
        exitToDesktop = exitToDesktop.GetComponent<Button>();

        escMenu.enabled = false;
    }

    public void EscPress(bool visible)
    {
        escMenu.enabled = visible;
    }

    public void Settings()
    {
        Debug.Log("Settings button pressed!");
    }

    public void ResumeGame()
    {
        isPressed = true;
    }

    public void ExitToDesktop()
    {
        // Need to disconnect from server
        Application.Quit();
    }

    public void ExitToMain()
    {
        // Need to disconnect from server
        manager.disconnectFromServer();
        SceneManager.LoadScene("Lobby");
    }

    public bool resumePressed()
    {
        return isPressed;
    }

    public void rusumePressedReset()
    {
        isPressed = false;
    }
}
