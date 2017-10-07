using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
	// Use this for initialization
	void Start () {
        createSettings();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void createSettings() {
        GameObject panel = transform.GetChild(0).gameObject;

        GameObject nameSection = addSection("Default name", panel);
        GameObject customDefaultName = addTextOption("", nameSection);

        GameObject soundSection = addSection("Sound", panel);
        GameObject masterVolume = addSliderOption("Master Volume", soundSection, 0, 100);
        GameObject musicVolume = addSliderOption("Music Volume", soundSection, 0, 100);

        GameObject cameraSection = addSection("Camera", panel);
        GameObject fov = addSliderOption("FOV", cameraSection, 50, 150);
        GameObject mouseSensitivity = addSliderOption("Mouse Sensitivity", cameraSection, 0, 100);

        GameObject videoSettings = addSection("Video", panel);
        GameObject resolution = addDropdownOption("Resolution", panel, new string[]{"1920x1080","1280x720"});




        pack(panel);
    }

    public GameObject addSection(string title, GameObject parent) {
        GameObject sectionPanel = Resources.Load<GameObject>("Prefabs/Settings/Section");

        sectionPanel.transform.SetParent(parent.transform);
        return sectionPanel;
    }

    // Resize the panel and sections inside so that they fit the number of settings and size of the window
    private void pack(GameObject panel) {
        // Starting at 1 becuase child 0 is the Settings window title
        for(int i = 1; i < panel.transform.childCount; i++) {
            int sectionItems = panel.transform.GetChild(i).childCount - 1; // Child 1 is section title
            
            // TODO set the size of the panel based on how many children it has

        }

        // TODO Set the size of the panel so that it fits as many settings as possible at once without being to big for the window

    }

    // Creates a basic ui object with a rect transform and a canvas renderer
    private GameObject createBaseUIObject()
    {
        GameObject uiObject = new GameObject();
        uiObject.AddComponent<RectTransform>();
        uiObject.AddComponent<CanvasRenderer>();
        return uiObject;
    }

    private GameObject addBasicOption(string text, GameObject parent) {
        GameObject option = createBaseUIObject();
        // Set up size, position, etc...

        GameObject optionText = createBaseUIObject();
        optionText.transform.SetParent(option.transform);
        optionText.AddComponent<Text>();
        optionText.GetComponent<Text>().text = text;
        // Set up size, position, font size, font color, etc...

        GameObject interactiveElement = createBaseUIObject();
        interactiveElement.transform.SetParent(option.transform);
        // Set up size and position etc...


        return interactiveElement;
    }

    public GameObject addDropdownOption(string name, GameObject parent, string[] elements) {
        GameObject optionpanel = addBasicOption(name, parent);


        GameObject dropdown = Resources.Load<GameObject>("Prefabs/Settings/Dropdown");
        dropdown.transform.SetParent(parent.transform);
        dropdown.GetComponent<Dropdown>();

        return dropdown;
    }

    public GameObject addTextOption(string name, GameObject parent, string placeholderText = "") {
        GameObject textOption = Resources.Load<GameObject>("Prefabs/Settings/TextOption");
        textOption.transform.SetParent(parent.transform);

        return textOption;
    }

    public GameObject addSliderOption(string name, GameObject parent, int minval, int maxval) {
        GameObject sliderOption = Resources.Load<GameObject>("Prefabs/Settings/Slider");
        sliderOption.transform.SetParent(parent.transform);

        return sliderOption;
    }

    public GameObject addToggleOption(string name, GameObject parent) {
        GameObject toggle = Resources.Load<GameObject>("Prefabs/Settings/Toggle");
        toggle.transform.SetParent(parent.transform);

        return toggle;
    }
}
