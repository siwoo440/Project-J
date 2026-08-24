using Fusion; // Networked 상태와 TickTimer 사용
using ProjectJ.Items; // 폭죽 공통 정책 사용
using UnityEngine; // 물리 판정과 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 폭죽 네트워크 기능 분리
    {
        private const float FireworkSightHeight = 1f; // 벽 차폐 Ray 높이

        [Networked] // 폭죽 준비 상태 동기화
        private NetworkBool NetworkFireworkPreparing
        {
            get;
            set;
        }

        [Networked] // 폭죽 준비 시간 동기화
        private TickTimer NetworkFireworkPreparationTimer
        {
            get;
            set;
        }

        [Networked] // 준비 시작 시 부활 횟수 동기화
        private int NetworkFireworkStartRespawnCount
        {
            get;
            set;
        }

        [Networked] // 폭죽 발동 횟수 동기화
        private int NetworkFireworkActivationCount
        {
            get;
            set;
        }

        [Networked] // 폭죽 준비 취소 횟수 동기화
        private int NetworkFireworkCancellationCount
        {
            get;
            set;
        }

        [Networked] // 마지막 폭죽 유효 Target 수 동기화
        private int NetworkFireworkLastTargetCount
        {
            get;
            set;
        }

        public bool IsFireworkPreparing => NetworkFireworkPreparing; // 현재 준비 상태 조회
        public int FireworkActivationCount => NetworkFireworkActivationCount; // 발동 횟수 조회
        public int FireworkCancellationCount => NetworkFireworkCancellationCount; // 취소 횟수 조회
        public int FireworkLastTargetCount => NetworkFireworkLastTargetCount; // 마지막 Target 수 조회

        public float FireworkRemaining =>
            GetRemainingTime(NetworkFireworkPreparationTimer); // 남은 준비 시간 조회

        private void InitializeFireworkAuthority() // 폭죽 Networked 상태 초기화
        {
            NetworkFireworkPreparing = false; // 준비 상태 초기화
            NetworkFireworkPreparationTimer = TickTimer.None; // 준비 Timer 초기화
            NetworkFireworkStartRespawnCount = 0; // 시작 부활 횟수 초기화
            NetworkFireworkActivationCount = 0; // 발동 횟수 초기화
            NetworkFireworkCancellationCount = 0; // 취소 횟수 초기화
            NetworkFireworkLastTargetCount = 0; // Target 수 초기화
        }

        private bool UseFireworkAuthority() // 폭죽 준비 시작
        {
            bool canBegin = ProjectJFireworkPolicy.CanBeginPreparation( // 준비 시작 조건 계산
                Runner != null, // Runner 존재 여부
                externalGameplay != null && externalGameplay.GameplayInputAllowed, // 경기 입력 허용 여부
                NetworkFireworkPreparing // 기존 준비 여부
            );

            if (!canBegin) // 준비 시작 불가 검사
            {
                return false; // 아이템 소비 차단
            }

            NetworkFireworkPreparing = true; // 준비 상태 시작
            NetworkFireworkPreparationTimer = TickTimer.CreateFromSeconds( // 준비 Timer 생성
                Runner, // 현재 NetworkRunner
                ProjectJFireworkPolicy.PreparationSeconds // 확정 준비 시간
            );
            NetworkFireworkStartRespawnCount = externalGameplay.RespawnCount; // 시작 부활 횟수 저장
            NetworkFireworkLastTargetCount = 0; // 이전 Target 수 초기화

            Debug.Log( // 준비 시작 로그 출력
                "[Project J/Fusion] 107일차 폭죽 준비 / P" + OwnerIndex, // 준비 시작 정보
                this // 로그 대상
            );

            return true; // 준비 시작 성공
        }

        private void UpdateFireworkAuthority() // 폭죽 준비 상태 갱신
        {
            if (!NetworkFireworkPreparing) // 준비 상태 확인
            {
                return; // 비활성 상태 처리 생략
            }

            if (Runner == null || externalGameplay == null) // 필수 참조 확인
            {
                CancelFireworkPreparationAuthority(); // 잘못된 준비 상태 취소
                return; // 발동 차단
            }

            bool shouldCancel = ProjectJFireworkPolicy.ShouldCancelPreparation( // 취소 조건 계산
                externalGameplay.GameplayInputAllowed, // 현재 경기 입력 상태
                NetworkFireworkStartRespawnCount, // 시작 부활 횟수
                externalGameplay.RespawnCount // 현재 부활 횟수
            );

            if (shouldCancel) // 경기 종료·부활 검사
            {
                CancelFireworkPreparationAuthority(); // 준비 취소
                return; // 발동 차단
            }

            if (!NetworkFireworkPreparationTimer.ExpiredOrNotRunning(Runner)) // 준비 시간 진행 확인
            {
                return; // 준비 유지
            }

            NetworkFireworkPreparing = false; // 준비 상태 종료
            NetworkFireworkPreparationTimer = TickTimer.None; // 준비 Timer 제거
            NetworkFireworkActivationCount++; // 발동 횟수 증가
            NetworkFireworkLastTargetCount = ApplyFireworkTargetsAuthority(); // 다중 Target 밀치기 적용

            Debug.Log( // 발동 결과 로그 출력
                "[Project J/Fusion] 107일차 폭죽 발동 / P" +
                OwnerIndex +
                " / Targets " +
                NetworkFireworkLastTargetCount, // 발동 Target 정보
                this // 로그 대상
            );
        }

        internal void CancelFireworkPreparationAuthority() // 외부 부활 처리용 준비 취소
        {
            CancelFireworkPreparationAuthority(true); // 취소 횟수 포함 처리
        }

        private void CancelFireworkPreparationAuthority( // 폭죽 준비 상태 제거
            bool countCancellation // 취소 횟수 기록 여부
        )
        {
            if (!NetworkFireworkPreparing) // 준비 상태 확인
            {
                return; // 중복 취소 차단
            }

            NetworkFireworkPreparing = false; // 준비 상태 종료
            NetworkFireworkPreparationTimer = TickTimer.None; // 준비 Timer 제거
            NetworkFireworkLastTargetCount = 0; // 미발동 Target 수 초기화

            if (countCancellation) // 실제 취소 기록 여부 확인
            {
                NetworkFireworkCancellationCount++; // 취소 횟수 증가
            }
        }

        private int ApplyFireworkTargetsAuthority() // 범위 내 모든 Target 처리
        {
            int appliedTargetCount = 0; // 성공 Target 수 초기화
            Vector3 origin = transform.position; // 사용자 위치 저장
            Vector3 forward = transform.forward; // 사용자 전방 저장
            ProjectJNetworkExternalGameplay[] candidates = // 현재 Player 후보 조회
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkExternalGameplay>( // Scene Player 전체 검색
                    FindObjectsSortMode.None // 불필요한 정렬 제거
                );

            for (int i = 0; i < candidates.Length; i++) // 모든 후보 순회
            {
                ProjectJNetworkExternalGameplay candidate = candidates[i]; // 현재 후보 저장

                if (candidate == null || candidate == externalGameplay) // 자기 자신·누락 후보 검사
                {
                    continue; // 적용 대상 제외
                }

                bool isWithinArea = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 부채꼴 판정
                    origin, // 사용자 위치
                    forward, // 사용자 전방
                    candidate.transform.position // Target 위치
                );

                if (!isWithinArea) // 범위 밖 후보 검사
                {
                    continue; // 적용 대상 제외
                }

                if (!HasFireworkLineOfSight(candidate.transform)) // 벽 차폐 검사
                {
                    continue; // 벽 뒤 Target 제외
                }

                Vector3 velocityChange = ProjectJFireworkPolicy.CreateDefaultHorizontalVelocityChange( // 확정 수평 외력 계산
                    origin, // 사용자 위치
                    candidate.transform.position, // Target 위치
                    forward, // 같은 위치 대체 전방
                    candidate.ExternalVelocity // 기존 외부 속도
                );

                bool applied = candidate.TryApplyExternalVelocityChange( // 서버 권한 적대 외력 적용
                    ProjectJExternalForceSource.Item, // 아이템 외력 원인
                    velocityChange // 계산된 수평 외력
                );

                if (applied) // 보호 상태를 통과한 Target 확인
                {
                    appliedTargetCount++; // 성공 Target 수 증가
                }
            }

            return appliedTargetCount; // 최종 성공 Target 수 반환
        }

        private bool HasFireworkLineOfSight(Transform target) // 폭죽 벽 차폐 판정
        {
            Vector3 rayOrigin = transform.position + Vector3.up * FireworkSightHeight; // 사용자 Ray 시작점
            Vector3 rayTarget = target.position + Vector3.up * FireworkSightHeight; // Target Ray 도착점
            Vector3 rayDirection = rayTarget - rayOrigin; // Ray 방향 계산
            float rayDistance = rayDirection.magnitude; // Ray 거리 계산

            if (rayDistance <= 0.0001f) // 같은 Ray 위치 검사
            {
                return true; // 차폐 없음 처리
            }

            bool blocked = Physics.Raycast( // 벽과 Target Collider 검사
                rayOrigin, // Ray 시작점
                rayDirection.normalized, // 정규화 방향
                out RaycastHit hit, // 최초 충돌 정보
                rayDistance, // Target까지 검사 거리
                Physics.DefaultRaycastLayers, // 기본 물리 Layer
                QueryTriggerInteraction.Ignore // Trigger 무시
            );

            if (!blocked) // 충돌체 없음 확인
            {
                return true; // 시야 확보
            }

            return
                hit.transform == target || // Target Root 직접 적중
                hit.transform.IsChildOf(target); // Target 자식 Collider 적중
        }
    }
}
