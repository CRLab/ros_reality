using UnityEngine;
using UnityEngine.UI;

public class ArmController : MonoBehaviour {
    // string of which arm to control. Valid values are "left" and "right"
    public string arm;

    public Text frameText;
    public GameObject laserPrefab;
    public GameObject headCam;
    public GameObject baselink;
    public GameObject leftHandle;
    public GameObject rightHandle;

    private GameObject laser;
    private Vector3 laserHitPoint;

    //websocket client connected to ROS network
    private WebsocketClient wsc;
    TFListener TFListener;
    //scale represents how resized the virtual robot is
    float scale;

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;

    bool[] triggerDown = { false, false };  // { left, right }
    bool moveReady = true;
    bool gripperOpen = true;
    bool leftTouchpadPressed = false;

    private enum Frame { Base=0, Head };    // frame of rotation for robot
    private string[] frameStrings = { "base", "head"};
    private Frame currFrame = Frame.Base;

    bool useNavigation = true;//testing parameter, determining the mode
    bool triggerDownMove = false;

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        baselink = GameObject.Find("base_linkPivot");
        //should try to store the left and right handles, don't know if is going to succeed

    }

    void Start() {
        setCurrentFrame(currFrame);

        // init laser
        laser = Instantiate(laserPrefab);
        laser.SetActive(false);

        // Get the live websocket client
        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();

        // Get the live TFListener
        TFListener = GameObject.Find("TFListener").GetComponent<TFListener>();

        // Create publisher to the Baxter's arm topic (uses Ein)
        wsc.Advertise("forth_commands", "std_msgs/String");
        // Asychrononously call sendControls every .1 seconds
        // InvokeRepeating("SendControls", .1f, .1f);
        InvokeRepeating("CheckMove", .1f, .1f);
    }

    void FixedUpdate() {
        device = SteamVR_Controller.Input((int)trackedObj.index);
    }

    private void Update() {
        scale = TFListener.scale;

        // left grip cancel move
        if (arm == "left") {
            handleLeft();
        }
        else if (arm == "right") {
            handleRight();
        }
    }
    
    void CheckMove() {
        if (triggerDownMove) {
            if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
                Vector2 axis = device.GetAxis();
                string msg = 
                    "directMove^"+axis[1]+","+axis[0];
                wsc.SendEinMessage(msg);
                Debug.Log(msg);
                //Debug.Log(gameObject.name + device.GetAxis());
            }
        }
    }

    void handleLeft() {
        
        // cancel movement on grip press
        //tempporarily disabled
        /*if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            wsc.SendEinMessage("cancelMove");
            return;
        }*/
        //change to navigation mode
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            useNavigation = !useNavigation;
            Debug.Log("Navigation: " + useNavigation);
            //should change the right as well
            return;
        }

        //under navigation mode
        if (useNavigation) {
            if (device.GetHairTriggerDown()) {
                triggerDownMove = true;
            }
            
            if (device.GetHairTriggerUp()) {
                triggerDownMove = false;
            }
            return;
        }

        //not under navigation mode
        if (!useNavigation) {
            // Touchpad to toggle through rotation frames
            if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
                if (!leftTouchpadPressed) {
                    int newIndex = ((int)currFrame + 1) % Frame.GetNames(typeof(Frame)).Length;
                    setCurrentFrame((Frame)newIndex);
                }
                leftTouchpadPressed = true;
            }
            else {
                leftTouchpadPressed = false;
            }

            // orient robot to head cam
            if (!triggerDown[0] && device.GetHairTriggerDown()) {
                triggerDown[0] = true;

                // orient the base
                if (currFrame == Frame.Base) {
                    Quaternion outQuat = UnityToRosRotationAxisConversion(headCam.GetComponent<Transform>().rotation);
                    outQuat *= Quaternion.Euler(0, 90, 0);

                    string msg =
                        "rotateTo^" +
                        outQuat.x + "," + outQuat.y + "," + outQuat.z + "," + outQuat.w;
                    wsc.SendEinMessage(msg);
                    return;

                    // orient the head
                }
                else if (currFrame == Frame.Head) {
                    Vector3 pos = headCam.transform.position + (headCam.transform.forward * 0.5f);
                    pos = UnityToRosPositionAxisConversion(pos);

                    string msg =
                       "pointHead^" +
                       pos.x + "," + pos.y + "," + pos.z;
                    wsc.SendEinMessage(msg);
                    return;
                }
            }

            // Reset trigger variables
            if (device.GetHairTriggerUp()) {
                triggerDown[0] = false;
            }
        }
    }

    void setCurrentFrame(Frame frame) {
        currFrame = frame;
        frameText.text = "Rotation frame: " + frameStrings[(int)currFrame];
    }

    //helper function to get relative position
    private Vector3 getRelativePosition(Transform origin, Vector3 position) {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

        return relativePosition;
    }

    void handleRight() {

        // Touchpad press shows the laser pointer
        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100)) {
                laserHitPoint = hit.point;
                ShowLaser(hit);
            }
        }
        else {
            laser.SetActive(false);
        }

        // Move robot arm to controller position (right controller) or cancel all movement (left controller)
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
                Vector3 outPos = UnityToRosPositionAxisConversion(getRelativePosition(baselink.transform, GetComponent<Transform>().position)) / scale;
                Quaternion outQuat = UnityToRosRotationAxisConversion(GetComponent<Transform>().rotation);



            wsc.SendEinMessage(
                    "moveGripper^" +
                    outPos.x + "," + outPos.y + "," + outPos.z
                    + "^" + outQuat.x + "," + outQuat.y + "," + outQuat.z + "," + outQuat.w);
                return; 
        }

        // close gripper on trigger press
        if (!triggerDown[1] && device.GetHairTriggerDown()) {
            triggerDown[1] = true;

            if (!laser.activeSelf) {
                string msg = !gripperOpen ? "openGripper" : "closeGripper";
                gripperOpen = !gripperOpen;
                wsc.SendEinMessage(msg);
                return;
            }
        }

        // Reset trigger variables
        if (device.GetHairTriggerUp()) {
            triggerDown[1] = false;
            moveReady = true;
        }

        // move to a point on trigger and touchpad press
        if (laser.activeSelf && triggerDown[1] && moveReady) {
            wsc.SendEinMessage("moveTo^" + (-laserHitPoint.x).ToString() + "," + (-laserHitPoint.z).ToString());
            moveReady = false;
            return;
        }
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

