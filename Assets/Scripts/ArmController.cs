using UnityEngine;

public class ArmController : MonoBehaviour {
    // string of which arm to control. Valid values are "left" and "right"
    public string arm;

    public GameObject laserPrefab;

    private GameObject laser;
    private Vector3 laserHitPoint;

    private string grip_label;
    private string trigger_label;
    //websocket client connected to ROS network
    private WebsocketClient wsc;
    TFListener TFListener;
    //scale represents how resized the virtual robot is
    float scale;

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;

    bool gripperClosed = false;
    bool moveMessageReady = true;   // move message is ready to be sent

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate() {
        device = SteamVR_Controller.Input((int)trackedObj.index);
    }

    private void Update() {
        scale = TFListener.scale;

        // Touchpad press shows the laser pointer
        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100)) {
                laserHitPoint = hit.point;
                ShowLaser(hit);
            }
        } else {
            laser.SetActive(false);
        }

        // cancel move actions on grip press
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            Vector3 outPos = UnityToRosPositionAxisConversion(GetComponent<Transform>().position) / scale;
            Quaternion outQuat = UnityToRosRotationAxisConversion(GetComponent<Transform>().rotation);
            wsc.SendEinMessage(
                "moveGripper^" + 
                outPos.x + "," + outPos.y + "," + outPos.z
                + "^" + outQuat.x + "," + outQuat.y + "," + outQuat.z + "," + outQuat.w);
            //wsc.SendEinMessage("cancelMove");
            return;
        }

        // close gripper on trigger press
        if (device.GetHairTriggerDown()) {
            gripperClosed = true;

            if (!laser.activeSelf) {
                wsc.SendEinMessage("closeGripper");
            }
        }

        // open gripper on trigger relsease
        if (device.GetHairTriggerUp()) {
            gripperClosed = false;
            moveMessageReady = true;

            if (!laser.activeSelf) {
                wsc.SendEinMessage("openGripper");
            }
        }

        // move to a point on trigger and touchpad press
        if (laser.activeSelf && gripperClosed && moveMessageReady) {
            wsc.SendEinMessage("moveTo^" + (-laserHitPoint.x).ToString() + "," + (-laserHitPoint.z).ToString());
            moveMessageReady = false;
        }
    }

    void Start() {
        // init laser
        laser = Instantiate(laserPrefab);

        // Get the live websocket client
        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();

        // Get the live TFListener
        TFListener = GameObject.Find("TFListener").GetComponent<TFListener>();

        // Create publisher to the Baxter's arm topic (uses Ein)
        wsc.Advertise("forth_commands", "std_msgs/String");
        // Asychrononously call sendControls every .1 seconds
        // InvokeRepeating("SendControls", .1f, .1f);

        if (arm == "left") {
            grip_label = "Left Grip";
            trigger_label = "Left Trigger";
        }
        else if (arm == "right") {
            grip_label = "Right Grip";
            trigger_label = "Right Trigger";
        }
        else
            Debug.LogError("arm variable is not set correctly");
    }

    private void ShowLaser(RaycastHit hit) {
        laser.SetActive(true);
        laser.transform.position = Vector3.Lerp(trackedObj.transform.position, laserHitPoint, .5f);
        laser.transform.LookAt(laserHitPoint);
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, hit.distance);
    }

    //Convert 3D Unity position to ROS position 
    Vector3 UnityToRosPositionAxisConversion(Vector3 rosIn) {
        return new Vector3(-rosIn.x, -rosIn.z, rosIn.y);
    }

    //Convert 4D Unity quaternion to ROS quaternion
    Quaternion UnityToRosRotationAxisConversion(Quaternion qIn) {
        return (new Quaternion(qIn.x, qIn.z, -qIn.y, qIn.w)) * (new Quaternion(0, 1, 0, 0));
    }

}

