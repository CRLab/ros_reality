/***************************************************************
 * ******* Test visualizing pointcloud from CSV file************
 * *************************************************************/

using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.IO;


public class CSVPointCloud : MonoBehaviour {
    public bool active = false;
    public GameObject pointCloudPrefab;

    string colorTopic;
    TFListener tfListener;
    float scale;

    bool inited = false;

    private int maxPoints = 60000;  // max points allowed in one mesh

    Color[] categories = { Color.black, Color.red, Color.green, Color.blue, Color.cyan, Color.yellow, new Color(1f, 0f, 1f,1f), new Color(0f,0f,188f/255f,1f)};
    string csv_file_path = "CSVImage\\" + "lbl_pcd_bg_" +"0000" + ".csv";
    bool single = false;//whether we are using a single picture
    int maxImage = 49;//total frames
    private int curImage = 0; //mark the current picture to display

    // Use this for initialization
    void Start() {
        if (!active) return;

        tfListener = GameObject.Find("TFListener").GetComponent<TFListener>();
        if (single) {
            UpdateTexture();
        }
        else {
            InvokeRepeating("UpdateTexture", 0.1f, 5f);
        }
        
    }

    // Update is called once per frame
    void UpdateTexture() {
        int totalNumField = 6; //total number of field used

        // REMOVE ME
        //if (inited) return;

        inited = true;

        scale = tfListener.scale;

        // reset mesh
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) {
            if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            Destroy(child.gameObject);
        }

        /*************************************read starting from here**************************************/
        if (!single) {
                csv_file_path ="CSVImage\\Jeff_pcd_5\\"+ "lbl_pcd_color_"+ curImage.ToString().PadLeft(4, '0')+".csv";
                curImage++;
                if (curImage > maxImage) {
                    curImage = 0;
                }
        }
        
        int totalPoints = System.IO.File.ReadAllLines(csv_file_path).Length / totalNumField;

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

        
        try {
            using (var csvReader = new StreamReader(csv_file_path)) {
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
                        //Debug.Log(2);
                        string data = csvReader.ReadLine();
                        //Debug.Log(3);
                        //Making empty value as null
                        switch (counter % totalNumField) {
                            case 0: x = float.Parse(data); break;
                            case 1: y = float.Parse(data); break;
                            case 2: z = float.Parse(data); break;
                            case 3: r = (int)float.Parse(data); break;
                            case 4: g = (int)float.Parse(data); break;
                            case 5:
                                if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z)) break;
                                b = (int)float.Parse(data);
                                int pointIndex = counter / totalNumField;
                                // store the points and colors
                                points[pointIndex] = RosToUnityPositionAxisConversion(new Vector3(x, y, z)) / scale;
                                indices[pointIndex] = pointIndex;
                                colors[pointIndex] = new Color(r / 255f, g / 255f, b / 255f, 1.0f); //categories[color];
                                break;
                        }
                    }
                    // Assign the points and colors to the current point cloud mesh
                    Mesh mesh = new Mesh();
                    mesh.vertices = points;
                    mesh.colors = colors;
                    mesh.SetIndices(indices, MeshTopology.Points, 0);
                    pointClouds[i].GetComponent<MeshFilter>().mesh = mesh;
                    
                }

            }
        }
        catch (Exception ex) {
            Debug.Log("unable to read csv"+ex);
            return;
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
