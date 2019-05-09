using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;


public class PointCloudParser : MonoBehaviour {

    // private RobWebsocketClient wsc;
    private WebsocketClient wsc;
    public bool active = false;
    public GameObject pointCloudPrefab;

    //string depthTopic = "head_camera/depth_registered/points";
    string depthTopic = "head_camera/depth_registered/points";
    //string depthTopic = "filtered_pc"; //this is the downsampled data
    TFListener tfListener;
    float scale;

    bool inited = false;

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

        wsc.Subscribe(depthTopic, "sensor_msgs/PointCloud2", 300);
        //InvokeRepeating("UpdateTexture", 0.1f, 0.5f);
        StartCoroutine("UpdateTexture");
    }

    // Update is called once per frame
    IEnumerator UpdateTexture() {
        //Debug.Log("updating the point cloud"+debug);
        while (true) {
            while (!wsc.messages.ContainsKey(depthTopic)) {
                yield return new WaitForSeconds(0.1f);
            }
            String depthMessage = wsc.messages[depthTopic];
            PointCloudMsg pc = JsonConvert.DeserializeObject<PointCloud>(depthMessage).msg;
            wsc.messages.Remove(depthTopic);
            scale = tfListener.scale;

            // Init offset values
            int xOffset = -1, yOffset = -1, zOffset = -1, colorOffset = -1;
            foreach (PointField field in pc.fields) {
                switch (field.name) {
                    case "x": xOffset = field.offset; break;
                    case "y": yOffset = field.offset; break;
                    case "z": zOffset = field.offset; break;
                    case "rgb": colorOffset = field.offset; break;
                }
            }

            // init necessary number of point clouds base on maximum capacity of vertices
            int totalPoints = pc.data.Length / pc.point_step;
            int numPointClouds = (int)(Math.Ceiling(totalPoints / ((double)maxPoints)));


            Mesh[] meshes = new Mesh[numPointClouds];

            yield return null;
            // process points for each point cloud mesh
            for (int i = 0; i < numPointClouds; i++) {

                // init arrays
                int numPointsInCloud = i != numPointClouds - 1 ? maxPoints : totalPoints % maxPoints;
                Vector3[] points = new Vector3[numPointsInCloud];
                int[] indices = new int[numPointsInCloud];
                Color[] colors = new Color[numPointsInCloud];

                int start = i * maxPoints * pc.point_step;  // start offset for this point cloud in the point cloud data array
                int pointIndex = 0;                         // current index of point being processed
                for (int j = start; j < start + numPointsInCloud * pc.point_step; j += pc.point_step) {

                    //// reverse data chunk base don endiness of client and server
                    //if (System.BitConverter.IsLittleEndian == pc.is_bigendian) {
                    //    Array.Reverse(pointData);
                    //}

                    // Convert the data chunk into x,y,z, point data
                    float x = BitConverter.ToSingle(pc.data, j + xOffset);
                    float y = BitConverter.ToSingle(pc.data, j + yOffset);
                    float z = BitConverter.ToSingle(pc.data, j + zOffset);

                    if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z)) continue;

                    // convert data chunk into color. downsamples topic does not have color data
                    byte r = 255, g = 0, b = 0;
                    if (colorOffset != -1) {
                        b = pc.data[j + colorOffset];
                        g = pc.data[j + colorOffset + 1];
                        r = pc.data[j + colorOffset + 2];
                    }

                    // store the points and colors
                    points[pointIndex] = RosToUnityPositionAxisConversion(new Vector3(x, y, z)) / scale;
                    indices[pointIndex] = pointIndex;
                    colors[pointIndex] = new Color(r / 255f, g / 255f, b / 255f, 1.0f);
                    pointIndex += 1;
                }

                // Assign the points and colors to the current point cloud mesh
                Mesh mesh = new Mesh();

                //at here
                mesh.vertices = points;
                mesh.colors = colors;
                mesh.SetIndices(indices, MeshTopology.Points, 0);
                meshes[i] = mesh;
                //pointClouds[i].GetComponent<MeshFilter>().mesh = mesh;
                yield return null; //new WaitForSeconds(0.03f);
            }

            // reset mesh
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren) {
                if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
                Destroy(child.gameObject);
            }

            GameObject[] pointClouds = new GameObject[numPointClouds];
            for (int i = 0; i < numPointClouds; i++) {
                GameObject pointCloud = Instantiate(pointCloudPrefab);
                pointCloud.transform.SetParent(transform);
                pointCloud.transform.localRotation = Quaternion.identity;
                pointCloud.transform.localPosition = Vector3.zero;
                pointCloud.GetComponent<MeshFilter>().mesh = new Mesh();
                pointClouds[i] = pointCloud;
            }

            for (int i = 0; i < numPointClouds; i++) {
                pointClouds[i].GetComponent<MeshFilter>().mesh = meshes[i];
            }
            yield return null;
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
