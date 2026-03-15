using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LabFrame2023;

#if USE_PICO
using Unity.XR.PXR;
#endif

public class EyeTrackManager : LabSingleton<EyeTrackManager>, IManager
{
    protected bool _doWriteLabData = false;
    protected string _currentEyeTrackTag = "eyetrack";

    protected EyeLeftRightData _leftRightData = null;
    protected EyeCombinedData _combinedData = null;
    protected EyeFocusData _focusData = null;

#if USE_VIVE_ANDROID
    // Keep the Wave dependency optional so this package can still compile
    // before the host project installs Wave Essence.
    private const string WaveEyeManagerTypeName = "Wave.Essence.Eye.EyeManager";
    private const string WaveEyeManagerAssemblyQualifiedName = "Wave.Essence.Eye.EyeManager, Wave.Essence";

    private bool _waveReflectionInitialized = false;
    private bool _waveTypeMissingLogged = false;
    private bool _waveInstanceMissingLogged = false;

    private Type _waveEyeManagerType = null;
    private PropertyInfo _waveInstanceProperty = null;
    private PropertyInfo _waveEnableEyeTrackingProperty = null;
    private readonly Dictionary<string, MethodInfo> _waveMethodCache = new Dictionary<string, MethodInfo>();
#endif

    public void ManagerInit()
    {
#if USE_PICO

#elif USE_VIVE_ANDROID
        if (!TrySetWaveEnableEyeTracking(true))
        {
            Debug.LogWarning("[EyeTrackManager] USE_VIVE_ANDROID is enabled, but Wave EyeManager is not available yet.");
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

        result &= PXR_EyeTracking.GetLeftEyeGazeOpenness(out var leftEyeOpenness);
        result &= PXR_EyeTracking.GetLeftEyePositionGuide(out var leftEyePositionGuide);
        result &= PXR_EyeTracking.GetRightEyeGazeOpenness(out var rightEyeOpenness);
        result &= PXR_EyeTracking.GetRightEyePositionGuide(out var rightEyePositionGuide);
        result &= PXR_EyeTracking.GetCombineEyeGazePoint(out var combineEyeGazePoint);
        result &= PXR_EyeTracking.GetCombineEyeGazeVector(out var combineEyeGazeVector);
        if (result)
        {
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

            if (_doWriteLabData)
            {
                LabDataManager.Instance.WriteData(_leftRightData, _currentEyeTrackTag);
                LabDataManager.Instance.WriteData(_combinedData, _currentEyeTrackTag);
            }

            Ray ray = new Ray(combineEyeGazePoint, combineEyeGazeVector);
            Transform mainCamTransform = Camera.main.transform;
            Ray rayGlobal = new Ray(mainCamTransform.position, mainCamTransform.TransformDirection(ray.direction));
            if (Physics.Raycast(rayGlobal, out var hit))
            {
                _focusData = new EyeFocusData
                {
                    FocusName = hit.collider.gameObject.name,
                    FocusPoint = hit.point,
                    FocusNormal = hit.normal,
                    FocusDistance = hit.distance,
                };

                if (_doWriteLabData)
                {
                    LabDataManager.Instance.WriteData(_focusData, _currentEyeTrackTag);
                }
            }
            else
            {
                _focusData = null;
            }
        }
#elif USE_VIVE_ANDROID
        bool result = true;

        result &= TryInvokeWaveOutMethod("GetLeftEyeOrigin", Vector3.zero, out var leftEyePosition);
        result &= TryInvokeWaveOutMethod("GetRightEyeOrigin", Vector3.zero, out var rightEyePosition);
        result &= TryInvokeWaveOutMethod("GetLeftEyePupilDiameter", 0f, out var leftEyePupilDiameter);
        result &= TryInvokeWaveOutMethod("GetRightEyePupilDiameter", 0f, out var rightEyePupilDiameter);
        result &= TryInvokeWaveOutMethod("GetLeftEyeOpenness", 0f, out var leftEyeOpenness);
        result &= TryInvokeWaveOutMethod("GetRightEyeOpenness", 0f, out var rightEyeOpenness);
        result &= TryInvokeWaveOutMethod("GetLeftEyeDirectionNormalized", Vector3.zero, out var leftGazeDirection);
        result &= TryInvokeWaveOutMethod("GetRightEyeDirectionNormalized", Vector3.zero, out var rightGazeDirection);
        result &= TryInvokeWaveOutMethod("GetLeftEyePupilPositionInSensorArea", Vector2.zero, out var leftPupilPositionInSensorArea);
        result &= TryInvokeWaveOutMethod("GetRightEyePupilPositionInSensorArea", Vector2.zero, out var rightPupilPositionInSensorArea);
        result &= TryInvokeWaveOutMethod("GetCombinedEyeOrigin", Vector3.zero, out var combinedEyeOrigin);
        result &= TryInvokeWaveOutMethod("GetCombindedEyeDirectionNormalized", Vector3.zero, out var combinedDirection);
        if (result)
        {
            _leftRightData = new EyeLeftRightData
            {
                LeftOrigin = leftEyePosition,
                RightOrigin = rightEyePosition,
                LeftEyePupilDiameter = leftEyePupilDiameter,
                RightEyePupilDiameter = rightEyePupilDiameter,
                LeftEyeOpenness = leftEyeOpenness,
                RightEyeOpenness = rightEyeOpenness,
                LeftDirection = leftGazeDirection,
                RightDirection = rightGazeDirection,
            };

            _combinedData = new EyeCombinedData
            {
                CombineEyeGazePoint = combinedEyeOrigin,
                CombineEyeGazeVector = combinedDirection,
            };

            if (_doWriteLabData)
            {
                LabDataManager.Instance.WriteData(_leftRightData, _currentEyeTrackTag);
                LabDataManager.Instance.WriteData(_combinedData, _currentEyeTrackTag);
            }

            Ray ray = new Ray(combinedEyeOrigin, combinedDirection);
            Transform mainCamTransform = Camera.main.transform;
            Ray rayGlobal = new Ray(mainCamTransform.position, mainCamTransform.TransformDirection(ray.direction));
            if (Physics.Raycast(rayGlobal, out var hit))
            {
                _focusData = new EyeFocusData
                {
                    FocusName = hit.collider.gameObject.name,
                    FocusPoint = hit.point,
                    FocusNormal = hit.normal,
                    FocusDistance = hit.distance,
                };

                if (_doWriteLabData)
                {
                    LabDataManager.Instance.WriteData(_focusData, _currentEyeTrackTag);
                }
            }
            else
            {
                _focusData = null;
            }
        }
#endif
    }

#if USE_VIVE_ANDROID
    private void EnsureWaveReflection()
    {
        if (_waveReflectionInitialized)
        {
            return;
        }

        _waveReflectionInitialized = true;
        _waveEyeManagerType = Type.GetType(WaveEyeManagerAssemblyQualifiedName);

        if (_waveEyeManagerType == null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                _waveEyeManagerType = assemblies[i].GetType(WaveEyeManagerTypeName);
                if (_waveEyeManagerType != null)
                {
                    break;
                }
            }
        }

        if (_waveEyeManagerType == null)
        {
            if (!_waveTypeMissingLogged)
            {
                _waveTypeMissingLogged = true;
                Debug.LogWarning("[EyeTrackManager] Wave Essence assembly was not found. Vive eye tracking will stay disabled until the package is available.");
            }
            return;
        }

        _waveInstanceProperty = _waveEyeManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        _waveEnableEyeTrackingProperty = _waveEyeManagerType.GetProperty("EnableEyeTracking", BindingFlags.Public | BindingFlags.Instance);
    }

    private object GetWaveEyeManagerInstance()
    {
        EnsureWaveReflection();
        if (_waveInstanceProperty == null)
        {
            return null;
        }

        var instance = _waveInstanceProperty.GetValue(null, null);
        if (instance == null)
        {
            if (!_waveInstanceMissingLogged)
            {
                _waveInstanceMissingLogged = true;
                Debug.LogWarning("[EyeTrackManager] Wave EyeManager.Instance is null. Add the Wave EyeManager component before using Vive eye tracking.");
            }
            return null;
        }

        _waveInstanceMissingLogged = false;
        return instance;
    }

    private MethodInfo GetWaveMethod(string methodName)
    {
        EnsureWaveReflection();
        if (_waveEyeManagerType == null)
        {
            return null;
        }

        if (_waveMethodCache.TryGetValue(methodName, out var cachedMethod))
        {
            return cachedMethod;
        }

        var methods = _waveEyeManagerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < methods.Length; i++)
        {
            if (methods[i].Name == methodName && methods[i].GetParameters().Length == 1)
            {
                _waveMethodCache[methodName] = methods[i];
                return methods[i];
            }
        }

        return null;
    }

    private bool TrySetWaveEnableEyeTracking(bool enable)
    {
        var instance = GetWaveEyeManagerInstance();
        if (instance == null || _waveEnableEyeTrackingProperty == null)
        {
            return false;
        }

        _waveEnableEyeTrackingProperty.SetValue(instance, enable, null);
        return true;
    }

    private bool TryInvokeWaveOutMethod<T>(string methodName, T defaultValue, out T value)
    {
        value = defaultValue;

        var instance = GetWaveEyeManagerInstance();
        var method = GetWaveMethod(methodName);
        if (instance == null || method == null)
        {
            return false;
        }

        try
        {
            var args = new object[] { defaultValue };
            var invoked = method.Invoke(instance, args);

            if (args[0] is T typedValue)
            {
                value = typedValue;
            }

            return invoked is bool success && success;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[EyeTrackManager] Wave call failed for " + methodName + ": " + ex.Message);
            return false;
        }
    }
#endif

    #region Public Methods
    public void AutoWriteLabData(bool enable = true, string tag = "eyetrack")
    {
        _doWriteLabData = enable;
        if (!string.IsNullOrEmpty(tag))
        {
            _currentEyeTrackTag = tag;
        }
    }

    public void SetEyeTrackTag(string tag)
    {
        _currentEyeTrackTag = string.IsNullOrEmpty(tag) ? "eyetrack" : tag;
        Debug.Log($"[EyeTrackManager] EyeTrack data tag dynamically changed to: {_currentEyeTrackTag}");
    }

    public EyeLeftRightData GetEyeLeftRightData()
    {
        return _leftRightData;
    }

    public EyeCombinedData GetEyeCombinedData()
    {
        return _combinedData;
    }

    public EyeFocusData GetEyeFocusData()
    {
        return _focusData;
    }
    #endregion
}
