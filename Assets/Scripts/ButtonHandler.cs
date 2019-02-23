using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour {
    public GameObject ButtonParent;
    public GameObject LeftController;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "VRController") {
            Debug.Log("YoYoYo");
            ButtonParent.SetActive(false);
            LeftController.GetComponent<ArmController>().buttonEnabled = false;
        }
    }
}
