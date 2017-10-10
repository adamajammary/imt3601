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
        player  = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerEffects>();
    }
	
	// Update is called once per frame
	void Update () {
		t.sizeDelta = new Vector2(20, player.getToughness() * 100);
        d.sizeDelta = new Vector2(20, player.getDamage() * 100);
        s.sizeDelta = new Vector2(20, player.getSpeed() * 100);
        j.sizeDelta = new Vector2(20, player.getJump() * 100);
    }
}
