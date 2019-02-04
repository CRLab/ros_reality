using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;


public class WebsocketClient : MonoBehaviour {

    private WebSocket ws;
    private int counter = 1;
    private bool connected = false;
    public Dictionary<string, string> messages = new Dictionary<string, string>();
    public string ip_address;
    private readonly object pcLock = new object();

    // Connect happens in Awake so it is finished before other GameObjects are made
    void Awake() {
        Debug.Log("instantiating websocket...");
        ws = new WebSocket(ip_address);

        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;

        Debug.Log("Connecting to websocket");
        ws.Connect();
    }

    void OnApplicationQuit() {
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
        string[] input = e.Data.Split(new char[] { ',' }, 2);
        string topic = input[0].Substring(12).Replace("\"", "");
        messages[topic] = e.Data;

        /****************************************
         * *******Latest version by David********
         * **************************************/

        //var sw = System.Diagnostics.Stopwatch.StartNew();
        //var elapsedMs = sw.ElapsedMilliseconds;

        //Debug.Log("Do nothing in " + elapsedMs + "ms");

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //int topic_index = 12;
        //int end_topic_index = topic_index;
        ////for (end_topic_index = topic_index; e.Data[end_topic_index].Equals("\""); ++end_topic_index) {
        ////    Debug.Log(e.Data[end_topic_index]);
        ////}
        //Debug.Log(end_topic_index);
        //elapsedMs = sw.ElapsedMilliseconds;
        //Debug.Log("Get index in " + elapsedMs + "ms");
        ////char[] topic_arr = new char[end_topic_index - topic_index];
        ////for(var index = 0; index < topic_arr.Length; ++index) {
        ////    topic_arr[index] = e.Data[topic_index + index];
        ////}
        ////Debug.Log(end_topic_index);
        //sw = System.Diagnostics.Stopwatch.StartNew();
        //string topic = e.Data.Substring(topic_index, end_topic_index - 1);
        ////string topic = new string(topic_arr);
        //elapsedMs = sw.ElapsedMilliseconds;
        //Debug.Log("[" + topic + "] Scanned string in " + elapsedMs + "ms");

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //messages[topic] = e.Data;
        //elapsedMs = sw.ElapsedMilliseconds;
        //Debug.Log("[" + topic + "] Store in wsc in " + elapsedMs + "ms");
        //sw.Stop();

        // Read e.Data as a JSON object
        // string topic = json_data["topic"];
        // string data = json_data["msg"];
        // messages[topic] = data;

        //input[0] is always {"topic": "<topic_name>"
        // There are 11 characters before topic name


        /****************************************
         * *******Previous versions by David********
         * **************************************/
        //@"{\"topic\":\"/(\w+)"
        //sw = System.Diagnostics.Stopwatch.StartNew();
        //string pattern = "topic\\\": \\\"/" + @"(\w+)";
        //Regex regex = new Regex(pattern);
        //Match match = regex.Match(e.Data);
        //string topic = match.Groups[1].ToString();
        //var elapsedMs = sw.ElapsedMilliseconds;
        //Debug.Log("[" + topic + "] Regex applied in " + elapsedMs + "ms");
        /*if (match.Success) {
            Debug.Log("Match: " + match.Value);
            Debug.Log("topic: " + match.Groups[1]);
        }*/

        //string[] input = e.Data.Split(new char[] { ',' }, 2);
        //var elapsedMs = sw.ElapsedMilliseconds;
        //Debug.Log("First time: " + elapsedMs);

        //string topic = input[0].Substring(12).Replace("\"", "");
        //elapsedMs = sw.ElapsedMilliseconds - elapsedMs;
        //Debug.Log("Second time: " + elapsedMs);

        //Debug.Log("Number of strings: " + input.Length);
        //Debug.Log(input[0]);
        //Debug.Log(input[1]);


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

