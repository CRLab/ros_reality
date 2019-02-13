using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;


public class LabelWebsocketClient : MonoBehaviour {
    public Boolean active = true;
    private WebSocket ws;
    private int counter = 1;
    private bool connected = false;
    public Dictionary<string, string> messages = new Dictionary<string, string>();
    public string ip_address;
    private readonly object pcLock = new object();

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
        ws.Send("HI, I've subscribed");
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
        string msg = "{\"op\":\"subscribe\",\"id\":\"subscribe:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"throttle_rate\":" + throttle_rate.ToString() + ",\"queue_length\":0}";
        Debug.Log(msg);
        ws.Send(msg);
        counter++;
    }

    public void Unsubscribe(string topic) {
        string msg = "{\"op\":\"unsubscribe\",\"id\":\"unsubscribe:/" + topic + ":" + counter + "\",\"topic\":\"" + topic + "\"}";
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
    }

    public void Advertise(string topic, string type) {
        string msg = "{\"op\":\"advertise\",\"id\":\"advertise:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"latch\":false,\"queue_size\":0}";
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
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
        /****************************************
         * *******Current version********
         * **************************************/
        Debug.Log("message received");
        //var watch = System.Diagnostics.Stopwatch.StartNew();
        //Debug.Log(e.Data);
        //string[] input = e.Data.Split(new char[] { ',' }, 2);
        //string topic = input[0].Substring(12).Replace("\"", "");
        //messages[topic] = e.Data;
        //watch.Stop();
        //var elapsedMs = watch.ElapsedMilliseconds;
        //Debug.Log("OnMessageHandler time: " + elapsedMs);
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

