﻿using UnityEngine;
using System.Collections;
using CnControls;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PilotMechController : NetworkBehaviour {
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
    public Transform bulletSpawn2;

    public Animator anim;
    private bool altShoot;

    // Mech team info
	[SyncVar]
    public GameManager.Team team;
	[SyncVar]
    public GameManager.Role role;

    // Mech camera stuff
    private Vector3 camOffset;

    // Game related info
    public int ammo;
    public int powerupType;
    public int fuel;
    private Combat combat;
	private GlobalDataHook globalData;

    void Awake () {
        rb = GetComponent<Rigidbody>();
		globalData = GetComponent<GlobalDataHook> ();
    }

    void Start () {
        // set up physics
        statusTextOffset = transform.position - statusText.transform.position;
        powerupType = 0;
        // set up manager to pull data from game room

        // test set up string
        // set up name

		//int blue = globalData.getParam (GameManager.Team.Blue); 
		//int red = globalData.getParam (GameManager.Team.Red); 
		//statusText.GetComponent<TextMesh> ().text = "B:" + blue.ToString () + "R:" + red.ToString ();

        //used for switching arms for shooting
        altShoot = false;
        combat = transform.GetComponent<Combat>();
        //set up camera
        SetCamera();
        //set up animations
        anim = transform.GetComponent<Animator>();
        //set up network animations
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
		if(isClient) CmdNotifyNewPlayer (team, role);

    }

	public void updateStatusText(){
		var globalData = GetComponent<GlobalDataHook>();
		int blue = globalData.getParam (GameManager.Team.Blue, GlobalDataController.Param.Health); 
		int red = globalData.getParam (GameManager.Team.Red, GlobalDataController.Param.Health); 
		statusText.GetComponent<TextMesh> ().text = "B:" + blue.ToString () + "R:" + red.ToString ();
	}

	void Update () {
        if (!isLocalPlayer)
            return;

        if (role == GameManager.Role.Pilot) { 
            var eng = GameObject.Find("Engineer camera");
            if (eng != null) {
                eng.SetActive(false);
            }
        } else if (role == GameManager.Role.Engineer) {
            var controllerCanvas = GameObject.Find("ControllerCanvas");
            var cameraRig = GameObject.Find("CameraRig");
            if (controllerCanvas != null) {
                controllerCanvas.SetActive(false);
            }
            if (cameraRig != null) {
                cameraRig.SetActive(false);
            }
        }

        rb.isKinematic = true;
        statusText.transform.position = transform.position + statusTextOffset;

        moveH = CnInputManager.GetAxis("Horizontal");
        moveV = CnInputManager.GetAxis("Vertical");

		if (globalData.getParam(team, GlobalDataController.Param.Health) > 0) {
            Move();
            Turn();
            Fire();
        } else {
            anim.SetBool("death", true);
        }
    }

    private void SetCamera () {
        if (isLocalPlayer) {
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
        if (moveH != 0 || moveV != 0) {
            rb.isKinematic = false;
            globalData.setParam (team, GlobalDataController.Param.Fuel, fuel-1); 
        }
        fuel = globalData.getParam (team, GlobalDataController.Param.Fuel); 

        if (fuel > 0) {
            Vector3 movement = new Vector3(moveH, 0.0f, moveV);
            anim.SetFloat("inputH", moveH);
            anim.SetFloat("inputV", moveV);
            rb.velocity = movement * speed;
        } else {
            fuel = 0;
            globalData.setParam (team, GlobalDataController.Param.Fuel, 0); 
        }
    }

    private void Turn () {
        float angle = Mathf.Atan2(moveH, moveV) * Mathf.Rad2Deg;
        if (angle != 0) {
            rb.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }

    [Command]
	void CmdFire (GameManager.Team shooter) {
        ammo = globalData.getParam (team, GlobalDataController.Param.Ammo); 
        Debug.Log(ammo);
        if (ammo != 0) {
            nextFire = Time.time + fireRate;
            GameObject b;
            if (altShoot) {
                b = (GameObject)Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
                altShoot = false;
            } else {
                b = (GameObject)Instantiate(bullet, bulletSpawn2.position, bulletSpawn2.rotation);
                altShoot = true;
            }
            //transform.Translate(new Vector3(0, 0, -0.05f));
            b.GetComponent<Rigidbody>().velocity = transform.forward * speed;
			RpcClienMessage (GameManager.teamString(shooter) + " shot a bullet");
			b.GetComponent<BulletBehaviour> ().shooter = shooter;
            NetworkServer.Spawn(b);
            Destroy(b, 2.0f);
            ammo--;
            globalData.setParam (team, GlobalDataController.Param.Ammo, ammo); 
        }
    }

	[ClientRpc]
	public void RpcClienMessage(string message){
		Debug.Log (message);
	}

    void Fire () {
        // this is for touch
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                if (touch.position.x > (Screen.width / 2) && Time.time > nextFire) {
					CmdFire(this.team);
                }
            }
        }

        // this is for mouse click
        if (Input.GetMouseButtonDown(0)) {
            if (Input.mousePosition.x > (Screen.width / 2) && Time.time > nextFire) {
				CmdFire(this.team);
            }
        }
    }

	[Command]
	public void CmdNotifyNewPlayer(GameManager.Team team, GameManager.Role role){
		RpcNotifyNewPlayer (team, role);
	}

	[ClientRpc]
	public void RpcNotifyNewPlayer(GameManager.Team team, GameManager.Role role){
		Debug.Log (GameManager.teamString(team) + " " + GameManager.roleString(role) + " joined the game");
	}

	public void setTeamInfo(GameManager.Team team, GameManager.Role role){		
		Debug.Log ("Setting mech team to " + GameManager.teamString(team));
		this.team = team;
		this.role = role;
	}
		
}
