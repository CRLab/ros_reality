using UnityEngine;
using System.Collections;
using System;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DepthRosGeometryView : MonoBehaviour {

    private WebsocketClient wsc;
    string depthTopic;
    string colorTopic;
    int framerate = 100;
    public string compression = "none"; //"png" is the other option, haven't tried it yet though
    string depthMessage;
    string colorMessage;

    public Material Material;
    Texture2D depthTexture;
    Texture2D colorTexture;

    int width = 512;
    int height = 424;

    private Mesh mesh;
    int numPoints = 60000;

    Matrix4x4 m;

    // Use this for initialization
    void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();

        wsc = GameObject.Find("WebsocketClient").GetComponent<WebsocketClient>();
        depthTopic = "/head_camera/depth_downsample/points";
        //colorTopic = "kinect2/sd/image_color_rect/compressed_throttle";
        //wsc.Subscribe(depthTopic, "sensor_msgs/PointCloud2", compression, framerate);
        wsc.Subscribe(depthTopic, "sensor_msgs/PointCloud2", 0);
        //wsc.Subscribe(colorTopic, "sensor_msgs/CompressedImage", compression, framerate);
        InvokeRepeating("UpdateTexture", 0.1f, 0.1f);
    }

    void CreateMesh() {
        System.Random rand = new System.Random(DateTime.Now.Millisecond);
        Vector3[] points = new Vector3[numPoints];
        int[] indecies = new int[numPoints];
        Color[] colors = new Color[numPoints];

        for (int i = 0; i < points.Length; ++i) {
            points[i] = new Vector3(rand.Next(-10, 10), rand.Next(-10, 10), rand.Next(-10, 10));
            indecies[i] = i;
            colors[i] = new Color(0f, 1f, 0f, 1.0f);
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
    }


    // Update is called once per frame
    void UpdateTexture() {
        try {
            if (!wsc.messages.ContainsKey(depthTopic)) return;

            depthMessage = wsc.messages[depthTopic];
            byte[] depthImage = System.Convert.FromBase64String(depthMessage);


            //depthTexture.LoadRawTextureData(depthImage);
            //depthTexture.LoadImage(depthImage);
            //depthTexture.Apply();
            //Debug.Log(depthTexture.GetType());

        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }

        //try {
        //    colorMessage = wsc.messages[colorTopic];
        //    byte[] colorImage = System.Convert.FromBase64String(colorMessage);
        //    colorTexture.LoadImage(colorImage);
        //    colorTexture.Apply();
        //}
        //catch (Exception e) {
        //    Debug.Log(e.ToString());
        //    return;
        //}
    }
}
