using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributeUI : MonoBehaviour {
    private RectTransform t; // Toughness
    private RectTransform d; // Damage
    private RectTransform s; // Speed
    private RectTransform j; // Jump
    private PlayerEffects player;

    // Use this for initialization
    void Start () {
        t       = GameObject.Find("toughnessBar").GetComponent<RectTransform>();
        d       = GameObject.Find("damageBar").GetComponent<RectTransform>();
        s       = GameObject.Find("speedBar").GetComponent<RectTransform>();
        j       = GameObject.Find("jumpBar").GetComponent<RectTransform>();
        player = null;
    }
	
	// Update is called once per frame
	void Update () {
        if (player == null)
            tryGetPlayer();
        else {
            t.sizeDelta = new Vector2(20, player.getToughness() * 100);
            d.sizeDelta = new Vector2(20, player.getDamage() * 100);
            s.sizeDelta = new Vector2(20, player.getSpeed() * 100);
            j.sizeDelta = new Vector2(20, player.getJump() * 100);
        }
    }

    void tryGetPlayer() { //Due to networking, this is needed because this gameobject will spawn before the player
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.GetComponent<PlayerEffects>();
    }
}
