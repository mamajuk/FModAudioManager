using FMODUnity;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

[System.Serializable]
public struct NPData
{
    public string Name;
    public string Path;
}

[System.Serializable]
public struct FModParamDesc
{
    public string ParamName;
    public string Path;
    public string ParentFolderName;
    public int    LableCount;
    public bool   isGlobal;
    public float  Min;
    public float  Max;
}

[System.Serializable]
public struct FModEventCategoryDesc
{
    public string CategoryName;
    public int EventCount;
    public int GroupIndex;
    public bool foldout;
    public bool EventIsValid;

    public FModEventCategoryDesc( FModEventCategoryDesc copy, int addEventCount )
    {
        CategoryName = copy.CategoryName;
        EventCount = copy.EventCount + addEventCount;
        GroupIndex = copy.GroupIndex;
        EventIsValid = true;
        foldout = false;
    }
}

[System.Serializable]
public struct FModEventInfo
{
    public string Name;
    public string Path;
    public FMOD.GUID GUID;
}

[System.Serializable]
public struct FModGroupInfo
{
    public string RootFolderName;
    public int    TotalEvent;
}

public enum FModEditorFoldout : int
{
    None=0,
    BankFoldout=1,
    BusFoldout=2,
    ParamFoldout=4,
    EventFoldout=8,
    PathFoldout=16
}

public class FModAudioEditorSettings : ScriptableObject
{
    [SerializeField] public int                     FoldoutBooleans     = (int)FModEditorFoldout.PathFoldout;
    [SerializeField] public List<NPData>            BankList            = new List<NPData>();
    [SerializeField] public List<NPData>            BusList             = new List<NPData>();

    [SerializeField] public int[]                   ParamCountList  = new int[2];
    [SerializeField] public List<FModParamDesc>     ParamDescList   = new List<FModParamDesc>();
    [SerializeField] public List<string>            ParamLableList  = new List<string>();

    [SerializeField] public List<FModEventCategoryDesc> CategoryDescList    = new List<FModEventCategoryDesc>();
    [SerializeField] public List<FModEventInfo>         EventRefs           = new List<FModEventInfo>();
    [SerializeField] public bool                        RootFolderFoldout   = true;

    [SerializeField]
    public FModGroupInfo[] EventGroups = new FModGroupInfo[3]
    {
        new FModGroupInfo { RootFolderName="BGM", TotalEvent=0 },
        new FModGroupInfo { RootFolderName="SFX", TotalEvent=0 },
        new FModGroupInfo { RootFolderName="NoGroup", TotalEvent=0 }
    };
}

#endif