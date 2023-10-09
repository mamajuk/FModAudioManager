using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************
 *   �ν����Ϳ��� FMod Event�� ����� �� �ִ� ����� �����մϴ�...
 * ***/
[AddComponentMenu("FMOD Studio/FModInspectorHelper")]
public sealed class FModInspectorHelper : MonoBehaviour
{
    [System.Serializable]
    public struct EventDesc
    {
        public FMODUnity.EventReference EventRef;
        public FModParameterReference   ParamRef;
        public Vector3                  EventPos;
    }


    //===============================================
    /////        Property and Fields             ////
    //===============================================
    [SerializeField] EventDesc[] EventDescs = new EventDesc[0];



    //=======================================
    /////        Public methods         /////
    //=======================================
    public void PlayOneShotSFX(int index)
    {
        if (EventDescs == null || EventDescs.Length < index) return;

        ref EventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayOneShotSFX( desc.EventRef, desc.EventPos, desc.ParamRef );
    }

    public void PlayBGM( int index )
    {
        if (EventDescs == null || EventDescs.Length < index) return;

        ref EventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayBGM(desc.EventRef, desc.ParamRef);
    }
}
