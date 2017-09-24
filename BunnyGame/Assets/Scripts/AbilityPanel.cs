using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanel : MonoBehaviour {
    private PlayerController _playerController;

    private List<GameObject> _abilities = new List<GameObject>();

    // Update is called once per frame
    void FixedUpdate () {
        updatePanel();
    }

    // Call this whenever the Player gets a new ability
    public void setupPanel(PlayerController playerController) {
        _playerController = playerController;

        _abilities = new List<GameObject>();
        GameObject abilityIcon = Resources.Load<GameObject>("Prefabs/AbilityIcon");
        GameObject iconMask = new GameObject();



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

    // Displays the name of abilities who doesn't have an image
    public void displayNames() {
        for (int i = 0; i < _abilities.Count; i++) {
            SpecialAbility ability = _playerController.abilities[i];
            if (ability.getImagePath() == "")
                GetComponent<RectTransform>().GetComponentInChildren<Text>().text = ability.name.Replace(" ", "\n");
            else
                GetComponent<RectTransform>().GetComponentInChildren<Text>().text = "";
        }
    }

    // Display the abilities' images
    public void displayImages() {
        for (int i = 0; i < _abilities.Count; i++) {
            SpecialAbility ability = _playerController.abilities[i];
            if (ability.getImagePath() != "") {
                Sprite s = Sprite.Create(
                    Resources.Load<Texture2D>(ability.getImagePath()), 
                    new Rect(new Vector2(0, 0), new Vector2(128,128)), 
                    new Vector2(0.5f, 0.5f)
                );
                _abilities[i].GetComponent<Image>().overrideSprite = s;
            }
        }
    }

    // Updates the cooldown indicators for the abilities
    public void updatePanel() {
        for(int i = 0; i < _abilities.Count; i++) {
            GameObject cooldownOverlay = _abilities[i].transform.GetChild(0).gameObject;
            RectTransform cort = cooldownOverlay.GetComponent<RectTransform>();
            cort.anchorMax = new Vector2(cort.anchorMax.x, _playerController.abilities[i].getCooldownPercent());
            cort.offsetMax = new Vector2(0, 0);
        }
    }


}
