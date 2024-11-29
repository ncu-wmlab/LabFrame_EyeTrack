using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

#if USE_PICO
using Unity.XR.PXR;
#elif USE_VIVE_ANDROID
using Wave.Essence.Eye;
#endif

public class EyeTrackManager : LabSingleton<EyeTrackManager>, IManager
{    
    protected bool _doWriteLabData = false;

    protected EyeLeftRightData _leftRightData = null;
    protected EyeCombinedData _combinedData = null;
    protected EyeFocusData _focusData = null;


    public void ManagerInit()
    {
#if USE_PICO

#elif USE_VIVE_ANDROID
        if(EyeManager.Instance != null)
        {
            EyeManager.Instance.EnableEyeTracking = true;
        }
#else
        Debug.LogWarning("[EyeManager] Unsupported Platform or you haven't set platform in LabFrame2023 Menu!!");
#endif        
    }

    public IEnumerator ManagerDispose()
    {
        _doWriteLabData = false;
        _leftRightData = null;
        _combinedData = null;
        _focusData = null;
        yield break;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
#if USE_PICO
        bool result = true;

        // // Pico 眼動目前只有這六個資料，其他都收不到
        result &= PXR_EyeTracking.GetLeftEyeGazeOpenness(out var leftEyeOpenness);
        result &= PXR_EyeTracking.GetLeftEyePositionGuide(out var leftEyePositionGuide);
        result &= PXR_EyeTracking.GetRightEyeGazeOpenness(out var rightEyeOpenness);
        result &= PXR_EyeTracking.GetRightEyePositionGuide(out var rightEyePositionGuide);
        result &= PXR_EyeTracking.GetCombineEyeGazePoint(out var combineEyeGazePoint);
        result &= PXR_EyeTracking.GetCombineEyeGazeVector(out var combineEyeGazeVector);

        if (result)
        {
            // LR, combine data
            _leftRightData = new EyeLeftRightData
            {
               
                LeftEyeOpenness = leftEyeOpenness,
                LeftEyePositionGuide = leftEyePositionGuide,
                RightEyeOpenness = rightEyeOpenness,
                RightEyePositionGuide = rightEyePositionGuide,
            };
            _combinedData = new EyeCombinedData
            {
                CombineEyeGazeVector = combineEyeGazeVector,
                CombineEyeGazePoint = combineEyeGazePoint,
            };   

            // Write Lab Data
            if(_doWriteLabData)
            {
                LabDataManager.Instance.WriteData(_leftRightData);
                LabDataManager.Instance.WriteData(_combinedData);
            }         

            // Focus data
            Ray ray = new Ray(combineEyeGazePoint, combineEyeGazeVector);   
            Transform mainCamTransform = Camera.main.transform;            
            Ray rayGlobal = new Ray(mainCamTransform.position, mainCamTransform.TransformDirection(ray.direction));
            // Physics.Raycast(rayGlobal, out var hit, maxDistance, layerMask);  
            // Physics.SphereCast(rayGlobal, radius, out var hit, maxDistance, layerMask);            
            if(Physics.Raycast(rayGlobal, out var hit))
            {
                _focusData = new EyeFocusData
                {
                    FocusName = hit.collider.gameObject.name,
                    FocusPoint = hit.point,
                    FocusNormal = hit.normal,
                    FocusDistance = hit.distance,
                };

                if(_doWriteLabData)
                    LabDataManager.Instance.WriteData(_focusData);
            }
            else
                _focusData = null;
        }
#elif USE_VIVE_ANDROID
        bool result = true;

        result &= EyeManager.Instance.GetLeftEyeOrigin(out var LeftEyePosition);//左眼位置
        result &= EyeManager.Instance.GetRightEyeOrigin(out var RightEyePosition);//右眼位置
        result &= EyeManager.Instance.GetLeftEyePupilDiameter(out var LeftEyePupilDiameter);//左眼瞳孔直徑
        result &= EyeManager.Instance.GetRightEyePupilDiameter(out var RightEyePupilDiameter);//右眼瞳孔直徑
        result &= EyeManager.Instance.GetLeftEyeOpenness(out var LeftEyeOpenness);//左眼睜開程度
        result &= EyeManager.Instance.GetRightEyeOpenness(out var RightEyeOpenness);//右眼睜開程度
        result &= EyeManager.Instance.GetLeftEyeDirectionNormalized(out var LeftGazeDirection);//左眼方向
        result &= EyeManager.Instance.GetRightEyeDirectionNormalized(out var RightGazeDirection);//右眼方向
        result &= EyeManager.Instance.GetLeftEyePupilPositionInSensorArea(out var LeftPupilPositionInSensorArea);//左眼瞳孔在感測區域的位置
        result &= EyeManager.Instance.GetRightEyePupilPositionInSensorArea(out var RightPupilPositionInSensorArea);//右眼瞳孔在感測區域的位置

        result &= EyeManager.Instance.GetCombinedEyeOrigin(out var CombinedEyeOrigin);//合併眼位置
        result &= EyeManager.Instance.GetCombindedEyeDirectionNormalized(out var CombinedDirection);//合併眼方向

        if (result)
        {
            // _eyeLeftRightFocus3 = new EyeLeftRightFocus3
            // {
            //     LeftOrigin = LeftEyePosition,
            //     RightOrigin = RightEyePosition,
            //     LeftEyePupilDiameter = LeftEyePupilDiameter,
            //     RightEyePupilDiameter = RightEyePupilDiameter,
            //     LeftEyeOpenness = LeftEyeOpenness,
            //     RightEyeOpenness = RightEyeOpenness,
            //     LeftDirection = LeftGazeDirection,
            //     RightDirection = RightGazeDirection,
            
            // };

            // _eyeCombinedFocus3 = new EyeCombinedFocus3
            // {
            //     CombinedEyeOrigin = CombinedEyeOrigin,
            //     CombinedDirection = CombinedDirection,
            // };

            _leftRightData = new EyeLeftRightData{
                LeftOrigin = LeftEyePosition,
                RightOrigin = RightEyePosition,
                LeftEyePupilDiameter = LeftEyePupilDiameter,
                RightEyePupilDiameter = RightEyePupilDiameter,
                LeftEyeOpenness = LeftEyeOpenness,
                RightEyeOpenness = RightEyeOpenness,
                LeftDirection = LeftGazeDirection,
                RightDirection = RightGazeDirection,
            };

            _combinedData = new EyeCombinedData{
                CombineEyeGazePoint = CombinedEyeOrigin,
                CombineEyeGazeVector = CombinedDirection,
            };

            if(_doWriteLabData)
            {
                LabDataManager.Instance.WriteData(_leftRightData);
                LabDataManager.Instance.WriteData(_combinedData);
            }

            // Focus data
            Ray ray = new Ray(CombinedEyeOrigin, CombinedDirection);   
            Transform mainCamTransform = Camera.main.transform;            
            Ray rayGlobal = new Ray(mainCamTransform.position, mainCamTransform.TransformDirection(ray.direction));
            // Physics.Raycast(rayGlobal, out var hit, maxDistance, layerMask);  
            // Physics.SphereCast(rayGlobal, radius, out var hit, maxDistance, layerMask);            
            if(Physics.Raycast(rayGlobal, out var hit))
            {
                _focusData = new EyeFocusData
                {
                    FocusName = hit.collider.gameObject.name,
                    FocusPoint = hit.point,
                    FocusNormal = hit.normal,
                    FocusDistance = hit.distance,
                };

                if(_doWriteLabData)
                    LabDataManager.Instance.WriteData(_focusData);
            }
            else
                _focusData = null;
        }
#endif
    }


    #region Public Methods
    /// <summary>
    /// 是否自動寫入 LabData，請先確認 LabDataManager 已初始化
    /// </summary>
    /// <param name="enable"></param>
    public void AutoWriteLabData(bool enable = true)
    {
        _doWriteLabData = enable;
    }

    /// <summary>
    /// 取得 EyeLeftRightData，可能為 null
    /// </summary>
    /// <returns></returns>
    public EyeLeftRightData GetEyeLeftRightData()
    {
        return _leftRightData;
    }

    /// <summary>
    /// 取得 EyeCombinedData，可能為 null
    /// </summary>
    /// <returns></returns>
    public EyeCombinedData GetEyeCombinedData()
    {
        return _combinedData;
    }

    /// <summary>
    /// 取得 EyeFocusData，可能為 null
    /// </summary>
    /// <returns></returns>
    public EyeFocusData GetEyeFocusData()
    {
        return _focusData;
    }
    #endregion
}