using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************************
 *  �����̴� UI�� ���� ���ϴ� Bus�� ������ �����ϴ� ������Ʈ...
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
        /**�ʱ�ȭ �� bus�ʱⰪ ����...*/
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
        /**�����̴��� ���� �ٲ�� FMod Bus�� ������ �ٲ۴�...*/
        FModAudioManager.SetBusVolume(busType, _slider.value);
    }
}
