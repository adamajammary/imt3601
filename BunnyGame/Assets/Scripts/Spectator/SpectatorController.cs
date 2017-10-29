using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpectatorMode
{
    FREE,
    FOLLOW
}

public class SpectatorController : MonoBehaviour {
    private SpectatorMode _spectatorMode;
    private SpectatorUI _ui;

    public GameObject body;
    

    private ThirdPersonCamera _thirdPersonCamera;
    private int _currentPlayerIdx = 0;
    public List<GameObject> _players = new List<GameObject>(); // Should contain only live players
    
    private float moveSpeed = 10;
    private float currentSpeed = 0;

    private GameObject _freeCameraTarget;

    private float _yaw = 0;
    private float _pitch = 0;
    private float _speedSmoothVelocity;


    // Use this for initialization
    void Start () {
        _ui = GameObject.Find("SpectatorUI").GetComponent<SpectatorUI>();
        _ui.gameObject.SetActive(false);

        _thirdPersonCamera = GetComponent<ThirdPersonCamera>();

        _freeCameraTarget = new GameObject() {
            name = "Spectator Camera [Free Mode]"
        };
        _freeCameraTarget.AddComponent<CharacterController>();


        this.enabled = false;
    }

    // Update is called once per frame
    void Update () {

        if (_spectatorMode == SpectatorMode.FREE) {
            freeMove();

        } else if (_spectatorMode == SpectatorMode.FOLLOW) {
            if (Input.GetKeyDown(KeyCode.PageUp))
                switchPlayerView(_currentPlayerIdx + 1);
            else if (Input.GetKeyDown(KeyCode.PageDown))
                switchPlayerView(_currentPlayerIdx - 1);
        }

        // Switch spectating mode with Home-key
        if (Input.GetKeyDown(KeyCode.Home))
            setSpectatorMode((SpectatorMode)((int)~_spectatorMode & 1));

        body.transform.position = this.transform.position;

	}

    // This should be called when the players dies and wants to go into spectating mode
    public void startSpectating() {
        this.enabled = true;
        _ui.gameObject.SetActive(true);
        setSpectatorMode(SpectatorMode.FREE);

        _thirdPersonCamera.canFPS = false;
        gameObject.tag = "Spectator";

        // Disable ui elements that aren't necessary to keep after going into spectate mode
        foreach (string str in "SpectateButton BloodSplatterOverlay AbilityPanel AttributeUI".Split(' '))
            GameObject.Find(str).SetActive(false);


        // Get all players(enemies) and update it to only include living ones
        _players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        updatePlayers();
    }


    // Update the list of players to only include living ones
    private void updatePlayers() {
        _players = _players.Where(player => !player.GetComponent<PlayerHealth>().IsDead()).ToList();
    }



    private void setSpectatorMode(SpectatorMode mode) {
        switch (mode) {
            case SpectatorMode.FOLLOW:
                setFollowCameraMode();
                break;
            case SpectatorMode.FREE:
                setFreeCameraMode();
                break;
            default:
                throw new InvalidOperationException("This should not happen!");
        }
    }


    /**
     * 
     * FREE MOVE MODE
     * Allows the spectator to move freely around the arena
     * 
     **/

    private void setFreeCameraMode() {

        _spectatorMode = SpectatorMode.FREE;
        _ui.modeSwitch(SpectatorMode.FREE);
        _thirdPersonCamera.enabled = false;

        _freeCameraTarget.SetActive(true);
        _freeCameraTarget.transform.position = transform.position;// _thirdPersonCamera.getTarget().position;
        transform.SetParent(_freeCameraTarget.transform);

        transform.localPosition = Vector3.zero;
    }


    private void freeMove() {
        // ROTATE:
        _yaw += Input.GetAxis("Mouse X") * _thirdPersonCamera.getSensitivity();
        _pitch -= Input.GetAxis("Mouse Y") * _thirdPersonCamera.getSensitivity();
        _pitch = Mathf.Clamp(_pitch, -80, 80);

        transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, new Vector3(_pitch, _yaw), 
            ref _thirdPersonCamera._rotationSmoothVelocity, _thirdPersonCamera._rotationSmoothTime);


        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, (Input.GetKey(KeyCode.LeftShift) ? moveSpeed * 2 : moveSpeed), ref _speedSmoothVelocity, 0.2f);

        // MOVE:
        Vector3 direction = new Vector3(
            Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A)), // Left/Right
            Convert.ToSingle(Input.GetKey(KeyCode.Q)) - Convert.ToSingle(Input.GetKey(KeyCode.E)), // Up/Down
            Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S))  // Forward/Backward
        );


        Vector3 velocity = transform.TransformDirection(direction).normalized * currentSpeed;

        _freeCameraTarget.GetComponent<CharacterController>().Move(velocity * Time.deltaTime);
    }

    /**
     *
     * FOLLOW MODE
     * Makes the spectator follow a live player
     * 
     **/

    private void setFollowCameraMode() {
        _spectatorMode = SpectatorMode.FOLLOW;
        _ui.modeSwitch(SpectatorMode.FOLLOW);
        _thirdPersonCamera.enabled = true;
        switchPlayerView(_currentPlayerIdx);
    }


    // Switch what player you're following
    // index: index of the player to view
    private void switchPlayerView(int index) {
        _currentPlayerIdx = (index + _players.Count) % _players.Count;
        updatePlayers();
        transform.localPosition = new Vector3(0, 0, 0);
        _thirdPersonCamera.enabled = true;
        _thirdPersonCamera.SetTarget(_players[_currentPlayerIdx].transform);
        _ui.followingPlayerChange(_players[_currentPlayerIdx].GetComponent<PlayerInformation>().playerName);
    }
}
