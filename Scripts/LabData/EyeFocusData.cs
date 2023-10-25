using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class EyeFocusData : LabDataBase
{
    /// <summary>
    /// 當下正在看的物件名稱
    /// </summary>
    /// <value></value>
    public string FocusName;

    /// <summary>
    /// 正在看的位置
    /// </summary>
    /// <value></value>
    public Vector3 FocusPoint;

    /// <summary>
    /// 正在看的方向
    /// </summary>
    /// <value></value>
    public Vector3 FocusNormal;

    /// <summary>
    /// 正在看的物件距離
    /// </summary>
    /// <value></value>
    public float FocusDistance;
}