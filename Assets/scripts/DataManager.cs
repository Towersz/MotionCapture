
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;


[Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
}


[Serializable]
public class Data
{
    public List<Landmark> left_hand;
    public List<Landmark> right_hand;
    public List<Landmark> pose;
}


public class DataManager : MonoBehaviour
{
    public Body body;
    public Hand hand;
    Thread receiveThread;
    UdpClient client;
    public int port = 5054;
    public string receivedData;

    void Start()
    {
        
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                receivedData = Encoding.UTF8.GetString(dataByte);

                
                Debug.Log($"Received data: {receivedData}");

                
                if (receivedData.EndsWith("<EOM>"))
                {
                    receivedData = receivedData.Substring(0, receivedData.Length - "<EOM>".Length);
                    Debug.Log("Removed <EOM> from received data.");
                }

                
                Data jsonData = JObject.Parse(receivedData).ToObject<Data>();

                
                if (jsonData.left_hand != null && jsonData.left_hand.Count > 0)
                {
                    hand.left_hand_data = jsonData.left_hand.Select(l => new float[] { l.x, l.y, l.z }).ToArray();
                }
                else
                {
                    hand.left_hand_data = null;
                }

                if (jsonData.right_hand != null && jsonData.right_hand.Count > 0)
                {
                    hand.right_hand_data = jsonData.right_hand.Select(l => new float[] { l.x, l.y, l.z }).ToArray();
                }
                else
                {
                    hand.right_hand_data = null;
                }

                if (jsonData.pose != null && jsonData.pose.Count > 0)
                {
                    body.pose_data = jsonData.pose.Select(l => new float[] { l.x, l.y, l.z }).ToArray();
                }
                else
                {
                    body.pose_data = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving or parsing data: {e.Message}\nReceived Data: {receivedData}");
            }
        }
    }

    void Update()
    {
      
    }
}
