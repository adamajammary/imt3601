using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour {
    public Transform cameraTransform;
    public int mapRadius;
    public float waterLevel;
    
    private float masterVolume;
    private float musicVolume;
    private float effectVolume;


    // Ambience
    private AudioSource _ocean;
    private AudioSource _underWater;
    private AudioSource _ambientNature;
    private AudioSource _pond; 


    // Firewall
    private AudioSource fire;

    private bool _isUnderWater = false;



	void Start () {
        updateVolume();

        GameObject oceanObj = new GameObject();
        oceanObj.name = "OceanSound";
        _ocean = oceanObj.AddComponent<AudioSource>();
        AudioClip oceanClip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        _ocean.clip = oceanClip;
        _ocean.dopplerLevel = 0;
        _ocean.Play();
        _ocean.loop = true;


	}
	
	void Update () {
        _isUnderWater = cameraTransform.position.y < waterLevel;

        updateOceanSound();

	}


    private void updateOceanSound() {
        float distFromCenter = Mathf.Sqrt(this.cameraTransform.position.x * this.cameraTransform.position.x + this.cameraTransform.position.z * this.cameraTransform.position.z);
        float distFromOcean = mapRadius - distFromCenter;
        _ocean.volume = Mathf.Max((distFromOcean > 0 ? 3f / distFromOcean : 1f), .05f) / 2f;
        _ocean.volume *= effectVolume;
        Debug.Log(_ocean.volume + " :: " + distFromOcean);

        _ocean.pitch = _isUnderWater ? 0.2f : 1f;


        // TODO : Surround/Stereo?
        // _ocean.panStereo = ...
    }



    // Sets the volume values based on player settings
    public void updateVolume() {
        masterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f;
        musicVolume = PlayerPrefs.GetFloat("Music Volume", 1)/100f * masterVolume;
        effectVolume = PlayerPrefs.GetFloat("Effect Volume", 100)/100f * masterVolume;
    }

}
