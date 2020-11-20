using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    public float deadzone = 0.15f;
    private float invertDeadzone;

    public float HeightValue => leftJoystick.Vertical;
    public float RollValue => leftJoystick.Horizontal;
    public float YValue => rightJoystick.Vertical;
    public float XValue => rightJoystick.Horizontal;

    public Joystick leftJoystick, rightJoystick;

    public RawImage m_tex;

    private void Awake()
    {
        invertDeadzone = 1 - deadzone;
    }

    public void SetTex(Texture2D tex)
    {
        m_tex.texture = tex;
    }
}
