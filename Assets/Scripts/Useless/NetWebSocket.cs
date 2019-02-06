using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.WebSockets;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Not useful now, could be saved for later use
/// </summary>

namespace NetWebSocketTest {

    public class NetWebSocket : MonoBehaviour {

        private WebSocketNet ws;
        private int counter = 1;
        private bool connected = false;
        public Dictionary<string, string> messages = new Dictionary<string, string>();
        public string ip_address;
        private readonly object pcLock = new object();

        // Connect happens in Awake so it is finished before other GameObjects are made
        void Awake() {

            Debug.Log("Net instantiating websocket...");
            ws = new WebSocketNet(ip_address);
            ws.OnReceive += (sender, e) => OnMessageHandler(sender, e);

            Debug.Log("Net Connecting to websocket");
            ws.Connect();
            //Subscribe("chatter", "std_msgs/Float32MultiArray", 100);
        }

        public void Subscribe(string topic, string type, int throttle_rate) {
            string msg = "{\"op\":\"subscribe\",\"id\":\"subscribe:/" + topic + ":" + counter + "\",\"type\":\"" + type + "\",\"topic\":\"/" + topic + "\",\"throttle_rate\":" + throttle_rate.ToString() + ",\"queue_length\":0}";
            Debug.Log(msg);
            ws.Send(Encoding.ASCII.GetBytes(msg));
            counter++;
        }
        

        private void OnMessageHandler(object sender, EventArgs e) {
            /****************************************
             * *******Current version********
             * **************************************/
            Debug.Log("Net message received");
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

    }
}

