using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpectatorUI : MonoBehaviour {

    private string spactatingPlayerName;
    private Text spectatingNameText;

    private SpectatorMode mode = SpectatorMode.FREE;

	// Use this for initialization
	void Start () {
        spectatingNameText = transform.GetChild(2).GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void modeSwitch(SpectatorMode mode) {
        this.mode = mode;
        spectatingNameText.gameObject.SetActive(mode == SpectatorMode.FOLLOW);
        transform.GetChild(1).gameObject.SetActive(mode == SpectatorMode.FOLLOW);
        transform.GetChild(3).gameObject.SetActive(mode == SpectatorMode.FOLLOW);
    }

    public void followingPlayerChange(string name) {
        if (this.mode != SpectatorMode.FOLLOW)
            return;

        spectatingNameText.text = name;
    }
}
