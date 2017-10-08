using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public Font settingsFont;

    private int optionHeight = -50; // value must be negative, as we are working downwards instead of upwards, which unity ui does


	void Start () {
        createSettings();
        close();
    }
	

    public void open() {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void close() {
        transform.GetChild(0).gameObject.SetActive(false);
    }


    private void createSettings() {
        GameObject panel = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject; // Yep

        GameObject testSection = addSection("Section 1", panel);
        GameObject textoptionTest = addTextOption("option1.1", testSection, "placeholder text");
        GameObject textoptionTest2 = addTextOption("option1.2", testSection, "placeholder text");

        //GameObject section2 = addSection("Section 2", panel);
        //GameObject textoptionTest3 = addTextOption("option2.1", testSection, "placeholder text");




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

        RectTransform rt = uiObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);

        return uiObject;
    }

    private GameObject addSection(string title, GameObject parent) {
        GameObject sectionPanel = createBaseUIObject("Section: " + title, parent);

        RectTransform rt = sectionPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, optionHeight);
        rt.offsetMax = new Vector2(0, 0);

        // Set section width, background etc..
        // Height will be set during packing

        GameObject sectionTitle = createBaseUIObject("Section title: " + title, sectionPanel);
        sectionTitle.AddComponent<Text>().text = title;
        sectionTitle.GetComponent<Text>().font = this.settingsFont;

        rt = sectionTitle.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, optionHeight);
        rt.offsetMax = new Vector2(0, 0);
        // Set title font size, color, positioning, etc...


        return sectionPanel;
    }

    private GameObject addBasicOption(string text, GameObject parent) {
        GameObject option = createBaseUIObject("Option panel: " + text, parent);
        RectTransform rt = option.GetComponent<RectTransform>();
        // Set up size, position, etc...
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, optionHeight * parent.transform.childCount);
        rt.offsetMax = new Vector2(0, optionHeight * (parent.transform.childCount - 1));



        GameObject optionText = createBaseUIObject("Option Text: " + text, option);
        Text textComponent = optionText.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = settingsFont;

        rt = optionText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);

        // Set up size, position, font size, font color, etc...

        GameObject interactiveElement = createBaseUIObject("Option: " + text, option);
        // Set up size and position etc...


        return interactiveElement;
    }


    private GameObject addTextOption(string optionName, GameObject parent, string placeholderText = "") {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        InputField inf = option.AddComponent<InputField>();
        inf.targetGraphic = option.GetComponent<Image>();

        RectTransform rt = option.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);


        GameObject text = createBaseUIObject("Text: " + optionName, option);
        Text textComponent = text.AddComponent<Text>();
        textComponent.font = settingsFont;
        textComponent.color = new Color(0,0,0);

        rt = text.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);


        GameObject placeholder = createBaseUIObject("Placeholder: " + optionName, option);
        Text placeholderTextComponent = placeholder.AddComponent<Text>();
        placeholderTextComponent.font = settingsFont;
        placeholderTextComponent.text = placeholderText;
        placeholderTextComponent.fontStyle = FontStyle.Italic;
        placeholderTextComponent.color = new Color(.5f, .5f, .5f);

        rt = placeholder.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);



        inf.textComponent = text.GetComponent<Text>();
        inf.placeholder = placeholder.GetComponent<Text>();


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
