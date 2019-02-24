using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandlerScan : MonoBehaviour {
    public GameObject ButtonParent;
    public GameObject LeftController;
    public GameObject canvas;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "VRController") {
            //Debug.Log("YoYoYo");
            //SceneManager.LoadScene("LoadingScene");
            canvas.SetActive(true);
            ButtonParent.SetActive(false);
            LeftController.GetComponent<ArmController>().buttonEnabled = false;
        }
    }
}
