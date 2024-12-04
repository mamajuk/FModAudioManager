using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        None            = 0,
        Start           = 1,
        CollisionEnter  = 2, 
        CollisionExit   = 4,
        TriggerEnter    = 8, 
        TriggerExit     = 16,
        Destroy         = 32,
        ALL             = (Start|CollisionEnter|CollisionExit|TriggerEnter|TriggerExit)
    }

    [System.Serializable]
    public struct InternalEventDesc
    {
        public FMODUnity.EventReference EventRef;
        public FModParameterReference   ParamRef;
        public FModEventApplyTiming     PlayApplyTiming;
        public FModEventApplyTiming     StopApplyTiming;
        public Vector3                  EventPos;
        public bool                     IsOneShot;
        public bool                     OverrideDistance;
        public float                    EventMinDistance;
        public float                    EventMaxDistance;
        public float                    Volume;
        public float                    StartTimelinePositionRatio;

        [System.NonSerialized]
        public FModEventInstance        Instance;
    }
    #endregion

    #region Editor_Extension
#if UNITY_EDITOR
    /***********************************************
     *    에디터 확장을 위한 private class...
     * *******/
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(InternalEventDesc))]
    private class InternalEventDescDrawer : PropertyDrawer
    {
        //=================================================
        ///////             Fields..                ///////
        ///================================================
        private Rect _initRect;

        private SerializedProperty _EventRefProperty;
        private SerializedProperty _ParamRefProperty;

        private SerializedProperty _PlayApplyTimingProperty;
        private SerializedProperty _StopApplyTimingProperty;

        private SerializedProperty _IsOneShotProperty;
        private SerializedProperty _VolumeProperty;
        private SerializedProperty _StartTimelinePositionProperty;

        private SerializedProperty _PositionProperty;
        private SerializedProperty _EventMinDistanceProperty;
        private SerializedProperty _EventMaxDistanceProperty;
        private SerializedProperty _OverrideDistanceProperty;



        //====================================================
        ///////           Override methods..           ///////
        ///===================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /********************************************************
             *    해당 객체가 펼쳐진 상태에서만 내용을 표시한다...
             * ******/
            if (GUI_Initialized(property) == false){
                return;
            }

            _initRect       = position;
            position.height = GetBaseHeight();

            EventReference selectedEventRef = _EventRefProperty.GetEventReference();
            string[]       eventNames       = selectedEventRef.Path.Split('/');
            string         eventName        = eventNames[eventNames.Length - 1];

            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, $"Event Description ({property.displayName.Replace("Element ", "")}) - {eventName}"))
            {
               GUI_ShowEventDesc(ref position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            #region Omit
            if (GUI_Initialized(property) == false) return 0f;

            float eventRefHeight = EditorGUI.GetPropertyHeight(_EventRefProperty) + 10f;
            float paramRefHeight = EditorGUI.GetPropertyHeight(_ParamRefProperty) + 10f;
            float timingHeight   = GetBaseHeight() * (_PlayApplyTimingProperty.isExpanded? 10f:1f) + 10f;
            float dddHeight      = GetBaseHeight() * (_PositionProperty.isExpanded? 10f:1f) + 10f;
            float total          = (eventRefHeight + paramRefHeight + timingHeight + dddHeight);

            return GetBaseHeight() + (property.isExpanded ? total : 0f);
            #endregion
        }



        //===============================================
        ///////          GUI methods...           ///////
        //===============================================
        private bool GUI_Initialized(SerializedProperty parentProperty)
        {
            #region Omit
            if (parentProperty == null) return false;


            /************************************************
             *    필요한 값들을 모두 초기화한다....
             * *******/
            _EventRefProperty = parentProperty.FindPropertyRelative("EventRef");
            _ParamRefProperty = parentProperty.FindPropertyRelative("ParamRef");
            _PlayApplyTimingProperty = parentProperty.FindPropertyRelative("PlayApplyTiming");

            _StopApplyTimingProperty = parentProperty.FindPropertyRelative("StopApplyTiming");
            _IsOneShotProperty = parentProperty.FindPropertyRelative("IsOneShot");
            _VolumeProperty = parentProperty.FindPropertyRelative("Volume");
            _StartTimelinePositionProperty = parentProperty.FindPropertyRelative("StartTimelinePositionRatio");

            _PositionProperty = parentProperty.FindPropertyRelative("EventPos");
            _EventMaxDistanceProperty = parentProperty.FindPropertyRelative("EventMaxDistance");
            _EventMinDistanceProperty = parentProperty.FindPropertyRelative("EventMinDistance");
            _OverrideDistanceProperty = parentProperty.FindPropertyRelative("OverrideDistance");


            return (_EventRefProperty!=null && _ParamRefProperty!=null) &&
                   (_PlayApplyTimingProperty!=null && _StopApplyTimingProperty!=null) &&
                   (_IsOneShotProperty!=null && _VolumeProperty!=null && _StartTimelinePositionProperty!=null) &&
                   (_PositionProperty!=null && _EventMinDistanceProperty!=null && _EventMaxDistanceProperty!=null && _OverrideDistanceProperty!=null);
            #endregion
        }

        private void GUI_ShowEventDesc(ref Rect position, SerializedProperty property, GUIContent label)
        {
            #region Omit
            /***************************************************
             *    프로퍼티 필드의 크기 및 위치를 초기화한다....
             * *****/
            position.x += 25f;
            position.y += GetBaseHeight();
            position.width -= 25f;


            /**Event Ref...**/
            EditorGUI.PropertyField(position, _EventRefProperty);
            position.y += EditorGUI.GetPropertyHeight(_EventRefProperty);
            GUI_DrawLine(ref position);


            /**Param Ref...**/
            EditorGUI.PropertyField(position, _ParamRefProperty);
            position.y += EditorGUI.GetPropertyHeight(_ParamRefProperty);
            GUI_DrawLine(ref position);


            /**Timing...**/
            if(_PlayApplyTimingProperty.isExpanded =  EditorGUI.Foldout(position, _PlayApplyTimingProperty.isExpanded, "Play/Stop Settings"))
            {
                position.x     += 25f;
                position.y     += 25f;
                position.width -= 25f;

                //Volume...
                _VolumeProperty.floatValue = EditorGUI.Slider(position,"Volume", _VolumeProperty.floatValue, 0f, 10f);
                position.y += 25f;

                //IsOneShot...
                _IsOneShotProperty.boolValue = EditorGUI.Toggle(position, "DestroyAtStop", _IsOneShotProperty.boolValue);
                position.y += 25f;

                //TimelinePositionRatio...
                _StartTimelinePositionProperty.floatValue = EditorGUI.Slider(position, "Start TimelinePosition Ratio", _StartTimelinePositionProperty.floatValue, 0f, 1f);
                position.y += 25f;

                //Play Timing...
                _PlayApplyTimingProperty.intValue = (int)(FModEventApplyTiming)EditorGUI.EnumFlagsField(position, "Play Timing", (FModEventApplyTiming)_PlayApplyTimingProperty.intValue);
                position.y += 25f;

                //Stop Timing...
                _StopApplyTimingProperty.intValue = (int)(FModEventApplyTiming)EditorGUI.EnumFlagsField(position, "Stop Timing", (FModEventApplyTiming)_StopApplyTimingProperty.intValue);
                position.x     -= 25f;
                position.width += 25f;
            }

            position.y += 25f;
            GUI_DrawLine(ref position);


            /**3D....**/
            if (_PositionProperty.isExpanded = EditorGUI.Foldout(position, _PositionProperty.isExpanded, "3D"))
            {
                position.x     += 25f;
                position.y     += 25f;
                position.width -= 25f;

                //Position...
                _PositionProperty.vector3Value = EditorGUI.Vector3Field(position, "position3D", _PositionProperty.vector3Value);
                position.y += 25f;

                //Override Distance
                _OverrideDistanceProperty.boolValue = EditorGUI.Toggle(position,"Override Default Distance",  _OverrideDistanceProperty.boolValue);
                position.y += 25f;

                //Min/Max Distance
                using (var scope = new EditorGUI.DisabledGroupScope(!_OverrideDistanceProperty.boolValue))
                {
                    position.x     += 25f;
                    position.width -= 25f;

                    _EventMinDistanceProperty.floatValue = EditorGUI.FloatField(position, "Min", _EventMinDistanceProperty.floatValue);
                    position.y += 25f;

                    _EventMaxDistanceProperty.floatValue = EditorGUI.FloatField(position, "Max", _EventMaxDistanceProperty.floatValue);
                }

                position.width += 50f;
                position.x     -= 50f;
            }
            #endregion
        }

        private void GUI_DrawLine(ref Rect position, float space = 5f, float subOffset = 0f)
        {
            #region Omit
            position.y += space;

            Handles.color = Color.gray;
            Handles.DrawLine(
                new Vector2(position.x - subOffset, position.y), 
                new Vector2(_initRect.width + 30f - subOffset, position.y)
            );

            position.y += space;
            #endregion
        }


        //=====================================================
        ///////           Utility methods..             ///////
        /////==================================================
        private float GetBaseHeight()
        {
            return GUI.skin.label.CalcSize(GUIContent.none).y;
        }
    }


    /***************************************************************
     *     FModInsepectorHelper를 확장하기 위한 private class...
     * ******/
    [CustomEditor(typeof(FModInspectorHelper))]
    private class FModInspectorHelperEditor : Editor
    {
        private SerializedProperty _EventDescs;
        private int                _prevArraySize = 0;

        public override void OnInspectorGUI()
        {
            /********************************************
             *    초기화를 진행한다....
             * *****/
            if((_EventDescs=serializedObject.FindProperty("EventDescs"))!=null){
                _prevArraySize = _EventDescs.arraySize;
            }


            if (_EventDescs == null) return;

            /*********************************************
             *    변화를 감지한다....
             * *****/
            base.OnInspectorGUI();

            //새로운 Event Desc가 추가되었다면 초기값을 넣는다...
            if (_EventDescs.arraySize > _prevArraySize)
            {
                SerializedProperty lastElement = _EventDescs.GetArrayElementAtIndex(_EventDescs.arraySize - 1);
                SerializedProperty volume      = lastElement.FindPropertyRelative("Volume");
                SerializedProperty position    = lastElement.FindPropertyRelative("EventPos");
                SerializedProperty min         = lastElement.FindPropertyRelative("EventMinDistance");
                SerializedProperty max         = lastElement.FindPropertyRelative("EventMaxDistance");
                SerializedProperty eventRef    = lastElement.FindPropertyRelative("EventRef");

                volume.floatValue     = 1f;
                min.floatValue        = 1f;
                max.floatValue        = 20f;
                position.vector3Value = ((FModInspectorHelper)target).transform.position;
                eventRef.SetEventReference(new FMOD.GUID(), "");

                _prevArraySize = _EventDescs.arraySize;
                serializedObject.ApplyModifiedProperties(); 
            }

        }
    }


#endif
    #endregion

    //=================================================
    /////          Property and Fields             ////
    //=================================================
    [SerializeField] InternalEventDesc[] EventDescs = new InternalEventDesc[0];



    //===========================================================
    /////               Magic and Core methods             //////
    //===========================================================
    private void Start()
    {
        PlayStop_Internal(FModEventApplyTiming.Start);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayStop_Internal(FModEventApplyTiming.TriggerEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayStop_Internal(FModEventApplyTiming.TriggerExit);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayStop_Internal(FModEventApplyTiming.CollisionEnter);
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayStop_Internal(FModEventApplyTiming.CollisionExit);
    }

    private void OnDestroy()
    {
        PlayStop_Internal(FModEventApplyTiming.Destroy);
    }

    private void PlayStop_Internal(FModEventApplyTiming timing, int filter = -1)
    {
        #region Omit
        int descCount = EventDescs.Length;

        for (int i=0; i<descCount; i++)
        {
            //특정 인스턴스만 재생할 경우, 필터와 일치하지 않는 인덱스를 스킵한다...
            if(filter>=0 && i!=filter){
                return;
            }

            ref InternalEventDesc desc = ref EventDescs[i];

            bool ContainPlayTiming = ((int)desc.PlayApplyTiming & (int)timing) > 0;
            bool ContainStopTiming = ((int)desc.StopApplyTiming & (int)timing)>0;


            /**종료 타이밍과 일치하는가?**/
            if(ContainStopTiming && desc.Instance.IsValid){
                desc.Instance.Stop();
            }

            /**시작 타이밍과 일치하는가?*/
            if(ContainPlayTiming){

                Play_Progress_Internal(ref desc);
            }

            //파괴되는 타이밍이라면 유효한 인스턴스를 파괴한다...
            if(timing==FModEventApplyTiming.Destroy && desc.Instance.IsValid)
            {
                desc.Instance.Destroy();
            }
        }

        #endregion
    }

    private void Play_Progress_Internal(ref InternalEventDesc desc)
    {
        #region Omit
        if (desc.Instance.IsValid) desc.Instance.Destroy();
        desc.Instance                       = FModAudioManager.CreateInstance(desc.EventRef);
        desc.Instance.Position3D            = desc.EventPos;
        desc.Instance.Volume                = desc.Volume;
        desc.Instance.TimelinePositionRatio = desc.StartTimelinePositionRatio;

        //Min-Max Distance를 덮어씌우는가?
        if (desc.OverrideDistance){
            desc.Instance.Set3DDistance(desc.EventMinDistance, desc.EventMaxDistance);
        }

        //파라미터를 세팅한다....
        if(desc.ParamRef.IsValid){
            desc.Instance.SetParameter(desc.ParamRef);
        }

        desc.Instance.Play();

        //사운드가 정지되면 자동으로 파괴되는가?
        if (desc.IsOneShot){
            desc.Instance.Destroy(true);
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

        ref InternalEventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayOneShotSFX( desc.EventRef, desc.EventPos, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventMinDistance, desc.EventMaxDistance );
        #endregion
    }

    public void PlayBGM( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        ref InternalEventDesc desc = ref EventDescs[index];
        FModAudioManager.PlayBGM(desc.EventRef, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventPos);
        #endregion
    }

    public void PlayInstance( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;
        Play_Progress_Internal(ref EventDescs[index]);
        #endregion
    }

    public void StopInstance( int index )
    {
        #region Omit
        if (EventDescs == null || EventDescs.Length < index) return;

        ref InternalEventDesc desc = ref EventDescs[index];
        if(desc.Instance.IsValid){

            desc.Instance.Stop();
        }
        #endregion
    }
}
#endif
