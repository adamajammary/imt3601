using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    public Canvas quitMenu;
    public Button playButton;
    public Button exitButton;

	// Use this for initialization
	void Start () {
        quitMenu = quitMenu.GetComponent<Canvas>();
        playButton = playButton.GetComponent<Button>();
        exitButton = exitButton.GetComponent<Button>();
        quitMenu.enabled = false;   
    }
	

    public void ExitPress()
    {
        quitMenu.enabled = true;
        playButton.enabled = false;
        exitButton.enabled = false;
    }

    public void NoPress()
    {
        quitMenu.enabled = false;
        playButton.enabled = true;
        exitButton.enabled = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void ExitGame()
    {
        Application.Quit();
    }


}
