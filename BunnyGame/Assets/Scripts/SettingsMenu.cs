using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
	// Use this for initialization

	void Start () {
        createSettings();
        close();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void open() {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void close() {
        transform.GetChild(0).gameObject.SetActive(false);
    }


    private void createSettings() {
        GameObject panel = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject; // Yep

        GameObject testSection = addSection("Test section", panel);
        GameObject textoptionTest = addTextOption("option name", testSection, "placeholder text");


        //GameObject nameSection = addSection("Default name", panel);
        //GameObject customDefaultName = addTextOption("", nameSection);

        //GameObject soundSection = addSection("Sound", panel);
        //GameObject masterVolume = addSliderOption("Master Volume", soundSection, 0, 100);
        //GameObject musicVolume = addSliderOption("Music Volume", soundSection, 0, 100);

        //GameObject cameraSection = addSection("Camera", panel);
        //GameObject fov = addSliderOption("FOV", cameraSection, 50, 150);
        //GameObject mouseSensitivity = addSliderOption("Mouse Sensitivity", cameraSection, 0, 100);

        //GameObject videoSettings = addSection("Video", panel);
        //GameObject resolution = addDropdownOption("Resolution", panel, new string[]{"1920x1080","1280x720"});


        // Add save and cancel button

        pack(panel);
    }

    // Creates a basic ui object with a rect transform and a canvas renderer
    private GameObject createBaseUIObject(string objectName = "Unnamed", GameObject parent = null)
    {
        GameObject uiObject = new GameObject();
        uiObject.AddComponent<RectTransform>();
        uiObject.AddComponent<CanvasRenderer>();
        uiObject.name = objectName;
        if(parent != null)
            uiObject.transform.SetParent(parent.transform);

        return uiObject;
    }

    private GameObject addSection(string title, GameObject parent) {
        GameObject sectionPanel = createBaseUIObject("Section: " + title, parent);

        // Set section width, background etc..
        // Height will be set during packing

        GameObject sectionTitle = createBaseUIObject("Section title: " + title, sectionPanel);
        sectionTitle.AddComponent<Text>().text = title;
        // Set title font size, color, positioning, etc...


        return sectionPanel;
    }

    private GameObject addBasicOption(string text, GameObject parent) {
        GameObject option = createBaseUIObject("Option panel: " + text, parent);
        // Set up size, position, etc...

        GameObject optionText = createBaseUIObject("Option Text: " + text, option);
        optionText.AddComponent<Text>().text = text;
        // Set up size, position, font size, font color, etc...

        GameObject interactiveElement = createBaseUIObject("Option: " + text, option);
        // Set up size and position etc...


        return interactiveElement;
    }


    private GameObject addTextOption(string optionName, GameObject parent, string placeholderText = "") {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        option.AddComponent<InputField>();

        GameObject text = createBaseUIObject("Text: " + optionName, option);
        text.AddComponent<Text>();

        GameObject placeholder = createBaseUIObject("Placeholder: " + optionName, option);
        placeholder.AddComponent<Text>();

        return option;
    }

    private GameObject addDropdownOption(string optionName, GameObject parent, string[] elements) {
        GameObject option = addBasicOption(optionName, parent);

        return option;
    }

    private GameObject addSliderOption(string optionName, GameObject parent, int minval, int maxval) {
        GameObject option = addBasicOption(optionName, parent);

        return option;
    }


    private GameObject addToggleOption(string optionName, GameObject parent) {
        GameObject option = addBasicOption(optionName, parent);

        return option;
    }


    // Resize the panel and sections inside so that they fit the number of settings and size of the window
    private void pack(GameObject panel)
    {
        // Starting at 1 becuase child 0 is the Settings window title
        for (int i = 1; i < panel.transform.childCount; i++) {
            int sectionItems = panel.transform.GetChild(i).childCount - 1; // Child 1 is section title

            // TODO set the size of the panel based on how many children it has

        }

        // TODO Set the size of the panel so that it fits as many settings as possible at once without being to big for the window

    }


}
