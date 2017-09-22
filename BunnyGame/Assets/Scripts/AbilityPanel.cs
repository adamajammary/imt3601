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
        setupPanel();
    }

    // Update is called once per frame
    void Update () {
        updatePanel();
    }

    public void setupPanel() {
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
        displayImages();
        displayNames();
    }

    // Displays the name of abilities where no image is found
    public void displayNames() {
        for (int i = 0; i < _abilities.Count; i++) {
            SpecialAbility ability = _playerController.abilities[i];
            if (ability.imagePath == "")
                GetComponent<RectTransform>().GetComponentInChildren<Text>().text = ability.name.Replace(" ", "\n");
            else
                GetComponent<RectTransform>().GetComponentInChildren<Text>().text = "";
        }
    }

    // Display the ability's image
    public void displayImages()
    {
        for (int i = 0; i < _abilities.Count; i++) {
            SpecialAbility ability = _playerController.abilities[i];
            if (ability.imagePath != "") {
                Sprite s = Sprite.Create(
                    Resources.Load<Texture2D>(ability.imagePath), 
                    new Rect(new Vector2(0, 0), new Vector2(128,128)), 
                    new Vector2(0.5f, 0.5f)
                );
                _abilities[i].GetComponent<Image>().overrideSprite = s;
            }
        }
    }


    public void updatePanel()
    {
        for(int i = 0; i < _abilities.Count; i++) {
            GameObject cooldownOverlay = _abilities[i].transform.GetChild(0).gameObject;
            RectTransform cort = cooldownOverlay.GetComponent<RectTransform>();
            //cort.localScale = new Vector2(cort.localScale.x, _playerController.abilities[i].getCooldownPercent());
            cort.anchorMax = new Vector2(cort.anchorMax.x, _playerController.abilities[i].getCooldownPercent());
            cort.offsetMax = new Vector2(0, 0);
        }
    }


}
