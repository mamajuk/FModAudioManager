# FModAudioManager For UnityEngine

## Overview
```FModAudioManager```는 기존 [FMod for unity](https://github.com/fmod/fmod-for-unity)를 더 쉽고 직관적으로 사용하기 위하여 디자인 되었습니다. 기존 **FMod for Unity API** 에서 특정 음원을 재생하기 위해서는, ```EventReference``` 구조체를 **직렬화(Serialized)** 한 다음 **Inspector Window** 에서 사용할 음원과 대응되는 값을 적절히 기입하거나, **FMod Studio Project** 에 표기된 이벤트의 **경로(Path)** 또는 **전역고유식별자(GUID)** 를 참조해 정확히 기입하여야 합니다.

**C# Script** 를 작성하면서 특정 음원이 필요할 때마다 이러한 작업들을 반복해야 하며, 이는 **FMod** 에서 사용되는 데이터인 ```Bus```, ```Bank```, ```Parameter```들을 사용할 때도 마찬가지 입니다. 이런 번거로운 작업을 해결하기 위해, ```FModAudioManager``` 는 **FMod Studio Project** 로부터 모든 ```Event```, ```Bus```, ```Bank```, ```Global/Local Parameter```, ```Parameter Max/Min Value```, ```Parameter labled Value```과 대응되는 열거형 및 구조체를 자동으로 작성해줍니다. 또한 생성된 **열거형**과 **구조체**를 사용할 수 있는 **Static methods** 들을 제공함으로서, 직관적이고 편리하게 오디오 관련 스크립트를 작성할 수 있도록 도와줍니다.

## Tutorial
먼저 **Package Manager**를 통해 **FMod for Unity API**를 프로젝트에 적용한 다음, ```FModAudioManager```를 적용합니다. 그 후 **Scene** 에서 기존 **Unity Audio API** 관련 컴포넌트들을 모두 제거하고, ```FMOD Studio Listener```와 ```FModAudioManager``` 컴포넌트를 원하는 ```GameObject```에 부착합니다. ( ```FModAudioManager```는 **Scene** 이동으로 파괴되지 않으며, 복수로 존재할 수 없는 컴포넌트입니다. )

<table><tr><td>
<img src="https://github.com/mamajuk/FModAudioManager/blob/main/Readmy_Data/Readmy_ConnectFModProject.gif?raw=true">
</td></tr></table>

이제 **FMod Studio Project**를 프로젝트에 연결해야할 차례입니다. 메뉴창에서 **FMOD > FMOD Audio Settings**를 눌러 ```FModAudioManager``` 에서 제공하는 **FModAudioSettings Editor**를 화면에 띄웁니다. **FModAudioSettings Editor** 상단에 존재하는 돋보기 버튼을 눌러 원하는 **FMOd Studio Project**를 프로젝트에 손쉽게 연결할 수 있습니다. 참고로 **FMod Studio** 아이콘이 새겨진 버튼을 누르면 연결된 **FMod Studio Project**에 빠르게 접근할 수도 있습니다.

<table><tr><td>
<img src="https://github.com/mamajuk/FModAudioManager/blob/main/Readmy_Data/Readmy_LoadedStudioData.gif?raw=true">
</td></tr></table>

**FMod Studio Project** 에서 작업이 끝났다면 이벤트를 빌드한 다음, **FModAudioSettings Editor** 상단의 **Loaded Studio Settings** 버튼을 눌러 연결된 **FMod Studio Project** 로부터 모든 데이터를 불러오도록 합니다. **FModAudioSettings Editor** 에 표시된 정보들을 통해 불러온 데이터들이 정확한지 확인한 후, **Save and Apply Settings** 버튼을 누르면 ```FModAudioManager``` 에서 사용할 수 있는 열거형 및 구조체가 작성되며, **Unity Engine** 에서 이에 대한 컴파일을 진행합니다.

<table><tr><td>
<img src="https://github.com/mamajuk/FModAudioManager/blob/main/Readmy_Data/Readmy_Scripting.gif?raw=true">
</td></tr></table>

위 과정을 통해 생성된 **열거형**과 **구조체**는 다음과 같습니다:<br/>

------------------------------------------------------------------------
```FModBusType```: **Bus**들을 나타내는 열거형입니다.<br/>
```FModBankType```: **Bank**들을 나타내는 열거형입니다.<br/>
```FModGlobalParamType```: **Global Parameter**를 나타내는 열거형입니다.<br/>
```FModLocalParamType```: **Local Parameter**를 나타내는 열거형입니다.<br/>
```FModParamLabel```: **Parameter**들의 **labled** 값이 보관된 구조체입니다.<br/>
```FModParamValueRange```: **Parameter**들의 **Min**, **Max**값이 보관된 구조체입니다.<br/>
```FModBGMEventType```: **루트폴더** 이름이 **BGM**인 **Event**들을 나타내는 열거형입니다.<br/>
```FModSFXEventType```: **루트폴더** 이름이 **SFX**인 **Event**들을 나타내는 열거형입니다.<br/>
```FModNoGroupEventType```: **BGM**, **SFX**로 분류되지 못한 **Event**들을 나타내는 열거형입니다.<br/>
( ※루트폴더는 **FModAudioSettings Editor** 에서 **FMod Events > EventGroup RootFolder Settings** 를 통해 변경할 수 있습니다. )<br/>
( ※이름이 동일한 **Event**, **Bus**는 경로가 이름에 포함됩니다. )

------------------------------------------------------------------------

이후 **FMod Studio Project** 에서 새로운 음원이 추가되거나, 변경된다면 위 과정을 통해 열거형 및 구조체를 만들면 됩니다. 이제 **Visual Studio** 와 같은 IDE를 통해 **C# Script**를 작성한다면, 열거형 및 구조체에 대한 코드 힌트가 표시되어 직관적인 스크립팅을 진행할 수 있습니다. 

다음은 기본적인 사용방법들에 대한 예시들을 보여주며, 더 자세한 사용방법은 [FMod Audio Manager Reference](https://bramble-route-61a.notion.site/Unity-C-FModAudioManager-e3837f0765fe4254aa40a0156d050288?pvs=4)를 참고하십시오.
``` c#
private IEnumerator Start()
{
    FModAudioManager.UsedBGMAutoFade     = true;
    FModAudioManager.BGMAutoFadeDuration = 3f;
    FModAudioManager.PlayBGM(FModBGMEventType.Wagtail_bgm_title, 2f);

    //10초간 대기한다....
    yield return new WaitForSecondsRealtime(10f);

    FModAudioManager.PlayBGM(FModBGMEventType.tavuti_ingame1, 3f);
}
```

(▲(1): BGM 자동 페이드 적용 및 BGM 실행 및 전환...  )

``` c#
private void Start()
{
    FModAudioManager.PlayOneShotSFX(
			FModSFXEventType.Player_Jump, 
			transform.position, 
			2f, 
			.5f
    );
}
```

(▲(2): 단발성 SFX 재생 및 볼륨 및 시작지점등 변경.  )

``` c#
private FModEventInstance WaterStream;

private void Start()
{
    WaterStream = FModAudioManager.CreateInstance(FModSFXEventType.Water_Stream);
    WaterStream.Play();

}

private void OnCollisionEnter(Collision collision)
{
   WaterStream.Stop();
}

private void OnDestroy()
{
   //사용하지 않는 Event를 파괴한다.
   WaterStream.Destroy();
}
```

(▲(3): FModEventInstance를 생성하고, 물리적인 충돌이 있을 때까지 Event를 재생. )

``` c#
FModAudioManager.SetBusVolume(FModBusType.Enviorment, .5f);
```

(▲(4): Enviorment Bus의 Volume을 50%로 감소. )

``` c#
FModAudioManager.LoadBank( FModBankType.Player );
FModAudioManager.UnLoadBank( FModBankType.Player );
FModAudioManager.LoadAllBanks();
```

(▲(5): 원하는 Bank 로드/언로드 및 모든 Bank로드. )

``` c#
 FModParameterReference paramRef = new FModParameterReference();
	paramRef.SetParameter(
   	FModGlobalParamType.BGMIsLooping,
   	FModParamLabel.BGMIsLooping.Used
);

FModAudioManager.PlayBGM(FModBGMEventType.Wagtail_bgm_title, paramRef);
```
```` c#
 FModAudioManager.PlayBGM(
     FModBGMEventType.tavuti_ingame1,
      FModGlobalParamType.BGMIsLooping,
      FModParamLabel.BGMIsLooping.UnUsed
);
````
```` c#
FModAudioManager.PlayOneShotSFX(

    FModSFXEventType.Broken,
    FModLocalParamType.BrokenType,
    FModParamValueRange.BrokenType.Max
);
````
```` c#
FModEventInstance Instance = FModAudioManager.CreateInstance(FModSFXEventType.Player_Walk);
Instance.Volume 		= .5f;
Instance.Pitch 			= .1f;
Instance.AttachedGameObject 	= gameObject;
Instance.Position 		= transform.position;
Instance.Set3DDistance(1f, 10f);
Instance.SetParameter(FModLocalParamType.PlayerWalkType, FModParamLabel.PlayerWalkType.Grass);
Instance.Play();
````
(▲(6): Event의 파리미터를 수정하는 다양한 방법들.  )


