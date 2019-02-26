using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandlerScan : MonoBehaviour {
    public GameObject ButtonParent;
    public GameObject LeftController;
    public GameObject canvas;
    public MeshObjectParser meshParser;
    private WebsocketClient wsc;
    // Use this for initialization
    void Start () {
        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>(); // hopefully this is the same socket (yeah it is)
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "VRController") {
            //Debug.Log("YoYoYo");
            //SceneManager.LoadScene("LoadingScene");
            if (canvas.activeSelf)
                return;
            canvas.SetActive(true);
            ButtonParent.SetActive(false);
            LeftController.GetComponent<ArmController>().buttonEnabled = false;
            //*****************send command to the wsc
            //call service
            meshParser.getMesh();
            //StartCoroutine("createMesh"); in meshObjectParser
        }
    }
}
