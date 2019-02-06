/***************************************************************
 * ***** Visualizing labelled pointcloud from web socket *****
 * *************************************************************/

using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Threading;
using NetWebSocketTest;


public class NetPCD : MonoBehaviour {
    private NetWebSocket wsc;

    public bool active = false;
    public GameObject pointCloudPrefab;

    string colorTopic;
    TFListener tfListener;
    float scale;

    string pcdTopic = "chatter";

    bool inited = false;

    private int maxPoints = 60000;  // max points allowed in one mesh

    Color[] categories = { Color.black, Color.red, Color.green, Color.blue, Color.cyan, Color.yellow, new Color(1f, 0f, 1f, 1f), new Color(0f, 0f, 188f / 255f, 1f) };

    // Use this for initialization
    void Start() {
        if (!active) return;
        wsc = GameObject.Find("TestNet").GetComponent<NetWebSocket>();
        tfListener = GameObject.Find("TFListener").GetComponent<TFListener>();
        wsc.Subscribe(pcdTopic, "std_msgs/Float32MultiArray", 100);//unit: milisecond
        //InvokeRepeating("UpdateTexture", 0f, 3f);
    }

    // Update is called once per frame
    void UpdateTexture() {
        Debug.Log("start");
        if (!wsc.messages.ContainsKey(pcdTopic)) return;




        int totalNumField = 6; //total number of field used, x, y, z, r, g, b in this case
        inited = true;
        scale = tfListener.scale;

        Debug.Log(0);
        // reset mesh
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) {
            if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            Destroy(child.gameObject);
        }
        Debug.Log(-1);
        SocketPCDMsg pc;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        Debug.Log(1);
        String depthMessage = wsc.messages[pcdTopic];
        Debug.Log(2);
        pc = JsonConvert.DeserializeObject<SocketPointCloud>(depthMessage).msg;
        Debug.Log(3);

        var elapsedMs = watch.ElapsedMilliseconds;
        var lastStamp = watch.ElapsedMilliseconds;
        Debug.Log("Json Convert Pointcloud time: " + elapsedMs);

        /*************************************read starting from here**************************************/

        int totalPoints = pc.data.Length / totalNumField;

        // init necessary number of point clouds base on maximum capacity of vertices
        int numPointClouds = (int)(Math.Ceiling(totalPoints / ((double)maxPoints)));
        GameObject[] pointClouds = new GameObject[numPointClouds];
        for (int i = 0; i < pointClouds.Length; i++) {
            GameObject pointCloud = Instantiate(pointCloudPrefab);
            pointCloud.transform.SetParent(transform);
            pointCloud.transform.localRotation = Quaternion.identity;
            pointCloud.transform.localPosition = Vector3.zero;
            pointCloud.GetComponent<MeshFilter>().mesh = new Mesh();
            pointClouds[i] = pointCloud;
        }

        elapsedMs = watch.ElapsedMilliseconds - lastStamp;
        lastStamp = watch.ElapsedMilliseconds;
        Debug.Log("Initialize Pointcloud time: " + elapsedMs);


        int cur_data_index = -1;
        for (int i = 0; i < pointClouds.Length; i++) {
            //Debug.Log(1);
            // init arrays
            int numPointsInCloud = i != pointClouds.Length - 1 ? maxPoints : totalPoints % maxPoints;
            Vector3[] points = new Vector3[numPointsInCloud];
            int[] indices = new int[numPointsInCloud];
            Color[] colors = new Color[numPointsInCloud];


            float x = 0, y = 0, z = 0;
            int r = 0, g = 0, b = 0;
            // current index of point being processed
            for (int counter = 0; counter < numPointsInCloud * totalNumField; counter++) {
                //Making empty value as null
                cur_data_index++;
                switch (counter % totalNumField) {
                    case 0: x = pc.data[cur_data_index]; break;
                    case 1: y = pc.data[cur_data_index]; break;
                    case 2: z = pc.data[cur_data_index]; break;
                    case 3: r = (int)pc.data[cur_data_index]; break;
                    case 4: g = (int)pc.data[cur_data_index]; break;
                    case 5:
                        if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z)) break;
                        b = (int)pc.data[cur_data_index];
                        int pointIndex = counter / totalNumField;
                        // store the points and colors
                        points[pointIndex] = RosToUnityPositionAxisConversion(new Vector3(x, y, z)) / scale;
                        indices[pointIndex] = pointIndex;
                        colors[pointIndex] = new Color(r / 255f, g / 255f, b / 255f, 1.0f); //categories[color];
                        break;
                }

            }

            elapsedMs = watch.ElapsedMilliseconds - lastStamp;
            lastStamp = watch.ElapsedMilliseconds;
            Debug.Log("Setup real Pointcloud time: " + elapsedMs);

            // Assign the points and colors to the current point cloud mesh
            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            pointClouds[i].GetComponent<MeshFilter>().mesh = mesh;

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
