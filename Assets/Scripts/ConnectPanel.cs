using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ConnectPanel : MonoBehaviour
{
    public TMP_InputField ip, port;

    internal bool CheckAnswers(out string ipTxt, out string portTxt)
    {
        Regex ipRegex, portRegex;
        ipRegex = new Regex(@"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
        portRegex = new Regex(@"[0-9]{1,5}");

        ipTxt = ip.text;
        portTxt = port.text;

        return ipRegex.IsMatch(ipTxt) && portRegex.IsMatch(portTxt);
    }
}
