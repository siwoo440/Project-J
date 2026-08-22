# Project J - 79일차 개발일지

## 1. 개발 목표

78일차까지 구축한 8인 Host Mode 전체 경기 환경을 유지하면서,
의도적으로 RTT·Jitter·Packet Loss를 발생시켜 네트워크 악조건에서도
게임 규칙과 최종 결과가 동일하게 유지되는지 검증할 수 있는 환경을 구축한다.

이번 일차의 핵심 목표는 다음과 같다.

- Photon Fusion Network Conditions Preset 도구 추가
- 정상/100ms/150ms급/200ms급/250ms급 테스트 환경 구성
- F6 Network Condition Debug View 추가
- 실제 Player RTT 측정
- RTT 변화량 기반 Jitter 진단
- Correction·Rollback·Resimulation 관찰
- 8인 기본 Gate 상태 유지
- 78일차 F5 화면과 79일차 F6 화면 분리
- 기존 Game Scene과 경기 규칙은 수정하지 않음

---

## 2. 최신 개발 구조

79일차에서도 다음 실제 경기 Scene을 그대로 사용한다.

```text
Assets/ProjectJ/Scenes/Game.unity
```

77일차에 구축한 Greybox 경기장:

```text
START
↓
CP1
↓
CP2
↓
CP3
↓
CP4
↓
FINISH
```

과 76일차의 최대 8인 Spawn 구조:

```text
Spawn_00
Spawn_01
Spawn_02
Spawn_03
Spawn_04
Spawn_05
Spawn_06
Spawn_07
```

도 그대로 유지한다.

79일차에서는 맵이나 Player Gameplay 코드를 변경하지 않고
네트워크 테스트 및 진단 계층만 추가했다.

---

## 3. Network Condition Preset Window

신규 Editor Tool:

```text
Assets/ProjectJ/Editor/
└─ ProjectJDay79NetworkConditionPresetWindow.cs
```

을 추가했다.

Unity 상단 메뉴:

```text
Project J
→ Day79
→ Network Condition Presets
```

에서 실행한다.

이 창은 Photon Fusion의 `NetworkProjectConfig.NetworkConditions` 값을
테스트 목적에 맞게 빠르게 변경하기 위한 도구다.

---

## 4. Fusion Debug DLL

Photon Fusion의 내장 Network Conditions는
Debug 버전의 `fusion.dll`에서만 활성화된다.

79일차 Preset Window에는 다음 기능을 추가했다.

```text
Fusion Network Project Config 열기
Fusion Debug DLL Toggle 메뉴 실행
```

Debug DLL이 활성화되지 않은 경우
Network Conditions가 보이지 않거나
설정이 실제 네트워크 Simulation에 반영되지 않을 수 있다.

필요한 경우:

```text
Fusion
→ Toggle Debug Dlls
```

를 실행한 뒤 Unity를 재시작한다.

---

## 5. Network Condition Preset

79일차 Editor Tool에는 다음 테스트 Preset을 구성했다.

### A. NORMAL

```text
Simulation
→ OFF

Delay
→ 0

Jitter
→ 0

Packet Loss
→ 0%
```

정상 환경의 기준값 측정에 사용한다.

---

### B. 100ms / 0% Loss

```text
Simulation
→ ON

Delay
→ 100ms

Additional Jitter
→ 0

Packet Loss
→ 0%
```

비교적 일반적인 원거리 네트워크 지연 상황을 확인한다.

---

### C. 약 150ms + Jitter / 1% Loss

```text
Base Delay
→ 130ms

Additional Jitter
→ 최대 40ms

Packet Loss
→ 1%
```

중간 수준의 불안정한 네트워크 환경을 검증한다.

---

### D. 약 200ms + Jitter / 3% Loss

```text
Base Delay
→ 180ms

Additional Jitter
→ 최대 40ms

Packet Loss
→ 3%
```

높은 지연과 손실이 동시에 존재하는 환경을 검증한다.

---

### E. 약 250ms + Jitter / 5% Loss

```text
Base Delay
→ 220ms

Additional Jitter
→ 최대 60ms

Packet Loss
→ 5%
```

일반적인 플레이 권장 환경보다 나쁜
스트레스 테스트용 조건으로 사용한다.

---

## 6. Preset 저장 방식

Preset 버튼을 누르면 다음 Fusion 설정에 직접 반영한다.

```text
NetworkProjectConfig
└─ NetworkConditions
   ├─ Enabled
   ├─ DelayMin
   ├─ DelayMax
   ├─ DelayPeriod
   ├─ DelayThreshold
   ├─ AdditionalJitter
   ├─ LossChanceMin
   ├─ LossChanceMax
   ├─ LossChancePeriod
   ├─ LossChanceThreshold
   └─ AdditionalLoss
```

고정 환경 테스트를 위해
Delay Min/Max와 Loss Min/Max는 동일한 값으로 설정한다.

Preset 적용 후 Asset을 Dirty 처리하고
`AssetDatabase.SaveAssets()`를 호출해 설정을 저장한다.

---

## 7. Host Mode Network Condition 기준

Project J는 현재 Photon Fusion Host Mode를 사용한다.

Fusion Host/Server Mode의 내장 Network Conditions는
설정된 네트워크 조건을 Host와 Client 양쪽에 나누어 적용해
전체 통신 지연이 설정한 값에 가까워지도록 동작한다.

따라서 테스트에서는 설정된 Delay 숫자뿐 아니라
실제로 측정된 RTT를 함께 관찰한다.

---

## 8. Day79 Network Condition Debug View

신규 Runtime Debug View:

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay79NetworkConditionDebugView.cs
```

를 추가했다.

실제 `Game` Scene에서 자동 생성된다.

Runtime Object:

```text
=== Project J Day79 Network Condition Debug ===
```

Editor 또는 Development Build에서 표시된다.

단축키:

```text
F6
→ Day79 Network Condition Debug View
```

---

## 9. 기존 Debug View 정리

기존 Debug View는 삭제하지 않는다.

현재 단축키 구조:

```text
F4
→ 77일차 4인 Gate

F5
→ 78일차 8인 Gate

F6
→ 79일차 Network Condition Gate
```

79일차부터는 F6가 기본 화면이므로
78일차 F5 Debug View의 기본 표시 상태를 `false`로 변경했다.

필요하면 F5를 눌러 언제든 다시 확인할 수 있다.

---

## 10. F6 상단 정보

F6 화면에는 다음 정보가 표시된다.

```text
DAY 79 - NETWORK CONDITION GATE

FPS
Players
PlayerObjects

SIM CONFIG
Delay
Additional Jitter
Loss

Measured RTT Avg
Measured RTT Max
RTT Jitter Avg

Resimulation Batches
Max Correction

8P BASE GATE
```

---

## 11. 실제 RTT 측정

Fusion의 `NetworkRunner.GetPlayerRtt(PlayerRef)`를 사용해
각 Player의 실제 RTT를 초 단위로 가져온 뒤
화면에는 ms 단위로 표시한다.

예:

```text
Measured RTT Avg : 148.2ms
Measured RTT Max : 171.7ms
```

Preset에 설정한 값과
실제 NetworkRunner가 측정하는 RTT를 함께 비교할 수 있다.

---

## 12. RTT Jitter 진단

79일차 Debug View에서는
연속된 RTT 측정값의 차이를 이용해
진단용 Jitter 값을 계산한다.

예:

```text
이전 RTT
100ms

현재 RTT
138ms

변화량
38ms
```

이 변화량을 단순 평활화해
Player별 및 전체 평균 Jitter 지표로 사용한다.

중요:

```text
SIM CONFIG Additional Jitter
```

와

```text
Measured RTT Jitter
```

는 서로 다른 값이다.

Additional Jitter는 Fusion에 설정한 인위적 입력값이고,
Measured RTT Jitter는 실제 RTT 변화에서 계산한 관찰값이다.

---

## 13. Player별 Network 정보

P0~P7 각각 다음 정보를 표시한다.

```text
RTT
Jitter
Height
Rank
Checkpoint
FINISH
Correction
Rollback
Resimulation
```

예:

```text
P3
RTT:184.2
Jit:24.7
H:9.60
R:2
CP:CP3
FIN:false
Corr:0.021
Roll:0.008
ReSim:14
```

---

## 14. Correction 진단

기존 `ProjectJNetworkPlayer`에 기록되는:

```text
LastCorrectionDistance
MaxCorrectionDistance
```

를 그대로 사용한다.

79일차에서는 네트워크 조건이 악화될수록
Correction 값이 어떻게 변하는지 확인한다.

Correction 자체가 발생하는 것은
Prediction 기반 네트워크에서 무조건 오류를 의미하지 않는다.

다음과 같이 실제 화면 문제와 함께 판단한다.

```text
Correction 증가
+
캐릭터 순간이동 반복
+
입력 불안정
```

이 동시에 발생하면 추가 분석 대상이다.

---

## 15. Rollback 진단

기존:

```text
LastRollbackDistance
```

를 F6 화면에 표시한다.

특히 다음 상황에서 확인한다.

- 8명 동시 이동
- Jump
- Sprint
- Push 난전
- Checkpoint 통과
- Respawn
- FINISH 접근

네트워크 상태가 나빠졌을 때
Rollback 거리 증가와 실제 위치 튐이 함께 발생하는지 비교한다.

---

## 16. Resimulation 진단

기존:

```text
ResimulationBatchCount
```

를 Player별로 표시하고
전체 Player의 합도 상단에서 확인한다.

지연 또는 손실 환경에서는
Prediction 보정을 위해 Resimulation이 증가할 수 있다.

따라서 단순히 값이 0보다 크다는 이유로 실패 판정하지 않는다.

대신 다음을 비교한다.

```text
NORMAL
↓
100ms
↓
150ms + Jitter + Loss
↓
200ms + Jitter + Loss
↓
250ms + Jitter + Loss
```

조건이 나빠질수록 증가하는 추세와
실제 플레이 품질을 함께 기록한다.

---

## 17. 8 Player Base Gate

F6 화면에서도 기존 8인 기본 Gate를 확인한다.

정상:

```text
Players : 8 / 8
Objects : 8 / 8

8P BASE GATE : PASS
```

악조건 테스트를 시작하기 전에
항상 이 상태를 먼저 확인한다.

---

## 18. 권장 테스트 순서

각 Preset은 다음 순서로 테스트한다.

```text
A. NORMAL
↓
B. 100ms
↓
C. 150ms급 + Jitter + 1%
↓
D. 200ms급 + Jitter + 3%
↓
E. 250ms급 + Jitter + 5%
```

각 단계에서 동일한 경기 동작을 반복한다.

---

## 19. 기본 테스트 절차

```text
8명 Session 접속
↓
8P BASE GATE PASS 확인
↓
Host GAME START
↓
3초 Countdown
↓
8인 이동
↓
Jump / Sprint / Crouch
↓
Push
↓
Checkpoint
↓
Respawn
↓
Rank
↓
FINISH
```

네트워크 조건만 변경하고
게임 행동 순서는 최대한 동일하게 유지한다.

---

## 20. 이동 테스트

각 Preset에서 다음을 반복한다.

```text
8명 동시 이동
8명 동시 Jump
8명 동시 Sprint
8명 동시 Crouch
```

관찰 대상:

- Local Player 입력 반응
- Remote Player 이동 부드러움
- 순간이동
- 위치 떨림
- Correction
- Rollback
- Resimulation
- FPS

---

## 21. Push 테스트

CP1 Push Arena에서 다음을 확인한다.

```text
8인 밀집
↓
여러 명 동시 Push
↓
가장 가까운 Target 선택
```

악조건에서도 최종 Push Target과 결과는
Host Authority 기준으로 모든 Client에서 일치해야 한다.

특히 확인:

- 가장 가까운 대상
- Shield 대상
- Respawn Protection 대상
- 여러 Push 동시 발생

---

## 22. Checkpoint 테스트

악조건 상태에서 각 Player를
서로 다른 Checkpoint로 이동시킨다.

예:

```text
P0 → Start
P1 → CP1
P2 → CP1
P3 → CP2
P4 → CP2
P5 → CP3
P6 → CP4
P7 → CP4
```

모든 Client의 F6 화면에서
최종 CP 상태가 일치해야 한다.

---

## 23. Respawn 테스트

다수 Player를 거의 동시에 추락시킨다.

확인:

```text
각 Player
→ 자신의 Checkpoint Respawn

각 Player
→ 3초 Respawn Protection
```

네트워크 손실 때문에
Checkpoint 상태가 누락되거나
다른 Player의 Respawn Point를 사용해서는 안 된다.

---

## 24. Rank 테스트

네트워크 상태가 나쁜 경우
화면에서 Remote Player의 위치는 순간적으로 차이가 날 수 있다.

하지만 Host가 확정한 경기 Rank는
결국 모든 Client에서 동일해야 한다.

특히 공동순위를 포함해 확인한다.

---

## 25. FINISH 테스트

FINISH는 79일차에서 가장 중요한 결과 검증 대상 중 하나다.

예:

```text
1. P4
2. P1
3. P7
4. P3
5. P0
6. P5
7. P2
8. P6
```

와 같이 의도적으로 순서를 정해 완주한다.

모든 Client에서
동일한 최종 FINISH 순서를 유지해야 한다.

일시적인 화면 표시 차이는 발생할 수 있지만
최종 Network Authority 결과가 달라져서는 안 된다.

---

## 26. Packet Loss 판정 기준

Packet Loss 환경에서 화면이 약간 덜 부드러운 것은
테스트 조건에 따라 허용할 수 있다.

하지만 다음은 실패로 본다.

- Push 결과 영구 불일치
- Checkpoint 누락
- 잘못된 Respawn 위치
- Rank 영구 불일치
- FINISH 순서 불일치
- PlayerObject 소실
- Session 비정상 종료
- Console Network Error 반복

---

## 27. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay79NetworkConditionPresetWindow.cs
└─ ProjectJDay79NetworkConditionPresetWindow.cs.meta

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay79NetworkConditionDebugView.cs
└─ ProjectJDay79NetworkConditionDebugView.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay78EightPlayerDebugView.cs
```

변경 내용:

```text
F5 Debug View
기본 표시 → 기본 숨김
```

### 삭제 파일

```text
없음
```

### Scene 수정

```text
없음
```

---

## 28. 최신 커밋 검토 결과

최신 GitHub 커밋에서
78일차 완료 커밋 대비 79일차 변경을 확인했다.

변경 범위는 다음과 같다.

- Day79 Network Condition Preset Editor Window 추가
- Day79 F6 Network Condition Debug View 추가
- Day78 F5 View 기본 숨김 처리
- Game Scene 및 Gameplay 코드 변경 없음

현재 사용한 Photon Fusion 2 API는 다음 항목과 연결된다.

```text
NetworkProjectConfig.Global
NetworkProjectConfig.NetworkConditions

NetworkProjectConfigAsset.Global
NetworkProjectConfigAsset.Config

NetworkSimulationConfiguration
Enabled
DelayMin
DelayMax
DelayPeriod
DelayThreshold
AdditionalJitter
LossChanceMin
LossChanceMax
LossChancePeriod
LossChanceThreshold
AdditionalLoss

NetworkRunner.GetPlayerRtt()
```

현재 Fusion 2 API 기준으로 해당 타입과 필드는 제공되는 항목이다.

정적 코드와 API 연결 기준으로
즉시 수정해야 할 명백한 오류는 발견하지 못했다.

다만 최신 GitHub 커밋에는
Unity Compile 또는 멀티플레이 실행을 자동 검증하는
CI 상태 검사가 등록되어 있지 않다.

따라서 최종 완료 여부는
Unity Console 및 실제 Host/Client 테스트로 판정한다.

---

## 29. 79일차 완료 기준

다음 조건을 실제 실행에서 만족하면 완료한다.

- Unity Console Error 0
- Fusion Debug DLL 환경 확인
- Network Condition Preset 적용 가능
- A NORMAL 테스트 완료
- B 100ms 테스트 완료
- C Jitter + 1% Loss 테스트 완료
- D Jitter + 3% Loss 테스트 완료
- E 5% Loss 스트레스 환경 확인
- 8 Player Session 유지
- PlayerObjects = 8
- Host GAME START 정상
- Countdown 정상
- 이동 가능
- Push 최종 결과 일치
- Checkpoint 일치
- Respawn 일치
- Rank 일치
- FINISH 결과 일치
- RTT 측정 가능
- Jitter 진단값 표시
- Correction 표시
- Rollback 표시
- Resimulation 표시
- 심각한 Network Error 없음

---

## 30. 다음 개발 방향

80일차에서는 네트워크 악조건 검증 이후
Steam 인증과 Account ID 연결 단계로 진행한다.

주요 목표:

- Steam 초기화
- Steam 로그인 상태 확인
- Steam Account ID 획득
- Fusion Player와 Steam User 식별 연결
- Host/Client 사용자 구분
- 인증 실패 처리
- Debug UI에서 Steam User 확인

79일차까지는 네트워크 연결 자체와 경기 동기화를 중심으로 검증하고,
80일차부터 실제 사용자 계정 식별을 온라인 경기 흐름에 연결한다.
