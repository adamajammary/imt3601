using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public Font settingsFont;

    private int optionHeight = -50; // value must be negative, as we are working downwards instead of upwards, which unity ui does

    private Dictionary<string, string> stringValues = new Dictionary<string, string>();
    private Dictionary<string, float> floatValues = new Dictionary<string, float>();
    private Dictionary<string, int> intValues = new Dictionary<string, int>();


    void Start () {
        createSettingsMenu();
        close();


    }
	

    public void open() {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void close() {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void save()
    {
        foreach (KeyValuePair<string, string> entry in stringValues)
            PlayerPrefs.SetString(entry.Key, entry.Value);
        foreach (KeyValuePair<string, float> entry in floatValues)
            PlayerPrefs.SetFloat(entry.Key, entry.Value);
        foreach (KeyValuePair<string, int> entry in intValues)
            PlayerPrefs.SetInt(entry.Key, entry.Value);

        PlayerPrefs.Save();
    }

    private void updateDict<value_t>(Dictionary<string, value_t> dict, string key, value_t value) {
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);

        save();
    }


    private void createSettingsMenu() {
        GameObject panel = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject; // Yep
       
        GameObject videoSettings = addSection("Video", panel);
        GameObject resolution = addDropdownOption("Resolution", videoSettings, new string[]{"1920x1080","1280x720", "1024x768"});


        GameObject cameraSection = addSection("Camera", panel);
        GameObject fov = addSliderOption("FOV", cameraSection, 50, 150);
        GameObject mouseSensitivity = addSliderOption("Mouse Sensitivity", cameraSection, 0, 100);


        GameObject soundSection = addSection("Sound", panel);
        GameObject masterVolume = addSliderOption("Master Volume", soundSection, 0, 100);
        GameObject musicVolume = addSliderOption("Music Volume", soundSection, 0, 100);


        // TODO: Add save and cancel button
        // TODO: Add a blocker, so you can't click on anything else while the settings panel is open

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
        // Height will be set during packing
        

        GameObject sectionTitle = createBaseUIObject("Section title: " + title, sectionPanel);
        Text titleText = sectionTitle.AddComponent<Text>();
        titleText.text = title;
        titleText.font = this.settingsFont;
        titleText.fontSize = 25;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        rt = sectionTitle.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, optionHeight);
        rt.offsetMax = new Vector2(0, 0);


        return sectionPanel;
    }

    private GameObject addBasicOption(string text, GameObject parent) {
        GameObject option = createBaseUIObject("Option panel: " + text, parent);
        RectTransform rt = option.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, optionHeight * parent.transform.childCount);
        rt.offsetMax = new Vector2(0, optionHeight * (parent.transform.childCount - 1));

        float titleSpace = (text == "" ? 0 : 0.5f);
        if (text == "")
            titleSpace = 0;


        GameObject optionText = createBaseUIObject("Option Text: " + text, option);
        Text textComponent = optionText.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = settingsFont;
        rt = optionText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(titleSpace, 1);
        rt.offsetMin = new Vector2(5, 5);
        rt.offsetMax = new Vector2(-5, -5);

        GameObject interactiveElement = createBaseUIObject("Option: " + text, option);
        rt = interactiveElement.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(titleSpace, 0);
        rt.offsetMax = new Vector2(-15, -15);
        rt.offsetMin = new Vector2(15, 15);

        return interactiveElement;
    }

    private GameObject addTextOption(string optionName, GameObject parent, string placeholderText = "") {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        InputField inf = option.AddComponent<InputField>();
        inf.targetGraphic = option.GetComponent<Image>();


        GameObject text = createBaseUIObject("Text: " + optionName, option);
        Text textComponent = text.AddComponent<Text>();
        textComponent.font = settingsFont;
        textComponent.color = new Color(0,0,0);
        textComponent.alignment = TextAnchor.MiddleLeft;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 0);
        rt.offsetMax = new Vector2(-5, 0);


        GameObject placeholder = createBaseUIObject("Placeholder: " + optionName, option);
        Text placeholderTextComponent = placeholder.AddComponent<Text>();
        placeholderTextComponent.font = settingsFont;
        placeholderTextComponent.text = placeholderText;
        placeholderTextComponent.fontStyle = FontStyle.Italic;
        placeholderTextComponent.color = new Color(.5f, .5f, .5f);
        placeholderTextComponent.alignment = TextAnchor.MiddleLeft;

        rt = placeholder.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 0);
        rt.offsetMax = new Vector2(-5, 0);


        inf.textComponent = text.GetComponent<Text>();
        inf.placeholder = placeholder.GetComponent<Text>();

        // Call function updateDict() when the text changes
        inf.onValueChanged.AddListener(delegate { updateDict(stringValues, optionName, inf.text); });
        // Load initial value from settings
        inf.text = PlayerPrefs.GetString(optionName, "");


        return option;
    }

    // TODO: Actually do something with min and max value
    private GameObject addSliderOption(string optionName, GameObject parent, int minval, int maxval) {
        GameObject option = addBasicOption(optionName, parent);
        Slider slider = option.AddComponent<Slider>();

        GameObject background = createBaseUIObject("Background: " + optionName, option);
        background.AddComponent<Image>();

        GameObject fillArea = createBaseUIObject("Fill Area: " + optionName, option);
        GameObject fill = createBaseUIObject("Fill: " + optionName, fillArea);
        fill.AddComponent<Image>();

        GameObject handleSlideArea = createBaseUIObject("Handle Slide Area: " + optionName, option);
        GameObject handle = createBaseUIObject("Handle: " + optionName, handleSlideArea);
        handle.AddComponent<Image>().color = new Color(1, 0.5f, 0.5f);
        RectTransform rt = handle.GetComponent<RectTransform>();
        rt.offsetMax = new Vector2(5, 0);
        rt.offsetMin = new Vector2(-5, 0);


        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();

        // Call function updateDict() when the value changes
        slider.onValueChanged.AddListener(delegate { updateDict(floatValues, optionName, slider.value); });
        // Load initial value from settings
        slider.value = PlayerPrefs.GetFloat(optionName, minval + (maxval-minval)/2);

        return option;
    }


    private GameObject addDropdownOption(string optionName, GameObject parent, string[] elements) {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        Dropdown dropdown = option.AddComponent<Dropdown>();
        foreach(string element in elements)
            dropdown.options.Add(new Dropdown.OptionData(element));


        GameObject label = createBaseUIObject("Label: " + optionName, option);
        Text labelText = label.AddComponent<Text>();
        labelText.text = "";
        labelText.font = settingsFont;
        labelText.color = new Color(0, 0, 0);
        RectTransform rt = label.GetComponent<RectTransform>();


        GameObject arrow = createBaseUIObject("Arrow: " + optionName, option);
        arrow.AddComponent<Image>().color = new Color(.5f, .5f, .5f);
        rt = arrow.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.offsetMin = new Vector2(-20, 0);

        GameObject template = createBaseUIObject("Template", option);
        template.AddComponent<Image>();
        ScrollRect templateSR = template.AddComponent<ScrollRect>();
        rt = template.GetComponent<RectTransform>();
        rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(0, -20 * elements.Length);
        template.SetActive(false);


        GameObject viewport = createBaseUIObject("Viewport", template);
        viewport.AddComponent<Mask>();
        viewport.AddComponent<Image>();

        GameObject content = createBaseUIObject("Content", viewport);
        rt = content.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, -20);

        GameObject item = createBaseUIObject("Item", content);
        Toggle itemToggle = item.AddComponent<Toggle>();

        GameObject itemBackground = createBaseUIObject("Item Background", item);
        itemBackground.AddComponent<Image>();

        GameObject itemHighlight = createBaseUIObject("Item Highlight", item);
        itemHighlight.AddComponent<Image>();

        GameObject itemLabel = createBaseUIObject("Item Label", item);
        Text itemLabelText = itemLabel.AddComponent<Text>();
        itemLabelText.font = settingsFont;
        itemLabelText.color = new Color(0, 0, 0);

        itemToggle.targetGraphic = itemBackground.GetComponent<Image>();
        itemToggle.graphic = itemHighlight.GetComponent<Image>();

        templateSR.content = content.GetComponent<RectTransform>();
        templateSR.viewport = viewport.GetComponent<RectTransform>();


        dropdown.captionText = label.GetComponent<Text>();
        dropdown.targetGraphic = dropdown.GetComponent<Image>();
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = itemLabelText;

        // Call function updateDict() when the value changes
        dropdown.onValueChanged.AddListener(delegate { updateDict(intValues, optionName, dropdown.value); });
        // Load initial value from settings
        dropdown.value = PlayerPrefs.GetInt(optionName, 0);


        return option;
    }


    // Resize the panel and sections inside so that they fit the number of settings and size of the window
    private void pack(GameObject panel) {
        int totalHeight = 0;
        

        for (int i = 0; i < panel.transform.childCount; i++) {
            RectTransform sectionRT = panel.transform.GetChild(i).gameObject.GetComponent<RectTransform>();
            int sectionItems = panel.transform.GetChild(i).childCount;
            sectionRT.offsetMin = new Vector2(0, totalHeight + sectionItems * optionHeight);
            sectionRT.offsetMax = new Vector2(0, totalHeight);
            totalHeight += sectionItems * optionHeight;

        }

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, totalHeight);
    }


}
