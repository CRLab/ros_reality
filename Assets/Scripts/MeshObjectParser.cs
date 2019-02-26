using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;


public class MeshObjectParser : MonoBehaviour {

    // private RobWebsocketClient wsc;
    private WebsocketClient wsc;
    public bool active = false;
    public GameObject MeshPrefab;
    public ArmController armComtroller;

    //string depthTopic = "head_camera/depth_registered/points";
    string meshTopic = "test_mesh";
    //string depthTopic = "filtered_pc"; //this is the downsampled data
    string colorTopic;
    TFListener tfListener;
    float scale;

    public bool scanMode = false;

    private int maxPoints = 60000;  // max points allowed in one mesh

    // Use this for initialization
    void Start() {
        if (!active) return;

        tfListener = GameObject.Find("TFListener").GetComponent<TFListener>();

        //connect to the robot directly
        // wsc = GameObject.Find("RobotWebsocketClient").GetComponent<RobotWebsocketClient>();

        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();

        //alternatively, connect to the server
        //wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();

        
        //InvokeRepeating("UpdateTexture", 0.1f, 0.5f);
        getMesh();
    }

    public void getMesh() {
        wsc.Subscribe(meshTopic, "shape_msgs/Mesh", 10);
        StartCoroutine("createMesh");
    }

    // Update is called once per frame
    IEnumerator createMesh() {
        //Debug.Log("updating the point cloud"+debug);
        
        //If using Json
        if (wsc.connectionType == WebsocketClient.CT.JSON) {
            while (!wsc.messages.ContainsKey(meshTopic)) {
                Debug.Log("No Message");
                yield return new WaitForSeconds(0.1f);
            }
        }

        //We don't need the information anymore once we already have them
        wsc.Unsubscribe(meshTopic);


        scale = tfListener.scale;

        //think about when you actually distroy them
        //this should get called when the action is actually executed

        // reset mesh
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) {
            if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            Destroy(child.gameObject);
        }

        MeshMsg meshmsg;
        // Get mesh message, using Json
        String meshMessage = wsc.messages[meshTopic];
        wsc.messages.Remove(meshTopic);
        meshmsg = JsonConvert.DeserializeObject<MeshObj>(meshMessage).msg;
        Debug.Log("successfully get the mesh!");

        //checking total number of vertices is probably not necessary?
        
        int totalPoints = meshmsg.vertices.Length;
        //Debug.Log(totalPoints);
        //int numMeshes = (int)(Math.Ceiling(totalPoints / ((double)maxPoints)));
        //GameObject[] Meshes = new GameObject[numMeshes];
        //for (int i = 0; i < numMeshes; i++) {}

        GameObject Mesh = Instantiate(MeshPrefab);
        Mesh.transform.SetParent(transform);
        Mesh.transform.localRotation = Quaternion.identity;
        Mesh.transform.localPosition = Vector3.zero;

        Mesh mesh = new Mesh();

        Vector3[] vertexList = new Vector3[totalPoints];
        for (int i = 0; i< totalPoints; i++) {
            vertexList[i] = RosToUnityPositionAxisConversion(new Vector3(meshmsg.vertices[i].x, meshmsg.vertices[i].y, meshmsg.vertices[i].z))/scale;
        }
        mesh.vertices = vertexList;
        //Debug.Log(vertexList[0]);

        int totalTriangles = meshmsg.triangles.Length;
        int[]triangleList = new int[3 * totalTriangles];
        for (int i = 0; i< totalTriangles; i++) {
            triangleList[i * 3] = meshmsg.triangles[i].vertex_indices[0];
            triangleList[i * 3 + 1] = meshmsg.triangles[i].vertex_indices[1];
            triangleList[i * 3 + 2] = meshmsg.triangles[i].vertex_indices[2];
        }
        //Debug.Log(triangleList[0]);
        mesh.triangles = triangleList;

        Mesh.GetComponent<MeshFilter>().mesh = mesh;
        Mesh.GetComponent<MeshCollider>().sharedMesh = mesh;

        Debug.Log("Finish creating the object");

        //create a dictionary for the mesh -> index

        //next step is to select the object and plan grasp
        while (!armComtroller.selectedObject) {
            yield return null;
        }
        int indexVal = 1;//dict(armComtroller.selectedObject)
        wsc.CallService("plan_grasp", "index", indexVal.ToString());

        //subscribe and wait
        wsc.CallService("execute_grasp", "exe", "true");

    }

    //convert ROS position to Unity Position
    Vector3 RosToUnityPositionAxisConversion(Vector3 rosIn) {
        //return new Vector3(-rosIn.x, rosIn.z, -rosIn.y);

        Vector3 pos = new Vector3(-rosIn.x, rosIn.z, -rosIn.y);
        Vector3 newPos = Quaternion.Euler(0, 90, 0) * pos;
        return Quaternion.Euler(0, 0, 90) * newPos;

        //return rosIn;
    }

}
