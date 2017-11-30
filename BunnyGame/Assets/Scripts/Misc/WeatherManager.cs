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

    private GameObject _rainEmitter;
    private AudioSource _rainSound;
    private GameObject _camera;
    private PlayerController _player;


    private WeatherType weatherType = WeatherType.CLEAR;

    void Start() {
        _rainEmitter = Instantiate(Resources.Load<GameObject>("Prefabs/Rain"));
        _rainEmitter.SetActive(false);
        _rainSound = GameObject.Find("Main Camera").AddComponent<AudioSource>();
        _rainSound.clip = Resources.Load<AudioClip>("Audio/rain-03");
        _rainSound.loop = true;
        _rainSound.volume = 0;

        _camera = GameObject.Find("Main Camera");
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.F))
            init(WeatherType.FOG);

        if(this.weatherType == WeatherType.FOG) {
            if (Input.GetKeyDown(KeyCode.O))
                setFog(RenderSettings.fogDensity + 0.01f);
            if (Input.GetKeyDown(KeyCode.L))
                setFog(RenderSettings.fogDensity - 0.01f);
        }


        if (Input.GetKeyDown(KeyCode.R))
            init(WeatherType.RAIN);

        if(this.weatherType == WeatherType.RAIN) {
            _rainEmitter.transform.position = _camera.transform.position + new Vector3(0, 50, 0);
            _rainSound.volume = (PlayerPrefs.GetFloat("Master Volume", 100) / 100f) * (PlayerPrefs.GetFloat("Effect Volume", 100) / 100f) * 0.25f;

            _rainSound.pitch = _player.inWater ? 0.2f : 1f;
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
                setRain();
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

    private void setRain() {
        setFog(0.005f);
        _rainEmitter.SetActive(true);
        _rainSound.Play();

    }
}
