using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : NetworkBehaviour {
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
        if (this.isServer)
            StartCoroutine(serverInit());
    }


    void Update() {
        if(this.weatherType == WeatherType.RAIN) {
            _rainEmitter.transform.position = _camera.transform.position + new Vector3(0, 50, 0);
            _rainSound.volume = (PlayerPrefs.GetFloat("Master Volume", 100) / 100f) * (PlayerPrefs.GetFloat("Effect Volume", 100) / 100f) * 0.25f;

            _rainSound.pitch = _player.inWater ? 0.2f : 1f;
        }
    }

    public IEnumerator serverInit() {
        int playerCount = UnityEngine.Object.FindObjectOfType<NetworkPlayerSelect>().numPlayers;
        while (playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1)) //When this is true, all clients are connected and in the game scene
            yield return 0;

        RpcInit((WeatherType)UnityEngine.Random.Range(0, 3));
    }

    [ClientRpc]
    public void RpcInit(WeatherType w) {
        init(w);
    }


    public void init(WeatherType w) {
        weatherType = w;
        switch (w) {
            case WeatherType.CLEAR:
                break;
            case WeatherType.FOG:
                setFog();
                break;
            case WeatherType.RAIN:
                setRain();
                break;
        }
    }

    private void setFog(float density = 0.05f) {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = new Color(0.75f, 0.75f, 0.75f);
        RenderSettings.fogMode = FogMode.Exponential;
        GameObject.Find("Main Light").GetComponent<Light>().color = new Color(0.8f, 0.8f, 0.8f);
        RenderSettings.skybox = fogSkyBox;
    }

    private void setRain() {
        _rainEmitter = Instantiate(Resources.Load<GameObject>("Prefabs/Rain"));
        _rainEmitter.SetActive(false);
        _rainSound = GameObject.Find("Main Camera").AddComponent<AudioSource>();
        _rainSound.clip = Resources.Load<AudioClip>("Audio/rain-03");
        _rainSound.loop = true;
        _rainSound.volume = 0;

        _camera = GameObject.Find("Main Camera");
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();


        setFog(0.005f);
        _rainEmitter.SetActive(true);
        _rainSound.Play();
    }
}
