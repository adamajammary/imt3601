using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour {
    public enum WeatherType {
        CLEAR,
        FOG,
        RAIN
    };

    public Material normalSkyBox;
    public Material fogSkyBox;
    public Material rainSkyBox;



    private WeatherType weatherType = WeatherType.CLEAR;

    void Start() {

    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.F))
            init(WeatherType.FOG);

        if(this.weatherType == WeatherType.FOG) {
            if (Input.GetKeyDown(KeyCode.O))
                setFog(RenderSettings.fogDensity + 0.025f);
            if (Input.GetKeyDown(KeyCode.L))
                setFog(RenderSettings.fogDensity - 0.025f);
        }
    }

    public void init(WeatherType w) {
        weatherType = w;
        switch (w) {
            case WeatherType.CLEAR:
                initClear();
                break;
            case WeatherType.FOG:
                setFog();
                break;
            case WeatherType.RAIN:
                initRain();
                break;
        }
    }

    private void initClear() {

    }


    private void setFog(float density = 0.05f) {
        Debug.Log("Setting fog");
        RenderSettings.fog = true;
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = new Color(0.9f, 0.9f, 0.9f);
        RenderSettings.fogMode = FogMode.Exponential;
        GameObject.Find("Main Light").GetComponent<Light>().color = new Color(0.9f, 0.9f, 0.9f);
        RenderSettings.skybox = fogSkyBox;
    }

    private void initRain() {

    }
}
