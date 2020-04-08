using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateShow : MonoBehaviour {
 public  Vector3 speed =Vector3.up*100;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(speed*Time.deltaTime);
	}
}
