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
    public GameObject canvas;

    //string depthTopic = "head_camera/depth_registered/points";
    string meshTopic = "completed_meshes";
    //string depthTopic = "filtered_pc"; //this is the downsampled data
    string planService = "plan_grasp";
    string exeService = "execute_grasp";
    string colorTopic;
    TFListener tfListener;
    float scale;

    public bool scanMode = false;

    private int maxPoints = 60000;  // max points allowed in one mesh
    //we probably don't want several mesh object for the same object. When that happens, maybe just clip the object

    private GameObject[] mesh_list; //the mesh list for determining the index of the selected mesh

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
        //StartScanSystem();
    }

    public void StartScanSystem() {
        canvas.SetActive(true);
        wsc.Subscribe(meshTopic, "ros_reality_bridge/MeshArray", 10);
        StartCoroutine("createMesh");
    }
    
    IEnumerator createMesh() {
        
        //If using Json
        if (wsc.connectionType == WebsocketClient.CT.JSON) {
            while (!wsc.messages.ContainsKey(meshTopic)) {
                //Debug.Log("No Message");
                yield return new WaitForSeconds(0.1f);
            }
        }

        //We don't need the information anymore once we already have them
        wsc.Unsubscribe(meshTopic);


        scale = tfListener.scale;

        

        MeshMsg[] meshmsg;
        // Get mesh message, using Json
        String meshMessage = wsc.messages[meshTopic];
        wsc.messages.Remove(meshTopic);
        meshmsg = JsonConvert.DeserializeObject<MeshObj>(meshMessage).msg.meshes;
        Debug.Log("successfully get the mesh!");

        int totalNumMeshes = meshmsg.Length;
        mesh_list = new GameObject[totalNumMeshes];

        for (int k = 0; k < totalNumMeshes; k++) {
            int totalPoints = meshmsg[k].vertices.Length;
            //Debug.Log(totalPoints);
            //int numMeshes = (int)(Math.Ceiling(totalPoints / ((double)maxPoints)));
            //GameObject[] Meshes = new GameObject[numMeshes];
            //for (int i = 0; i < numMeshes; i++) {}

            mesh_list[k] = Instantiate(MeshPrefab);
            mesh_list[k].transform.SetParent(transform);
            mesh_list[k].transform.localRotation = Quaternion.identity;
            mesh_list[k].transform.localPosition = Vector3.zero;

            Mesh mesh = new Mesh(); //hopefully there's no problem with this

            Vector3[] vertexList = new Vector3[totalPoints];
            for (int i = 0; i < totalPoints; i++) {
                vertexList[i] = RosToUnityPositionAxisConversion(new Vector3(meshmsg[k].vertices[i].x, meshmsg[k].vertices[i].y, meshmsg[k].vertices[i].z)) / scale;
            }
            mesh.vertices = vertexList;
            //Debug.Log(vertexList[0]);

            int totalTriangles = meshmsg[k].triangles.Length;
            int[] triangleList = new int[3 * totalTriangles];
            for (int i = 0; i < totalTriangles; i++) {
                triangleList[i * 3] = meshmsg[k].triangles[i].vertex_indices[0];
                triangleList[i * 3 + 1] = meshmsg[k].triangles[i].vertex_indices[1];
                triangleList[i * 3 + 2] = meshmsg[k].triangles[i].vertex_indices[2];
            }


            //Debug.Log(triangleList[0]);
            mesh.triangles = triangleList;

            mesh_list[k].GetComponent<MeshFilter>().mesh = mesh;
            mesh_list[k].GetComponent<MeshCollider>().sharedMesh = mesh;
            yield return null;
        }
        

        Debug.Log("Finish creating the object");
        canvas.SetActive(false);

        //next step is to select the object and plan grasp
        while (!armComtroller.selectedObject) {
            yield return null;
        }
        armComtroller.selectedObject = null;//maybe do this later?

        //call the service which returns the planned grasp
        int indexVal = Array.IndexOf(mesh_list, armComtroller.selectedObject);//this won't work if changed color? maybe it will still work
        wsc.CallService(planService, new[] { "mesh_index" }, new[] { indexVal.ToString() }, planService);

        while(!wsc.services.ContainsKey(planService)) {
            yield return null;
        }
        string plan_message = wsc.services[planService];
        wsc.services.Remove(planService);
        //parse and use it first
        PlanServiceMsg plan_result = JsonConvert.DeserializeObject<PlanServiceMsg>(plan_message);
        if (!plan_result.result) {
            Debug.Log("Planning failed!");
            //Should probably display a UI telling the user that the planning has failed
            //Should set everything back to normal
        }
        

        //The grasp is planned, should display the gesture and allow the user to choose whether to accept the grasp or not

        //For now, let's just assume that the user always execute the plan
        wsc.CallService(exeService, new string[0], new string[0], exeService);
        //after executed, maybe check if the execution is successful?

        Debug.Log("action executed!");
        //wait for 3 seconds
        yield return new WaitForSeconds(2f);

        // reset mesh
        destroy_mesh();

    }

    //distroy all the meshes that is getting displayed
    void destroy_mesh() {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) {
            if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            Destroy(child.gameObject);
        }
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
