using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoEyeTrackScene : MonoBehaviour
{
    [SerializeField] Text _demoText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _demoText.text = $"==EyeCombinedData==\n{JsonUtility.ToJson(EyeTrackManager.Instance.GetEyeCombinedData())}" + 
            $"\n\n==EyeLeftRightData==\n{JsonUtility.ToJson(EyeTrackManager.Instance.GetEyeLeftRightData())}" +
            $"\n\n==EyeFocusData==\n{JsonUtility.ToJson(EyeTrackManager.Instance.GetEyeFocusData())}";
    }
}
