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

    private bool isMenu = false;
    private bool lockCursor = true;

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

    void Update() {
        handleMouse();

        if (SceneManager.GetActiveScene().name == "Lobby")
            return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            isMenu = !isMenu;
            GetComponent<Canvas>().enabled = isMenu;
            lockCursor = !isMenu;
        }
    }

    public void onResume() {
        isMenu = !isMenu;
        GetComponent<Canvas>().enabled = isMenu;
        lockCursor = !isMenu;
    }


    public void Settings()
    {
        Debug.Log("Settings button pressed!");
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

    void handleMouse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            lockCursor = !lockCursor;

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
