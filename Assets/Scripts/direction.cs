using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class direction : MonoBehaviour {
    public GameObject baselink;
    // Use this for initialization
    void Start () {
        baselink = GameObject.Find("base_linkPivot");
    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = baselink.transform.rotation;
        transform.position = baselink.transform.position;
        transform.Translate(Vector3.left * 3);
        transform.Translate(0,1,0);
    }
}
