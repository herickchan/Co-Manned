﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletBehaviour : MonoBehaviour {

	public float speed;
	private float lifeTime = 2.0f;

	
	void Start() {
		GetComponent<Rigidbody> ().velocity = transform.forward * speed;
		Destroy (gameObject, lifeTime);
	}
	

	private void OnTriggerEnter (Collider other) {
			
		Debug.Log ("bullet hit someting");
		var hit = other.gameObject;
		var hitMech = hit.GetComponent<PilotMechController> ();
		if(hitMech != null){
			var combat = hitMech.GetComponent<Combat> ();
			if (combat != null) {
				combat.TakeDamage (10);
				Destroy (gameObject); // delete bullet
			} else {
				Debug.Log ("Error: combat is null");
			}
		}



	}
}
