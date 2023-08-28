using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using System;
using FMOD;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System.Xml;
using System.Diagnostics.Tracing;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.PackageManager;
using UnityEditor.Rendering;
using static UnityEditor.PlayerSettings;
using static UnityEditor.ObjectChangeEventStream;
#endif

#region Define_FModEventInstance
#if FMOD_Event_ENUM

public struct FModEventInstance
{
    //==================================
    ////     Property And Fields   ///// 
    //==================================
    public FMOD.GUID    GUID
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
    public bool         IsPaused 
    { 
        get {

            bool ret;
            Ins.getPaused(out ret);
            return ret;
        } 
    }
    public bool         IsLoop
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            bool isOneShot;
            desc.isSnapshot(out isOneShot);
            return isOneShot;
        }
    }
    public bool         Is3DEvent
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
    public Vector3      Position
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
    public bool         IsValid{ get{ return Ins.isValid(); } }
    public float        Volume
    {
        get
        {

            float volume;
            Ins.getVolume(out volume);
            return volume;
        }

        set { Ins.setVolume((value < 0 ? 0f : value)); }
    }
    public GameObject   AttachedGameObject
    {
        get { return _AttachedGameObject; }

        set {

            if (value == null) FMODUnity.RuntimeManager.DetachInstanceFromGameObject(Ins);
            else FMODUnity.RuntimeManager.AttachInstanceToGameObject(Ins, value.transform); 
        
            _AttachedGameObject = value;
        }
    }
    public float        Pitch
    {
        get
        {
            float pitch;
            Ins.getPitch(out pitch);
            return pitch;
        }

        set { Ins.setPitch(value); }
    }
    public bool         IsPlaying
    {
        get
        {
            FMOD.Studio.PLAYBACK_STATE state;
            Ins.getPlaybackState(out state);
            return (state == FMOD.Studio.PLAYBACK_STATE.PLAYING);
        }

    }
    public int          TimelinePosition
    {
        get
        {
            int position;
            Ins.getTimelinePosition(out position);
            return position;
        }

        set{  Ins.setTimelinePosition(value); }
    }
    public float        TimelinePositionRatio
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);

            int position;
            Ins.getTimelinePosition(out position);

            return (position/length);
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

    private EventInstance Ins;
    private GameObject _AttachedGameObject;

    //==================================
    ////        Public Methods     ///// 
    //==================================
    public FModEventInstance(EventInstance instance, Vector3 position=default) 
    { 
        Ins = instance;
        _AttachedGameObject = null;

        bool is3D;
        FMOD.Studio.EventDescription desc;
        Ins.getDescription(out desc);
        desc.is3D(out is3D);
        if (is3D)
        {
            Ins.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        }
    }

    public void Play(float volume = -1f, int startTimelinePosition = -1, string paramName = "", float paramValue = 0f)
    {
        if(volume>=0) Ins.setVolume(volume);
        if (startTimelinePosition >= 0) Ins.setTimelinePosition(startTimelinePosition);
        if(paramName!="") Ins.setParameterByName(paramName, paramValue);

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

    public void Destroy(bool destroyAtComplete=false)
    {
        if(destroyAtComplete)
        {
            Ins.release();
            return;
        }

        Ins.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Ins.release();
        Ins.clearHandle();
    }

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

    public void SetParameter(string paramName, float value)
    {
        Ins.setParameterByName(paramName, value);
    }

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

    //TODO: 
    public void SetCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
    {
        Ins.setCallback(callback, callbackmask);
    }

}
#endif
#endregion

public interface IFModEventFadeComplete { void OnFModEventComplete(int fadeID, float goalVolume); }
public delegate void FModEventFadeCompleteNotify( int fadeID, float goalVolume );

public sealed class FModAudioManager : MonoBehaviour
{
    #region Editor_Extension
    /********************************
     * ������ Ȯ���� ���� private class
     ***/
#if UNITY_EDITOR
    private sealed class FModAudioManagerWindow : EditorWindow
    {
        //=====================================
        ////            Fields           ///// 
        //====================================

        /*************************************
         *   Editor Data Path String...
         * **/
        private const string _DataScriptPath     = "Assets/Plugins/FMOD/src/FMODAudioManagerDefine.cs";
        private const string _EditorSettingsPath = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
        private const string _StudioSettingsPath = "Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset";
        private const string _GroupFolderPath    = "Metadata/Group";
        private const string _ScriptDefine       = "FMOD_Event_ENUM";
        private const string _EditorVersion      = "v1.230817";

        private const string _EventRootPath      = "event:/";
        private const string _BusRootPath        = "bus:/";
        private const string _BankRootPath       = "bank:/";

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
        private Regex   _regex     = new Regex(@"[^a-zA-Z0-9_]");
        private Vector2 _Scrollpos = Vector2.zero;

        /** Categorys... *************************/
        private static readonly string[] _EventGroups = new string[] { "BGM", "SFX", "NoGroup" };
        private static readonly string[] _ParamGroups = new string[] { "Global Parameters", "Local Parameters" };
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
        private string _CountColorStyle = "#8DFF9E";
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
            using(var scope = new GUILayout.AreaScope(new Rect(position.width*.5f-5f, 100f - 20, 300, 30))){

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
                    if (GUILayout.Button("Save and Apply Settings", GUILayout.Width(position.width * .5f))){

                        CreateEnumScript();
                        if (_EditorSettings != null)
                        {
                            EditorUtility.SetDirty(_EditorSettings);
                        }

                        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, _ScriptDefine);
                        AssetDatabase.Refresh();
                    }
                }

                //FMod Studio ������ �ҷ�����
                if(GUILayout.Button("Loaded Studio Settings", GUILayout.Width(position.width*.5f)))
                {

                    if (_EditorSettings!=null){

                        GetBusList(_EditorSettings.BusList);
                        GetBankList(_EditorSettings.BankList);
                        GetParamList(_EditorSettings.ParamDescList, _EditorSettings.ParamLableList);
                        GetEventList(_EditorSettings.CategoryDescList, _EditorSettings.EventRefs, _EditorSettings.EventGroups);
                    }
                }



                scope.Dispose();
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
            int Count = bankList.Count;

            EditorGUI.indentLevel++;
            _BankSettingsFade.target = EditorGUILayout.Foldout(_BankSettingsFade.target, $"FMod Banks<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
            EditorGUILayout.Space(3f);


            using (var fadeScope = new EditorGUILayout.FadeGroupScope(_BankSettingsFade.faded))
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

                    if(Count>0) EditorGUILayout.HelpBox("An FModBankType enum is created based on the information shown below.", MessageType.Info);

                    //��� ��ũ ��ϵ��� �����ش�.
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < Count; i++){

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{bankList[i].Name}</color>", _ContentTxtStyle, GUILayout.Width(150));
                            EditorGUILayout.TextArea(bankList[i].Path, _TxtFieldStyle, pathWidthOption, pathHeightOption);
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
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            FModParamDesc desc = descs[i];

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

                                int realIndex      = (startIndex+j);
                                int groupIndex     = (category.GroupIndex);
                                bool isValid       = CheckEventIsValid(realIndex, infos);
                                FModEventInfo info = infos[realIndex];
                                GUIStyle usedStyle = (isValid?_TxtFieldStyle:_TxtFieldErrorStyle);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.TextField(info.Name, $"{info.Path}", usedStyle, GUILayout.Width(position.width-50f));
                                EditorGUILayout.EndHorizontal();
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
        private void ResetEditorSettings()
        {
            #region Omit
            if (_EditorSettings == null) return;

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
            #endregion
        }

        private void CreateEnumScript()
        {
            #region Ommision
            if (_EditorSettings == null) return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("");


            /**************************************
             * Bus Enum ����.
             *****/
            builder.AppendLine("public enum FModBusType");
            builder.AppendLine("{");

            List<NPData> busLists = _EditorSettings.BusList;
            int Count = busLists.Count;
            for(int i=0; i<Count; i++)
            {
                string busName  = RemoveInValidChar(busLists[i].Name);
                string comma    = (i == Count - 1 ? "" : ",");
                builder.AppendLine($"   {busName}={i}{comma}");
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /********************************************
             *  Bank Enum ����
             ***/
            builder.AppendLine("public enum FModBankType");
            builder.AppendLine("{");

            List<EditorBankRef> bankList = FMODUnity.EventManager.Banks;

            Count = bankList.Count;
            for (int i = 0; i < Count; i++)
            {
                EditorBankRef bank = bankList[i];
                string bankName = RemoveInValidChar(bank.Name);
                string comma = (i == Count - 1 ? "" : ",");
                builder.AppendLine($"   {bankName}={i}{comma}");
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /*******************************************
             *  Global Parameter Enum ����
             * ***/
            builder.AppendLine("public enum FModGlobalParamType");
            builder.AppendLine("{");
            builder.AppendLine("   None_Parameter =-1,");

            List<FModParamDesc> paramDescs = _EditorSettings.ParamDescList;

            Count = paramDescs.Count;
            for(int i=0; i<Count; i++)
            {
                FModParamDesc desc = paramDescs[i];
                if (desc.isGlobal == false) continue;

                string comma    = (i == Count - 1 ? "" : ",");
                string enumName = RemoveInValidChar(desc.ParamName);
                builder.AppendLine($"   {enumName}={i}{comma}");
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /*******************************************
             *  Local Parameter Enum ����
             * ***/
            builder.AppendLine("public enum FModLocalParamType");
            builder.AppendLine("{");
            builder.AppendLine($"   None_Parameter =-1,");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++)
            {
                FModParamDesc desc = paramDescs[i];
                if (desc.isGlobal) continue;

                string comma    = (i == Count - 1 ? "" : ",");
                string enumName = RemoveInValidChar(desc.ParamName);
                builder.AppendLine($"   {enumName}={i}{comma}");
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /**********************************************
             *   Param Lable Struct ����
             * *****/
            builder.AppendLine("public struct FModParamLabel");
            builder.AppendLine("{");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++) {

                FModParamDesc desc = paramDescs[i];
                if(desc.LableCount<=0) continue;

                string structName = RemoveInValidChar(desc.ParamName);

                builder.AppendLine($"    public struct {structName}");
                builder.AppendLine("    {");

                AddParamLabelListScript(builder, i);

                builder.AppendLine("    }");
            }


            builder.AppendLine("}");
            builder.AppendLine("");


            /**********************************************
             *   Param Range Struct ����
             * *****/
            builder.AppendLine("public struct FModParamValueRange");
            builder.AppendLine("{");

            Count = paramDescs.Count;
            for (int i = 0; i < Count; i++){

                FModParamDesc desc = paramDescs[i];
                if (desc.LableCount <= 0) continue;

                string structName = RemoveInValidChar(desc.ParamName);

                builder.AppendLine($"    public struct {structName}");
                builder.AppendLine("    {");

                AddParamRangeListScript(builder, i);

                builder.AppendLine("    }");
            }


            builder.AppendLine("}");
            builder.AppendLine("");


            /**************************************
             * BGM Events Enum ����
             ***/
            int     total           = 0;
            float   writeEventCount = 0;
            List<FModEventCategoryDesc> categoryDescs = _EditorSettings.CategoryDescList;
            List<FModEventInfo>         infos         = _EditorSettings.EventRefs;
            Count = _EditorSettings.CategoryDescList.Count;

            builder.AppendLine("public enum FModBGMEventType");
            builder.AppendLine("{");

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

                    int realIndex = (total + j);
                    string comma = (++writeEventCount == _EditorSettings.EventGroups[0].TotalEvent ? "" : ",");
                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    builder.AppendLine($"   {infos[realIndex].Name}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /**************************************
             *   SFX Events Enum ����
             * ****/
            builder.AppendLine("public enum FModSFXEventType");
            builder.AppendLine("{");

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
                    int realIndex = (total + j);
                    string comma = (++writeEventCount == _EditorSettings.EventGroups[1].TotalEvent ? "" : ",");
                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    builder.AppendLine($"   {infos[realIndex].Name}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /**************************************
             *   NoGroups Events Enum ����
             * ****/
            builder.AppendLine("public enum FModNoGroupEventType");
            builder.AppendLine("{");

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
                    int realIndex = (total + j);
                    string comma = (++writeEventCount == _EditorSettings.EventGroups[2].TotalEvent ? "" : ",");
                    if (CheckEventIsValid(realIndex, infos) == false) continue;
                    builder.AppendLine($"   {infos[realIndex].Name}={realIndex}{comma}");
                }

                total += desc.EventCount;
            }

            builder.AppendLine("}");
            builder.AppendLine("");


            /***************************************
             * Event Reference List class ����
             ***/
            builder.AppendLine("public sealed class FModReferenceList");
            builder.AppendLine("{");
            builder.AppendLine("    public static readonly FMOD.GUID[] Events = new FMOD.GUID[]");
            builder.AppendLine("    {");
            AddEventListScript(builder, _EditorSettings.EventRefs); 
            builder.AppendLine("    };");
            builder.AppendLine("");

            builder.AppendLine("    public static readonly string[] Banks = new string[]");
            builder.AppendLine("    {");
            AddBankListScript(builder);
            builder.AppendLine("    };");
            builder.AppendLine("");


            builder.AppendLine("    public static readonly string[] Params = new string[]");
            builder.AppendLine("    {");
            AddParamListScript(builder);
            builder.AppendLine("    };");
            builder.AppendLine("");

            builder.AppendLine("    public static readonly string[] BusPaths = new string[]");
            builder.AppendLine("    {");
            AddBusPathListScript(builder);
            builder.AppendLine("    };");
            builder.AppendLine("");

            builder.AppendLine("}");
            builder.AppendLine("");


            //���� �� ���ΰ�ħ
            File.WriteAllText(_DataScriptPath, builder.ToString());

            #endregion
        }

        private void AddBusPathListScript(StringBuilder builder)
        {
            #region Omit
            List<NPData> list = _EditorSettings.BusList;
            int Count = list.Count;
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
            //���� ���� ����
            int removeCount = Regex.Match(inputString, @"^\d*").Length;
            inputString = inputString.Substring(removeCount, inputString.Length-removeCount);

            //������� ���ϴ� Ư������ ����
            inputString = _regex.Replace(inputString, "_");

            //���鹮�� ����.
            inputString = inputString.Replace(" ", "_");

            return inputString;
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
                    Name = bankName,
                    Path = bankPath
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
                    string idNode = document.SelectSingleNode("//object/@id")?.InnerText;
                    string nameNode = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                    string outputNode = document.SelectSingleNode("//object/relationship[@name='output']/destination")?.InnerText;

                    if (idNode != null && nameNode != null && outputNode != null)
                    {

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

        private void GetParamList(List<FModParamDesc> descList, List<string> lableList)
        {
            #region Omit
            if (descList == null || lableList == null) return;
            descList.Clear();
            lableList.Clear();

            List<EditorEventRef> EventRefs = FMODUnity.EventManager.Events;
            _EditorSettings.ParamCountList[0] = _EditorSettings.ParamCountList[1] = 0;

            int eventCount = EventRefs.Count;
            for(int i=0; i<eventCount; i++)
            {
                EditorEventRef eventRef = EventRefs[i];
                List<EditorParamRef> paramRef = eventRef.Parameters;
                int paramCount = paramRef.Count;

                for(int j=0; j<paramCount; j++)
                {
                    EditorParamRef param = paramRef[j];
                    int labelCount = (param.Labels == null ? 0 : param.Labels.Length);

                    if (GetParamIsAlreadyContain(param.Name, descList)) continue;

                    descList.Add(new FModParamDesc()
                    {
                        Max = param.Max,
                        Min = param.Min,
                        LableCount = labelCount,
                        ParamName= param.Name,
                        isGlobal=param.IsGlobal
                    });

                    int CountIndex = (param.IsGlobal? 0:1);
                    _EditorSettings.ParamCountList[CountIndex]++;

                    //���̺��� �����Ѵٸ�
                    for(int k=0; k<labelCount; k++) {

                        lableList.Add(param.Labels[k]);
                    }
                }

            }

            #endregion
        }

        private bool GetParamIsAlreadyContain(string checkParamName, List<FModParamDesc> descs)
        {
            int Count = descs.Count;
            for(int i=0; i<Count; i++)
            {
                if (descs[i].ParamName == checkParamName) return true;
            }

            return false;
        }

        private int GetCategoryIndex(string categoryName, List<FModEventCategoryDesc> descs)
        {
            int Count = descs.Count;
            for(int i=0; i<Count; i++)
            {
                if (descs[i].CategoryName.Equals(categoryName)) return i;
            }

            return -1;
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
        public int FadeID;
        public float duration;
        public float durationDiv;
        public float startVolume;
        public float distance;
        public bool destroyAtCompleted;
        public bool pendingKill;
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
    public static bool  AutoFadeInOutBGM    { get; set; } = false;
    public static float AutoFadeBGMDuration { get; set; } = 3f;
    public const  int   AutoFadeBGMID = -9324;
    public static FModEventFadeCompleteNotify OnEventFadeComplete;


    //=======================================
    /////            Fields            /////
    ///======================================
    private static FModAudioManager _Instance;
#if FMOD_Event_ENUM
    private FModEventInstance       _BGMIns;
#endif

    private FMOD.Studio.Bus[] _BusList;

    private FadeInfo[]   _fadeInfos = new FadeInfo[10];
    private Coroutine    _fadeCoroutine;
    private int          _fadeCount = 0;
    private FadeState    _fadeState = FadeState.None;

    private int     _NextBGMEvent = -1;
    private float   _NextBGMVolume = 0f;
    private int     _NextBGMStartPos = 0;
    private int     _NextBGMParam = -1;
    private float   _NextBGMParamValue = 0f;
    private Vector3 _NextBGMPosition = Vector3.zero;


    //=======================================
    /////         Core Methods          /////
    ///======================================

#if FMOD_Event_ENUM
    /*****************************************
     *   Bus Methods
     * ***/
    public static void SetBusVolume(FModBusType busType, float newVolume)
     {
        if (_Instance == null) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.setVolume(newVolume);
     }

    public static float GetBusVolume(FModBusType busType)
    {
        if (_Instance== null) return 0;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return 0;

        float volume;
        bus.getVolume(out volume);
        return volume;
    }

    public static void StopBusAllEvents(FModBusType busType)
    {
        if (_Instance == null) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public static void SetBusMute(FModBusType busType, bool isMute)
    {
        if (_Instance == null) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;

        bus.setMute(isMute);
    }

    public static bool GetBusMute(FModBusType busType)
    {
        if (_Instance == null) return false;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return false;

        bool isMute;
        bus.getMute(out isMute);
        return isMute;
    }

    public static void SetAllBusMute(bool isMute)
    {
        if (_Instance == null) return;

        int Count = _Instance._BusList.Length;
        for(int i=0; i<Count; i++)
        {
            if (!_Instance._BusList[i].isValid()) continue;
            _Instance._BusList[i].setMute(isMute);
        }
    }


    /****************************************
     *   Bank Methods
     * ****/
    public static void LoadBank(FModBankType bankType)
    {
        if (_Instance == null) return;

        string bankName = FModReferenceList.Banks[(int)bankType];
        FMODUnity.RuntimeManager.LoadBank(bankName);
    }

    public static void UnloadBank(FModBankType bankType)
    {
        if (_Instance == null) return;

        string bankName = FModReferenceList.Banks[(int)bankType];
        FMODUnity.RuntimeManager.UnloadBank(bankName);
    }

    public static bool BankIsLoaded(FModBankType bankType)
    {
        if (_Instance == null) return false;

        string bankName = FModReferenceList.Banks[(int)bankType];
        return FMODUnity.RuntimeManager.HasBankLoaded(bankName);
    }

    public static void LoadAllBank()
    {
        if (_Instance == null) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //��� ��ũ�� �ε��Ѵ�.
        for(int i=0; i<Count; i++){

            if (FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            try { FMODUnity.RuntimeManager.LoadBank(bankLists[i]); } catch { continue; }
        }
    }

    public static void UnLoadAllBank()
    {
        if (_Instance == null) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //��� ��ũ�� ��ε��Ѵ�.
        for (int i = 0; i < Count; i++)
        {
            if (!FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            FMODUnity.RuntimeManager.UnloadBank(bankLists[i]);
        }
    }


    /******************************************
     *   FModEventInstance Methods
     * **/
    public static FModEventInstance CreateInstance(FModBGMEventType eventType, Vector3 position=default)
    {
        if(_Instance== null) return new FModEventInstance();

        FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
        FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid), position);

        return newInstance;
    }

    public static FModEventInstance CreateInstance(FModSFXEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static FModEventInstance CreateInstance(FModNoGroupEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static void StopAllInstance()
    {
        if (_Instance == null) return;

        FMOD.Studio.Bus[] busLists = _Instance._BusList;
        int Count = busLists.Length;

        for(int i=0; i<Count; i++){

            busLists[i].stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }


    /*********************************************
     *  PlayOneShot Methods
     * ***/
    private static void PlayOneShotSFX(FModSFXEventType eventType, Vector3 position, float volume, float startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, float minDistance, float maxDistance)
    {
        #region Omit
        if (_Instance == null) return;

        FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
        bool volumeIsChanged = ( volume>=0f );
        bool stPosIsChanged  = ( startTimelinePositionRatio>=0f );
        bool paramIsChanged  = ( paramType!=-1);

        FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid));
        newInstance.Set3DDistance(minDistance, maxDistance);
        newInstance.Position = position;
        if(stPosIsChanged) newInstance.TimelinePositionRatio = startTimelinePositionRatio;
        if(volumeIsChanged) newInstance.Volume = volume;  
        if(paramIsChanged)
        {
            if (isGlobal) newInstance.SetParameter((FModGlobalParamType)paramType, paramValue);
            else newInstance.SetParameter((FModLocalParamType)paramType, paramValue);
        }
        newInstance.Play();
        newInstance.Destroy(true);
        #endregion
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX(eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX(eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX(eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f,  float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = -1f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    /*********************************************
     *   BGM Methods
     * ***/
    private static void PlayBGM(FModBGMEventType eventType, float volume, int startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, Vector3 position)
    {
        #region Omit
        if (_Instance == null) return;

        bool volumeIsChanged = (volume >= 0f);
        bool stPosIsChanged = (startTimelinePositionRatio >= 0f);
        bool paramIsChanged = (paramType!=-1);

        //������ BGM �ν��Ͻ��� ������ ���
        if(_Instance._BGMIns.IsValid)
        {
            if (AutoFadeInOutBGM){

                _Instance._NextBGMEvent = (int)eventType;
                _Instance._NextBGMVolume = volume;
                _Instance._NextBGMStartPos = startTimelinePositionRatio;
                _Instance._NextBGMParam = (int)paramType;
                _Instance._NextBGMParamValue = paramValue;
                _Instance._NextBGMPosition = position;

                StopFade(AutoFadeBGMID);
                ApplyBGMFade(0f, AutoFadeBGMDuration * .5f, AutoFadeBGMID, true);
                return;
            }
            else _Instance._BGMIns.Destroy();
        }

        _Instance._BGMIns = CreateInstance(eventType, position);
        _Instance._BGMIns.Position = position;
        if (stPosIsChanged) _Instance._BGMIns.TimelinePositionRatio = startTimelinePositionRatio;
        if (paramIsChanged)
        {
            if (isGlobal) _Instance._BGMIns.SetParameter((FModGlobalParamType)paramType, paramValue);
            else _Instance._BGMIns.SetParameter((FModLocalParamType)paramType, paramValue);
        }

        //���̵带 �����ϸ鼭 ������ ���
        if (AutoFadeInOutBGM)
        {
            float newVolume = (volumeIsChanged ? volume : _Instance._BGMIns.Volume);
            _Instance._NextBGMEvent = -1;
            _Instance._BGMIns.Volume = 0f;
            ApplyBGMFade(newVolume, AutoFadeBGMDuration, AutoFadeBGMID);
        }
        else if (volumeIsChanged) _Instance._BGMIns.Volume = volume;

        _Instance._BGMIns.Play();
        #endregion
    }

    public static void PlayBGM(FModBGMEventType eventType, float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM(eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM(eventType, volume, startTimelinePositionRatio, false,  (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModGlobalParamType paramType, float paramValue = 0f, float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM(eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,float volume = -1f, int startTimelinePositionRatio = -1, Vector3 position = default)
    {
        PlayBGM((FModBGMEventType)eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void StopBGM()
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;

        if(AutoFadeInOutBGM)
        {
            _Instance._NextBGMEvent = -1;

            StopFade(AutoFadeBGMID);
            ApplyBGMFade(0f, AutoFadeBGMDuration, AutoFadeBGMID, true);
            return;
        }

        _Instance._BGMIns.Stop();
    }

    public static void DestroyBGM(bool destroyAtComplete = false)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Destroy(destroyAtComplete);
    }

    public static void SetBGMPause()
    {
        if (_Instance == null || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Pause();
    }

    public static void SetBGMResume()
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Resume();
    }

    public static void SetBGMVolume(float newVolume)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Volume = newVolume;
    }

    public static float GetBGMVolume(float newVolume)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.Volume;
    }

    public static void SetBGMParameter(FModGlobalParamType paramType, float paramValue)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
    }

    public static void SetBGMParameter(FModLocalParamType paramType, float paramValue)
    {
        _Instance._BGMIns.SetParameter(paramType, paramValue);
    }

    public static void SetBGMParameter(string paramType, float paramValue)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
    }

    public static float GetBGMParameter(FModGlobalParamType paramType) 
    {
        if(_Instance==null || _Instance._BGMIns.IsValid==false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
    }

    public static float GetBGMParameter(FModLocalParamType paramType) {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
    }

    public static float GetBGMParameter(string paramName) {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramName);
    }

    public static void SetBGMTimelinePosition(int timelinePositionMillieSeconds)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePosition= timelinePositionMillieSeconds;
    }

    public static void SetBGMTimelinePosition(float timelinePositionRatio)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePositionRatio = timelinePositionRatio;
    }

    public static int GetBGMTimelinePosition()
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return 0;
        return _Instance._BGMIns.TimelinePosition;
    }

    public static float GetBGMTimelinePositionRatio()
    {
        if (_Instance == null || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.TimelinePositionRatio;
    }


    /********************************************
     *   Fade Methods
     * ***/
    public static void ApplyBusFade(FModBusType busType, float goalVolume, float fadeTime, int fadeID=0, bool completeBusAllEventDestroy=false)
    {
        //TODO:
    }

    public static void ApplyInstanceFade(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        if (_Instance == null || Instance.IsValid==false) return;

        _Instance.AddFadeInfo(Instance, goalVolume, fadeTime, fadeID, completeDestroy);
    }

    public static void ApplyBGMFade(float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        if (_Instance == null || _Instance._BGMIns.IsValid==false) return;

        _Instance.AddFadeInfo(_Instance._BGMIns, goalVolume, fadeTime, fadeID, completeDestroy);
    }

    public static bool FadeIsPlaying(int FadeID)
    {
        if (_Instance == null) return false;

        for(int i=0; i<_Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) return true;
        }

        return false;
    }

    public static int GetFadeCount(int FadeID)
    {
        if (_Instance == null) return 0;

        int total = 0;
        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) total++;
        }

        return total;
    }

    public static void StopFade(int FadeID)
    {
        if (_Instance == null) return;

        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID)
            {
                _Instance.RemoveFadeInfo(i);
                if(_Instance._fadeState==FadeState.None) i--;
            }
        }
    }

    public static void StopAllFade()
    {
        if(_Instance == null) return;

        if (_Instance._fadeCoroutine != null){

            _Instance.StopCoroutine(_Instance._fadeCoroutine);
            _Instance._fadeCoroutine = null;
        }

        _Instance._fadeCount = 0;
    }

    private void AddFadeInfo(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID, bool completeDestroy )
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
        _fadeInfos[_fadeCount++] = new FadeInfo()
        {
            startVolume = Instance.Volume,
            distance    = ( goalVolume-Instance.Volume ),
            duration    = fadeTime,
            durationDiv = (1f / fadeTime),
            FadeID      = fadeID,
            TargetIns   = Instance,
            pendingKill = false,
            destroyAtCompleted = completeDestroy,
        };

        //���̵� �ڷ�ƾ�� ���ٸ� �����Ѵ�.
        if (_fadeCoroutine == null) _fadeCoroutine = StartCoroutine(FadeProgress());

        #endregion
    }

    private void RemoveFadeInfo(int index)
    {
        if (_fadeInfos == null) return;

        //PendingKill Ready���, PendingKill���·� ��ȯ.
        if(_fadeState!=FadeState.None){

            _fadeState= FadeState.PendingKill;
            _fadeInfos[index].pendingKill = true;
            return;
        }

        _fadeInfos[index] = _fadeInfos[_fadeCount - 1];
        _fadeCount--;
    }

    private IEnumerator FadeProgress()
    {
        #region Omit
        //���̵尡 �����ϴ� ���� ���ӵȴ�.
        while (_fadeCount>0)
        {
            float DeltaTime = Time.deltaTime;
            int   fadeCount = _fadeCount;

            /********************************
             *  ��� ���̵� ������ ������Ʈ�Ѵ�.
             * **/
            _fadeState = FadeState.PendingKill_Ready;
            for(int i=0; i< fadeCount; i++)
            {
                //���̵� ��� �ν��Ͻ��� ��ȿ���� ���� ���, PendingKill���¸� ����.
                if (_fadeInfos[i].TargetIns.IsValid==false)
                {
                    _fadeInfos[i].pendingKill = true;
                    _fadeState = FadeState.PendingKill;
                    continue;
                }

                _fadeInfos[i].duration -= DeltaTime;
                float fadeTimeRatio = Mathf.Clamp(1f - ( _fadeInfos[i].duration * _fadeInfos[i].durationDiv ), 0f, 1f);
                float newVolume     = _fadeInfos[i].startVolume + (fadeTimeRatio * _fadeInfos[i].distance);

                //���� ����
                _fadeInfos[i].TargetIns.Volume = _fadeInfos[i].startVolume + ( fadeTimeRatio * _fadeInfos[i].distance );
                if (fadeTimeRatio < 1f) continue;

                /*********************************
                 *  ���̵� ������ �ܰ�...
                 * **/
                OnEventFadeComplete?.Invoke(_fadeInfos[i].FadeID, newVolume);

                //������ �ܰ� �ı� ����.
                if (_fadeInfos[i].destroyAtCompleted){

                    _fadeInfos[i].TargetIns.Destroy();
                }

                _fadeInfos[i].pendingKill = true;
                _fadeState = FadeState.PendingKill;
            }

            /*********************************
             *  PendingKill ���¸� ó���Ѵ�.
             * **/
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

            yield return null;
        }

        _fadeCoroutine = null;
        #endregion
    }

    private void BGMFadeComplete(int fadeID, float goalVolume)
    {
        if (fadeID != AutoFadeBGMID ) return;
        if (goalVolume <= 0f) _BGMIns.Destroy();
        if(_NextBGMEvent >=0) PlayBGM((FModBGMEventType)_NextBGMEvent, (FModLocalParamType)_NextBGMParam, _NextBGMParamValue, _NextBGMVolume, _NextBGMStartPos, _NextBGMPosition);
    }

#endif
    //=======================================
    /////        Magic Methods           /////
    ///======================================
    private void Awake()
    {
        #region Omit
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);

            #if FMOD_Event_ENUM
            OnEventFadeComplete += BGMFadeComplete;

            //��� ���� ����� ���´�.
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
        //Destroy...
        if(_Instance==this){

            _Instance = null;
            OnEventFadeComplete = null;
        }
    }

}