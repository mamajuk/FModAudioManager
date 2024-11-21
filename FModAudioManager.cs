using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Xml;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using FMOD;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

#region Define

[System.Serializable]
public struct FModParameterReference
{
    #region Editor_Extension
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(FModParameterReference))]
    private sealed class FModParameterInstanceDrawer : PropertyDrawer
    {
        //=====================================
        /////           Fields            /////
        //=====================================
        private SerializedProperty ParamTypeProperty;
        private SerializedProperty ParamValueProperty;
        private SerializedProperty isGlobalProperty;
        private SerializedProperty isInitProperty;


        /**������ ������ ����...*/
        private const string                   _EditorSettingsPath = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
        private static FModAudioEditorSettings _EditorSettings;

        /**��Ÿ�� ����...*/
        private static GUIStyle _labelStyle;
        private static GUIStyle _labelStyleLight;

        /**�Ķ���� �� ����...*/
        private float    _min, _max;
        private int      _select = -1;
        private string[] _labels;
        private bool     _startApply = false;


        //===============================================
        //////          Override methods            /////
        //===============================================

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /**�ʱ�ȭ�� �����ߴٸ�, ���� ��Ĵ�� ����Ѵ�.*/
            if (GUI_Initialized(property)==false) return;

            /**������ ���¿����� ���� ������� ������ ǥ���Ѵ�....*/
            position.y -= (property.isExpanded ? 25f : 0f);
            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
            {
                position.height = GetBaseHeight(property);

                position.y += 50f;

                /**��� ������Ƽ���� ǥ���Ѵ�...*/
                GUI_ShowParamType(ref position);

                GUI_ShowParamValue(ref position);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetBaseHeight(property) + (property.isExpanded? 50f:0f);
        }



        //======================================
        /////         GUI methods          /////
        //======================================
        private bool GUI_Initialized(SerializedProperty property)
        {
            #region Omit
            /******************************************
             *   ��� ������Ƽ���� �ʱ�ȭ�Ѵ�...
             * ***/
            ParamTypeProperty   = property.FindPropertyRelative("_paramType");
            ParamValueProperty  = property.FindPropertyRelative("_paramValue");
            isGlobalProperty    = property.FindPropertyRelative("_isGlobal");
            isInitProperty      = property.FindPropertyRelative("_init");

            /**������ ���� �ʱ�ȭ...*/
            if (_EditorSettings == null){

                _EditorSettings = AssetDatabase.LoadAssetAtPath<FModAudioEditorSettings>(_EditorSettingsPath);
            }

            /**��Ÿ�� �ʱ�ȭ...*/
            if(_labelStyle==null){

                _labelStyle = new GUIStyle(EditorStyles.foldout);
                _labelStyle.normal.textColor = Color.white;
                _labelStyle.fontStyle = FontStyle.Bold;
                _labelStyle.fontSize = 12;
            }

            if (_labelStyleLight == null){

                _labelStyleLight = new GUIStyle(EditorStyles.boldLabel);
                _labelStyleLight.normal.textColor = Color.black;
                _labelStyleLight.fontSize = 12;
            }

            bool result = (_EditorSettings != null && ParamTypeProperty != null && ParamValueProperty != null && isGlobalProperty != null);

            /**���� ����� �Ķ������ ���� �����Ѵ�...*/
            if (result && isInitProperty.boolValue==false)
            {
                ParamTypeProperty.intValue = -1;
                isInitProperty.boolValue = true;
                _startApply = true;
            }

            if (_startApply == false)
            {
                GUI_SetParameter();
                _startApply = true;
            }

            return result;
            #endregion
        }

        private void GUI_ShowPropertyRect( ref Rect header, SerializedProperty property, float space = 3f)
        {
            #region Omit
            bool     isBlack = EditorGUIUtility.isProSkin;
            GUIStyle style   = (isBlack ? _labelStyle : _labelStyleLight);
            Color    color   = (isBlack ? new Color(.3f, .3f, .3f):new Color(0.7254f, 0.7254f, 0.7254f));

 
            /************************************
             *   ������Ƽ �̸��� ����Ѵ�...
             * ***/
            EditorGUI.LabelField(header, property.displayName, style);

            header.x     += 10f;
            header.y     += 20f;
            header.width -= 40f;

            #endregion
        }

        private void GUI_ShowParamType( ref Rect header, float space = 3f )
        {
            #region Omit
#if FMOD_Event_ENUM
            Rect rect = header;
            //rect.x += 20f;

            /**���� �簢���� �׸���...*/
            

            /**************************************
             *   �Ķ���� Ÿ���� ǥ���Ѵ�...
             * ***/
            using (var scope = new EditorGUI.ChangeCheckScope()){

                bool        isGlobal = isGlobalProperty.boolValue;
                int         value    = ParamTypeProperty.intValue;
                System.Enum result;

                rect.width *= .8f;

                /**�۷ι� �Ķ������ ���...*/
                if(isGlobal){

                    FModGlobalParamType global = (FModGlobalParamType)value;
                    result = EditorGUI.EnumPopup(rect, "��-ParamType", global);
                }

                /**���� �Ķ������ ���...*/
                else{

                    FModLocalParamType local = (FModLocalParamType)value;
                    result = EditorGUI.EnumPopup(rect, "��-ParamType", local);
                }

                /**���� ����Ǿ��� ��� �����Ѵ�...*/
                if(scope.changed)
                {
                    ParamTypeProperty.intValue = Convert.ToInt32(result);
                    GUI_SetParameter();
                }
            }

            /*****************************************
             *   �۷ι������� ���� ���θ� ǥ���Ѵ�...
             * ***/
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                rect.x += rect.width;
                bool value = EditorGUI.ToggleLeft(rect, "Is Global", isGlobalProperty.boolValue);
            
                /**���� ����Ǿ��ٸ� �����Ѵ�...*/
                if(scope.changed){

                    isGlobalProperty.boolValue = value;
                    ParamTypeProperty.intValue = -1;
                    ParamValueProperty.floatValue = 0f;
                }
            }


            header.y += (20f + space);
#endif
            #endregion
        }

        private void GUI_ShowParamValue( ref Rect header, float space = 3f )
        {
            #region Omit
            Rect rect = header;
            //rect.x += 20f;

            using (var scope = new EditorGUI.ChangeCheckScope()){

                /*******************************************
                 *   �Ķ���Ͱ� ���õ��� �ʾ��� ���...
                 * ***/
                if(ParamTypeProperty.intValue<0)
                {
                    EditorGUI.TextField(rect, "��-Param Value", "(No Value)");
                    return;
                }

                /******************************************
                 *   ���̺��� �������� �ʴ� �Ķ������ ���...
                 * ***/
                if (_labels==null)
                {
                    float value = EditorGUI.Slider(rect, "��-Param Value", ParamValueProperty.floatValue, _min, _max);

                    /**���� ����Ǿ��ٸ� ����.*/
                    if(scope.changed){

                        ParamValueProperty.floatValue = value;
                    }

                    return;
                }


                /******************************************
                 *   ���̺��� �����ϴ� �Ķ������ ���...
                 * ***/
                int selected = EditorGUI.Popup(rect, "��-Param Value", _select, _labels);

                /**���� ����Ǿ��ٸ� ����...*/
                if(_select != selected){

                    _select = selected;
                    ParamValueProperty.floatValue = (float)selected;
                }
            }


            #endregion
        }

        private void GUI_SetParameter()
        {
            #region Omit
            int index = ParamTypeProperty.intValue;
            if (index < 0) return;

            FModParamDesc desc  = _EditorSettings.ParamDescList[index];
            _min = desc.Min;
            _max = desc.Max;
            _select = 0;

            /**���� �������� �������, Ż���Ѵ�...*/
            if (desc.LableCount==0){

                _labels = null;
                return;
            }

            
            /*************************************************
             *   �ش� �Ķ������ ���̺��� ������̺��� �����Ѵ�...
             * ***/

            /**���̺��� ���� �ε����� ���Ѵ�...*/
            int labelBegin = 0;  
            for(int i=0; i<index; i++){

                labelBegin += _EditorSettings.ParamDescList[i].LableCount;
            }

            /**�ش� ���̺��� �迭�� �����Ѵ�...*/
            if (_labels==null || _labels.Length != desc.LableCount){

                _labels = new string[desc.LableCount];
            }

            for(int i=0; i<desc.LableCount; i++)
            {
                _labels[i] = _EditorSettings.ParamLableList[labelBegin + i];
            }
            _select = (int)ParamValueProperty.floatValue;
            #endregion
        }



        //===============================================
        /////            Utility Methods             ////
        ///==============================================
        private float GetBaseHeight(SerializedProperty property)
        {
            return GUI.skin.textField.CalcSize(GUIContent.none).y;
        }

    }
#endif
    #endregion

    //===============================================
    /////          Property and Fields           ////
    //===============================================
    public int   ParamType  { get { return _paramType; } }
    public float ParamValue { get { return _paramValue; } }
    public bool  IsValid    { get { return (_paramType >= 0); } }
    public bool  IsGlobal   { get { return _isGlobal; } }

    [SerializeField] private int   _paramType;
    [SerializeField] private float _paramValue;
    [SerializeField] private bool  _isGlobal;

#if UNITY_EDITOR
    [SerializeField] private bool _init;
#endif



    //======================================
    /////        Public methods        /////
    //======================================
#if FMOD_Event_ENUM
    public FModParameterReference(FModLocalParamType paramType, float value = 0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = false;
#if UNITY_EDITOR
        _init = true;
#endif
        #endregion
    }

    public FModParameterReference(FModGlobalParamType paramType, float value = 0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal = false;
#if UNITY_EDITOR
        _init = true;
#endif
        #endregion
    }

    public void SetParameter(FModLocalParamType paramType, float value=0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = false;
        #endregion
    }

    public void SetParameter(FModGlobalParamType paramType, float value=0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = true;
        #endregion
    }
#endif

    public void ClearParameter()
    {
        #region Omit
        _paramType = -1;
        _paramValue = 0f;
        _isGlobal   = false;
        #endregion
    }
}

public struct FModEventInstance
{
    //==================================
    ////     Property And Fields   ///// 
    //==================================
    public FMOD.GUID     GUID
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            FMOD.GUID guid;
            desc.getID(out guid);

            return guid;
        }

    }
    public bool          IsPaused 
    { 
        get {

            bool ret;
            Ins.getPaused(out ret);
            return ret;
        } 
    }
    public bool          IsLoop
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            bool isOneShot;
            desc.isOneshot(out isOneShot);
            return isOneShot;
        }
    }
    public bool          Is3DEvent
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            bool is3D;
            desc.is3D(out is3D);
            return is3D;
        }

    }
    public Vector3       Position
    {
        get
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            FMOD.VECTOR pos = attributes.position;
            return new Vector3( pos.x, pos.y, pos.z );
        }

        set
        {
            Ins.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(value));
        }
    }
    public bool          IsValid{ get{ return Ins.isValid(); } }
    public float         Volume
    {
        get
        {

            float volume;
            Ins.getVolume(out volume);
            return volume;
        }

        set { Ins.setVolume((value < 0 ? 0f : value)); }
    }
    public float         Pitch
    {
        get
        {
            float pitch;
            Ins.getPitch(out pitch);
            return pitch;
        }

        set { Ins.setPitch(value); }
    }
    public bool          IsPlaying
    {
        get
        {
            FMOD.Studio.PLAYBACK_STATE state;
            Ins.getPlaybackState(out state);
            return (state == FMOD.Studio.PLAYBACK_STATE.PLAYING);
        }

    }
    public int           TimelinePosition
    {
        get
        {
            int position;
            Ins.getTimelinePosition(out position);
            return position;
        }

        set{  Ins.setTimelinePosition(value); }
    }
    public float         TimelinePositionRatio
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);

            int position;
            Ins.getTimelinePosition(out position);

            return ((float)position/(float)length);
        }

        set
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);

            float ratio = Mathf.Clamp(value, 0.0f, 1.0f);
            Ins.setTimelinePosition( Mathf.RoundToInt(length*ratio) );
        }
    }
    public int           Length
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);
            return length;
        }
    }
    public float         Min3DDistance
    {
        get
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                float distance;
                Ins.getProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, out distance);
                return distance;
            }

            return 0f;
        }

        set
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, value);
            }
        }
    }
    public float         Max3DDistance
    {
        get
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                float distance;
                Ins.getProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, out distance);
                return distance;
            }

            return 0f;
        }

        set
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, value);
            }
        }
    }

    private EventInstance Ins;

    //==================================
    ////        Public Methods     ///// 
    //==================================
    public FModEventInstance(EventInstance instance, Vector3 position=default) 
    { 
        Ins = instance;

        bool is3D;
        FMOD.Studio.EventDescription desc;
        Ins.getDescription(out desc);
        desc.is3D(out is3D);
        if (is3D)
        {
            Ins.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        }
    }

    public static explicit operator EventInstance(FModEventInstance ins) { 
        return ins.Ins; 
    }

    public void Play(float volume = -1f, float startTimelinePositionRatio = -1f, string paramName = "", float paramValue = 0f)
    {
        if(volume>=0)     Ins.setVolume(volume);
        if(paramName!="") Ins.setParameterByName(paramName, paramValue);
        if(startTimelinePositionRatio>=0f) TimelinePositionRatio = startTimelinePositionRatio;
        Ins.start();
    }

    public void Pause()
    {
        Ins.setPaused(true);
    }

    public void Resume()
    {
        Ins.setPaused(false);
    }

    public void Stop()
    {
        Ins.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void Destroy(bool destroyAtStop=false)
    {
        if (destroyAtStop)
        {
            Ins.release();
            return;
        }

        Ins.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Ins.release();
        Ins.clearHandle();
    }

#if FMOD_Event_ENUM
    public void SetParameter(FModGlobalParamType paramType, float paramValue)
    {
        string paramName = FModReferenceList.Params[(int)paramType];

        FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
        system.setParameterByName(paramName, paramValue);
    }

    public void SetParameter(FModLocalParamType paramType, float paramValue)
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        Ins.setParameterByName(paramName, paramValue);
    }
#endif

    public void SetParameter(FModParameterReference paramRef)
    {
#if FMOD_Event_ENUM
        if (paramRef.IsValid == false) return;
        string paramName = FModReferenceList.Params[paramRef.ParamType];

        /**�۷ι� �Ķ������ ���...*/
        if(paramRef.IsGlobal)
        {
            FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
            system.setParameterByName(paramName, paramRef.ParamValue);
            return;
        }

        /**���� �Ķ������ ���...*/
        Ins.setParameterByName( paramName, paramRef.ParamValue);
#endif
    }

    public void SetParameter(string paramName, float value)
    {
        Ins.setParameterByName(paramName, value);
    }

#if FMOD_Event_ENUM
    public float GetParameter(FModGlobalParamType paramType)
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        float value;
        FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
        system.getParameterByName(paramName, out value);

        return value;
    }

    public float GetParameter(FModLocalParamType paramType) 
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        float value;
        Ins.getParameterByName(paramName, out value);

        return value;
    }
#endif

    public float GetParameter(string paramName) 
    {
        float value;
        Ins.getParameterByName(paramName, out value);

        return value;
    }

    public void Set3DDistance(float minDistance, float maxDistance )
    {
        bool is3D;
        FMOD.Studio.EventDescription desc;
        Ins.getDescription(out desc);
        desc.is3D(out is3D);
        if (is3D)
        {
            Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, minDistance);
            Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct TIMELINE_MARKER_PROPERTIESEX
{
    public string MarkerName;
    public int    TimelinePosition;
    public float  TimelinePositionRatio;
}

#endregion

public interface IFModEventFadeComplete { void OnFModEventComplete(int fadeID, float goalVolume); }
public interface IFModEventCallBack     { void OnFModEventCallBack(FMOD.Studio.EVENT_CALLBACK_TYPE eventType, FModEventInstance eventTarget, int paramKey); }
public delegate void FModEventFadeCompleteNotify( int fadeID, float goalVolume );
public delegate void FModEventCallBack( FMOD.Studio.EVENT_CALLBACK_TYPE eventType, FModEventInstance eventTarget, int paramKey );

[AddComponentMenu("FMOD Studio/FModAudioManager")]
public sealed class FModAudioManager : MonoBehaviour
{
    #region Editor_Extension
    /********************************
     * ������ Ȯ���� ���� private class
     ***/
#if UNITY_EDITOR
    private sealed class FModAudioManagerWindow : EditorWindow
    {
        private struct ParamFolderInfo
        {
            public string name;
            public string id;
            public string output;
        }

        private enum BankLoadType
        {
            Load_At_Initialized,
            Not_Load
        }

        //=====================================
        ////            Fields           ///// 
        //====================================

        /*************************************
         *   Editor Data Path String...
         * **/
        private const string _DataScriptPath           = "Assets/Plugins/FMOD/src/FMODAudioManagerDefine.cs";
        private const string _EditorSettingsPath       = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
        private const string _StudioSettingsPath       = "Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset";
        private const string _GroupFolderPath          = "Metadata/Group";
        private const string _PresetFolderPath         = "Metadata/ParameterPreset";
        private const string _PresetFolderFolderPath   = "Metadata/ParameterPresetFolder";
        private const string _ScriptDefine             = "FMOD_Event_ENUM";
        private const string _EditorVersion            = "v1.241121";

        private const string _EventRootPath            = "event:/";
        private const string _BusRootPath              = "bus:/";
        private const string _BankRootPath             = "bank:/";
        private const string _ParamRootPath            = "param:/";

        /*************************************
         *  Texture Path and reference
         ***/
        private static Texture  _BannerWhiteTex;
        private const string    _BannerWhitePath = "Assets/Plugins/FMOD/images/FMODLogoWhite.png";

        private static Texture  _BannerBlackTex;
        private const string    _BannerBlackPath = "Assets/Plugins/FMOD/images/FMODLogoBlack.png";

        private static Texture  _StudioIconTex;
        private const string    _StudioIconPath = "Assets/Plugins/FMOD/images/StudioIcon.png";

        private static Texture  _SearchIconTex;
        private const string    _SearchIconPath = "Assets/Plugins/FMOD/images/SearchIconBlack.png";

        private static Texture _BankIconTex;
        private const string   _BankIconPath    = "Assets/Plugins/FMOD/images/BankIcon.png";

        private static Texture  _AddIconTex;
        private const string    _AddIconPath = "Assets/Plugins/FMOD/images/AddIcon.png";

        private static Texture  _XYellowIconTex;
        private const string    _XYellowIconPath = "Assets/Plugins/FMOD/images/CrossYellow.png";

        private static Texture _DeleteIconTex;
        private const string   _DeleteIconPath = "Assets/Plugins/FMOD/images/Delete.png";

        private static Texture _NotFoundIconTex;
        private const string   _NotFoundIconPath = "Assets/Plugins/FMOD/images/NotFound.png";

        private static Texture _ParamLabelIconTex;
        private const string   _ParamLabelIconPath = "Assets/Plugins/FMOD/images/LabeledParameterIcon.png";


        /****************************************
         *   Editor ScriptableObject Fields...
         ***/
        private static FModAudioEditorSettings _EditorSettings;
        private static FMODUnity.Settings      _StudioSettings;


        /******************************************
         *   Editor GUI Fields...
         * **/
        private Regex           _regex     = new Regex(@"[^a-zA-Z0-9_]");
        private StringBuilder   _builder   = new StringBuilder();
        private bool            _refresh   = false;
        private Vector2         _Scrollpos = Vector2.zero;

        /** Categorys... *************************/
        private static readonly string[] _EventGroups   = new string[] { "BGM", "SFX", "NoGroup" };
        private static readonly string[] _ParamGroups   = new string[] { "Global Parameters", "Local Parameters" };
        private int _EventGroupSelected = 0;
        private int _ParamGroupSelected = 0;

        /** Styles... ****************************/
        private static GUIStyle _BoldTxtStyle;
        private static GUIStyle _BoldErrorTxtStyle;
        private static GUIStyle _FoldoutTxtStyle;
        private static GUIStyle _ButtonStyle;
        private static GUIStyle _TxtFieldStyle;
        private static GUIStyle _TxtFieldErrorStyle;
        private static GUIStyle _CategoryTxtStyle;
        private static GUIStyle _ContentTxtStyle;
        private string _CountColorStyle   = "#8DFF9E";
        private string _ContentColorStyle = "#6487AA";
        private string _FoldoutColorStyle = "black";

        /** GUI Boolean ****************************/
        private static AnimBool _StudioPathSettingsFade;
        private static AnimBool _BusSettingsFade;
        private static AnimBool _BankSettingsFade;
        private static AnimBool _ParamSettingsFade;
        private static AnimBool _EventSettingsFade;

        private static bool[] _GroupIsValids = new bool[3] { true, true, true };


        /** SerializedProperty **********************/
        private SerializedObject   _StudioSettingsObj;
        private SerializedProperty _StudioPathProperty;



        //=======================================
        ////          Magic Methods           ////
        //=======================================
        [MenuItem("FMOD/FMODAudio Settings")]
        public static void OpenWindow()
        {
            EditorWindow.GetWindow(typeof(FModAudioManagerWindow), false, "FMODAudio Settings");
        }

        private void OnEnable()
        {
            /**AnimBool ����...***************************/
            FadeAnimBoolInit();
        }

        private void OnFocus()
        {
            /** Banks ����... ****************************/
            try { FMODUnity.EventManager.RefreshBanks(); } catch { /*TODO:...*/ }
        }

        private void OnGUI()
        {
            GUI_InitEditor();

            //�̺�Ʈ���� ��ȿ���� ���� �Ǵ�.
            EventGroupIsValid(_GroupIsValids);

            //������ ��Ų�� ���� ���� ��ȭ
            bool isBlack = ( EditorGUIUtility.isProSkin==false );
            _BoldTxtStyle.normal.textColor = (isBlack ? Color.black : Color.white);

            _FoldoutTxtStyle.normal.textColor   = (isBlack ? Color.black : Color.white);
            _FoldoutTxtStyle.onNormal.textColor = (isBlack ? Color.black : Color.white);
            _FoldoutColorStyle                  = (isBlack ? "black" : "white");

            _CategoryTxtStyle.normal.textColor   = (isBlack ? Color.black : Color.white);
            _CategoryTxtStyle.onNormal.textColor = (isBlack ? Color.black : Color.white);

            //-----------------------------------------------------
            using (var view = new EditorGUILayout.ScrollViewScope(_Scrollpos, false, false, GUILayout.Height(position.height))) 
            {
                /** ��ũ�� �� ����. **************************/
                _Scrollpos = view.scrollPosition;
                GUI_ShowLogo();
                GUI_DrawLine();

                GUI_StudioPathSettings();
                GUI_DrawLine();

                using (var scope = new EditorGUI.DisabledGroupScope(!StudioPathIsValid()))
                {
                    GUI_BankSettings();
                    GUI_DrawLine();

                    GUI_BusSettings();
                    GUI_DrawLine();

                    GUI_ParamSettings();
                    GUI_DrawLine();

                    GUI_EventSettings();

                    scope.Dispose();
                }
                /** ��ũ�� �� ��. ***************************/

                view.Dispose();
            }
            //--------------------------------------------------------
        }



        //========================================
        ////        GUI Content Methods       ////
        //========================================
        private void GUI_DrawLine(float space=5f, float subOffset=0f)
        {
            #region Omit
            EditorGUILayout.Space(15f);
            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x - 15 + subOffset, rect.y), new Vector2(rect.width + 15 - subOffset*2, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10f);
            #endregion
        }

        private void GUI_InitEditor()
        {
            #region Omit
            /*************************************
             *   �����Ϳ��� ����� ���� �ʱ�ȭ.
             * **/
            if (_EditorSettings == null) _EditorSettings = AssetDatabase.LoadAssetAtPath<FModAudioEditorSettings>(_EditorSettingsPath);
            if (_StudioSettings == null) _StudioSettings = AssetDatabase.LoadAssetAtPath<FMODUnity.Settings>(_StudioSettingsPath);

            //EditorSettings�� ���ٸ� ���� �����Ѵ�.
            if(_EditorSettings==null) {

                _EditorSettings = new FModAudioEditorSettings();
                AssetDatabase.CreateAsset(_EditorSettings, _EditorSettingsPath);
            }

            /**************************************
             *  ���̵� Anim���� �ʱ�ȭ�Ѵ�.
             * **/
            if(_StudioPathSettingsFade==null){

                _StudioPathSettingsFade = new AnimBool(true);
                _StudioPathSettingsFade.speed = 3f;
                _StudioPathSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
            }

            if (_BusSettingsFade == null){

                _BusSettingsFade = new AnimBool(false);
                _BusSettingsFade.speed = 3f;
                _BusSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
            }

            if (_BankSettingsFade == null){

                _BankSettingsFade = new AnimBool(false);
                _BankSettingsFade.speed = 3f;
                _BankSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
            }

            if (_ParamSettingsFade == null){

                _ParamSettingsFade = new AnimBool(false);
                _ParamSettingsFade.speed = 3f;
                _ParamSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
            }

            if (_EventSettingsFade == null)
            {

                _EventSettingsFade = new AnimBool(false);
                _EventSettingsFade.speed = 3f;
                _EventSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
            }

            /*************************************
             *  ��� �ؽ��ĵ��� �ʱ�ȭ�Ѵ�.
             * **/
            if(_ParamLabelIconTex==null){

                _ParamLabelIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_ParamLabelIconPath, typeof(Texture));  
            }

            if(_BankIconTex==null){

                _BankIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_BankIconPath, typeof(Texture));
            }

            if (_BannerBlackTex == null) {

                _BannerBlackTex = (Texture)AssetDatabase.LoadAssetAtPath(_BannerBlackPath, typeof(Texture));
            }

            if (_BannerWhiteTex == null){

                _BannerWhiteTex = (Texture)AssetDatabase.LoadAssetAtPath(_BannerWhitePath, typeof(Texture));
            }

            if(_StudioIconTex==null) {

                _StudioIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_StudioIconPath, typeof(Texture));
            }

            if(_SearchIconTex==null){

                _SearchIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_SearchIconPath, typeof(Texture));
            }

            if(_AddIconTex==null) {

                _AddIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_AddIconPath, typeof(Texture));
            }

            if(_XYellowIconTex==null){

                _XYellowIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_XYellowIconPath, typeof(Texture));
            }

            if(_DeleteIconTex==null){

                _DeleteIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_DeleteIconPath, typeof(Texture));
            }

            if(_NotFoundIconTex==null){

                _NotFoundIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_NotFoundIconPath, typeof(Texture));  
            }

            /*********************************
             *  �ؽ�Ʈ ��Ÿ�� �ʱ�ȭ
             ***/
            if(_BoldTxtStyle==null) {

                _BoldTxtStyle = new GUIStyle();
                _BoldTxtStyle.normal.textColor = Color.white;
                _BoldTxtStyle.fontStyle = FontStyle.Bold;
                _BoldTxtStyle.richText = true;
            }

            if (_FoldoutTxtStyle == null){

                _FoldoutTxtStyle = new GUIStyle(EditorStyles.foldout);
                _FoldoutTxtStyle.normal.textColor = Color.white;
                _FoldoutTxtStyle.fontStyle = FontStyle.Bold;
                _FoldoutTxtStyle.richText = true;
                _FoldoutTxtStyle.fontSize = 14;
            }

            if(_CategoryTxtStyle==null){

                _CategoryTxtStyle = new GUIStyle(EditorStyles.foldout);
                _CategoryTxtStyle.normal.textColor = Color.white;
                _CategoryTxtStyle.richText = true;
                _CategoryTxtStyle.fontStyle = FontStyle.Bold;
                _CategoryTxtStyle.fontSize = 12;
            }

            if(_ContentTxtStyle==null)
            {
                _ContentTxtStyle = new GUIStyle(EditorStyles.label);
                _ContentTxtStyle.richText= true;
                _ContentTxtStyle.fontStyle = FontStyle.Bold;
                _ContentTxtStyle.fontSize = 12;
            }

            if (_BoldErrorTxtStyle == null)
            {

                _BoldErrorTxtStyle = new GUIStyle();
                _BoldErrorTxtStyle.normal.textColor = Color.red;
                _BoldErrorTxtStyle.onNormal.textColor = Color.red;
                _BoldErrorTxtStyle.fontStyle = FontStyle.Bold;
                _BoldErrorTxtStyle.fontSize = 14;
                _BoldErrorTxtStyle.richText = true;
            }

            if (_ButtonStyle==null){

                _ButtonStyle = new GUIStyle(GUI.skin.button);
                _ButtonStyle.padding.top = 1;
                _ButtonStyle.padding.bottom = 1;
            }

            if(_TxtFieldStyle==null){

                _TxtFieldStyle = new GUIStyle(EditorStyles.textField);
                _TxtFieldStyle.richText = true;
            }

            if (_TxtFieldErrorStyle == null){

                _TxtFieldErrorStyle = new GUIStyle(EditorStyles.textField);
                _TxtFieldErrorStyle.richText = true;
                _TxtFieldErrorStyle.normal.textColor = Color.red;
                _TxtFieldErrorStyle.fontStyle = FontStyle.Bold;
                _TxtFieldErrorStyle.onNormal.textColor= Color.red;
            }


            /**************************************
             *  SerializedProperty �ʱ�ȭ
             ****/
            if (_StudioSettingsObj==null) _StudioSettingsObj = new SerializedObject(_StudioSettings);
            if(_StudioPathProperty==null) _StudioPathProperty = _StudioSettingsObj.FindProperty("sourceProjectPath");

            bool isBlackSkin = ( EditorGUIUtility.isProSkin );
            _CountColorStyle = ( isBlackSkin ? "#8DFF9E" : "#107C05" );
            _ContentColorStyle = (isBlackSkin ? "#6487AA" : "#104C87");
            #endregion
        }

        private void GUI_ShowLogo()
        {
            #region Omit
            bool isBlack        = ( EditorGUIUtility.isProSkin==false );
            Texture useBanner   = ( isBlack ? _BannerBlackTex : _BannerWhiteTex);

            GUILayout.Box(useBanner, GUILayout.Width(position.width), GUILayout.Height(100f));

            /**Editor Version�� ����.*/
            using(var scope = new GUILayout.AreaScope(new Rect(position.width*.5f-95f, 100f - 20, 300, 30))){

                GUILayout.Label($"FModAudio Settings Editor {_EditorVersion}", _BoldTxtStyle);
                scope.Dispose();
            }

            #endregion
        }

        private void GUI_StudioPathSettings()
        {
            #region Omit

            EditorGUI.indentLevel++;
            _StudioPathSettingsFade.target = EditorGUILayout.Foldout(_StudioPathSettingsFade.target,"FMod Studio Path Setting", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);


            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_StudioPathSettingsFade.faded))
            {
                bool EventIsValid = (_GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
                bool PathIsValid = StudioPathIsValid();

                if (fadeScope.visible)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    /*************************************/
                    float buttonWidth = 25f;
                    float pathWidth = (position.width - buttonWidth * 4f);

                    GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
                    GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
                    GUILayoutOption pathWidthOption = GUILayout.Width(pathWidth);
                    GUILayoutOption pathHeightOption = GUILayout.Height(buttonWidth);

                    //��� ǥ��
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        GUIStyle usedStyle  = ( PathIsValid?_TxtFieldStyle:_TxtFieldErrorStyle );
                        string newPath      = EditorGUILayout.TextField("Studio Project Path: ", _StudioPathProperty.stringValue,usedStyle, pathWidthOption, pathHeightOption);

                        //��ΰ� ����Ǿ��� ���
                        if (scope.changed && newPath.EndsWith(".fspro")) {

                            _StudioPathProperty.stringValue = newPath;
                            _StudioSettings.HasSourceProject = true;
                            ResetEditorSettings();
                        }

                        scope.Dispose();
                    }

                    //������ ��ư�� ������ ���
                    if (GUILayout.Button(_SearchIconTex, _ButtonStyle, buttonWidthOption, buttonHeightOption)){

                        string prevPath = _StudioSettings.SourceProjectPath;
                        if(FMODUnity.SettingsEditor.BrowseForSourceProjectPath(_StudioSettingsObj) && !prevPath.Equals(_StudioSettings.SourceProjectPath))
                        {
                            ResetEditorSettings();
                        }
                        
                    }

                    //��Ʃ��� �ٷΰ��� ��ư
                    if (GUILayout.Button(_StudioIconTex, _ButtonStyle, buttonWidthOption, buttonHeightOption)){

                        string projPath = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;

                        if (StudioPathIsValid()){

                            System.Diagnostics.Process.Start(projPath);
                        }
                    }
                    EditorGUI.indentLevel--;

                    /**************************************/
                    EditorGUILayout.EndHorizontal();

                    GUI_CreateEnumAndRefreshData();
                    if (!PathIsValid) EditorGUILayout.HelpBox("The FMod Studio Project for that path does not exist.", MessageType.Error);
                    else if (!EventIsValid) EditorGUILayout.HelpBox("Among the events currently loaded into the Editor, an invalid event exists. Press the Loaded Studio Settings button to reload.", MessageType.Error);
                }

                fadeScope.Dispose();
            }

            EditorGUI.indentLevel--; 

            #endregion
        }

        private void GUI_CreateEnumAndRefreshData()
        {
            #region Omit
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(10f);

                //�ɼ� ���� �� ����
                bool allGroupInValid    = !(_GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
                bool studioPathInValid  = !StudioPathIsValid();

                using (var disableScope = new EditorGUI.DisabledGroupScope(allGroupInValid || studioPathInValid))
                {
                    if (GUILayout.Button("Save Settings and Create Enums", GUILayout.Width(position.width * .5f))){

                        CreateEnumScript();
                        ApplyBankLoadInfo();
                        if (_EditorSettings != null && _StudioSettings != null)
                        {
                            EditorUtility.SetDirty(_StudioSettings);
                            EditorUtility.SetDirty(_EditorSettings);
                        }

                        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, _ScriptDefine);
                        AssetDatabase.Refresh();
                    }
                }

                //FMod Studio ������ �ҷ�����
                if(GUILayout.Button("Load Studio Settings", GUILayout.Width(position.width*.5f)))
                {
                    if (_EditorSettings!=null){

                        GetBusList(_EditorSettings.BusList);
                        GetBankList(_EditorSettings.BankList);
                        GetParamList(_EditorSettings.ParamDescList, _EditorSettings.ParamLableList);
                        GetEventList(_EditorSettings.CategoryDescList, _EditorSettings.EventRefs, _EditorSettings.EventGroups);
                        _refresh = true;
                    }
                }

                scope.Dispose();
            }

            /**�����͸� �����ߴµ� �̺�Ʈ�� ���ٸ� ������� ��� ����..*/
            if(_refresh && _EditorSettings.EventRefs.Count==0){
                EditorGUILayout.HelpBox("Did the event not load even though you added it in FMod Studio? Make sure you've build the event in FMod Studio", MessageType.Info);
            }

            #endregion
        }

        private void GUI_BusSettings()
        {
            #region Omit
            List<NPData> busList = _EditorSettings.BusList;
            int Count = busList.Count;

            EditorGUI.indentLevel++;
            _BusSettingsFade.target = EditorGUILayout.Foldout(_BusSettingsFade.target, $"FMod Bus<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);


            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_BusSettingsFade.faded))
            {
                if (fadeScope.visible)
                {
                    EditorGUILayout.BeginVertical();
                    /*******************************************/
                    float buttonWidth = 25f;
                    float pathWidth = (position.width - buttonWidth * 8f);

                    GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
                    GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
                    GUILayoutOption pathWidthOption = GUILayout.Width(pathWidth);
                    GUILayoutOption pathHeightOption = GUILayout.Height(buttonWidth);

                    if(Count>0) EditorGUILayout.HelpBox("An FModBusType enum is created based on the information shown below.", MessageType.Info);

                    //��� ���� ��ϵ��� �����ش�.
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);

                    using(var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"<color={_ContentColorStyle}>Master</color>", _ContentTxtStyle, GUILayout.Width(150));
                        EditorGUILayout.TextArea("bus:/", _TxtFieldStyle, pathWidthOption, pathHeightOption);
                        horizontal.Dispose();
                    }
                    EditorGUI.EndDisabledGroup();

                    for (int i=0; i<Count; i++) {

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{busList[i].Name}</color>", _ContentTxtStyle, GUILayout.Width(150));
                            EditorGUILayout.TextArea(busList[i].Path, _TxtFieldStyle, pathWidthOption, pathHeightOption);
                            horizontal.Dispose();
                        }
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    /*******************************************/
                }

                fadeScope.Dispose();
            }

            EditorGUI.indentLevel--;
            #endregion
        }

        private void GUI_BankSettings()
        {
            #region Omit
            List<NPData> bankList = _EditorSettings.BankList;
            int Count     = bankList.Count;

            EditorGUI.indentLevel++;
            _BankSettingsFade.target = EditorGUILayout.Foldout(_BankSettingsFade.target, $"FMod Banks<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);

            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_BankSettingsFade.faded))
            {
                if (fadeScope.visible)
                {
                    EditorGUILayout.BeginVertical();

                    /*******************************************/
                    float buttonWidth  = 150f;
                    float buttonHeight = 25f;
                    float pathWidth    = (position.width - buttonWidth - 40f);

                    GUILayoutOption buttonWidthOption  = GUILayout.Width(buttonWidth);
                    GUILayoutOption buttonHeightOption = GUILayout.Height(buttonHeight);
                    GUILayoutOption pathWidthOption    = GUILayout.Width(pathWidth);
                    GUILayoutOption pathHeightOption   = GUILayout.Height(buttonHeight);

                    if(Count>0) EditorGUILayout.HelpBox("An FModBankType enum is created based on the information shown below.", MessageType.Info);

                    //��� ��ũ ��ϵ��� �����ش�.
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < Count; i++){

                        NPData bank    = bankList[i];
                        string rawName = bank.Name.Replace("_", ".");

                        EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{bank.Name}</color>", _ContentTxtStyle, buttonWidthOption);
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.TextArea(bank.Path, _TxtFieldStyle, pathWidthOption, pathHeightOption);

                            //�ʱ�ȭ�� ��ũ�� �ε�ǵ��� ������ �Ǿ��ִٸ�....
                            bool         isLoaded  = bank.Extra;
                            BankLoadType loadType  = (isLoaded? BankLoadType.Load_At_Initialized:BankLoadType.Not_Load);

                            /***���۽� �ε� �ʵ带 �������� ���....**/
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                loadType = (BankLoadType)EditorGUILayout.EnumPopup(loadType, GUILayout.Width(170f), buttonHeightOption);
                                if(scope.changed){
                                    bank.Extra  = (loadType==BankLoadType.Load_At_Initialized);
                                    bankList[i] = bank;
                                }
                            }

                            horizontal.Dispose();
                        }

                        EditorGUILayout.Space(5f);
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    /*******************************************/
                }

                fadeScope.Dispose();
            }

            EditorGUI.indentLevel--;
            #endregion
        }

        private void GUI_MenuSelected()
        {
            //TODO: 
        }

        private void GUI_ParamSettings()
        {
            #region Omit
            List<FModParamDesc> descs   = _EditorSettings.ParamDescList;
            List<string>        labels  = _EditorSettings.ParamLableList;
            int Count = descs.Count;
            int labelStartIndex = 0;

            float buttonWidth = 25f;
            float pathWidth = (position.width - buttonWidth * 10f);
            if (pathWidth <= 0f) pathWidth = 0f;

            GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
            GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
            GUILayoutOption pathWidthOption = GUILayout.Width(pathWidth);
            GUILayoutOption pathHeightOption = GUILayout.Height(buttonWidth);

            EditorGUI.indentLevel++;
            _ParamSettingsFade.target = EditorGUILayout.Foldout(_ParamSettingsFade.target, $"FMod Parameters<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);

            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_ParamSettingsFade.faded))
            {
                if (fadeScope.visible)
                {
                    if(Count>0) EditorGUILayout.HelpBox("An FModGlobalParameter/FModLocalParameter enum is created based on the information shown below.", MessageType.Info);

                    _ParamGroups[0] = $"Global Parameters({_EditorSettings.ParamCountList[0]})";
                    _ParamGroups[1] = $"Local Parameters({_EditorSettings.ParamCountList[1]})";
                    _ParamGroupSelected = GUILayout.Toolbar(_ParamGroupSelected, _ParamGroups, GUILayout.Height(40f));

                    //��� �Ķ���� ��ϵ��� �����ش�.
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < Count; i++)
                    {
                        FModParamDesc desc = descs[i];

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            if (descs[i].isGlobal && _ParamGroupSelected != 0 
                                || !descs[i].isGlobal && _ParamGroupSelected!=1) {

                                labelStartIndex += desc.LableCount;
                                continue;
                            }

                            GUILayout.Space(5f);
                            EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{desc.ParamName}</color>", _ContentTxtStyle, GUILayout.Width(140));
                            EditorGUILayout.TextArea($"( <color=red>Min:</color> {desc.Min}~ <color=red>Max:</color> {desc.Max} )", _TxtFieldStyle, pathWidthOption, pathHeightOption);

                            //���̺� Ȯ�ι�ư
                            using(var disable = new EditorGUI.DisabledGroupScope( desc.LableCount<=0 ))
                            {
                                //���̺��� �����Ѵٸ� ��ư�� ���� �� �ֵ��� �Ѵ�.
                                if (EditorGUILayout.DropdownButton(new GUIContent("Labeld"), FocusType.Passive, GUILayout.Width(70f))) {

                                    GenericMenu menu= new GenericMenu();
                                    for (int j = 0; j < desc.LableCount; j++){

                                        menu.AddItem(new GUIContent(labels[labelStartIndex + j]), true, GUI_MenuSelected);
                                    }

                                    menu.ShowAsContext();
                                }
                            }

                            horizontal.Dispose();
                            labelStartIndex += desc.LableCount;
                        }

                        /**�Ķ���� ��θ� ǥ���Ѵ�....**/
                        using ( var vertical = new EditorGUILayout.VerticalScope()) {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField($"<color=#6487AA00>{desc.ParamName}</color>", _ContentTxtStyle, GUILayout.Width(140));
                                EditorGUILayout.TextArea(desc.Path, _TxtFieldStyle, pathWidthOption);
                            }
                        }

                        GUILayout.Space(5f);

                    }
                    EditorGUI.indentLevel--;

                }

                fadeScope.Dispose();
            }

            EditorGUI.indentLevel--;
            #endregion
        }

        private void GUI_EventSettings()
        {
            #region Omit
            FModGroupInfo[]             descs       = _EditorSettings.EventGroups;
            List<FModEventCategoryDesc> categorys   = _EditorSettings.CategoryDescList;
            List<FModEventInfo>         infos       = _EditorSettings.EventRefs;
            int infoCount = infos.Count;
            int descCount = descs.Length;
            int categoryCount = categorys.Count;

            EditorGUI.indentLevel++;
            bool allGroupIsValid = ( _GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
            string eventSettingsColor = ( allGroupIsValid ? _FoldoutColorStyle : "red");
            _EventSettingsFade.target = EditorGUILayout.Foldout(_EventSettingsFade.target, $"<color={eventSettingsColor}>FMod Events</color><color={_CountColorStyle}>({infoCount})</color>", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);


            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_EventSettingsFade.faded))
            {
                ////////////////////////////////////////////////////
                if (fadeScope.visible)
                {
                    EditorGUI.indentLevel--;
                    if (categoryCount>0) EditorGUILayout.HelpBox("An FModBGMEventType/FModSFXEventType/FModNoGroupEventType enum is created based on the information shown below.", MessageType.Info);
                    EditorGUI.indentLevel++;

                    //��Ʈ���� ����...
                    if (_EditorSettings.RootFolderFoldout = EditorGUILayout.Foldout(_EditorSettings.RootFolderFoldout, "Event Group RootFolder Settings"))
                    {
                        EditorGUILayout.HelpBox("Events are categorized into BGM and SFX groups based on the name of the root folder they belong to. If an event does not belong to either of these two groups, it is categorized into the NoGroup group.", MessageType.Info);

                        using (var scope = new EditorGUILayout.HorizontalScope()){

                            _EditorSettings.EventGroups[0].RootFolderName = EditorGUILayout.TextField("BGM RootFolder", _EditorSettings.EventGroups[0].RootFolderName, GUILayout.Width(position.width * .5f));
                            _EditorSettings.EventGroups[1].RootFolderName = EditorGUILayout.TextField("SFX RootFolder", _EditorSettings.EventGroups[1].RootFolderName, GUILayout.Width(position.width * .5f));
                            scope.Dispose();
                        }
                    }


                    _EventGroups[0] = $"BGM({descs[0].TotalEvent})";
                    _EventGroups[1] = $"SFX({descs[1].TotalEvent})";
                    _EventGroups[2] = $"NoGroups({descs[2].TotalEvent})";
                    _EventGroupSelected = GUILayout.Toolbar(_EventGroupSelected, _EventGroups, GUILayout.Height(40f));


                    //��� ī�װ��� ��ȸ�Ѵ�.
                    EditorGUI.indentLevel += 1;

                    int startIndex = 0;
                    for (int i = 0; i < categoryCount; i++)
                    {
                        FModEventCategoryDesc category = categorys[i];

                        //���� ���õ� �׷��� ī�װ��� �ƴ϶�� ��ŵ.
                        if (category.GroupIndex != _EventGroupSelected){

                            startIndex += category.EventCount;
                            continue;
                        }

                        //�ش� ī�װ��� ����ƿ� ȿ�� ����.
                        using(var change = new EditorGUI.ChangeCheckScope())
                        {
                            string groupColor = (category.EventIsValid ? _ContentColorStyle : "red");
                            category.foldout = EditorGUILayout.Foldout(category.foldout, $"<color={groupColor}>{category.CategoryName}</color>" + $"<color={_CountColorStyle}>({category.EventCount})</color>", _CategoryTxtStyle);

                            //foldout���� �ٲ���� ���
                            if(change.changed){

                                categorys[i] = category;
                            }

                            change.Dispose();
                        }

                        //ī�װ��� ������ ���¶�� �ش� ī�װ��� �����ִ� ��� �̺�Ʈ�� ����Ѵ�.
                        if (category.foldout)
                        {
                            int EventCount = categorys[i].EventCount;
                            EditorGUI.indentLevel++;
                            for (int j = 0; j < EventCount; j++){

                                int  realIndex  = (startIndex+j);
                                int  groupIndex = (category.GroupIndex);
                                bool isValid    = CheckEventIsValid(realIndex, infos);

                                FModEventInfo   info  = infos[realIndex];
                                GUILayoutOption width = GUILayout.Width(position.width-25f);

                                EditorGUILayout.BeginVertical();
                                {
                                    EditorGUILayout.LabelField(info.Name, width);
                                    EditorGUILayout.TextField(info.Path, width);
                                }
                                EditorGUILayout.EndVertical();

                            }
                            EditorGUI.indentLevel--;
                        }

                        GUI_DrawLine(3f, 40f);
                        startIndex += category.EventCount;
                    }

                    EditorGUI.indentLevel -= 1;
                }

                fadeScope.Dispose();
                /////////////////////////////////////////////////////////
            }

            EditorGUI.indentLevel--;
            #endregion
        }



        //========================================
        ////          Utility Methods         ////
        //========================================
        private void ApplyBankLoadInfo()
        {
            #region Omit
            /***************************************************
             *    ��ũ���� �ε� ������ ���� �����Ѵ�....
             * *****/
            int                    count    = _EditorSettings.BankList.Count;
            FMODUnity.BankLoadType loadType = FMODUnity.BankLoadType.All;

            _StudioSettings.BanksToLoad.Clear();
            for(int i=0; i<count; i++)
            {
                NPData bank = _EditorSettings.BankList[i];

                /**FMod �ʱ�ȭ �ܰ迡�� �ش� ��ũ�� �ε��ϴ°�?**/
                if (bank.Extra==false){
                    loadType = FMODUnity.BankLoadType.Specified;
                }
                else _StudioSettings.BanksToLoad.Add(bank.Name.Replace("_", "."));
            }

            /**��� ��ũ�� �ε��ϴ� ���̶��, Ư�� ��ũ���� �ε� ������ �ʱ�ȭ�Ѵ�....**/
            if (loadType == FMODUnity.BankLoadType.All){
                _StudioSettings.BanksToLoad.Clear();
            }

            /**��� ��ũ�� �ε����� �ʴ°�?**/
            else if (loadType == FMODUnity.BankLoadType.Specified && _StudioSettings.BanksToLoad.Count==0){
                loadType = FMODUnity.BankLoadType.None;
            }

            _StudioSettings.BankLoadType = loadType;
            #endregion
        }

        private void ResetEditorSettings()
        {
            #region Omit
            if (_EditorSettings == null || _StudioSettings==null) return;

            //Editor Settings Reset
            _EditorSettings.FoldoutBooleans = 0;
            _EditorSettings.BankList.Clear();
            _EditorSettings.BusList.Clear();
            _EditorSettings.ParamCountList[0] = 0;
            _EditorSettings.ParamCountList[1] = 0;
            _EditorSettings.ParamDescList.Clear();
            _EditorSettings.ParamLableList.Clear();
            _EditorSettings.CategoryDescList.Clear();
            _EditorSettings.EventRefs.Clear();
            _EditorSettings.EventGroups[0].TotalEvent = 0;
            _EditorSettings.EventGroups[1].TotalEvent = 0;
            _EditorSettings.EventGroups[2].TotalEvent = 0;
            EditorUtility.SetDirty(_EditorSettings);

            //Studio Settings Reset
            _StudioSettings.BankLoadType = FMODUnity.BankLoadType.All;
            _StudioSettings.BanksToLoad.Clear();
            #endregion
        }

        private void CreateEnumScript()
        {
            #region Omit
            if (_EditorSettings == null) return;

            _builder.Clear();
            _builder.AppendLine("using UnityEngine;");
            _builder.AppendLine("");


            /**************************************
             * Bus Enum ����.
             *****/
            _builder.AppendLine("public enum FModBusType");
            _builder.AppendLine("{");
            _builder.AppendLine("   Master=0,");

            List<NPData> busLists = _EditorSettings.BusList;
            int Count = busLists.Count;
            for(int i=0; i<Count; i++)
            {
                string busName  = RemoveUnnecessaryChar(busLists[i].Path, _BusRootPath);
                string comma    = (i == Count - 1 ? "" : ",");
                _builder.AppendLine($"   {busName}={i+1}{comma}");
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /********************************************
             *  Bank Enum ����
             ***/
            _builder.AppendLine("public enum FModBankType");
            _builder.AppendLine("{");

            List<EditorBankRef> bankList = FMODUnity.EventManager.Banks;

            Count = bankList.Count;
            for (int i = 0; i < Count; i++)
            {
                EditorBankRef bank = bankList[i];
                string bankName    = RemoveInValidChar(bank.Name);
                string comma       = (i == Count - 1 ? "" : ",");
                _builder.AppendLine($"   {bankName}={i}{comma}");
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /*******************************************
             *  Global Parameter Enum ����
             * ***/
            _builder.AppendLine("public enum FModGlobalParamType");
            _builder.AppendLine("{");
            _builder.AppendLine("   None_Parameter =-1,");

            List<FModParamDesc> paramDescs = _EditorSettings.ParamDescList;

            Count = paramDescs.Count;
            for(int i=0; i<Count; i++)
            {
                FModParamDesc desc = paramDescs[i];
                if (desc.isGlobal == false) continue;

                string comma    = (i == Count - 1 ? "" : ",");
                string enumName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);
                _builder.AppendLine($"   {enumName}={i}{comma}");
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /*******************************************
             *  Local Parameter Enum ����
             * ***/
            _builder.AppendLine("public enum FModLocalParamType");
            _builder.AppendLine("{");
            _builder.AppendLine($"   None_Parameter =-1,");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++)
            {
                FModParamDesc desc = paramDescs[i];
                if (desc.isGlobal) continue;

                string comma    = (i == Count - 1 ? "" : ",");
                string enumName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);
                _builder.AppendLine($"   {enumName}={i}{comma}");
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /**********************************************
             *   Param Lable Struct ����
             * *****/
            _builder.AppendLine("public struct FModParamLabel");
            _builder.AppendLine("{");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++) {

                FModParamDesc desc = paramDescs[i];
                if(desc.LableCount<=0) continue;

                string structName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);

                _builder.AppendLine($"    public struct {structName}");
                _builder.AppendLine("    {");

                AddParamLabelListScript(_builder, i);

                _builder.AppendLine("    }");
            }


            _builder.AppendLine("}");
            _builder.AppendLine("");


            /**********************************************
             *   Param Range Struct ����
             * *****/
            _builder.AppendLine("public struct FModParamValueRange");
            _builder.AppendLine("{");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++){

                FModParamDesc desc = paramDescs[i];
                string structName  = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);

                _builder.AppendLine($"    public struct {structName}");
                _builder.AppendLine("    {");

                AddParamRangeListScript(_builder, i);

                _builder.AppendLine("    }");
            }


            _builder.AppendLine("}");
            _builder.AppendLine("");


            /**************************************
             * BGM Events Enum ����
             ***/
            int     total           = 0;
            float   writeEventCount = 0;
            List<FModEventCategoryDesc> categoryDescs = _EditorSettings.CategoryDescList;
            List<FModEventInfo>         infos         = _EditorSettings.EventRefs;
            Count = _EditorSettings.CategoryDescList.Count;

            _builder.AppendLine("public enum FModBGMEventType");
            _builder.AppendLine("{");

            for (int i = 0; i < Count; i++)
            {
                FModEventCategoryDesc desc = categoryDescs[i];

                //BGM �׷��� �ƴ϶�� ��ŵ.
                if (desc.GroupIndex != 0){

                    total += desc.EventCount;
                    continue;
                }

                //�ش� ī�װ��� ��� �̺�Ʈ�� �߰��Ѵ�.
                for (int j = 0; j < desc.EventCount; j++){

                    int    realIndex = (total + j);
                    string comma     = (++writeEventCount == _EditorSettings.EventGroups[0].TotalEvent ? "" : ",");
                    string path      = infos[realIndex].Path;
                    string enumName  = RemoveUnnecessaryChar(path, _EventRootPath, _EditorSettings.EventGroups[0].RootFolderName, "/");

                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    _builder.AppendLine($"   {enumName}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /**************************************
             *   SFX Events Enum ����
             * ****/
            _builder.AppendLine("public enum FModSFXEventType");
            _builder.AppendLine("{");

            total = 0;
            writeEventCount = 0;
            for(int i=0; i<Count; i++)
            {
                FModEventCategoryDesc desc = categoryDescs[i];

                //SFX �׷��� �ƴ϶�� ��ŵ.
                if (desc.GroupIndex != 1){

                    total += desc.EventCount;
                    continue;
                }

                //�ش� ī�װ��� ��� �̺�Ʈ�� �߰��Ѵ�.
                for (int j = 0; j < desc.EventCount; j++)
                {
                    int realIndex   = (total + j);
                    string comma    = (++writeEventCount == _EditorSettings.EventGroups[1].TotalEvent ? "" : ",");
                    string path     = infos[realIndex].Path;
                    string enumName = RemoveUnnecessaryChar(path, _EventRootPath, _EditorSettings.EventGroups[1].RootFolderName, "/");

                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    _builder.AppendLine($"   {enumName}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /**************************************
             *   NoGroups Events Enum ����
             * ****/
            _builder.AppendLine("public enum FModNoGroupEventType");
            _builder.AppendLine("{");

            total = 0;
            writeEventCount = 0;
            for (int i = 0; i < Count; i++)
            {
                FModEventCategoryDesc desc = categoryDescs[i];

                //NoGroup �׷��� �ƴ϶�� ��ŵ.
                if (desc.GroupIndex != 2)
                {

                    total += desc.EventCount;
                    continue;
                }

                //�ش� ī�װ��� ��� �̺�Ʈ�� �߰��Ѵ�.
                for (int j = 0; j < desc.EventCount; j++)
                {
                    int realIndex   = (total + j);
                    string comma    = (++writeEventCount == _EditorSettings.EventGroups[2].TotalEvent ? "" : ",");
                    string path     = infos[realIndex].Path;
                    string enumName = RemoveUnnecessaryChar(path, _EventRootPath);

                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    _builder.AppendLine($"   {enumName}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            _builder.AppendLine("}");
            _builder.AppendLine("");


            /***************************************
             * Event Reference List class ����
             ***/
            _builder.AppendLine("public sealed class FModReferenceList");
            _builder.AppendLine("{");
            _builder.AppendLine("    public static readonly FMOD.GUID[] Events = new FMOD.GUID[]");
            _builder.AppendLine("    {");
            AddEventListScript(_builder, _EditorSettings.EventRefs); 
            _builder.AppendLine("    };");
            _builder.AppendLine("");

            _builder.AppendLine("    public static readonly string[] Banks = new string[]");
            _builder.AppendLine("    {");
            AddBankListScript(_builder);
            _builder.AppendLine("    };");
            _builder.AppendLine("");


            _builder.AppendLine("    public static readonly string[] Params = new string[]");
            _builder.AppendLine("    {");
            AddParamListScript(_builder);
            _builder.AppendLine("    };");
            _builder.AppendLine("");

            _builder.AppendLine("    public static readonly string[] BusPaths = new string[]");
            _builder.AppendLine("    {");
            AddBusPathListScript(_builder);
            _builder.AppendLine("    };");
            _builder.AppendLine("");

            _builder.AppendLine("}");
            _builder.AppendLine("");


            //���� �� ���ΰ�ħ
            File.WriteAllText(_DataScriptPath, _builder.ToString());

            #endregion
        }

        private void AddBusPathListScript(StringBuilder builder)
        {
            #region Omit
            List<NPData> list = _EditorSettings.BusList;
            int Count = list.Count;

            builder.AppendLine($"        \"{_BusRootPath}\"{ (Count==0?"":",") }");
            for (int i = 0; i < Count; i++)
            {
                string comma = (i == Count - 1 ? "" : ",");
                builder.AppendLine($"        \"{list[i].Path}\"" + comma);
            }
            #endregion
        }

        private void AddParamListScript(StringBuilder builder)
        {
            #region Omit
            List<FModParamDesc> list = _EditorSettings.ParamDescList;
            int Count = list.Count;
            for (int i = 0; i < Count; i++)
            {
                string comma = (i == Count - 1 ? "" : ",");
                builder.AppendLine($"        \"{list[i].ParamName}\"" + comma);
            }
            #endregion
        }

        private void AddParamRangeListScript(StringBuilder builder, int descIndex)
        {
            #region Omit
            List<FModParamDesc> descs = _EditorSettings.ParamDescList;
            FModParamDesc desc = descs[descIndex];

            builder.AppendLine($"       public const float Min={desc.Min};");
            builder.AppendLine($"       public const float Max={desc.Max};");
            #endregion
        }

        private void AddParamLabelListScript(StringBuilder builder, int descIndex) 
        {
            #region Omit
            List<FModParamDesc> descs   = _EditorSettings.ParamDescList;
            List<string>        labels  = _EditorSettings.ParamLableList;

            FModParamDesc desc = descs[descIndex];
            if( desc.LableCount<=0 ) return;

            int startIndex = GetParamLabelStartIndex(descIndex);
            for(int i=0; i<desc.LableCount; i++) 
            {
                int realIndex = (startIndex+i);
                string labelName = RemoveInValidChar(labels[realIndex]);
                builder.AppendLine($"       public const float {labelName}  ={i}f;");
            }
            #endregion
        }

        private void AddBankListScript(StringBuilder builder)
        {
            #region Omit
            List<FMODUnity.EditorBankRef> list = FMODUnity.EventManager.Banks;
            int Count = list.Count;
            for (int i = 0; i < Count; i++)
            {
                string comma = (i == Count - 1 ? "" : ",");
                builder.AppendLine($"        \"{list[i].Name}\"" + comma);
            }
            #endregion
        }

        private void AddEventListScript(StringBuilder builder, List<FModEventInfo> list, bool lastWork = false)
        {
            #region Omit
            int Count = list.Count;
            for (int i = 0; i < Count; i++)
            {
                string comma = (i == Count - 1 && lastWork ? "" : ",");
                string guidValue = $"Data1={list[i].GUID.Data1}, Data2={list[i].GUID.Data2}, Data3={list[i].GUID.Data3}, Data4={list[i].GUID.Data4}";
                if (CheckEventIsValid(i, list) == false) continue;
                builder.AppendLine("        new FMOD.GUID{ " + guidValue + " }" + comma);
            }
            #endregion
        }

        private void FadeAnimBoolInit()
        {
            #region Omit
            UnityAction repaintEvent = new UnityAction(base.Repaint);

            _StudioPathSettingsFade?.valueChanged.RemoveAllListeners();
            _StudioPathSettingsFade?.valueChanged.AddListener(repaintEvent);

            _BusSettingsFade?.valueChanged.RemoveAllListeners();
            _BusSettingsFade?.valueChanged.AddListener(repaintEvent);

            _ParamSettingsFade?.valueChanged.RemoveAllListeners();
            _ParamSettingsFade?.valueChanged.AddListener(repaintEvent);

            _EventSettingsFade?.valueChanged.RemoveAllListeners();
            _EventSettingsFade?.valueChanged.AddListener(repaintEvent);

            _BusSettingsFade?.valueChanged.RemoveAllListeners();
            _BusSettingsFade?.valueChanged.AddListener(repaintEvent);

            #endregion
        }

        private bool StudioPathIsValid()
        {
            string projPath = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
            return File.Exists(projPath);
        }

        private string RemoveInValidChar(string inputString)
        {
            #region Omit
            //���� ���� ����
            int removeCount = Regex.Match(inputString, @"^\d*").Length;
            inputString = inputString.Substring(removeCount, inputString.Length-removeCount);

            //������� ���ϴ� Ư������ ����
            inputString = _regex.Replace(inputString, "_");

            //���鹮�� ����.
            inputString = inputString.Replace(" ", "_");

            return inputString;
            #endregion
        }

        private string RemoveUnnecessaryChar(string inputString, params string[] removeFirstString)
        {
            #region Omit
            //���ڿ� �պκп��� ���ʿ��� �κ��� �߰ߵȴٸ� �����Ѵ�....
            int len = removeFirstString.Length;
            for (int i=0; i<len; i++)
            {
                string cur = removeFirstString[i];

                if (inputString.IndexOf(cur) == 0){
                    int curLen  = cur.Length;
                    inputString = inputString.Substring(curLen, inputString.Length-curLen);
                }
            }

            return RemoveInValidChar(inputString);
            #endregion
        }

        private void GetBankList(List<NPData> lists)
        {
            #region Omit
            if (lists == null) return;
            lists.Clear();

            List<FMODUnity.EditorBankRef> banks = FMODUnity.EventManager.Banks;
            
            int Count = banks.Count;
            for (int i = 0; i < Count; i++){

                EditorBankRef bank = banks[i];
                string bankName = RemoveInValidChar(bank.Name);
                string bankPath = bank.Path;

                NPData info = new NPData()
                {
                    Name  = bankName,
                    Path  = bankPath,
                    Extra = true
                };

                lists.Add(info);
            }
            #endregion
        }

        private void GetBusList(List<NPData> lists)
        {
            #region Omit
            if (lists == null || StudioPathIsValid() == false) return;
            lists.Clear();

            string studiopath   = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
            string[] pathSplit  = studiopath.Split('/');
            string busPath      = studiopath.Replace(pathSplit[pathSplit.Length-1], "") + _GroupFolderPath;

            Dictionary<string, NPData> busMap = new Dictionary<string, NPData>();
            DirectoryInfo dPath = new DirectoryInfo(busPath);
            XmlDocument document= new XmlDocument();

            try
            {
                //Studio folder�� ��� bus �����͵��� �о������ ���.
                foreach (FileInfo file in dPath.GetFiles())
                {
                    if (!file.Exists) continue;

                    try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                    string idNode       = document.SelectSingleNode("//object/@id")?.InnerText;
                    string nameNode     = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                    string outputNode   = document.SelectSingleNode("//object/relationship[@name='output']/destination")?.InnerText;

                    if (idNode != null && nameNode != null && outputNode != null){

                        busMap.Add(idNode, new NPData { Name = nameNode, Path = outputNode });
                        lists.Add(new NPData { Name = nameNode, Path = outputNode });
                    }
                }
            }
            catch{

                //������ ã�µ� �����ϸ� ��ŵ�Ѵ�.
                return;
            }

            //�ҷ��� ��� busData���� ��θ� ����� ����Ѵ�.
            int Count = lists.Count;
            for(int i=0; i<Count; i++)
            {
                string busName   = lists[i].Name;
                string parentBus = lists[i].Path;
                string finalPath = busName;

                while( busMap.ContainsKey(parentBus) ) {

                    finalPath = busMap[parentBus].Name + "/" + finalPath;
                    parentBus = busMap[parentBus].Path;
                }

                //������ �۾�
                lists[i] = new NPData { Name = busName, Path = _BusRootPath + finalPath };
            }

            #endregion
        }

        private void GetParamList(List<FModParamDesc> descList, List<string> labelList)
        {
            #region Omit
            if (descList == null || labelList == null) return;

            /**********************************************************
             *    �Ķ���� ����� ������ ���� �ʱ�ȭ ������ �����Ѵ�...
             * ******/
            descList.Clear();
            labelList.Clear();
            _EditorSettings.ParamCountList[0] = _EditorSettings.ParamCountList[1] = 0;

            string   studiopath       = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
            string[] pathSplit        = studiopath.Split('/');
            string   studioRootPath   = studiopath.Replace(pathSplit[pathSplit.Length - 1], "");
            string   presetPath       = studioRootPath + _PresetFolderPath;
            string   presetFolderPath = studioRootPath + _PresetFolderFolderPath;

            DirectoryInfo pPath      = new DirectoryInfo(presetPath);
            DirectoryInfo pfPath     = new DirectoryInfo(presetFolderPath);
            XmlDocument   document   = new XmlDocument();
            Dictionary<string, ParamFolderInfo> folderMap = new Dictionary<string, ParamFolderInfo>();
            


            /************************************************************
             *     �Ķ���͵��� ������ �θ� ���� �̸��� ����Ѵ�...
             * ******/
            try
            {
                foreach(FileInfo file in pPath.GetFiles()) {
                    if (!file.Exists) continue;

                    try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                    string nameNode        = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                    string outputNode      = document.SelectSingleNode("//object/relationship[@name='folder']/destination")?.InnerText;
                    string typeNode        = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='parameterType']/value")?.InnerText; 
                    string minNode         = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='minimum']/value")?.InnerText; //null�̶�� 0.
                    string maxNode         = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='maximum']/value")?.InnerText; //null�̶�� 0.
                    string isGlobalNode    = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='isGlobal']/value")?.InnerText; //null�̶�� false.
                    XmlNodeList labelNodes = document.SelectNodes("//object[@class='GameParameter']/property[@name='enumerationLabels']/value");


                    /**�Ķ���� ���� ����Ѵ�...*/
                    FModParamDesc newDesc = new FModParamDesc()
                    {
                        ParamName        = (nameNode==null? "":nameNode),
                        Path             = "",
                        ParentFolderName = (outputNode==null? "":outputNode),
                        isGlobal         = (isGlobalNode!=null),
                        LableCount       = labelNodes.Count,
                        Min              = (minNode==null? 0f:float.Parse(minNode)),
                        Max              = (maxNode==null? 0f:float.Parse(maxNode)),
                    };

                    for (int i=0; i<newDesc.LableCount; i++){
                        labelList.Add(labelNodes[i].InnerText);
                    }

                    descList.Add(newDesc);
                    _EditorSettings.ParamCountList[(newDesc.isGlobal ? 0 : 1)]++;
                }
            }
            catch{

                /**������ ã�� �� �����ٸ� ��ŵ�Ѵ�...**/
                return;
            }



            /*****************************************************
             *     �Ķ���͵��� ��� ���� �������� ��� ����Ѵ�...
             * *******/
            try
            {
                foreach (FileInfo file in pfPath.GetFiles()){
                    if (!file.Exists) continue;

                    try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                    string idNode     = document.SelectSingleNode("//object/@id")?.InnerText;
                    string nameNode   = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                    string outputNode = document.SelectSingleNode("//object/relationship[@name='folder']/destination")?.InnerText;

                    /**���� �������� ��� ����Ѵ�...**/
                    ParamFolderInfo newInfo = new ParamFolderInfo()
                    {
                        id     = idNode,
                        name   = nameNode,
                        output = outputNode,
                    };

                    /**��Ʈ������ �߰����� �ʴ´�...**/
                    if (outputNode == null) continue;
                    folderMap.Add(idNode, newInfo);
                }


            }
            catch{
                /**���� ����� ���ٸ� ��ŵ�Ѵ�....**/
                return;
            }



            /********************************************************
             *    �Ķ���� �������� ��ġ�� �������� ��θ� ����Ѵ�...
             * ******/
            int paramCount = descList.Count;    
            for(int i = 0; i<paramCount; i++){

                FModParamDesc cur       = descList[i];
                string        parent    = cur.ParentFolderName;
                string        finalPath = "";


                /**�θ������� �������� ���� ������ �Ž��� �ö󰣴�...*/
                while (folderMap.ContainsKey(parent))
                {
                    finalPath = folderMap[parent].name + "/" + cur.Path;
                    parent    = folderMap[parent].output;
                }

                /**������ �۾�...*/
                cur.Path    = (_ParamRootPath + finalPath + cur.ParamName);
                descList[i] = cur;
            }

            #endregion
        }

        private bool GetParamIsAlreadyContain(string checkParamName, List<FModParamDesc> descs)
        {
            #region Omit
            int Count = descs.Count;
            for(int i=0; i<Count; i++)
            {
                if (descs[i].ParamName == checkParamName) return true;
            }

            return false;
            #endregion
        }

        private int GetCategoryIndex(string categoryName, List<FModEventCategoryDesc> descs)
        {
            #region Omit
            int Count = descs.Count;
            for(int i=0; i<Count; i++)
            {
                if (descs[i].CategoryName.Equals(categoryName)) return i;
            }

            return -1;
            #endregion
        }

        private int GetCategoryEventStartIndex(string categoryName, List<FModEventCategoryDesc> descs)
        {
            #region Omit
            int total = 0;
            int Count = descs.Count;
            for(int i=0; i<Count; i++)
            {
                if (descs[i].CategoryName == categoryName) return total;
                total += descs[i].EventCount;
            }

            return total;
            #endregion
        }

        private void GetEventList(List<FModEventCategoryDesc> categoryList, List<FModEventInfo> refList, FModGroupInfo[] groupList)
        {
            #region Omit
            if (categoryList == null || refList == null || groupList==null) return;
            categoryList.Clear();
            refList.Clear();

            groupList[0].TotalEvent = groupList[1].TotalEvent = groupList[2].TotalEvent = 0;

            List<EditorEventRef> eventRefs = FMODUnity.EventManager.Events;
            int eventCount = eventRefs.Count;

            //��� �̺�Ʈ���� �׷캰�� �з��Ѵ�.
            for (int i = 0; i < eventCount; i++)
            {
                EditorEventRef eventRef = eventRefs[i];
                string Path             = eventRef.Path;
                string[] PathSplit      = Path.Split('/');
                string Name             = RemoveInValidChar(PathSplit[PathSplit.Length - 1]);
                string CategoryName     = Path.Replace("event:/", "").Replace("/"+PathSplit[PathSplit.Length-1], "");
                int GroupIndex          = GetEventGroupIndex(PathSplit[1]);
                int CategoryIndex       = GetCategoryIndex(CategoryName, categoryList);

                //ī�װ��� ������ ī�װ��� ���� �����.
                if (CategoryName.Equals(PathSplit[PathSplit.Length - 1])){

                    CategoryName = "Root";
                    CategoryIndex = GetCategoryIndex(CategoryName, categoryList);
                }

                if (CategoryIndex == -1){

                    categoryList.Add(new FModEventCategoryDesc { CategoryName = CategoryName, EventCount = 1, GroupIndex = GroupIndex });
                    CategoryIndex = (categoryList.Count - 1);
                }
                else categoryList[CategoryIndex] = new FModEventCategoryDesc(categoryList[CategoryIndex], 1);
                groupList[GroupIndex].TotalEvent++;

                int eventStartIndex = GetCategoryEventStartIndex(CategoryName, categoryList);
                refList.Insert( eventStartIndex, new FModEventInfo { Name=Name, Path=Path, GUID=eventRef.Guid } );
            }
            #endregion
        }

        private int GetEventGroupIndex(string RootFolder)
        {
            if (_EditorSettings.EventGroups[0].RootFolderName.Equals(RootFolder)) return 0;
            else if (_EditorSettings.EventGroups[1].RootFolderName.Equals(RootFolder)) return 1;
            else return 2;
        }

        private int GetParamLabelStartIndex(int descIndex) 
        {
            #region Omit
            int total = 0;
            List<FModParamDesc> descs = _EditorSettings.ParamDescList;

            for(int i=0; i<descIndex; i++) 
            {
                total+=descs[i].LableCount;
            }

            return total;
            #endregion
        }

        private void EventGroupIsValid(bool[] groupEventBooleans)
        {
            #region Omit
            if (_EditorSettings == null) return;

            FModGroupInfo[]             groups    = _EditorSettings.EventGroups;
            List<FModEventCategoryDesc> categorys = _EditorSettings.CategoryDescList;
            List<FModEventInfo>         infos     = _EditorSettings.EventRefs;

            groupEventBooleans[0] = groupEventBooleans[1] = groupEventBooleans[2] = true;

            int total = 0;
            int CategoryCount = categorys.Count;

            /************************************
             *   ��� ī�װ��� ��ȸ�Ѵ�.
             * **/
            for(int i=0; i<CategoryCount; i++){

                FModEventCategoryDesc desc = categorys[i];
                desc.EventIsValid = true;

                for(int j=0; j<desc.EventCount; j++)
                {
                    int realIndex = (total+j);
                    bool eventIsValid = CheckEventIsValid(realIndex, infos);

                    //��ȿ���� ���� �ε����� ������ �ִٸ�....
                    if(eventIsValid==false){

                        groupEventBooleans[desc.GroupIndex] = false;
                        desc.EventIsValid = false;
                    }
                }

                categorys[i] = desc;
                total += desc.EventCount;
            }
            #endregion
        }

        private bool CheckEventIsValid(int index, List<FModEventInfo> lists)
        {
            #region Omit
            if (Settings.Instance.EventLinkage == EventLinkage.Path && !lists[index].GUID.IsNull)
            {
                EditorEventRef eventRef = EventManager.EventFromGUID(lists[index].GUID);

                if (eventRef == null || eventRef != null && (eventRef.Path != lists[index].Path))
                {
                    return false;
                }
            }
            return true;

            #endregion
        }

    }
#endif

    #endregion

    #region Define
    private struct FadeInfo
    {
#if FMOD_Event_ENUM
        public FModEventInstance TargetIns;
#endif
        public int   TargetBusIdx;
        public int   FadeID;
        public float duration;
        public float durationDiv;
        public float startVolume;
        public float distance;
        public bool  destroyAtCompleted;
        public bool  pendingKill;
    }

    private struct CallBackInfo
    {
        public FModEventInstance   EventKey;
        public EVENT_CALLBACK_TYPE EventType;
        public int                 ParamKey;
    }

    private struct CallBackRegisterInfo
    {
        public FModEventCallBack Func;
        public bool              UsedDestroyEvent;
    }

    private struct ParamDesc
    {
        public int StartIdx;
        public int Length;
    }

    private enum FadeState
    {
        None,
        PendingKill_Ready,
        PendingKill
    }
    #endregion

    //=======================================
    /////            Property           /////
    ///======================================
    public static bool  UsedBGMAutoFade     { get; set; } = false;
    public static bool  IsInitialized       { get { return _Instance != null; } }
    public static float BGMAutoFadeDuration { get; set; } = 3f;
    public const  int   BGMAutoFadeID = -9324;
    public static FModEventFadeCompleteNotify OnEventFadeComplete;



    //=======================================
    /////            Fields            /////
    ///======================================
    private static FModAudioManager _Instance;
#if FMOD_Event_ENUM
    private FModEventInstance       _BGMIns;
#endif

    private FMOD.Studio.Bus[] _BusList;
 
    /**Fade fields....**/
    private FadeInfo[]   _fadeInfos = new FadeInfo[10];
    private int          _fadeCount = 0;
    private FadeState    _fadeState = FadeState.None;


    /**event callback fields...**/
    private static EVENT_CALLBACK _cbDelegate = new EVENT_CALLBACK(Callback_Internal);
    private static Dictionary<FModEventInstance, CallBackRegisterInfo> _callBackTargets = new Dictionary<FModEventInstance, CallBackRegisterInfo>();

    private static CallBackInfo[] _callbackInfos = new CallBackInfo[10];
    private static int            _callbackCount = 0;

    private static List<ParamDesc>  _paramDescs = new List<ParamDesc>();
    private static byte[]           _paramBytes = new byte[10];
    private static int              _usedBytes  = 0;


    /**Audo bgm fields...**/
    private int     _NextBGMEvent         = -1;
    private float   _NextBGMVolume        = 0f;
    private float   _NextBGMStartPosRatio = 0;
    private int     _NextBGMParam         = -1;
    private float   _NextBGMParamValue    = 0f;
    private Vector3 _NextBGMPosition      = Vector3.zero;



    //=======================================
    /////         Core Methods          /////
    ///======================================
    
    private static bool InstanceIsValid()
    {
        #region Omit
        if (_Instance==null)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("To use the methods provided by FModAudioManager, there must be at least one GameObject in the scene with the FModAudioManager component attached.");
            UnityEngine.Debug.LogWarning("Alternatively, this error might occur if you try to use the methods before FModAudioManager has finished initializing.\nIf this error appears when calling a function in the Awake() magic method, try moving the code to the Start() magic method instead.");
#endif
            return false;
        }

        return true;
        #endregion
    }


    /*************************************************
     *      Callback Methods....
     * *******/
    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT Callback_Internal(EVENT_CALLBACK_TYPE eventType, IntPtr instance, IntPtr ptrParams)
    {
        #region Omit

        /****************************************************************
         *    ������ �������� ����, �ݹ� ������ Update���� ó���ϵ��� �Ѵ�...
         * *****/
        lock(_cbDelegate)
        {
            bool isDestroyedEvent     = (eventType == EVENT_CALLBACK_TYPE.DESTROYED);
            FModEventInstance target  = new FModEventInstance(new EventInstance(instance));
            CallBackRegisterInfo info = _callBackTargets[target];


            /*********************************************************
             *    �̺�Ʈ�� ���� �Ķ���� ����ü ������ ����Ѵ�...
             * *****/

            #region PARAMETERS
            /**MARKER �̺�Ʈ�� ���.....**/
            if (eventType == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<TIMELINE_MARKER_PROPERTIES>(ptrParams);
                TIMELINE_MARKER_PROPERTIESEX newEx = new TIMELINE_MARKER_PROPERTIESEX()
                {
                    MarkerName = parameters.name,
                    TimelinePosition = parameters.position
                };

                PasteStructByte(newEx);
            }

            /**TIMELINE BEAT �̺�Ʈ�� ���...**/
            else if (eventType == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**PROGRAMMER SOUND ���� �̺�Ʈ�� ���...*/
            else if (eventType == EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND || eventType == EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<PROGRAMMER_SOUND_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**PLUGIN INSTANCE ���� �̺�Ʈ�� ���...**/
            else if (eventType == EVENT_CALLBACK_TYPE.PLUGIN_CREATED || eventType == EVENT_CALLBACK_TYPE.PLUGIN_DESTROYED)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<PLUGIN_INSTANCE_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**SOUND ���� �̺�Ʈ�� ���...**/
            else if (eventType == EVENT_CALLBACK_TYPE.SOUND_PLAYED || eventType == EVENT_CALLBACK_TYPE.SOUND_STOPPED)
            {
                var parameter = System.Runtime.InteropServices.Marshal.PtrToStructure<IntPtr>(ptrParams);
                PasteStructByte(parameter);
            }

            /**���� ���� �̺�Ʈ�� ���...**/
            else if (eventType == EVENT_CALLBACK_TYPE.START_EVENT_COMMAND)
            {
                var parameter = System.Runtime.InteropServices.Marshal.PtrToStructure<IntPtr>(ptrParams);
                PasteStructByte(parameter);
            }

            #endregion


            /**********************************************************
             *    �̺�Ʈ ȣ���� �����Ѵ�....
             * *****/
            CallBackInfo newInfo = new CallBackInfo()
            {
                EventKey = target,
                ParamKey = _callbackCount,
                EventType = eventType
            };

            /**�ݹ� ������ ���� �迭�� ����á�ٸ� ��� �Ҵ��Ѵ�...*/
            int len = _callbackInfos.Length;
            if (len <= _callbackCount)
            {
                CallBackInfo[] newArr = new CallBackInfo[len * 2];
                Array.Copy(_callbackInfos, newArr, len);
                _callbackInfos = newArr;
            }

            _callbackInfos[_callbackCount++] = newInfo;
        }

        return FMOD.RESULT.OK;
        #endregion
    }

    private void CallbackProgress_internal()
    {
        #region Omit
        /**********************************************
         *   ��� �ݹ� �������� ó���Ѵ�...
         * *******/
        lock (_cbDelegate)
        {
            try
            {
                for (int i = 0; i < _callbackCount; i++){

                    ref CallBackInfo     info         = ref _callbackInfos[i];
                    EventInstance        ins          = (EventInstance)info.EventKey;
                    CallBackRegisterInfo registerInfo = _callBackTargets[info.EventKey];

                    /**�̺�Ʈ�� �ı��� ���, ���� ��󿡼� ���ܽ�Ų��...**/
                    if (info.EventType == EVENT_CALLBACK_TYPE.DESTROYED)
                    {
                        _callBackTargets.Remove(info.EventKey);
                        if (registerInfo.UsedDestroyEvent == false) continue;
                    }

                    registerInfo.Func?.Invoke(info.EventType, info.EventKey, info.ParamKey);
                }
            }
            catch { }

            _callbackCount = 0;
            _usedBytes     = 0;
            _paramDescs.Clear();
        }

        #endregion
    }

    private static void PasteStructByte<T>(T copyTarget)
    {
        #region Omit
        int size       = Marshal.SizeOf(typeof(T));
        int Targetlen  = _paramBytes.Length;

        /***********************************************
         *    �Ҵ��� ������ �����ϸ� �����Ѵ�....
         * *****/
        if(Targetlen < (_usedBytes + size))
        {
            byte[] newArr = new byte[(Targetlen*2)+size];
            Array.Copy(_paramBytes, newArr, _usedBytes);
            _paramBytes = newArr;
        }


        /************************************************
         *    byte �迭�� ����ü�� �Ҵ��Ѵ�...
         * ******/
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(copyTarget, ptr, false);
        Marshal.Copy(ptr, _paramBytes, _usedBytes, size);
        Marshal.FreeHGlobal(ptr);

        _paramDescs.Add(new ParamDesc()
        {
             Length   = size,
             StartIdx = _usedBytes
        });
        _usedBytes += size;
        #endregion
    }

    private static bool ParamKeyIsValid(int paramKey)
    {
        return (paramKey >=0 || paramKey < _paramDescs.Count);
    }

    private static T GetCallbackParam_internal<T>(int paramKey) where T : struct
    {
        #region Omit

        /**paramKey�� ��ȿ���� �ʴٸ� Ż���Ѵ�....**/
        if(!ParamKeyIsValid(paramKey)){
            return new T();
        }

        /***************************************************************
         *    byte �迭���� ����ü�� ���� �޸𸮸� ������ ����ü�� �����...
         * ******/
        ParamDesc desc = _paramDescs[paramKey];
        IntPtr    ptr  = Marshal.AllocHGlobal(desc.Length);
        T         ret;

        Marshal.Copy(_paramBytes, desc.StartIdx, ptr, desc.Length);
        ret = Marshal.PtrToStructure<T>(ptr);

        Marshal.FreeHGlobal(ptr);
        return ret;

        #endregion
    }

    public static void SetEventCallback(FModEventInstance eventTarget, EVENT_CALLBACK_TYPE eventTypeMask, FModEventCallBack callbackFunc)
    {
        #region Omit
        if (!InstanceIsValid() || !eventTarget.IsValid) return;

        bool usedDestroyEvent  = ((int)(eventTypeMask & EVENT_CALLBACK_TYPE.DESTROYED))>0;
        EventInstance ins      = (EventInstance)eventTarget;
        eventTypeMask         |= EVENT_CALLBACK_TYPE.DESTROYED;

        /**����� �ݹ������� �ʱ�ȭ�Ѵ�....*/
        CallBackRegisterInfo newInfo = new CallBackRegisterInfo()
        {
            Func             = callbackFunc,
            UsedDestroyEvent = usedDestroyEvent
        };

        /**Ű�� �̹� �����Ѵٸ� ���� �����Ѵ�....**/
        if (_callBackTargets.ContainsKey(eventTarget))
        {
            _callBackTargets[eventTarget] = newInfo;
        }

        /**Ű�� �������� �ʴٸ� ���Ӱ� �߰��Ѵ�...*/
        else
        {
            _callBackTargets.Add(eventTarget, newInfo);
        }

        ins.setCallback(_cbDelegate, eventTypeMask);
        #endregion
    }

    public static void SetBGMEventCallback(EVENT_CALLBACK_TYPE eventTypeMask, FModEventCallBack callbackFunc)
    {
#if FMOD_Event_ENUM
        if (!InstanceIsValid()) return;
        SetEventCallback(_Instance._BGMIns, eventTypeMask, callbackFunc);
#endif
    }

    public static void ClearEventCallback(FModEventInstance eventTarget)
    {
        #region Omit
        if (!InstanceIsValid() || !eventTarget.IsValid) return;

        _callBackTargets.Remove(eventTarget);

        EventInstance ins = (EventInstance)eventTarget;
        ins.setCallback(null);
        #endregion
    }

    public static void ClearBGMEventCallback()
    {
#if FMOD_Event_ENUM
        if (!InstanceIsValid()) return;
        ClearEventCallback(_Instance._BGMIns);
#endif
    }

    public static TIMELINE_MARKER_PROPERTIESEX GetCallbackParams_Marker(int parameterKey)
    {
        TIMELINE_MARKER_PROPERTIESEX ret = GetCallbackParam_internal<TIMELINE_MARKER_PROPERTIESEX>(parameterKey);

        if(ParamKeyIsValid(parameterKey)){
            ret.TimelinePositionRatio = ((float)ret.TimelinePosition / (float)_callbackInfos[parameterKey].EventKey.Length);
        }

        return ret;
    }

    public static TIMELINE_BEAT_PROPERTIES GetCallbackParams_Beat(int parameterKey)
    {
        return GetCallbackParam_internal<TIMELINE_BEAT_PROPERTIES>(parameterKey);
    }

    public static PROGRAMMER_SOUND_PROPERTIES GetCallbackParams_ProgrammerSound(int parameterKey)
    {
        return GetCallbackParam_internal<PROGRAMMER_SOUND_PROPERTIES>(parameterKey);
    }

    public static PLUGIN_INSTANCE_PROPERTIES GetCallbackParams_PluginInstance(int parameterKey)
    {
        return GetCallbackParam_internal<PLUGIN_INSTANCE_PROPERTIES>(parameterKey);
    }

    public static Sound GetCallbackParams_Sound(int parameterKey)
    {
        return GetCallbackParam_internal<Sound>(parameterKey);
    }

    public static FModEventInstance GetCallbackParams_StartEventCommand(int parameterKey)
    {
        return new FModEventInstance(GetCallbackParam_internal<EventInstance>(parameterKey));
    }

    public static TIMELINE_NESTED_BEAT_PROPERTIES GetCallbackParams_NestedBeat(int parameterKey)
    {
        return GetCallbackParam_internal<TIMELINE_NESTED_BEAT_PROPERTIES>(parameterKey);
    }



#if FMOD_Event_ENUM
    /*****************************************
     *   Bus Methods
     * ***/
    public static void SetBusVolume(FModBusType busType, float newVolume)
     {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.setVolume(newVolume);
        #endregion
    }

    public static float GetBusVolume(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return 0;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return 0;

        float volume;
        bus.getVolume(out volume);
        return volume;
        #endregion
    }

    public static void StopBusAllEvents(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        #endregion
    }

    public static void SetBusMute(FModBusType busType, bool isMute)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;

        bus.setMute(isMute);
        #endregion
    }

    public static bool GetBusMute(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return false;

        bool isMute;
        bus.getMute(out isMute);
        return isMute;
        #endregion
    }

    public static void SetAllBusMute(bool isMute)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int Count = _Instance._BusList.Length;
        for(int i=0; i<Count; i++)
        {
            if (!_Instance._BusList[i].isValid()) continue;
            _Instance._BusList[i].setMute(isMute);
        }
        #endregion
    }


    /****************************************
     *   Bank Methods
     * ****/
    public static void LoadBank(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string bankName = FModReferenceList.Banks[(int)bankType];
        try
        {
            FMODUnity.RuntimeManager.LoadBank(bankName);
        }
        catch 
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.LoadBank(...)!!");
            #endif
        }
        #endregion
    }

    public static void UnloadBank(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            string bankName = FModReferenceList.Banks[(int)bankType];
            FMODUnity.RuntimeManager.UnloadBank(bankName);
        }
        catch
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.UnLoadBank(...)!!");
            #endif
        }
        #endregion
    }

    public static bool BankIsLoaded(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        string bankName = FModReferenceList.Banks[(int)bankType];
        return FMODUnity.RuntimeManager.HasBankLoaded(bankName);
        #endregion
    }

    public static void LoadAllBank()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //��� ��ũ�� �ε��Ѵ�.
        for(int i=0; i<Count; i++){

            if (FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            try { FMODUnity.RuntimeManager.LoadBank(bankLists[i]); } catch { continue; }
        }
        #endregion
    }

    public static void UnLoadAllBank()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //��� ��ũ�� ��ε��Ѵ�.
        for (int i = 0; i < Count; i++)
        {
            if (!FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            try { FMODUnity.RuntimeManager.UnloadBank(bankLists[i]); } catch { continue; }
        }
        #endregion
    }


    /******************************************
     *   FModEventInstance Methods
     * **/
    public static FModEventInstance CreateInstance(FModBGMEventType eventType, Vector3 position=default)
    {
        #region Omit
        if (!InstanceIsValid()) return new FModEventInstance();

        try
        {
            FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid), position);
            return newInstance;
        }
        catch {

#if UNITY_EDITOR
            UnityEngine.Debug.Log("FModAudioManager.CreateInstance() failed! Please check if the Bank with the event you want to use has been loaded.");
#endif
            return new FModEventInstance(); 
        }
        #endregion
    }

    public static FModEventInstance CreateInstance(FModSFXEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static FModEventInstance CreateInstance(FModNoGroupEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static FModEventInstance CreateInstance(FMODUnity.EventReference eventRef, Vector3 position = default)
    {
        #region Omit
        if (!InstanceIsValid()) return new FModEventInstance();

        try
        {
            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(eventRef.Guid), position);
            return newInstance;
        }
        catch { return new FModEventInstance(); }
        #endregion
    }

    public static void StopAllInstance()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        FMOD.Studio.Bus[] busLists = _Instance._BusList;
        int Count = busLists.Length;

        for(int i=0; i<Count; i++){

            busLists[i].stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        #endregion
    }


    /*********************************************
     *  PlayOneShot Methods
     * ***/
    private static void PlayOneShotSFX_internal(FModSFXEventType eventType, Vector3 position, float volume, float startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, float minDistance, float maxDistance)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
            bool volumeIsChanged = (volume >= 0f);
            bool paramIsChanged  = (paramType != -1);

            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid));
            newInstance.Set3DDistance(minDistance, maxDistance);
            newInstance.Position              = position;
            newInstance.TimelinePositionRatio = startTimelinePositionRatio;
            if (volumeIsChanged) newInstance.Volume = volume;
            if (paramIsChanged)
            {
                if (isGlobal) newInstance.SetParameter((FModGlobalParamType)paramType, paramValue);
                else newInstance.SetParameter((FModLocalParamType)paramType, paramValue);
            }
            newInstance.Play();
            newInstance.Destroy(true);
        }
        catch 
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.PlayOneShotSFX(...)!!");
            #endif
        }
        #endregion
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModGlobalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModParameterReference paramRef, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**�۷ι� �Ķ������ ���...*/
        if (paramRef.IsGlobal){

            PlayOneShotSFX( eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance,maxDistance);
            return;
        }

        /**���� �Ķ������ ���...*/
        PlayOneShotSFX(eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
        #endregion
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModParameterReference paramRef, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**�۷ι� �Ķ������ ���...*/
        if (paramRef.IsGlobal)
        {

            PlayOneShotSFX((FModSFXEventType)eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
            return;
        }

        /**���� �Ķ������ ���...*/
        PlayOneShotSFX((FModSFXEventType)eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
        #endregion
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f,  float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FMODUnity.EventReference eventRef, Vector3 position, FModParameterReference paramRef =default, float volume=-1f, float startTimelinePositionRatio = 0f, float minDistance=1f, float maxDistance=20f)
    {
        #region Omit
        /**************************************
         *   �ش� GUID�� ���� �ε������� ��´�...
         * ***/
        int Count = FModReferenceList.Events.Length;
        int       index = -1;
        FMOD.GUID guid  = eventRef.Guid;
        for(int i=0; i<Count; i++)
        {
            if (FModReferenceList.Events[i].Equals(guid)){

                index = i;
                break;
            }
        }

        /**�������� �ʴ� �̺�Ʈ�� ������ �� ����...*/
        if (index == -1) return;

        /**********************************************
         *   �Ķ���Ͱ��� ���� ������ �����ε��� ȣ���Ѵ�...
         * ***/
        PlayOneShotSFX_internal((FModSFXEventType)index, position, volume, startTimelinePositionRatio, paramRef.IsGlobal, paramRef.ParamType, paramRef.ParamValue, minDistance, maxDistance);
        #endregion
    }


    /*********************************************
     *   BGM Methods
     * ***/
    private static void PlayBGM_internal(FModBGMEventType eventType, float volume, float startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, Vector3 position)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            bool volumeIsChanged = (volume >= 0f);
            bool paramIsChanged = (paramType != -1);

            //������ BGM �ν��Ͻ��� ������ ���
            if (_Instance._BGMIns.IsValid)
            {
                if (UsedBGMAutoFade)
                {
                    _Instance._NextBGMEvent         = (int)eventType;
                    _Instance._NextBGMVolume        = volume;
                    _Instance._NextBGMStartPosRatio = startTimelinePositionRatio;
                    _Instance._NextBGMParam         = (int)paramType;
                    _Instance._NextBGMParamValue    = paramValue;
                    _Instance._NextBGMPosition      = position;

                    StopFade(BGMAutoFadeID);
                    ApplyBGMFade(0f, BGMAutoFadeDuration * .5f, BGMAutoFadeID, true);
                    return;
                }
                else _Instance._BGMIns.Destroy();
            }

            _Instance._BGMIns                       = CreateInstance(eventType, position);
            _Instance._BGMIns.Position              = position;
            _Instance._BGMIns.TimelinePositionRatio = startTimelinePositionRatio;
            if (paramIsChanged)
            {
                if (isGlobal) _Instance._BGMIns.SetParameter((FModGlobalParamType)paramType, paramValue);
                else _Instance._BGMIns.SetParameter((FModLocalParamType)paramType, paramValue);
            }

            //���̵带 �����ϸ鼭 ������ ���
            if (UsedBGMAutoFade)
            {
                float newVolume = (volumeIsChanged ? volume : _Instance._BGMIns.Volume);
                _Instance._NextBGMEvent = -1;
                _Instance._BGMIns.Volume = 0f;
                ApplyBGMFade(newVolume, BGMAutoFadeDuration, BGMAutoFadeID);
            }
            else if (volumeIsChanged) _Instance._BGMIns.Volume = volume;

            _Instance._BGMIns.Play();
        }
        catch
        {
           #if UNITY_EDITOR
           UnityEngine.Debug.LogWarning("failed FModAudioManager.PlayBGM(...)!!");
           #endif
        }
        #endregion
    }

    public static void PlayBGM(FModBGMEventType eventType, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, false,  (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModGlobalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModParameterReference paramRef, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**�۷ι� �Ķ������ ���...*/
        if(paramRef.IsGlobal)
        {
            PlayBGM(eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        }

        /**���� �Ķ������ ���...*/
        PlayBGM(eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        #endregion
    }

    public static void PlayBGM(FModNoGroupEventType eventType, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModParameterReference paramRef, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**�۷ι� �Ķ������ ���...*/
        if (paramRef.IsGlobal)
        {
            PlayBGM((FModBGMEventType)eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        }

        /**���� �Ķ������ ���...*/
        PlayBGM((FModBGMEventType)eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        #endregion
    }

    public static void PlayBGM(FMODUnity.EventReference eventRef, FModParameterReference paramRef=default, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        /**************************************
         *   �ش� GUID�� ���� �ε������� ��´�...
         * ***/
        int Count = FModReferenceList.Events.Length;
        int index = -1;
        FMOD.GUID guid = eventRef.Guid;
        for (int i = 0; i < Count; i++)
        {
            if (FModReferenceList.Events[i].Equals(guid)){

                index = i;
                break;
            }
        }

        /**�������� �ʴ� �̺�Ʈ�� ������ �� ����...*/
        if (index == -1) return;

        PlayBGM_internal((FModBGMEventType)index, volume, startTimelinePositionRatio, paramRef.IsGlobal, paramRef.ParamType, paramRef.ParamValue, position);
        #endregion
    }

    public static void StopBGM()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;

        if(UsedBGMAutoFade)
        {
            _Instance._NextBGMEvent = -1;

            StopFade(BGMAutoFadeID);
            ApplyBGMFade(0f, BGMAutoFadeDuration, BGMAutoFadeID, true);
            return;
        }

        _Instance._BGMIns.Stop();
        #endregion
    }

    public static void DestroyBGM(bool destroyAtStop = false)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Destroy(destroyAtStop);
        #endregion
    }

    public static void SetBGMPause()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Pause();
        #endregion
    }

    public static void SetBGMResume()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Resume();
        #endregion
    }

    public static void SetBGMVolume(float newVolume)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Volume = newVolume;
        #endregion
    }

    public static float GetBGMVolume()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.Volume;
        #endregion
    }

    public static void SetBGMParameter(FModGlobalParamType paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(FModLocalParamType paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(FModParameterReference paramRef, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || paramRef.IsValid == false) return;

        /**�۷ι� �Ķ������ ���....*/
        if(paramRef.IsGlobal)
        {
            SetBGMParameter( (FModGlobalParamType)paramRef.ParamType, paramValue);
            return;
        }

        /**���� �Ķ������ ���...*/
        SetBGMParameter( (FModLocalParamType)paramRef.ParamType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(string paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static float GetBGMParameter(FModGlobalParamType paramType) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
        #endregion
    }

    public static float GetBGMParameter(FModLocalParamType paramType) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
        #endregion
    }

    public static float GetBGMParameter(string paramName) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramName);
        #endregion
    }

    public static void SetBGMTimelinePosition(int timelinePositionMillieSeconds)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePosition= timelinePositionMillieSeconds;
        #endregion
    }

    public static void SetBGMTimelinePosition(float timelinePositionRatio)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePositionRatio = timelinePositionRatio;
        #endregion
    }

    public static int GetBGMTimelinePosition()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0;
        return _Instance._BGMIns.TimelinePosition;
        #endregion
    }

    public static float GetBGMTimelinePositionRatio()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.TimelinePositionRatio;
        #endregion
    }


    /********************************************
     *   Fade Methods
     * ***/
    public static void ApplyBusFade(FModBusType busType, float goalVolume, float fadeTime, int fadeID=0)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        _Instance.AddFadeInfo(new FModEventInstance(), goalVolume, fadeTime, fadeID, false, (int)busType);
        #endregion
    }

    public static void ApplyInstanceFade(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        #region Omit
        if (!InstanceIsValid() || Instance.IsValid==false) return;

        _Instance.AddFadeInfo(Instance, goalVolume, fadeTime, fadeID, completeDestroy);
        #endregion
    }

    public static void ApplyBGMFade(float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;

        _Instance.AddFadeInfo(_Instance._BGMIns, goalVolume, fadeTime, fadeID, completeDestroy);
        #endregion
    }

    public static bool FadeIsPlaying(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        for(int i=0; i<_Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) return true;
        }

        return false;
        #endregion
    }

    public static int GetFadeCount(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return 0;

        int total = 0;
        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) total++;
        }

        return total;
        #endregion
    }

    public static void StopFade(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID)
            {
                _Instance.RemoveFadeInfo(i);
                if(_Instance._fadeState==FadeState.None) i--;
            }
        }
        #endregion
    }

    public static void StopAllFade()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        _Instance._fadeCount = 0;
        #endregion
    }

    private void AddFadeInfo(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID, bool completeDestroy, int busType=-1)
    {
        #region Omit
        //FadeInfo �迭�� �������� �ʴ´ٸ� ��ŵ.
        if (_fadeInfos == null) return;


        //���ο� ���̵� ������ �� ������ �����ϴٸ� ��� �Ҵ�.
        int containerCount = _fadeInfos.Length;
        if( (_fadeCount) >= containerCount)
        {
            FadeInfo[] temp = _fadeInfos;
            _fadeInfos = new FadeInfo[containerCount*2];
            Array.Copy(temp, 0, _fadeInfos, 0, containerCount);
        }

        //���ο� ���̵� ������ �߰��Ѵ�.
        float startVolume = (busType >= 0 ? GetBusVolume((FModBusType)busType) : Instance.Volume);

        _fadeInfos[_fadeCount++] = new FadeInfo()
        {
            startVolume  = startVolume,
            distance     = ( goalVolume-startVolume),
            duration     = fadeTime,
            durationDiv  = (1f / fadeTime),
            FadeID       = fadeID,
            TargetIns    = Instance,
            pendingKill  = false,
            TargetBusIdx = busType,
            destroyAtCompleted = completeDestroy,
        };

        #endregion
    }

    private void RemoveFadeInfo(int index)
    {
        #region Omit
        if (_fadeInfos == null) return;

        //PendingKill Ready���, PendingKill���·� ��ȯ.
        if(_fadeState!=FadeState.None){

            _fadeState= FadeState.PendingKill;
            _fadeInfos[index].pendingKill = true;
            return;
        }

        _fadeInfos[index] = _fadeInfos[_fadeCount - 1];
        _fadeCount--;
        #endregion
    }

    private void FadeProgress_internal()
    {
        #region Omit
        float DeltaTime = Time.deltaTime;
        int fadeCount   = _fadeCount;

        /****************************************
         *    ��� ���̵� ������ ������Ʈ�Ѵ�...
         * *****/
        _fadeState = FadeState.PendingKill_Ready;
        for (int i = 0; i < fadeCount; i++)
        {
            ref FadeInfo info = ref _fadeInfos[i];

            //���̵� ��� �ν��Ͻ��� ��ȿ���� ���� ���, PendingKill���¸� ����.
            if (info.TargetIns.IsValid == false && info.TargetBusIdx<0){

                info.pendingKill = true;
                _fadeState = FadeState.PendingKill;
                continue;
            }

            info.duration      -= DeltaTime;
            float fadeTimeRatio = Mathf.Clamp(1f - (info.duration * info.durationDiv), 0f, 1f);
            float finalVolume   = info.startVolume + (fadeTimeRatio * info.distance);



            /*********************************************
             *    ���� ���̵� ��� ���� ������ �����Ѵ�..
             * ******/

            /**������ ���...*/
            if (info.TargetBusIdx>0){
                SetBusVolume((FModBusType)info.TargetBusIdx, finalVolume);
            }

            /**�ν��Ͻ��� ���...*/
            else info.TargetIns.Volume = finalVolume;



            /*********************************************
             *   ���̵� ������ �ܰ�...
             * ******/
            if (fadeTimeRatio < 1f) continue;
            OnEventFadeComplete?.Invoke(info.FadeID, finalVolume);

            /***������ �ܰ� �ı� ����.***/
            if (info.destroyAtCompleted && info.TargetBusIdx<0){

                info.TargetIns.Destroy();
            }

            info.pendingKill = true;
            _fadeState = FadeState.PendingKill;
        }



        /************************************************
         *   PendingKill ���¸� ó���Ѵ�.
         * ****/
        if (_fadeState == FadeState.PendingKill)
        {
            _fadeState = FadeState.None;

            for (int i = 0; i < fadeCount; i++){

                if (!_fadeInfos[i].pendingKill) continue;

                RemoveFadeInfo(i);
                fadeCount--;
                i--;
            }
        }

        #endregion
    }

    private void BGMFadeComplete(int fadeID, float goalVolume)
    {
        #region Omit
        if (fadeID != BGMAutoFadeID ) return;
        if (goalVolume <= 0f) _BGMIns.Destroy();
        if(_NextBGMEvent >=0) PlayBGM((FModBGMEventType)_NextBGMEvent, (FModLocalParamType)_NextBGMParam, _NextBGMParamValue, _NextBGMVolume, _NextBGMStartPosRatio, _NextBGMPosition);
        #endregion
    }



    //=======================================
    /////        Magic Methods           /////
    ///======================================
    private void Awake()
    {
        #region Omit
        if (_Instance == null)
        {
            /*******************************************
             *     �ʱ�ȭ ������ �����Ѵ�....
             * *****/
            _Instance = this;
            DontDestroyOnLoad(gameObject);

            _callbackCount = 0;
            _usedBytes     = 0;
            _callbackInfos = new CallBackInfo[10];
            _callBackTargets.Clear();
            _paramDescs.Clear();

            #if FMOD_Event_ENUM
            OnEventFadeComplete += BGMFadeComplete;


            /*******************************************
             *    ��� ���� ��ϵ��� ���´�.....
             * *****/
            int busCount = FModReferenceList.BusPaths.Length;
            _BusList = new Bus[busCount];

            for (int i = 0; i < busCount; i++){

                string busName = FModReferenceList.BusPaths[i];
                try { _BusList[i] = FMODUnity.RuntimeManager.GetBus(busName); } catch { continue; }
            }
            #endif

            return;
        }

        Destroy(gameObject);
        #endregion
    }

    private void OnDestroy()
    {
        #region Omit
        //Destroy...
        if (_Instance==this){

            _Instance = null;
            OnEventFadeComplete = null;
        }
        #endregion
    }

    private void Update()
    {
        #region Omit
        /****************************************
         *   ó���� ���̵� ������ ���� ���...
         * *****/
        if (_fadeCount > 0) { 
            FadeProgress_internal();
        }


        /***************************************
         *   ó���� �ݹ� ������ ���� ���....
         * *****/
        if (_callbackCount > 0){
            CallbackProgress_internal();
        }
        #endregion
    }

#endif

}