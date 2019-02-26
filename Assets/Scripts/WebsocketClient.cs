using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json;


public class WebsocketClient : MonoBehaviour {
    public enum CT { BSON, JSON };
    public Boolean active = true;
    private WebSocket ws;
    private int counter = 1;
    private bool connected = false;
    public Dictionary<string, string> messages = new Dictionary<string, string>();
    public string ip_address;
    private readonly object pcLock = new object();
    public PointCloudMsg pc;
    public CT connectionType;

    int debugCount = 0;

    // Connect happens in Awake so it is finished before other GameObjects are made
    void Awake() {
        if (!active)
            return;
        Debug.Log("instantiating websocket...");
        ws = new WebSocket(ip_address);

        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;

        Debug.Log("Connecting to websocket");
        ws.Connect();
    }

    void OnApplicationQuit() {
        if (!active)
            return;
        ws.Close();
    }

    public void Subscribe(string topic, string type, string compression, int throttle_rate) {
        string msg = "{\"op\":\"subscribe\",\"id\":\"subscribe:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"compression\":\"" + compression + "\",\"throttle_rate\":" + throttle_rate.ToString() + ",\"queue_length\":0}";
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
        counter++;
    }

    public void Subscribe(string topic, string type, int throttle_rate) {
        //subscribe the data in BSON way
        if (connectionType == CT.BSON) {
            //create the subscription type
            SubMsg subm = new SubMsg {
                op = "subscribe",
                id = "subscribe:/ " + topic + ":" + counter,
                type = type,
                topic = topic,
                throttle_rate = throttle_rate,
                queue_length = 0
            };

            //serialize it to a BSON string
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, subm);
            }
            ws.Send(ms.ToArray());
            counter++;
        }

        //subscribe the data in JSON way
        else {
            string msg = "{\"op\":\"subscribe\",\"id\":\"subscribe:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"throttle_rate\":" + throttle_rate.ToString() + ",\"queue_length\":0}";
            Debug.Log(msg);
            ws.Send(msg);
            counter++;
        }
    }

    public void Unsubscribe(string topic) {
        string msg = "{\"op\":\"unsubscribe\",\"id\":\"unsubscribe:/" + topic + ":" + counter + "\",\"topic\":\"/" + topic + "\"}";
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
    }

    public void Advertise(string topic, string type) {
        string msg = "{\"op\":\"advertise\",\"id\":\"advertise:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"latch\":false,\"queue_size\":0}";
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
        counter++;

    }

    public void CallService(string service, string argsName, string argsVal) {
        string msg = "{\"service\":\""+service+"\",\"args\":{\""+argsName+"\":"+argsVal+"}," +
                            "\"fragment_size\":2147483647,\"compression\":\"none\",\"op\":\"call_service\",\"id\":\"callService\"}";
        Debug.Log(msg);
        //ws.SendAsync(msg, OnSendComplete);
        counter++;
    }

    public void Publish(string topic, string message) {
        string msg = "{\"op\":\"publish\",\"id\":\"publish:/" + topic + ":" + counter + "\",\"topic\":\"/" + topic + "\",\"msg\":{\"data\":\"" + message + "\"},\"latch\":false}";
        ws.SendAsync(msg, OnSendComplete);
        counter++;
    }

    public void SendEinMessage(string message) {
        Debug.Log("MSG: " + message);
        Publish("forth_commands", message);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e) {
        //store data in BSON way
        if (connectionType == CT.BSON) {
            //lock to make sure the modification doesn't overlap
            Debug.Log(debugCount++);
            //if (Monitor.TryEnter(pcLock)) {
            //    //deserialize the BSON message and assign it to 
            //    MemoryStream ms = new MemoryStream(e.RawData);
            //    RecMsg msg;
            //    using (BsonReader reader = new BsonReader(ms)) {
            //        JsonSerializer serializer = new JsonSerializer();
            //        msg = serializer.Deserialize<RecMsg>(reader);
            //    }
            //    //assign it to the pc field
            //    pc = msg.msg;
            //    Monitor.Exit(pcLock);
            //}
            //else {
            //    return;
            //}
        }

        //store data in Json way
        else {

            //lock the variable if topic is cloud point
            if (Monitor.TryEnter(pcLock)) {
                string[] input = e.Data.Split(new char[] { ',' }, 2);
                string topic = input[0].Substring(12).Replace("\"", "");
                messages[topic] = e.Data;
                //var elapsedMs = sw.ElapsedMilliseconds;
                Debug.Log("New Message: "+ e.Data);
                Monitor.Exit(pcLock);
            }
            else {
                //var elapsedMs = sw.ElapsedMilliseconds;
                //Debug.Log("Unable to lock and exit in " + elapsedMs + "ms");
                return;
            }
        }

    }

    private void OnOpenHandler(object sender, System.EventArgs e) {
        Debug.Log("WebSocket connected!");
        connected = true;
    }

    private void OnCloseHandler(object sender, CloseEventArgs e) {
        Debug.Log("WebSocket closed");
    }

    private void OnSendComplete(bool success) {
        //Debug.Log("Message sent successfully? " + success);
    }

    public bool IsConnected() {
        return connected;
    }
}

