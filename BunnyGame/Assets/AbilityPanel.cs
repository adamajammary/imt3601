using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanel : MonoBehaviour {
    public PlayerController _playerController;

    public List<GameObject> _abilities;
    
    // Use this for initialization
	void Start () {
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        updatePanel();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void updatePanel() {
        _abilities = new List<GameObject>();
        GameObject abilityIcon = Resources.Load<GameObject>("Prefabs/AbilityIcon");
        int numAbilities = _playerController.abilities.Count;
        for (int i = 0; i < numAbilities; i++) {
            _abilities.Add(Instantiate(abilityIcon));
            _abilities[i].name = _playerController.abilities[i].abilityName;
            RectTransform rt = _abilities[i].GetComponent<RectTransform>();

            rt.SetParent(GetComponent<RectTransform>());
            rt.localPosition = new Vector2(50 * (i - numAbilities/2), 34);
            rt.localScale = new Vector2(1, 1);
        }
        displayNames();
    }

    public void displayNames() {
        foreach (GameObject ability in _abilities) {
            GetComponent<RectTransform>().GetComponentInChildren<Text>().text = ability.name;
        }
    }
}
