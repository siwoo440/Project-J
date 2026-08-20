# 26일차 개발일지 - 발 기준 플레이어 높이 계산 시스템

## 1. 개발 목표

26일차의 목표는 플레이어의 현재 진행 높이를 **캐릭터 중심이 아니라 발 위치 기준으로 계산하는 공식 높이 시스템**을 만드는 것이다.

이 값은 이후 실시간 순위, 공동 순위, 최고 높이 기록, 경기 결과, 네트워크 동기화의 기준값으로 사용할 예정이다.

현재 기준 커밋:

```text
637fc7dd2c4a41f6321629d5b4526a9a9b38fd25
```

현재 커밋 메시지:

```text
26
```

---

# 2. 높이 계산 기준

Project J의 공식 높이 기준은 다음과 같다.

```text
플레이어의 발 위치
→ World Y
→ 소수점 둘째 자리까지 유지
→ 셋째 자리 이하 버림
```

예:

```text
0.009   → 0.00
0.019   → 0.01
12.345  → 12.34
199.999 → 199.99
999.999 → 999.99
1000    → 1000.00
```

반올림하지 않는다.

높이를 먼저 100배 한 뒤 소수 부분을 제거하고 정수값으로 저장한다.

---

# 3. 발 기준점 추가

기존 Player Prefab은 CapsuleCollider가:

```text
Height = 2
Center Y = 0
```

이므로 Player Transform 중심에서 실제 발 위치는 아래쪽 1m 지점이다.

따라서 Player Prefab에 다음 기준점을 추가했다.

```text
Player
└─ HeightReference_Foot
```

현재 위치:

```text
Local Position
X = 0
Y = -1
Z = 0
```

이 기준점의 World Y를 공식 플레이어 높이로 사용한다.

---

# 4. PlayerHeightTracker

새 스크립트:

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerHeightTracker.cs
```

Player Root에 부착한다.

주요 역할:

```text
HeightReference_Foot World Y 읽기
↓
RawHeight 저장
↓
소수점 둘째 자리 기준 정수값 변환
↓
CurrentHeight 저장
↓
HighestHeight 갱신
```

---

# 5. 저장 값

## RawHeight

가공 전 실제 발 World Y 값이다.

예:

```text
283.47891
```

## CurrentHeightCentimeters

공식 순위 비교에 사용할 정수값이다.

```text
283.47891
→ 28347
```

향후 순위 계산에서는 float를 직접 비교하기보다 이 값을 사용하는 것을 기준으로 한다.

## CurrentHeight

표시용 meter 값이다.

```text
28347
→ 283.47m
```

## HighestHeightCentimeters

경기 중 도달한 가장 높은 공식 높이값이다.

현재 높이가 떨어져도 감소하지 않는다.

## HighestHeight

최고 높이를 meter 단위로 변환한 값이다.

---

# 6. 현재 높이와 최고 높이 분리

예:

```text
현재 위치 = 450.78m

CurrentHeight = 450.78
HighestHeight = 450.78
```

플레이어가 낙하하여:

```text
현재 위치 = 250.12m
```

가 되면:

```text
CurrentHeight = 250.12
HighestHeight = 450.78
```

로 유지된다.

실시간 순위는 `CurrentHeightCentimeters`를 사용하고, 향후 경기 기록에는 `HighestHeightCentimeters`를 사용할 수 있다.

---

# 7. 음수 높이 처리

START 아래로 떨어지는 상황도 고려한다.

현재 구현은 소수점 이하를 0 방향으로 버린다.

예:

```text
-0.009 → 0.00
-0.019 → -0.01
-1.239 → -1.23
```

---

# 8. CapsuleCollider 기반 Fallback

`HeightReference_Foot`가 연결되지 않았을 경우에도 높이를 계산할 수 있도록 CapsuleCollider 기반 Fallback을 추가했다.

Capsule 방향에 따라 다음 축을 사용한다.

```text
Direction 0 → X
Direction 1 → Y
Direction 2 → Z
```

현재 Player는 Y 방향 Capsule을 사용하므로:

```text
Capsule Center
-
Capsule Height / 2
```

위치를 발 기준점으로 계산한다.

정상 Player Prefab에서는 `HeightReference_Foot`가 우선 사용된다.

---

# 9. Player Prefab 수정

기존:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

에 다음이 추가되었다.

```text
PlayerHeightTracker
HeightReference_Foot
```

`PlayerHeightTracker.heightReferenceFoot`에는 생성된 `HeightReference_Foot`가 연결되어 있다.

기존 이동, 점프, Sprint, Crouch, Ledge 관련 컴포넌트는 그대로 유지한다.

---

# 10. Editor 설정 도구

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/
└─ Day26PlayerHeightSetup.cs
```

메뉴:

```text
ProjectJ
→ Day26
→ Setup Player Foot Height Reference
```

실행 시:

```text
Player.prefab 열기
↓
CapsuleCollider 확인
↓
HeightReference_Foot 생성 또는 검색
↓
Collider 기준 발 위치 계산
↓
PlayerHeightTracker 추가 또는 검색
↓
Reference 연결
↓
Prefab 저장
```

순서로 자동 설정한다.

---

# 11. Crouch와 높이 기준

공식 높이는 Player Root나 머리 위치가 아니라 발 기준점을 사용한다.

따라서 같은 바닥에서:

```text
Standing
→ 200.00m

Crouch
→ 200.00m
```

처럼 높이값이 유지되는 것을 목표로 한다.

Crouch로 Collider 높이가 변경되더라도 경기 진행 높이는 발 위치를 기준으로 판단한다.

---

# 12. Jump와 낙하

점프 중에는 실제 발이 상승하므로 CurrentHeight도 실시간으로 증가한다.

예:

```text
0.00
0.42
1.18
1.95
...
```

낙하하면 반대로 CurrentHeight가 감소한다.

따라서 Project J의 실시간 순위는 단순히 현재 발판이나 체크포인트 번호가 아니라 실제 Player 발의 World Y를 기준으로 계산할 수 있다.

---

# 13. 25일차 맵과 연결

25일차 기준 고정맵은:

```text
START = 0m
CP1 = 200m
CP2 = 400m
CP3 = 600m
CP4 = 800m
FINISH = 1000m
```

구조를 사용한다.

26일차 높이 시스템은 이 좌표 기준과 직접 연결된다.

향후 플레이 테스트에서는 각 기준점에서:

```text
0.00
200.00
400.00
600.00
800.00
1000.00
```

근처의 발 높이가 정상적으로 계산되는지 검증한다.

---

# 14. EditMode 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerHeightTrackerTests.cs
```

주요 검증 항목:

- 양수 높이 소수점 둘째 자리 버림
- 음수 높이 처리
- Capsule Height 2 / Center 0일 때 발 Local Y = -1
- Player Root가 아닌 Foot Reference 기준 높이 사용
- 낙하 후 HighestHeight 유지
- meter 표시값이 둘째 자리 기준으로 변환됨

---

# 15. 주요 예시

```text
0.000
→ 0 cm
→ 0.00m
```

```text
12.345
→ 1234 cm
→ 12.34m
```

```text
283.47891
→ 28347 cm
→ 283.47m
```

```text
999.999
→ 99999 cm
→ 999.99m
```

```text
1000.000
→ 100000 cm
→ 1000.00m
```

---

# 16. 향후 순위 시스템 연결

27일차부터는 각 플레이어의:

```text
CurrentHeightCentimeters
```

를 비교하면 된다.

예:

```text
Player A = 52437
Player B = 52437
Player C = 51102
```

A와 B는 같은 공식 높이값을 가지므로 이후 공동 순위 판정에 사용할 수 있다.

즉 26일차에서는 순위를 계산하지 않고 **순위를 계산하기 위한 신뢰 가능한 높이 데이터**를 만드는 것에 집중했다.

---

# 17. 생성 및 수정 파일

새 파일:

```text
Assets/ProjectJ/Runtime/Player/PlayerHeightTracker.cs
Assets/ProjectJ/Editor/Day26PlayerHeightSetup.cs
Assets/ProjectJ/Tests/EditMode/PlayerHeightTrackerTests.cs
```

수정 파일:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

삭제 파일:

```text
없음
```

---

# 18. 현재 하지 않은 기능

26일차에는 다음 기능을 구현하지 않았다.

```text
실시간 순위 계산
공동 순위 계산
순위 UI
FINISH 순위 고정
체크포인트 활성화
Respawn
멀티플레이 높이 동기화
서버 권한 높이 확정
```

이번 일차는 공식 높이값 계산에만 집중한다.

---

# 19. 수동 검증 체크리스트

- [ ] Player Prefab에 `HeightReference_Foot` 존재
- [ ] `HeightReference_Foot Local Y = -1`
- [ ] Player Root에 `PlayerHeightTracker` 존재
- [ ] Height Reference Foot 연결 정상
- [ ] 같은 바닥에서 Crouch 전후 높이 동일
- [ ] Jump 중 CurrentHeight 증가
- [ ] 낙하 중 CurrentHeight 감소
- [ ] HighestHeight는 낙하 후 감소하지 않음
- [ ] 0m 기준 높이 정상
- [ ] 200 / 400 / 600 / 800m 기준 높이 정상
- [ ] 1000m FINISH 높이 정상
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 20. 개발 결과

26일차에서는 플레이어의 경기 진행 높이를 **Player Transform 중심이 아닌 발 위치**를 기준으로 계산하도록 구조를 추가했다.

최종 흐름:

```text
HeightReference_Foot
↓
World Y
↓
RawHeight
↓
소수점 셋째 자리 이하 버림
↓
CurrentHeightCentimeters
↓
CurrentHeight
```

동시에:

```text
CurrentHeightCentimeters
↓
HighestHeightCentimeters 갱신
```

구조를 추가했다.

이제 각 플레이어는 `0.00m` 단위의 공식 현재 높이와 최고 높이를 가질 수 있으며, 다음 일차의 실시간 순위 시스템에서 이 값을 직접 사용할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬의 EditMode / PlayMode 테스트와 Console Error 0을 기준으로 확인한다.
