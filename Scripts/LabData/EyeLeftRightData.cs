using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class EyeLeftRightData : LabDataBase
{
    /// <summary>
    /// 左眼的位置座標
    /// </summary>
    public Vector3 LeftEyePositionGuide;
    /// <summary>
    /// 左眼張開程度，1是全開，0是閉眼
    /// </summary>
    public float LeftEyeOpenness;

    /// <summary>
    /// 右眼的位置座標
    /// </summary>
    public Vector3 RightEyePositionGuide;
    /// <summary>
    /// 右眼張開程度，1是全開，0是閉眼
    /// </summary>
    public float RightEyeOpenness;
}