using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud {
    public PointCloudMsg msg { get; set; }
}

public class PointCloudMsg {
    public int height { get; set; }
    public int width { get; set; }
    public bool is_bigendian { get; set; }
    public int point_step { get; set; }
    public int row_step { get; set; }
    public byte[] data { get; set; }
    public bool is_dense { get; set; }
    public PointField[] fields { get; set; }
}

public class PointField {
    public string name { get; set; }
    public int offset { get; set; }
    public byte datatype { get; set; }
    public int count { get; set; }
}


//For getting pointcloud from image segmentation

public class TF {
    public TFMsg msg { get; set; }
}

public class TFMsg {
    public string data { get; set; }
}

public class SubMsg {
    public string op { get; set; }
    public string id { get; set; }
    public string type { get; set; }
    public string topic { get; set; }
    public int throttle_rate { get; set; }
    public int queue_length { get; set; }
}

public class RecMsg {
    public string topic { get; set; }
    public PointCloudMsg msg { get; set; }
}

public class SocketPointCloud {
    public SocketPCDMsg msg { get; set; }
}

public class SocketPCDMsg {
    public float[] data { get; set; }
}


//for getting the mesh message

public class MeshObj {
    public MeshMsgList msg { get; set; }
}

public class MeshMsgList {
    public MeshMsg[] meshes { get; set; }
}

public class MeshMsg {
    public MeshTriangle[] triangles { get; set; }
    public Point[] vertices { get; set; }
}
 public class MeshTriangle {
    public int[] vertex_indices { get; set; }
}

public class Point {
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}
