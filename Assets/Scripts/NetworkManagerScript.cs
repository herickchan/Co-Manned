﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManagerScript : NetworkManager {

	public NetworkDiscovery discovery;

	public void enterLobby() {
		//discovery.Initialize();
		//discovery.showGUI = true;
	}

	// Use this for initialization
	public void Join () {
		StartClient();
		//discovery.StopBroadcast();
		//discovery.showGUI = false;
	}

	public void Host () {
		StartHost();
		//discovery.StartAsServer();
	}

	public override void OnServerConnect(NetworkConnection conn){
		Debug.Log ("player connected");
	}

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
        /*var player = (GameObject)GameObject.Instantiate(playerPrefab, new Vector3(0,0,0), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        //Setup Camera
        GameObject camera = GameObject.FindWithTag("MainCamera");
        if (camera != null) {
            PilotCamera followScript = camera.GetComponent("PilotCamera") as PilotCamera;

            if (followScript != null) {
                followScript.player = player;
            }
        }*/
		Debug.Log("player added");
    }
}
