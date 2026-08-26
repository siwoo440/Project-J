---
# Project J - 140일차 개발일지

---
## 개발 주제

PHASE 10→11 전환 전 전체 안정성 점검과 구형 Bot Route 구조 정리

---
## 개발 목표

139일차까지 구축한 자율 판단형 AI Bot 구조를 유지하면서, 현재 프로젝트에 남아 있는 구형 Route/Waypoint 관련 코드와 기능 없는 잔여 스크립트를 제거한다.

정리 과정에서 발생한 컴파일 의존성 문제를 해결하고, 다음 단계인 PHASE 11 맵 모듈 제작에 들어가기 전에 프로젝트 구조를 단순화한다.

이번 일차에서는 새로운 Gameplay 기능을 추가하기보다 기존 기능을 깨뜨릴 가능성이 있는 오래된 코드와 Scene 참조를 정리하는 데 집중한다.

---
## 기준 커밋

이번 개발일지 작성 시점의 `main` 기준:

```text
d4b3d8bbbb0613d67badfda27e72b8b19743be38
```

커밋 메시지:

```text
140
```

이전 기준 커밋:

```text
5fe815a07174b2576d843e2d7b2f32c9918ec006
```

---
## 주요 작업 내용

- 구형 `ProjectJBotRouteNode` Runtime 스크립트 제거
- 구형 `ProjectJDay137BotRouteSetup` Editor Setup 제거
- 기능이 없는 `ProjectJPrivateMatchPanel` 이동 표식 스크립트 제거
- `Game.unity`에 남아 있던 Day136 Bot Route 오브젝트 및 관련 직렬화 데이터 정리
- Day140 안정성 정리용 `ProjectJDay140Cleanup` Editor 도구 추가
- 삭제 대상 Asset을 Unity `AssetDatabase.DeleteAsset`으로 정리하도록 구성
- Scene에 남은 구형 Route Root와 Route Component를 정리하도록 구성
- 이미 삭제된 Asset에 대해 중복 실행해도 안전하게 건너뛰도록 처리
- Debug/Test/Inventory 및 다른 Day Setup 계열은 이번 정리 범위에서 보존
- Day136 Bot Setup이 삭제된 `ProjectJBotRouteNode`를 참조해 발생하던 `CS0246` 컴파일 오류 수정
- Day136 Bot Setup을 구형 Route를 다시 생성하지 않는 퇴역용 Editor 메뉴로 변경

---
## 구형 Bot Route 구조 정리

139일차부터 Bot은 미리 배치된 Route Node를 따라가는 방식이 아니라 Checkpoint와 FINISH를 장거리 목표로 사용하고 주변 Physics 결과로 Walk / Jump / Fall을 판단한다.

따라서 다음 구형 구조는 현재 Bot 설계와 맞지 않아 제거했다.

```text
=== DAY136 BOT ROUTE ===
        ↓
ProjectJBotRouteNode
        ↓
ProjectJDay137BotRouteSetup
        ↓
고정 Route / Waypoint 이동
```

현재 유지하는 구조는 다음과 같다.

```text
Checkpoint / FINISH
        ↓
장거리 진행 목표
        ↓
ProjectJBotTraversalSensor
        ↓
주변 Physics 탐색
        ↓
Walk / Jump / Fall 자율 판단
```

---
## 삭제한 파일

```text
Assets/ProjectJ/Runtime/AI/
└─ ProjectJBotRouteNode.cs

Assets/ProjectJ/Editor/
└─ ProjectJDay137BotRouteSetup.cs

Assets/ProjectJ/Runtime/SceneFlow/
└─ ProjectJPrivateMatchPanel.cs
```

각 파일의 `.meta`도 함께 제거했다.

---
## 수정 및 추가 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay136BotSetup.cs
└─ ProjectJDay140Cleanup.cs

Assets/ProjectJ/Scenes/
└─ Game.unity
```

`ProjectJDay136BotSetup`은 현재 Bot 구조를 다시 변경하지 않고 퇴역 안내만 출력하도록 축소했다.

---
## 컴파일 오류 수정

구형 `ProjectJBotRouteNode.cs`를 제거한 뒤 다음 컴파일 오류가 발생했다.

```text
CS0246:
The type or namespace name 'ProjectJBotRouteNode' could not be found
```

원인은 `ProjectJDay136BotSetup.cs`가 삭제된 타입을 계속 직접 참조하고 있었기 때문이다.

구형 Route 구조를 다시 복구하지 않고 Day136 Setup 자체를 퇴역 처리하여 삭제된 타입에 대한 의존성을 제거했다.

현재 Day136 메뉴를 실행해도 Bot Prefab이나 Route Node를 다시 생성하지 않고 현재 Bot 구조를 유지하라는 안내만 출력한다.

---
## AssetImportWorker 점검

정리 과정에서 Unity의 `AssetImportWorker`가 Serialized Asset을 읽는 도중 크래시하는 현상이 발생했다.

대표 스택 위치:

```text
SerializedFile::InitializeRead
PersistentManager
AssetDatabase::ReloadSingletonAssets
AssetImportWorkerClient
```

이 문제는 C# 타입 의존성 오류와는 별개의 Asset Pipeline 문제로 구분했다.

프로젝트 파일을 임의로 더 삭제하기보다 Unity 종료 후 `Library`, `Temp`, `obj` 캐시 재생성 방식으로 대응하도록 정리했다.

GitHub 저장소의 최신 커밋만으로 AssetImportWorker 재발 여부를 확인할 수 없으므로 실제 Unity Editor에서의 재임포트 상태는 별도 실행 확인 항목으로 남긴다.

---
## 최신 커밋 점검

최신 `main`에서 다음 항목을 정적으로 다시 확인했다.

- `ProjectJBotRouteNode.cs` 제거 상태
- `ProjectJDay137BotRouteSetup.cs` 제거 상태
- `ProjectJPrivateMatchPanel.cs` 제거 상태
- `ProjectJDay136BotSetup.cs`의 구형 Route 직접 참조 제거
- `ProjectJDay140Cleanup.cs`가 삭제된 타입을 직접 참조하지 않고 문자열 타입명과 Asset 경로로만 처리
- 삭제된 `ProjectJBotRouteNode` 타입 이름에 대한 저장소 코드 검색 결과 직접 참조 없음
- 삭제된 `ProjectJBotRouteNode` Script GUID에 대한 저장소 검색 결과 참조 없음
- 삭제된 `ProjectJPrivateMatchPanel` Script GUID에 대한 저장소 검색 결과 참조 없음
- `Devlogs/Day140`은 아직 존재하지 않아 이번 개발일지에서 새로 추가

---
## 검증 상태

GitHub 기준 정적 점검에서는 이번 정리로 인해 즉시 확인되는 삭제 타입 참조 문제는 발견되지 않았다.

다만 현재 커밋에는 GitHub Actions 또는 Commit Status 기반 자동 검증 결과가 등록되어 있지 않다.

따라서 다음 항목은 GitHub 저장소만으로 완료 여부를 확정할 수 없다.

```text
Unity Editor 실제 컴파일
AssetImportWorker 재발 여부
Game.unity Missing Script / Missing Reference
EditMode Test Runner 전체 결과
Host / Client 실제 경기
Human / Bot 실제 Spawn과 이동
```

이번 일차는 코드 및 프로젝트 정리 작업을 기준으로 마무리하고, 실제 Gameplay 동작 검증과 추가 Lobby/Spawn 규칙 수정은 다음 작업에서 이어서 확인한다.

---
## 다음 일차로 이관한 항목

이번 일차 중 추가 요청되었지만 최종 코드 반영까지 완료하지 않은 항목은 다음 작업으로 이관한다.

- 개인 방 `Max Players` 최소값을 2에서 1로 변경
- 선택한 Max Players 값을 Fusion 실제 `PlayerCount`에 연결
- 현재 참가자 전원이 Ready 상태일 때만 경기 시작
- 참가자 목록 변경 직후 잘못된 조기 시작을 방지하는 Ready 안정화
- Human Player가 지정된 `Spawn_00~07` 위치와 회전에서 확실히 생성되도록 Spawn 흐름 재점검
- 1인 / 다인 / Host / Client / Bot 조합별 Spawn Slot 재사용 확인

---
## 결과

140일차에는 새로운 기능 추가보다 PHASE 11 진입 전에 프로젝트를 안정화하는 작업을 진행했다.

현재 사용하지 않는 고정 Route / Waypoint 구조와 기능 없는 잔여 스크립트를 제거하고, 삭제된 Route 타입을 참조하던 Day136 Setup을 퇴역 처리해 기존 컴파일 오류의 원인을 정리했다.

현재 Bot의 Checkpoint / FINISH 기반 장거리 목표와 Physics 자율 이동 구조는 유지한다.

저장소 정적 점검에서는 삭제된 Route Script에 대한 직접 참조와 GUID 잔여 참조가 확인되지 않았으며, 실제 Unity Editor의 Import / Compile / Test Runner / Network Gameplay 검증은 다음 작업의 실행 확인 항목으로 남긴다.
