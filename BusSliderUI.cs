using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************
 *  슬라이더 UI를 통해 원하는 Bus의 볼륨을 조절하는 컴포넌트...
 * ***/
[RequireComponent(typeof(UnityEngine.UI.Slider))]
public sealed class BusSliderUI : MonoBehaviour
{
    //======================================
    /////      property and Felds       ////
    //======================================
    [SerializeField] FModBusType busType;

    private UnityEngine.UI.Slider _slider;


    //===========================================
    /////      Magic and Core methods...     /////
    //===========================================
    private void Start()
    {
        /**초기화 및 bus초기값 적용...*/
        if(_slider = GetComponent<UnityEngine.UI.Slider>())
        {
            float busVolume = Mathf.Clamp(FModAudioManager.GetBusVolume(busType), 0f, 1f);

            FModAudioManager.SetBusVolume(busType, busVolume);
            _slider.value = busVolume;
            _slider.onValueChanged.AddListener(delegate { ValueChanged(); });  
        }
    }

    private void ValueChanged()
    {
        /**슬라이더의 값이 바뀌면 FMod Bus의 볼륨을 바꾼다...*/
        FModAudioManager.SetBusVolume(busType, _slider.value);
    }
}
