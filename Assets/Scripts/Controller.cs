using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Linq;

public class Controller : MonoBehaviour
{

    public ConnectPanel connectPanel;
    public ControlPanel controlPanel;
    private TcpClient m_tcpClientControl;
    private TcpClient m_tcpClientCamFeed;
    private NetworkStream m_controlStream;
    private Socket m_camClient;
    private NetworkStream m_camStream;
    private bool m_controlMode;
    private float m_refreshDelay;
    private float m_time;
    public float refreshRate = 10;
    byte[] buffer = new byte[IMAGE_BUFFER_SIZE];
    private bool m_isDirty;
    private int m_currBufferPos;
    private bool m_lookForStart;
    private byte[] m_currBytes;
    public const int IMAGE_BUFFER_SIZE = 1024 * 1024 * 5; //5Mo buffer

    // Start is called before the first frame update
    void Start()
    {
        SetConnected(false);
        m_refreshDelay = 1f / refreshRate;
        m_time = 0;
    }

    private void SetConnected(bool val)
    {
        m_controlMode = val;
        connectPanel.gameObject.SetActive(!val);
        controlPanel.gameObject.SetActive(val);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_controlMode)
        {
            if (!m_tcpClientControl.Connected)
            {
                m_tcpClientControl = null;
                SetConnected(false);
            }


            m_time += Time.deltaTime;
            if (m_time >= m_refreshDelay)
            {
                m_time -= m_refreshDelay;
                Refresh();
            }

            if (m_isDirty)
            {
                m_isDirty = false;
                Texture2D tex = new Texture2D(1, 1);
                if (tex.LoadImage(m_currBytes))
                {
                    if (tex.width > 16 && tex.height > 16) //Remove corrupted images
                        controlPanel.SetTex(tex);
                }
            }
        }

    }

    private void Refresh()
    {
        StringBuilder formated = new StringBuilder();
        formated.AppendLine($"##");
        formated.AppendLine($"type:INPUT");
        formated.AppendLine($"input_height: {controlPanel.HeightValue}".Replace(',', '.'));
        formated.AppendLine($"input_roll: {controlPanel.RollValue}".Replace(',', '.'));
        formated.AppendLine($"input_translate_y: {controlPanel.YValue}".Replace(',', '.'));
        formated.AppendLine($"input_translate_x: {controlPanel.XValue}".Replace(',', '.'));
        formated.AppendLine($"##");
        SendInput(formated.ToString());
    }

    private void SendInput(string formated)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(formated);
        m_controlStream.Write(bytes, 0, bytes.Length);
    }

    public void TakeOff()
    {
        StringBuilder formated = new StringBuilder();
        formated.AppendLine($"##");
        formated.AppendLine($"type:COMMAND");
        formated.AppendLine($"command: TAKEOFF");
        formated.AppendLine($"##");
        SendInput(formated.ToString());
    }

    public void Land()
    {
        StringBuilder formated = new StringBuilder();
        formated.AppendLine($"##");
        formated.AppendLine($"type:COMMAND");
        formated.AppendLine($"command: LAND");
        formated.AppendLine($"##");
        SendInput(formated.ToString());
    }

    public void ToggleSurveillance()
    {
        StringBuilder formated = new StringBuilder();
        formated.AppendLine($"##");
        formated.AppendLine($"type:COMMAND");
        formated.AppendLine($"command: SURVEILLANCE");
        formated.AppendLine($"##");
        SendInput(formated.ToString());
    }

    public void Emergency()
    {
        StringBuilder formated = new StringBuilder();
        formated.AppendLine($"##");
        formated.AppendLine($"type:COMMAND");
        formated.AppendLine($"command: EMERGENCY");
        formated.AppendLine($"##");
        SendInput(formated.ToString());
    }

    public void Disconnect()
    {
        Land();
        m_tcpClientCamFeed?.Close();
        m_tcpClientControl?.Close();
        SetConnected(false);
    }

    private void OnDestroy()
    {
        Disconnect();
    }




    public void Connect()
    {
        string ip, port;
        if (connectPanel.CheckAnswers(out ip, out port))
        {
            int portInt = int.Parse(port);
            m_tcpClientControl = new TcpClient(ip, portInt);
            m_tcpClientCamFeed = new TcpClient(ip, portInt + 1);

            if (m_tcpClientControl.Connected && m_tcpClientCamFeed.Connected)
            {
                Debug.Log("Connected");
                SetConnected(true);
                m_controlStream = m_tcpClientControl.GetStream();
                m_camClient = m_tcpClientCamFeed.Client;
                m_camStream = m_tcpClientCamFeed.GetStream();
                Thread t = new Thread(RecieveThread);
                t.Start();


                m_lookForStart = true;
                buffer = new byte[IMAGE_BUFFER_SIZE];
                m_currBufferPos = 0;
            }
            else
            {
                Debug.Log("Couldn't connect");
            }
        }
        else
        {
            Debug.Log("Wrong input");
        }
    }

    public void RecieveThread()
    {
        while (m_tcpClientCamFeed.Connected)
        {
            Thread.Sleep(45);
            if (m_camStream.CanRead)
            {
                byte[] tempBuffer = new byte[IMAGE_BUFFER_SIZE];
                m_camStream.ReadTimeout = 2000;
                bool working = true;
                var requestObj = new object();
                int bytesRead = m_camStream.Read(tempBuffer, 0, IMAGE_BUFFER_SIZE);

                Debug.Log("Recieved " + bytesRead + " bytes");

                m_currBytes = new byte[bytesRead];
                Array.Copy(tempBuffer, m_currBytes, bytesRead);
                m_isDirty = true;
            }

        }
    }

}
