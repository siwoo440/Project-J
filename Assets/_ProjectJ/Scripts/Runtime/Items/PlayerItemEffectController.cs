using System.Collections.Generic; // 복어 풍선옷 대상 중복 방지 기능 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 이동과 외부 힘 기능 참조
using UnityEngine; // Unity 시간과 물리 기능 참조
using UnityEngine.InputSystem; // 추가 점프와 제트팩과 탈출 입력 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 효과 관리자 한 개만 허용
    [RequireComponent(typeof(PlayerMovementController))] // 플레이어 이동 관리자 필수 지정
    [RequireComponent(typeof(PlayerExternalForceController))] // 플레이어 외부 힘 관리자 필수 지정
    public sealed class PlayerItemEffectController : MonoBehaviour // P0와 P1 지속형 아이템 효과 관리자 선언
    { // 지속형 아이템 효과 관리자 묶음
        [SerializeField] private PlayerMovementController movementController; // 이동과 접지 상태 제공자 저장
        [SerializeField] private PlayerExternalForceController externalForceController; // 추가 이동과 방어 대상 저장
        [SerializeField] private PlayerRespawnController respawnController; // 부활 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private PlayerInputReader inputReader; // 비눗방울 이동 제한 대상 저장
        [SerializeField] private PlayerScreenObscureView screenObscureView; // 먹물과 연막과 비눗방울 화면 표시 저장
        [SerializeField] private LayerMask proximityEffectLayers = ~0; // 복어 풍선옷 접촉 대상 Layer 저장

        private float springShoesRemaining; // 스프링 신발 남은 시간 저장
        private float springJumpVelocity; // 스프링 신발 추가 상승 속도 저장
        private bool springJumpUsed; // 현재 효과의 추가 점프 사용 여부 저장
        private float jellyShieldRemaining; // 젤리 보호막 남은 시간 저장
        private bool ownsExternalForceDisable; // 보호막이 외부 힘 컴포넌트를 끈 상태 저장
        private float featherShoesRemaining; // 깃털 신발 남은 시간 저장
        private float featherSpeedMultiplier = 1f; // 깃털 신발 이동 배율 저장
        private float slowRemaining; // 눈덩이 감속 남은 시간 저장
        private float slowMultiplier = 1f; // 눈덩이 감속 이동 배율 저장
        private float jetpackRemaining; // 제트팩 남은 연료 시간 저장
        private float jetpackAscentSpeed; // 제트팩 Space 상승 속도 저장
        private float hammerRemaining; // 망치 강화 남은 시간 저장
        private float hammerForceMultiplier = 1f; // 망치 밀치기 힘 배율 저장
        private float hammerRange; // 망치 강화 사거리 저장
        private float pufferSuitRemaining; // 복어 풍선옷 남은 시간 저장
        private float pufferSuitForce; // 복어 풍선옷 밀치기 힘 저장
        private float pufferSuitRadius; // 복어 풍선옷 접촉 반경 저장
        private float pufferSuitInterval; // 복어 풍선옷 반복 판정 간격 저장
        private float pufferSuitCooldownRemaining; // 다음 복어 풍선옷 판정 대기 시간 저장
        private float giantBalloonRemaining; // 거대 풍선 남은 자동 상승 시간 저장
        private float giantBalloonAscentSpeed; // 거대 풍선 자동 상승 속도 저장
        private Vector3 grapplingAnchor; // 갈고리 이동 목표 위치 저장
        private float grapplingSpeed; // 갈고리 이동 속도 저장
        private float grapplingRemaining; // 갈고리 최대 이동 시간 저장
        private int soapBubbleRequiredInputs; // 비눗방울 탈출에 필요한 교대 입력 수 저장
        private int soapBubbleInputCount; // 비눗방울 현재 교대 입력 수 저장
        private int soapBubbleLastDirection; // 비눗방울 마지막 A 또는 D 방향 저장
        private bool soapBubbleActive; // 비눗방울 조작 제한 활성 여부 저장
        private bool wasRespawning; // 직전 프레임 부활 상태 저장

        public bool IsSpringShoesActive => springShoesRemaining > 0f; // 스프링 신발 활성 여부 반환
        public bool IsJellyShieldActive => jellyShieldRemaining > 0f; // 젤리 보호막 활성 여부 반환
        public bool IsFeatherShoesActive => featherShoesRemaining > 0f; // 깃털 신발 활성 여부 반환
        public bool IsSlowed => slowRemaining > 0f; // 눈덩이 감속 활성 여부 반환
        public bool IsJetpackActive => jetpackRemaining > 0f; // 제트팩 활성 여부 반환
        public bool IsHammerActive => hammerRemaining > 0f; // 망치 강화 활성 여부 반환
        public bool IsPufferBalloonSuitActive => pufferSuitRemaining > 0f; // 복어 풍선옷 활성 여부 반환
        public bool IsGiantBalloonActive => giantBalloonRemaining > 0f; // 거대 풍선 활성 여부 반환
        public bool IsGrappling => grapplingRemaining > 0f; // 갈고리 이동 활성 여부 반환
        public bool IsSoapBubbleActive => soapBubbleActive; // 비눗방울 조작 제한 활성 여부 반환
        public float CurrentPushForceMultiplier => IsHammerActive ? Mathf.Max(1f, hammerForceMultiplier) : 1f; // 망치 상태를 반영한 밀치기 힘 배율 반환
        public float CurrentPushRange => IsHammerActive ? Mathf.Max(0f, hammerRange) : 0f; // 망치 상태를 반영한 밀치기 사거리 반환

        private void Awake() // 실행 시작 시 플레이어 참조 준비
        { // 플레이어 참조 준비 처리
            ResolveReferences(); // 같은 플레이어와 Scene에서 누락 참조 자동 검색
        } // 플레이어 참조 준비 처리 종료

        private void Update() // 지속형 효과 시간과 입력과 근접 효과 갱신
        { // 지속형 효과 프레임 처리
            if (matchController != null && matchController.IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 종료 효과 정리 처리
                ClearAllEffects(); // 남은 아이템 효과 전체 제거
                return; // 지속형 효과 갱신 종료
            } // 경기 종료 효과 정리 처리 종료

            bool isRespawning = respawnController != null && respawnController.IsRespawning; // 현재 부활 진행 여부 조회

            if (isRespawning && !wasRespawning) // 새 부활 시작 프레임 여부 확인
            { // 부활 시작 효과 정리 처리
                ClearAllEffects(); // 사망 전 지속 효과 전체 제거
            } // 부활 시작 효과 정리 처리 종료

            wasRespawning = isRespawning; // 현재 부활 상태 저장

            if (isRespawning) // 부활 진행 중 여부 확인
            { // 부활 중 갱신 생략 처리
                return; // 지속 효과 입력 처리 중단
            } // 부활 중 갱신 생략 처리 종료

            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            springShoesRemaining = Mathf.Max(0f, springShoesRemaining - deltaTime); // 스프링 신발 남은 시간 감소
            jellyShieldRemaining = Mathf.Max(0f, jellyShieldRemaining - deltaTime); // 젤리 보호막 남은 시간 감소
            featherShoesRemaining = Mathf.Max(0f, featherShoesRemaining - deltaTime); // 깃털 신발 남은 시간 감소
            slowRemaining = Mathf.Max(0f, slowRemaining - deltaTime); // 눈덩이 감속 남은 시간 감소
            jetpackRemaining = Mathf.Max(0f, jetpackRemaining - deltaTime); // 제트팩 남은 연료 시간 감소
            hammerRemaining = Mathf.Max(0f, hammerRemaining - deltaTime); // 망치 남은 강화 시간 감소
            pufferSuitRemaining = Mathf.Max(0f, pufferSuitRemaining - deltaTime); // 복어 풍선옷 남은 시간 감소
            giantBalloonRemaining = Mathf.Max(0f, giantBalloonRemaining - deltaTime); // 거대 풍선 남은 시간 감소
            grapplingRemaining = Mathf.Max(0f, grapplingRemaining - deltaTime); // 갈고리 남은 이동 시간 감소
            pufferSuitCooldownRemaining = Mathf.Max(0f, pufferSuitCooldownRemaining - deltaTime); // 복어 풍선옷 다음 판정 시간 감소
            UpdateJellyShieldState(); // 보호막 기반 외부 힘 수신 상태 갱신
            TryPerformExtraJump(); // 공중 추가 점프 입력 처리
            UpdateSoapBubbleEscape(); // A와 D 교대 비눗방울 탈출 입력 처리
            TryApplyPufferBalloonSuit(); // 복어 풍선옷 근접 밀치기 처리
            UpdateGrapplingState(); // 갈고리 목표 도달 상태 처리
        } // 지속형 효과 프레임 처리 종료

        private void LateUpdate() // 이동 계산 뒤 아이템 전달 속도 보정
        { // 아이템 전달 속도 보정 처리
            if (externalForceController == null || !externalForceController.enabled || movementController == null) // 이동과 외부 힘 참조와 보호막 상태 확인
            { // 속도 보정 불가 처리
                return; // 이동 속도 보정 생략
            } // 속도 보정 불가 처리 종료

            float featherMultiplier = IsFeatherShoesActive ? Mathf.Max(0f, featherSpeedMultiplier) : 1f; // 현재 깃털 신발 배율 계산
            float currentSlowMultiplier = IsSlowed ? Mathf.Clamp(slowMultiplier, 0.1f, 1f) : 1f; // 현재 감속 배율 계산
            float combinedMultiplier = featherMultiplier * currentSlowMultiplier; // 강화와 감속을 곱한 최종 배율 계산
            Vector3 carrierVelocity = movementController.ControlledHorizontalVelocity * (combinedMultiplier - 1f); // 기본 이동에 더하거나 뺄 보정 속도 계산

            if (IsJetpackActive && Keyboard.current != null && Keyboard.current.spaceKey.isPressed) // 제트팩 활성 중 Space 유지 여부 확인
            { // 제트팩 상승 처리
                carrierVelocity += Vector3.up * jetpackAscentSpeed; // Space 유지 중 제트팩 상승 속도 추가
            } // 제트팩 상승 처리 종료

            if (IsGiantBalloonActive) // 거대 풍선 자동 상승 상태 확인
            { // 거대 풍선 상승 처리
                carrierVelocity += Vector3.up * giantBalloonAscentSpeed; // 입력과 무관한 자동 상승 속도 추가
            } // 거대 풍선 상승 처리 종료

            if (IsGrappling) // 갈고리 이동 상태 확인
            { // 갈고리 목표 이동 처리
                Vector3 grapplingDirection = grapplingAnchor - transform.position; // 현재 위치에서 갈고리 목표 방향 계산
                carrierVelocity += grapplingDirection.sqrMagnitude <= 0.0001f ? Vector3.zero : grapplingDirection.normalized * grapplingSpeed; // 목표 방향 이동 속도 추가
            } // 갈고리 목표 이동 처리 종료

            if (carrierVelocity.sqrMagnitude > 0.0001f) // 유효한 속도 보정 여부 확인
            { // 속도 보정 적용 처리
                externalForceController.ApplyPlatformVelocity(carrierVelocity); // 발판 전달 속도 방식으로 아이템 이동 적용
            } // 속도 보정 적용 처리 종료
        } // 아이템 전달 속도 보정 처리 종료

        private void OnDisable() // 효과 관리자 비활성화 시 상태 복원
        { // 효과 관리자 비활성화 정리 처리
            ClearAllEffects(); // 보호막과 지속 효과 전체 제거
        } // 효과 관리자 비활성화 정리 처리 종료

        public void ActivateSpringShoes(float duration, float extraJumpVelocity) // 스프링 신발 추가 점프 활성화
        { // 스프링 신발 활성화 처리
            springShoesRemaining = Mathf.Max(springShoesRemaining, duration); // 더 긴 남은 시간 유지
            springJumpVelocity = Mathf.Max(0f, extraJumpVelocity); // 추가 상승 속도 저장
            springJumpUsed = false; // 새 효과의 추가 점프 사용 상태 초기화
        } // 스프링 신발 활성화 처리 종료

        public void ActivateJellyShield(float duration) // 젤리 보호막 밀치기 방어 활성화
        { // 젤리 보호막 활성화 처리
            jellyShieldRemaining = Mathf.Max(jellyShieldRemaining, duration); // 더 긴 남은 시간 유지
            UpdateJellyShieldState(); // 즉시 외부 힘 수신 차단 적용
        } // 젤리 보호막 활성화 처리 종료

        public void ActivateFeatherShoes(float duration, float speedMultiplier) // 깃털 신발 이동 속도 강화 활성화
        { // 깃털 신발 활성화 처리
            featherShoesRemaining = Mathf.Max(featherShoesRemaining, duration); // 더 긴 남은 시간 유지
            featherSpeedMultiplier = Mathf.Max(1f, speedMultiplier); // 최소 기본 속도 이상의 배율 저장
        } // 깃털 신발 활성화 처리 종료

        public void ApplySlow(float duration, float speedMultiplier) // 눈덩이 감속 효과 적용
        { // 눈덩이 감속 적용 처리
            slowRemaining = Mathf.Max(slowRemaining, duration); // 더 긴 감속 시간 유지
            slowMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f); // 안전 범위 감속 배율 저장
        } // 눈덩이 감속 적용 처리 종료

        public void ActivateJetpack(float duration, float ascentSpeed) // Space 유지 방식 제트팩 활성화
        { // 제트팩 활성화 처리
            jetpackRemaining = Mathf.Max(jetpackRemaining, duration); // 더 긴 연료 시간 유지
            jetpackAscentSpeed = Mathf.Max(0f, ascentSpeed); // 제트팩 상승 속도 저장
        } // 제트팩 활성화 처리 종료

        public void ActivateHammer(float duration, float forceMultiplier, float range) // 망치 밀치기 강화 활성화
        { // 망치 강화 처리
            hammerRemaining = Mathf.Max(hammerRemaining, duration); // 더 긴 강화 시간 유지
            hammerForceMultiplier = Mathf.Max(1f, forceMultiplier); // 최소 기본 힘 이상의 배율 저장
            hammerRange = Mathf.Max(0f, range); // 음수가 없는 강화 사거리 저장
        } // 망치 강화 처리 종료

        public void ActivatePufferBalloonSuit(float duration, float force, float radius, float interval) // 복어 풍선옷 접촉 밀치기 활성화
        { // 복어 풍선옷 활성화 처리
            pufferSuitRemaining = Mathf.Max(pufferSuitRemaining, duration); // 더 긴 풍선옷 시간 유지
            pufferSuitForce = Mathf.Max(0f, force); // 풍선옷 밀치기 힘 저장
            pufferSuitRadius = Mathf.Max(0.1f, radius); // 풍선옷 접촉 반경 저장
            pufferSuitInterval = Mathf.Max(0.05f, interval); // 풍선옷 반복 판정 간격 저장
            pufferSuitCooldownRemaining = 0f; // 사용 직후 첫 접촉 판정 허용
        } // 복어 풍선옷 활성화 처리 종료

        public void ActivateGrapplingHook(Vector3 anchorPosition, float movementSpeed) // 구조물 목표 갈고리 이동 활성화
        { // 갈고리 이동 활성화 처리
            grapplingAnchor = anchorPosition; // 갈고리 목표 위치 저장
            grapplingSpeed = Mathf.Max(0.1f, movementSpeed); // 최소 갈고리 이동 속도 저장
            float distance = Vector3.Distance(transform.position, grapplingAnchor); // 현재 위치와 갈고리 목표 거리 계산
            grapplingRemaining = Mathf.Max(0.1f, distance / grapplingSpeed + 0.25f); // 목표 거리 기반 안전 이동 시간 저장
        } // 갈고리 이동 활성화 처리 종료

        public void ActivateGiantBalloon(float duration, float ascentSpeed) // 거대 풍선 자동 상승 활성화
        { // 거대 풍선 활성화 처리
            giantBalloonRemaining = Mathf.Max(giantBalloonRemaining, duration); // 더 긴 자동 상승 시간 유지
            giantBalloonAscentSpeed = Mathf.Max(0f, ascentSpeed); // 거대 풍선 상승 속도 저장
        } // 거대 풍선 활성화 처리 종료

        public void ApplyInk(float duration, float centerCoverage) // 먹물 문어 화면 중앙 가림 적용
        { // 먹물 화면 방해 처리
            screenObscureView?.ShowInk(duration, centerCoverage); // 대상 화면에 먹물 표시 적용
        } // 먹물 화면 방해 처리 종료

        public void ApplySmoke(float refreshDuration) // 연막 구역 내부 화면 방해 갱신
        { // 연막 화면 방해 처리
            screenObscureView?.RefreshSmoke(refreshDuration); // 대상 화면 연막 표시 갱신
        } // 연막 화면 방해 처리 종료

        public void ApplySoapBubble(int requiredAlternatingInputs) // A와 D 교대 탈출 비눗방울 적용
        { // 비눗방울 조작 제한 처리
            soapBubbleRequiredInputs = Mathf.Max(1, requiredAlternatingInputs); // 최소 탈출 입력 횟수 보정
            soapBubbleInputCount = 0; // 새 탈출 진행도 초기화
            soapBubbleLastDirection = 0; // 첫 교대 방향 초기화
            soapBubbleActive = true; // 비눗방울 제한 활성화
            inputReader?.SetItemMovementRestricted(true); // 이동과 달리기와 앉기 입력 제한
            screenObscureView?.ShowBubbleProgress(soapBubbleInputCount, soapBubbleRequiredInputs); // 최초 탈출 진행도 표시
        } // 비눗방울 조작 제한 처리 종료

        public void ClearAllEffects() // 부활과 경기 종료 시 모든 지속 효과 제거
        { // 모든 지속 효과 제거 처리
            springShoesRemaining = 0f; // 스프링 신발 시간 제거
            springJumpUsed = false; // 추가 점프 사용 상태 제거
            jellyShieldRemaining = 0f; // 젤리 보호막 시간 제거
            featherShoesRemaining = 0f; // 깃털 신발 시간 제거
            slowRemaining = 0f; // 눈덩이 감속 시간 제거
            jetpackRemaining = 0f; // 제트팩 연료 시간 제거
            hammerRemaining = 0f; // 망치 강화 시간 제거
            pufferSuitRemaining = 0f; // 복어 풍선옷 시간 제거
            giantBalloonRemaining = 0f; // 거대 풍선 상승 시간 제거
            grapplingRemaining = 0f; // 갈고리 이동 시간 제거
            ReleaseSoapBubble(); // 비눗방울 입력 제한 제거
            screenObscureView?.ClearAll(); // 먹물과 연막과 비눗방울 표시 제거
            RestoreExternalForceController(); // 보호막이 끈 외부 힘 컴포넌트 복원
        } // 모든 지속 효과 제거 처리 종료

        private void TryPerformExtraJump() // 스프링 신발 공중 추가 점프 입력 처리
        { // 공중 추가 점프 처리
            if (!IsSpringShoesActive || springJumpUsed || movementController == null || movementController.IsGrounded) // 효과와 사용 여부와 공중 상태 확인
            { // 추가 점프 불가 처리
                return; // 추가 점프 처리 생략
            } // 추가 점프 불가 처리 종료

            if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) // 현재 프레임 Space 입력 여부 확인
            { // 추가 점프 입력 없음 처리
                return; // 추가 점프 처리 생략
            } // 추가 점프 입력 없음 처리 종료

            if (externalForceController != null && externalForceController.enabled && externalForceController.ApplyObstacleImpulse(Vector3.up, springJumpVelocity)) // 위쪽 추가 상승 힘 적용 성공 여부 확인
            { // 추가 점프 성공 처리
                springJumpUsed = true; // 현재 효과 추가 점프 사용 완료 저장
            } // 추가 점프 성공 처리 종료
        } // 공중 추가 점프 처리 종료

        private void TryApplyPufferBalloonSuit() // 복어 풍선옷 근접 대상 반복 밀치기
        { // 복어 풍선옷 근접 효과 처리
            if (!IsPufferBalloonSuitActive || pufferSuitCooldownRemaining > 0f) // 효과 비활성 또는 재판정 대기 여부 확인
            { // 근접 판정 생략 처리
                return; // 복어 풍선옷 판정 종료
            } // 근접 판정 생략 처리 종료

            pufferSuitCooldownRemaining = pufferSuitInterval; // 다음 근접 판정 대기 시간 적용
            Collider[] overlaps = Physics.OverlapSphere(transform.position, pufferSuitRadius, proximityEffectLayers, QueryTriggerInteraction.Ignore); // 풍선옷 반경 안 Collider 수집
            HashSet<int> affectedIds = new HashSet<int>(); // 같은 대상 중복 밀치기 방지 목록 생성

            for (int index = 0; index < overlaps.Length; index++) // 근접 Collider 전체 순회
            { // 현재 근접 대상 처리
                ExternalForceReceiver receiver = overlaps[index] == null ? null : overlaps[index].GetComponentInParent<ExternalForceReceiver>(); // 현재 외부 힘 수신 대상 조회

                if (receiver == null || receiver.transform == transform || !receiver.CanReceivePush || affectedIds.Contains(receiver.GetInstanceID())) // 자기 자신과 수신 불가와 중복 대상 확인
                { // 무효 근접 대상 처리
                    continue; // 현재 대상 생략
                } // 무효 근접 대상 처리 종료

                Vector3 outwardDirection = receiver.ForceReceiverTransform.position - transform.position; // 사용자에서 대상 바깥쪽 방향 계산
                outwardDirection = Vector3.ProjectOnPlane(outwardDirection, Vector3.up); // 수평 밀치기 방향으로 보정

                if (outwardDirection.sqrMagnitude <= 0.0001f) // 방향 계산 실패 여부 확인
                { // 대체 방향 처리
                    outwardDirection = transform.forward; // 사용자 전방 대체 방향 적용
                } // 대체 방향 처리 종료

                receiver.TryReceiveExternalForce(outwardDirection.normalized, pufferSuitForce); // 복어 풍선옷 바깥쪽 밀치기 적용
                affectedIds.Add(receiver.GetInstanceID()); // 현재 대상 적용 완료 등록
            } // 현재 근접 대상 처리 종료
        } // 복어 풍선옷 근접 효과 처리 종료

        private void UpdateSoapBubbleEscape() // A와 D 교대 여섯 번 탈출 입력 갱신
        { // 비눗방울 탈출 처리
            if (!soapBubbleActive || Keyboard.current == null) // 비눗방울과 키보드 상태 확인
            { // 탈출 입력 생략 처리
                return; // 비눗방울 탈출 처리 종료
            } // 탈출 입력 생략 처리 종료

            int newDirection = Keyboard.current.aKey.wasPressedThisFrame ? -1 : Keyboard.current.dKey.wasPressedThisFrame ? 1 : 0; // 현재 프레임 A 또는 D 방향 계산

            if (newDirection == 0) // 새 A 또는 D 입력 없음 여부 확인
            { // 탈출 진행 유지 처리
                return; // 입력 누적 생략
            } // 탈출 진행 유지 처리 종료

            soapBubbleInputCount = P1ItemRules.RegisterAlternatingEscapeInput(soapBubbleInputCount, soapBubbleLastDirection, newDirection, soapBubbleRequiredInputs, out soapBubbleLastDirection); // 교대 입력 규칙 기반 탈출 횟수 누적
            screenObscureView?.ShowBubbleProgress(soapBubbleInputCount, soapBubbleRequiredInputs); // 새 탈출 진행도 표시

            if (soapBubbleInputCount >= soapBubbleRequiredInputs) // 필요한 교대 입력 완료 여부 확인
            { // 비눗방울 탈출 완료 처리
                ReleaseSoapBubble(); // 이동 제한과 화면 표시 해제
            } // 비눗방울 탈출 완료 처리 종료
        } // 비눗방울 탈출 처리 종료

        private void ReleaseSoapBubble() // 비눗방울 조작 제한과 표시 해제
        { // 비눗방울 해제 처리
            soapBubbleActive = false; // 비눗방울 활성 상태 제거
            soapBubbleInputCount = 0; // 탈출 진행도 초기화
            soapBubbleLastDirection = 0; // 마지막 교대 방향 초기화
            inputReader?.SetItemMovementRestricted(false); // 이동과 달리기와 앉기 입력 복원
            screenObscureView?.HideBubble(); // 비눗방울 화면 표시 제거
        } // 비눗방울 해제 처리 종료

        private void UpdateGrapplingState() // 갈고리 목표 도달과 시간 만료 처리
        { // 갈고리 이동 상태 처리
            if (!IsGrappling) // 갈고리 이동 비활성 여부 확인
            { // 갈고리 이동 생략 처리
                return; // 목표 확인 생략
            } // 갈고리 이동 생략 처리 종료

            if (Vector3.Distance(transform.position, grapplingAnchor) <= 1.2f) // 갈고리 목표 근접 여부 확인
            { // 갈고리 이동 완료 처리
                grapplingRemaining = 0f; // 남은 갈고리 이동 시간 제거
            } // 갈고리 이동 완료 처리 종료
        } // 갈고리 이동 상태 처리 종료

        private void UpdateJellyShieldState() // 젤리 보호막 기반 외부 힘 수신 상태 갱신
        { // 젤리 보호막 상태 갱신 처리
            if (externalForceController == null) // 외부 힘 관리자 누락 여부 확인
            { // 보호막 적용 불가 처리
                return; // 보호막 상태 갱신 생략
            } // 보호막 적용 불가 처리 종료

            if (IsJellyShieldActive) // 보호막 활성 여부 확인
            { // 외부 힘 수신 차단 처리
                externalForceController.ClearVelocity(); // 보호막 발동 전 남은 외부 힘 제거

                if (externalForceController.enabled) // 기존 활성 상태 여부 확인
                { // 보호막 소유 비활성화 처리
                    externalForceController.enabled = false; // 새 외부 힘 수신 차단
                    ownsExternalForceDisable = true; // 보호막이 비활성화한 상태 기록
                } // 보호막 소유 비활성화 처리 종료

                return; // 보호막 해제 처리 생략
            } // 외부 힘 수신 차단 처리 종료

            RestoreExternalForceController(); // 보호막 종료 뒤 외부 힘 수신 복원
        } // 젤리 보호막 상태 갱신 처리 종료

        private void RestoreExternalForceController() // 보호막이 비활성화한 외부 힘 관리자 복원
        { // 외부 힘 관리자 복원 처리
            if (externalForceController != null && ownsExternalForceDisable) // 보호막 소유 비활성화 상태 확인
            { // 외부 힘 관리자 활성화 처리
                externalForceController.enabled = true; // 외부 힘 수신 다시 허용
            } // 외부 힘 관리자 활성화 처리 종료

            ownsExternalForceDisable = false; // 보호막 소유 상태 초기화
        } // 외부 힘 관리자 복원 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            movementController = movementController == null ? GetComponent<PlayerMovementController>() : movementController; // 같은 오브젝트 이동 관리자 저장
            externalForceController = externalForceController == null ? GetComponent<PlayerExternalForceController>() : externalForceController; // 같은 오브젝트 외부 힘 관리자 저장
            respawnController = respawnController == null ? GetComponent<PlayerRespawnController>() : respawnController; // 같은 오브젝트 부활 관리자 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
            inputReader = inputReader == null ? GetComponent<PlayerInputReader>() : inputReader; // 같은 오브젝트 입력 관리자 저장
            screenObscureView = screenObscureView == null ? GetComponent<PlayerScreenObscureView>() : screenObscureView; // 같은 오브젝트 화면 방해 표시 조회
            screenObscureView = screenObscureView == null ? gameObject.AddComponent<PlayerScreenObscureView>() : screenObscureView; // 누락된 화면 방해 표시 자동 추가
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerMovementController newMovementController, PlayerExternalForceController newExternalForceController, PlayerRespawnController newRespawnController, PrototypeMatchController newMatchController) // 자동 설정 도구용 효과 참조 연결
        { // 자동 설정 도구용 효과 참조 연결 처리
            movementController = newMovementController; // 이동 관리자 저장
            externalForceController = newExternalForceController; // 외부 힘 관리자 저장
            respawnController = newRespawnController; // 부활 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
            inputReader = GetComponent<PlayerInputReader>(); // 같은 플레이어 입력 관리자 저장
            screenObscureView = GetComponent<PlayerScreenObscureView>(); // 같은 플레이어 화면 방해 표시 저장
            proximityEffectLayers = ~0; // 복어 풍선옷 모든 현재 물리 Layer 검사 적용
        } // 자동 설정 도구용 효과 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 지속형 아이템 효과 관리자 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
