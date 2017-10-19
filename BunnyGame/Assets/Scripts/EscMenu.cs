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
    private NetworkManager manager;

    private bool isPressed = false;

    // Use this for initialization
    void Start()
    {
        manager       = NetworkManager.singleton;
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
        manager.StopHost();
        Application.Quit();
    }

    public void ExitToMain()
    {
        manager.StopHost();
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

    // Is this used?
    public void DisconnectFromServer()
    {
        manager.GetComponent<NetworkManager>().StopServer();
        manager.GetComponent<NetworkManager>().enabled = false;
    }
}
