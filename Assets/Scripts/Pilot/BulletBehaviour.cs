﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletBehaviour : NetworkBehaviour {

	public float speed;
	[SyncVar]
	public GameManager.Team shooter;
    public ParticleSystem explosion;
    public AudioSource soundSystem;
    
	void Awake(){
	}
	
	void Start() {
		GetComponent<Rigidbody> ().velocity = transform.forward * speed;
	}
	
	private void OnTriggerEnter (Collider other) {
		var hit = other.gameObject;
		var hitMech = hit.GetComponent<PilotMechController> ();
        soundSystem.Play();

		if(hitMech != null){
			var combat = hitMech.GetComponent<Combat> ();

			if (combat != null) {
				Debug.Log (GameManager.teamString(this.shooter) + " shoots " + GameManager.teamString(hitMech.team));
                hitMech.anim.Play("Hit");
				combat.TakeDamage (hitMech.team);
			} else {
				Debug.Log ("Error: combat is null");
			}
		}

        // Unparent the particles from the shell.
        explosion.transform.parent = null;

        // Play the particle system.
        explosion.Play();

        // Once the particles have finished, destroy the gameobject they are on.
        Destroy (explosion.gameObject, explosion.duration);
		Destroy (gameObject); // delete bullet
	}
}
