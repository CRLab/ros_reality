﻿using System.Collections;
using System.Collections.Generic;

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