# FModAudioManager For UnityEngine

## Overview
```FModAudioManager```는 기존 [FMod for unity](https://github.com/fmod/fmod-for-unity)를 더 쉽고 직관적으로 사용하기 위하여 디자인 되었습니다. 기존 **FMod for Unity API** 에서 특정 음원을 재생하기 위해서는, ```EventReference``` 구조체를 **직렬화(Serialized)** 한 다음 **Inspector Window** 에서 사용할 음원과 대응되는 값을 적절히 기입하거나, **FMod Studio Project** 에 표기된 이벤트의 **경로(Path)** 또는 **전역고유식별자(GUID)** 를 참조해 정확히 기입하여야 합니다.

**C# Script** 를 작성하면서 특정 음원이 필요할 때마다 이러한 작업들을 반복해야 하며, 이는 **FMod** 에서 사용되는 데이터인 ```Bus```, ```Bank```, ```Parameter```들을 사용할 때도 마찬가지 입니다. 이런 번거로운 작업을 해결하기 위해, ```FModAudioManager``` 는 **FMod Studio Project** 로부터 모든 ```Event```, ```Bus```, ```Bank```, ```Global/Local Parameter```, ```Parameter Max/Min Value```, ```Parameter labled Value```과 대응되는 열거형 및 구조체를 자동으로 작성해줍니다. 또한 생성된 **열거형**과 **구조체**를 사용할 수 있는 **Static methods** 들을 제공함으로서, 직관적이고 편리하게 오디오 관련 스크립트를 작성할 수 있도록 도와줍니다.

## Tutorial
```FModAudioManager```는 각 열거형들을 생성하고, ```FMod Studio Project```와 연결 및 바로가기를 하는 기능을 도와주는 ```FModAudioSettings Editor```를 제공합니다. ```FModAudioManager API```를 사용하기 전에, ```FModAudioSettings Editor```를 이용한 사전작업이 필요합니다. 다음은 적용할 **Unity Project**에 ```FMod For Unity API```가 적용되었음을 가정하고 ```FModAudioManager```의 기초적인 사용방법을 보여줍니다. <br>

우선 적용할 **Unity Project**의 모든 **Scene**에 포함되어 있는 기존 ```Unity Audio API```컴포넌트를 제거하고, ```FMOD Studio Listener```와 ```FModAudioManager``` **Component**를 원하는 ```GameObject```에 부착합니다. ( ```FModAudioManager```는 **Scene** 이동에 대한 파괴를 보장하지 않으며, 각 **Scene**에서 복수로 존재할 수 없는 **Component**입니다. )

<table><tr><td>
<img src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F1b677688-1747-485d-a9c7-6b7c40386927%2FUntitled.png?table=block&id=07a1b1ea-13bd-4aee-afdd-2f66e1bfc949&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=230&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

```FModAudioManager``` 를 **Unity Project**에 적용하면, 위 이미지와 같이 FMOD/FMODAudio Settings 메뉴를 통해서 ```FModAudio Settings Editor```를 Editor Window에 표시할 수 있습니다.

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F5b3cfa88-fa93-4624-92e6-7b94afa5fd07%2FUntitled.png?table=block&id=1d9b9463-311e-4639-9ea0-a92be21e7047&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=1020&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

에디터를 표시한 후, ```Bus```, ```Bank```, ```Global Parameter```, ```Local Parameter```, ```Parameter Max/Min Value```, ```Parameter Labled```, ```Event```들을 불러올 ```FMod Studio Project```를 돋보기 버튼을 통해 연결합니다. 그 후, ```Loaded Studio Settings```버튼으로 해당 설정들을 모두 불러올 수 있습니다. 다음은 이렇게 불러온 각 요소들을 에디터가 표시해주는 것을 보여줍니다.

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F07e2ee36-a41b-4d82-b693-c018155d7a83%2FUntitled.png?table=block&id=cb7b59f2-e64f-4b70-b41b-a5a2e2c0e9cd&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=1010&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F965f79dc-aa16-45ca-b517-bf98d249b500%2FUntitled.png?table=block&id=47e2a21a-0b18-4848-80f4-c83a7c69f845&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=1020&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2Fc2228eb6-ffb0-43bd-8c54-cba8f2738430%2FUntitled.png?table=block&id=377d0336-299a-4ad4-b39a-de3c272bb230&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=1030&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F1c5afef9-0b32-4781-b108-0c60932cb7ea%2FUntitled.png?table=block&id=161bc43b-5264-4318-afe4-a0dc5f22eb90&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=990&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

이렇게 불러온 요소들은 그저 확인용이며, 확인이 끝났다면 ```Save and Apply Settings```버튼을 눌러 이 설정들을 ```FModAudioManager```에서 사용할 수 있는 열거형 및 구조체를 생성해야 합니다.

<table><tr><td>
<img width="400px" src="https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2Fffc72e99-6cff-4df5-85ae-d343d9f0b328%2FUntitled.png?table=block&id=f45b9979-5367-44bb-a0d8-48522b06e8c1&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=1400&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

위 과정을 적용하면 불러온 설정들에 대한 열거형 및 구조체가 작성된 **C# Script**를 컴파일을 하게 됩니다. ```Loaded Project Settings```버튼으로 사용할 ```FMod Studio Project```로부터 설정을 가져오고, ```Save and Apply Settings```버튼으로 ```FModAudioManager```에서 사용할 수 있는 열거형 및 구조체를 만드는 작업은 ```FMod Studio Project```를 갱신할 때마다 해주어야합니다. 이 과정을 끝마쳤다면 이제 ```FModAudioManager```를 사용할 준비가 끝났습니다. 다음은 기본적인 사용방법들에 대한 예시들을 보여주며, 더 자세한 사용방법은 [FMod Audio Manager Reference](https://bramble-route-61a.notion.site/Unity-C-FModAudioManager-e3837f0765fe4254aa40a0156d050288?pvs=4)를 참고하십시오.
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


