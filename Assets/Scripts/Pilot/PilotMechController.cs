﻿using UnityEngine;
using System.Collections;
using CnControls;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PilotMechController : NetworkBehaviour
{
	// Metch physics
    public float speed;
    private Rigidbody rb;
    private RectTransform joystickTrans;
	private float moveH;
	private float moveV;
	private Vector3 statusTextOffset;
	private float nextFire;
	public float fireRate;

	// Mech related objects
	public GameObject bullet;
	public GameObject statusText;
	public Transform bulletSpawn;
	public GameObject gameManager;
	public GameObject serverData;

	// Mech team info
	public GameManager.Team team;
	public GameManager.Role role;
	    
	// Mech camera stuff
    private Vector3 camOffset;

	void Awake(){
		rb = GetComponent<Rigidbody>();
		Debug.Log ("Mech awake status text: " + statusText.GetComponent<TextMesh>().text);
		gameManager = GameObject.Find ("GameManager");


		// init team info on load
		if (gameManager != null) {
			var gameData = gameManager.GetComponent<GameManager> ();
			if (gameData != null) {
				team = gameData.getTeamSelection ();
				role = gameData.getRoleSelection ();
			}
		} else {
			Debug.Log ("game manager not found");
		}
	}


    void Start () {
		// set up physics
     
		statusTextOffset = transform.position - statusText.transform.position;

		// set up manager to pull data from game room

		// test set up string
		// statusText.GetComponent<TextMesh> ().text = GetComponent<Combat> ().health.ToString ();

		// set up camera
        SetCamera();

		// check if health is right
		Debug.Log("Mech start status text: " + statusText.GetComponent<TextMesh>().text);
    }

	/*
	public void loadStatusText(){
		if (!isLocalPlayer)
			return;
		int health = GetComponent<Combat> ().health;
		Debug.Log ("loaded health: " + health.ToString());
		statusText.GetComponent<TextMesh> ().text = health.ToString();
	}*/

    void Update () {
		if(!isLocalPlayer)
			return;

		statusText.transform.position = transform.position + statusTextOffset;

		moveH = CnInputManager.GetAxis("Horizontal");
        moveV = CnInputManager.GetAxis("Vertical");

        Move();
        Turn();
        Fire();
    }

    private void SetCamera () {
        if(isLocalPlayer) { 
            Camera camera = Camera.main;
            if (camera != null) {
                PilotCamera followScript = camera.GetComponent("PilotCamera") as PilotCamera;
                if (followScript != null) {
                    followScript.player = transform;
                }
            }
        }
    }

    private void Move () {
        Vector3 movement = new Vector3(moveH, 0.0f, moveV);
        rb.velocity = movement * speed;
    }

    private void Turn () {
        float angle = Mathf.Atan2(moveH, moveV) * Mathf.Rad2Deg;
        if (angle != 0) {
            rb.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }
		
	[Command]
	void CmdFire(){
		nextFire = Time.time + fireRate;

		var b = (GameObject)Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);

		b.GetComponent<Rigidbody> ().velocity = transform.forward * speed;
		NetworkServer.Spawn (b);
		Destroy (b, 2.0f);
	}

    void Fire () {
		// this is for touch
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                if (touch.position.x > (Screen.width / 2) && Time.time > nextFire) {
					CmdFire ();
                }
            }
        }

		// this is for mouse click
        if (Input.GetMouseButtonDown(0)) {
            if (Input.mousePosition.x > (Screen.width / 2) && Time.time > nextFire) {
				CmdFire ();
            }
        }
    }
}
