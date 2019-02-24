using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSceneRightController : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;
    public GameObject buttonParent;
    public bool buttonEnabled;

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        buttonParent.SetActive(false);
        buttonEnabled = false;
    }

    void FixedUpdate() {
        device = SteamVR_Controller.Input((int)trackedObj.index);
    }

    // Update is called once per frame
    void Update () {
        if (device.GetHairTriggerDown()) {
            //show the button
            if (buttonEnabled == false) {
                buttonParent.SetActive(true);
                buttonEnabled = true;
            }
            else {
                buttonParent.SetActive(false);
                buttonEnabled = false;
            }
        }
    }
}
