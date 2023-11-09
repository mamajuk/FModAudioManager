using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************
 *   인스펙터에서 FMod Event를 재생할 수 있는 기능을 제공합니다...
 * ***/
#if FMOD_Event_ENUM
[AddComponentMenu("FMOD Studio/FModInspectorHelper")]
public sealed class FModInspectorHelper : MonoBehaviour
{
    #region Define
    public enum FModEventApplyTiming
    {
        None = 0,
        Start,
        CollisionEnter, 
        CollisionExit,
        TriggerEnter, 
        TriggerExit,
        Destroy,
        ALL
    }

    [System.Serializable]
    public struct EventDesc
    {
        public FMODUnity.EventReference EventRef;
        public FModParameterReference   ParamRef;
        public FModEventApplyTiming     PlayApplyTiming;
        public FModEventApplyTiming     StopApplyTiming;
        public Vector3                  EventPos;
        public bool                     IsOneShot;
        public float                    EventMinDistance;
        public float                    EventMaxDistance;
        public float                    Volume;
        public int                      StartPosition;

        [System.NonSerialized]
        public FModEventInstance        Instance;
    }
    #endregion

    //=================================================
    /////          Property and Fields             ////
    //=================================================
    [SerializeField] bool        UseDefaultVolume = true;
    [SerializeField] EventDesc[] EventDescs = new EventDesc[0];



    //===========================================================
    /////               Magic and Core methods             //////
    //===========================================================
    private void Start()
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++)
        {

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.Start);
        }
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++){

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.TriggerEnter);
        }
        #endregion
    }

    private void OnTriggerExit(Collider other)
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++){

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.TriggerExit);
        }
        #endregion
    }

    private void OnCollisionEnter(Collision collision)
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++){

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.CollisionEnter);
        }
        #endregion
    }

    private void OnCollisionExit(Collision collision)
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++){

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.CollisionExit);
        }
        #endregion
    }

    private void OnDestroy()
    {
        #region Omit
        int Count = EventDescs.Length;
        for (int i = 0; i < Count; i++){

            PlayStop_Internal(ref EventDescs[i], FModEventApplyTiming.Destroy);
        }
        #endregion
    }

    private void PlayStop_Internal( ref EventDesc desc, FModEventApplyTiming timing, bool ignorePlay=false, bool ignoreStop=false )
    {
        #region Omit
        bool timingIsAll = (timing == FModEventApplyTiming.ALL);

        /**종료 타이밍과 일치할 경우...*/
        if( (desc.StopApplyTiming==timing || desc.StopApplyTiming==FModEventApplyTiming.ALL || timingIsAll) && desc.Instance.IsValid && !ignoreStop){

            desc.Instance.Destroy();
            return;
        }

        /**시작 타이밍과 일치할 경우...*/
        if ((desc.PlayApplyTiming == timing || desc.StopApplyTiming == FModEventApplyTiming.ALL || timingIsAll) && !ignorePlay){

            /**FModEventInstance를 생성한다....*/
            if (desc.Instance.IsValid) desc.Instance.Destroy(false);

            desc.Instance = FModAudioManager.CreateInstance(desc.EventRef);
            desc.Instance.Volume           = (!UseDefaultVolume? desc.Instance.Volume:desc.Volume);
            desc.Instance.Position         = desc.EventPos;
            desc.Instance.TimelinePosition = desc.StartPosition;
            desc.Instance.SetParameter(desc.ParamRef);
            desc.Instance.Set3DDistance(desc.EventMinDistance, desc.EventMaxDistance);
            desc.Instance.Play();

            /**원샷 이벤트를 생성한다....*/
            if (desc.IsOneShot){

                desc.Instance.Destroy(true);
            }
        }

        #endregion
    }



    //=======================================
    /////        Public methods         /////
    //=======================================
    public void PlayOneShotSFX(int index)
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        ref EventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayOneShotSFX( desc.EventRef, desc.EventPos, desc.ParamRef, desc.Volume, desc.StartPosition, desc.EventMinDistance, desc.EventMaxDistance );
        #endregion
    }

    public void PlayBGM( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        ref EventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayBGM(desc.EventRef, desc.ParamRef, desc.Volume, desc.StartPosition);
        #endregion
    }

    public void PlayInstance( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        ref EventDesc desc = ref EventDescs[index];

        PlayStop_Internal(ref EventDescs[index], FModEventApplyTiming.ALL, false, true);

        #endregion
    }

    public void StopInstance( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        PlayStop_Internal(ref EventDescs[index], FModEventApplyTiming.ALL, true, false);
        #endregion
    }
}
#endif
