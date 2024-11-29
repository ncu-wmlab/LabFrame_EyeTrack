using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class EyeLeftRightData : LabDataBase
{
#if USE_PICO
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
#elif USE_VIVE_ANDROID
    /// <summary>
    /// Gets or sets the origin point of the left eye.
    /// </summary>
    public Vector3 LeftOrigin;

    /// <summary>
    /// Gets or sets the direction vector of the left eye.
    /// </summary>
    public Vector3 LeftDirection;

    /// <summary>
    /// Gets or sets the origin point of the right eye.
    /// </summary>
    public Vector3 RightOrigin;

    /// <summary>
    /// Gets or sets the direction vector of the right eye.
    /// </summary>
    public Vector3 RightDirection;

    /// <summary>
    /// Gets or sets the openness value of the left eye. 
    /// Value ranges from 0.0 (fully closed) to 1.0 (fully open).
    /// </summary>
    public float LeftEyeOpenness;

    /// <summary>
    /// Gets or sets the openness value of the right eye. 
    /// Value ranges from 0.0 (fully closed) to 1.0 (fully open).
    /// </summary>
    public float RightEyeOpenness;

    /// <summary>
    /// Gets or sets the pupil diameter of the left eye in millimeters.
    /// </summary>
    public float LeftEyePupilDiameter;

    /// <summary>
    /// Gets or sets the pupil diameter of the right eye in millimeters.
    /// </summary>
    public float RightEyePupilDiameter;

    /// <summary>
    /// Gets or sets the position of the left eye pupil in the sensor area.
    /// </summary>
    public Vector2 LeftEyePupliPositionInSensorArea;

    /// <summary>
    /// Gets or sets the position of the right eye pupil in the sensor area.
    /// </summary>
    public Vector2 RightEyePupliPositionInSensorArea;
#endif
}