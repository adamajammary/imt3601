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


    // Ambient
    private AudioSource _ocean;
    private AudioSource _ambientNature;
    private AudioSource _pond;


    // Firewall
    private FireWall _firewall;
    private AudioSource _fire;

    private bool _isUnderWater = false;



	void Start () {
        updateVolume();

        // OCEAN:
        GameObject oceanObj = new GameObject {
            name = "OceanSound"
        };
        _ocean = oceanObj.AddComponent<AudioSource>();
        _ocean.clip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        _ocean.Play();
        _ocean.loop = true;

        // FIRE:
        GameObject fireObj = new GameObject {
            name = "FireSound"
        };
        _fire = fireObj.AddComponent<AudioSource>();
        //_fire.clip = Resources.Load<AudioClip>("Audio/...");
        _fire.Play();
        _fire.loop = true;



	}
	
	void Update () {
        _isUnderWater = cameraTransform.position.y < waterLevel;

        updateOceanSound();
        updateFireSound();

	}


    private void updateOceanSound() {
        float distFromCenter = Vector2.Distance(
            new Vector2(this.cameraTransform.position.x, this.cameraTransform.position.z), 
            Vector2.zero);
        float distFromOcean = mapRadius - distFromCenter;

        _ocean.volume = (Mathf.Max((distFromOcean > 0 ? 3f / distFromOcean : 1f), .05f) / 2f) * effectVolume;
        _ocean.pitch = _isUnderWater ? 0.2f : 1f;


        // TODO : Surround/Stereo?
        // _ocean.panStereo = ...
    }

    private void updateFireSound() {
        // No updating if wall hasn't been created yet
        if (_firewall == null) {
            GameObject wall = GameObject.FindGameObjectWithTag("FireWall");
            if (wall == null)
                return;
            else
                _firewall = wall.GetComponent<FireWall>();
        }


        Vector2 firewallcenter = new Vector2(_firewall.transform.position.x, _firewall.transform.position.z);

        float distFromCenter = Vector2.Distance(
            new Vector2(cameraTransform.position.x, cameraTransform.position.z),
            firewallcenter);

        float distFromWall = Mathf.Abs(_firewall.transform.localScale.x/2 - distFromCenter);

        _fire.volume = (Mathf.Max((distFromWall > 0 ? 0.5f / distFromWall : 0.5f), .0005f) / 2f) * effectVolume;
        _fire.pitch = _isUnderWater ? 0.2f : 1f;

        // TODO : Surround/Stereo?
        // _fire.panStereo = ...
    }



    // Sets the volume values based on player settings
    public void updateVolume() {
        masterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f;
        musicVolume = PlayerPrefs.GetFloat("Music Volume", 1)/100f * masterVolume;
        effectVolume = PlayerPrefs.GetFloat("Effect Volume", 100)/100f * masterVolume;
    }

}
