using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpectatorMode
{
    FREE,
    FOLLOW
}

public class SpectatorController : MonoBehaviour {
    private SpectatorMode _spectatorMode;
    

    private ThirdPersonCamera _thirdPersonCamera;
    private int _currentPlayerIdx = 0;
    List<GameObject> _players = new List<GameObject>(); // Should contain only live players
    
    private float moveSpeed = 10;
    private float currentSpeed = 0;

    private GameObject _freeCameraTarget;

    private float _yaw = 0;
    private float _pitch = 0;
    private float _speedSmoothVelocity;

    bool aaa = false;


    // Use this for initialization
    void Start () {
        _thirdPersonCamera = GetComponent<ThirdPersonCamera>();

        _freeCameraTarget = new GameObject() {
            name = "Spectator Camera [Free Mode]"
        };
        _freeCameraTarget.AddComponent<CharacterController>();


        //this.enabled = false; // ONLY DISABLED FOR TESTING
    }
	
	// Update is called once per frame
	void Update () {
        if(_thirdPersonCamera.getTarget() != null && !aaa) { // ONLY HERE FOR TESTING
            aaa = true;
            startSpectating();
        }

        if (_spectatorMode == SpectatorMode.FREE) {
            freeMove();

        } else if (_spectatorMode == SpectatorMode.FOLLOW) {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                switchPlayerView(_currentPlayerIdx + 1);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                switchPlayerView(_currentPlayerIdx - 1);
        }

        // Switch spectating mode with space
        if (Input.GetKeyDown(KeyCode.Space))
            setSpectatorMode((SpectatorMode)((int)~_spectatorMode & 1));
	}

    // This should be called when the players dies and wants to go into spectating mode
    public void startSpectating() {
        Debug.Log("Start spectating");

        this.enabled = true;
        setSpectatorMode(SpectatorMode.FREE);

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
        Debug.Log("Set spectator mode to free mode");

        _spectatorMode = SpectatorMode.FREE;
        _thirdPersonCamera.enabled = false;

        _freeCameraTarget.SetActive(true);
        _freeCameraTarget.transform.position = _thirdPersonCamera.getTarget().position;
        transform.SetParent(_freeCameraTarget.transform);
        transform.localPosition = Vector3.zero;
        Debug.Log("Setting " + _freeCameraTarget.name + " as parent for " + name);
    }

    private void freeMove() {
        // ROTATE:
        _yaw += Input.GetAxis("Mouse X") * _thirdPersonCamera.getSensitivity();
        _pitch -= Mathf.Clamp(Input.GetAxis("Mouse Y") * _thirdPersonCamera.getSensitivity(),
                              -89, 89);

        transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, new Vector3(_pitch, _yaw), 
            ref _thirdPersonCamera._rotationSmoothVelocity, _thirdPersonCamera._rotationSmoothTime);


        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, moveSpeed, ref _speedSmoothVelocity, 0.2f);

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
        Debug.Log("Set spectator mode to follow mode");

        _spectatorMode = SpectatorMode.FOLLOW;
        _thirdPersonCamera.enabled = true;
        switchPlayerView(_currentPlayerIdx);
    }


    // Switch what player you're following
    // index: index of the player to view
    private void switchPlayerView(int index) {
        index %= _players.Count;
        transform.SetParent(_players[index].transform);
        transform.localPosition = new Vector3(0, 0, 0);
        _thirdPersonCamera.enabled = true;
        Debug.Log("Switch player view to player " + index);
    }
}
