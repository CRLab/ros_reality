using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DepthRosGeometryView : MonoBehaviour {

    private WebsocketClient wsc;

    public bool active = false;
    string depthTopic = "head_camera/depth_downsample/points";
    string colorTopic;
    TFListener tfListener;
    float scale;
    //int framerate = 100;
    //public string compression = "none"; //"png" is the other option, haven't tried it yet though

    private Mesh mesh;
    //int numPoints = 60000;

    Matrix4x4 m;

    // Use this for initialization
    void Start() {
        if (!active) return;

        tfListener = GameObject.Find("TFListener").GetComponent<TFListener>();
        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();
        //colorTopic = "kinect2/sd/image_color_rect/compressed_throttle";
        //wsc.Subscribe(depthTopic, "sensor_msgs/PointCloud2", compression, framerate);
        wsc.Subscribe(depthTopic, "sensor_msgs/PointCloud2", 1000*2*2*2);
        //wsc.Subscribe(colorTopic, "sensor_msgs/CompressedImage", compression, framerate);
        InvokeRepeating("UpdateTexture", 0.1f, 2f);
    }

    // Update is called once per frame
    void UpdateTexture() {
        if (!wsc.messages.ContainsKey(depthTopic)) return;

        scale = tfListener.scale;

        // reset mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Get pointcloud message
        String depthMessage = wsc.messages[depthTopic];
        PointCloudMsg pc = JsonConvert.DeserializeObject<PointCloud>(depthMessage).msg;

        // Init offset values
        int xOffset = 0, yOffset = 0, zOffset = 0;
        foreach (PointField field in pc.fields) {
            switch (field.name) {
                case "x": xOffset = field.offset; break;
                case "y": yOffset = field.offset; break;
                case "z": zOffset = field.offset; break;
            }
        }

        // init arrays
        int numPoints = pc.data.Length / pc.point_step;
        Vector3[] points = new Vector3[numPoints];
        int[] indices = new int[numPoints];
        Color[] colors = new Color[numPoints];

        // get each 3D point
        for (int i = 0; i < pc.data.Length; i += pc.point_step) {
            float x = BitConverter.ToSingle(pc.data, i + xOffset);
            float y = BitConverter.ToSingle(pc.data, i + yOffset);
            float z = BitConverter.ToSingle(pc.data, i + zOffset);

            int index = i / pc.point_step;
            points[index] = RosToUnityPositionAxisConversion(new Vector3(x, y, z)) / scale;
            indices[index] = index;
            colors[index] = new Color(0f, 1f, 0f, 1.0f);
        }

        // assign points and colors to mesh
        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);
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
