using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 풀 공 공통 정책 사용
using UnityEngine; // 물리 판정과 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    [DisallowMultipleComponent] // 투사체 동작 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion 네트워크 객체 보장
    [RequireComponent(typeof(NetworkTransform))] // 위치 동기화 보장
    public sealed class ProjectJNetworkPoolBallProjectile :
        NetworkBehaviour
    {
        private readonly RaycastHit[] hitBuffer = new RaycastHit[24]; // 충돌 후보 재사용 버퍼

        [Networked] // 투사체 초기화 상태 동기화
        private NetworkBool NetworkInitialized
        {
            get;
            set;
        }

        [Networked] // 투척 사용자 동기화
        private PlayerRef NetworkOwner
        {
            get;
            set;
        }

        [Networked] // 투사체 이동 방향 동기화
        private Vector3 NetworkDirection
        {
            get;
            set;
        }

        [Networked] // 투사체 누적 이동 거리 동기화
        private float NetworkTravelledDistance
        {
            get;
            set;
        }

        public PlayerRef Owner => NetworkOwner; // 투척 사용자 조회
        public float TravelledDistance => NetworkTravelledDistance; // 누적 이동 거리 조회

        public bool ConfigureAuthority( // 서버 투사체 초기화
            PlayerRef owner, // 투척 사용자
            Vector3 direction // 투척 방향
        )
        {
            if (
                Object == null || // NetworkObject 누락 확인
                !Object.IsValid || // NetworkObject 유효성 확인
                !Object.HasStateAuthority // State Authority 확인
            )
            {
                return false; // 서버 초기화 차단
            }

            direction.y = 0f; // 수평 투사체 이동 유지

            if (direction.sqrMagnitude <= 0.0001f) // 잘못된 방향 확인
            {
                return false; // 초기화 실패 반환
            }

            NetworkOwner = owner; // 투척 사용자 저장
            NetworkDirection = direction.normalized; // 일정한 이동 방향 저장
            NetworkTravelledDistance = 0f; // 누적 거리 초기화
            NetworkInitialized = true; // 다음 Fusion Tick 이동 허용
            return true; // 초기화 성공 반환
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) // 서버 권한 확인
            {
                return; // Proxy 물리 판정 차단
            }

            if (!NetworkInitialized) // 정상 초기화 여부 확인
            {
                DespawnAuthority(); // 잘못 생성된 투사체 제거
                return; // 이동 처리 중단
            }

            if (ProjectJPoolBallPolicy.HasReachedTravelLimit(NetworkTravelledDistance)) // 최대 거리 사전 확인
            {
                DespawnAuthority(); // 28m 도달 투사체 제거
                return; // 이동 처리 중단
            }

            float remainingDistance =
                ProjectJPoolBallPolicy.MaximumTravelDistance - // 전체 최대 거리
                NetworkTravelledDistance; // 이미 이동한 거리 차감

            float stepDistance = Mathf.Min(
                ProjectJPoolBallPolicy.ProjectileSpeed * Runner.DeltaTime, // 이번 Tick 기본 이동 거리
                remainingDistance // 남은 최대 거리 제한
            );

            if (TryResolveCollision(stepDistance)) // 이번 Tick 충돌 판정
            {
                DespawnAuthority(); // 첫 충돌 뒤 투사체 제거
                return; // 추가 이동 차단
            }

            transform.position += NetworkDirection * stepDistance; // 서버 투사체 위치 이동
            NetworkTravelledDistance += stepDistance; // 누적 이동 거리 갱신

            if (ProjectJPoolBallPolicy.HasReachedTravelLimit(NetworkTravelledDistance)) // 이동 후 최대 거리 확인
            {
                DespawnAuthority(); // 빗나간 투사체 제거
            }
        }

        private bool TryResolveCollision(float stepDistance) // 이번 Tick 최초 충돌 판정
        {
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position, // 현재 투사체 위치
                ProjectJPoolBallPolicy.CollisionRadius, // 기획 반경 0.24m
                NetworkDirection, // 서버 이동 방향
                hitBuffer, // 재사용 충돌 버퍼
                stepDistance, // 이번 Tick 이동 거리
                Physics.AllLayers, // 모든 물리 Layer 검사
                QueryTriggerInteraction.Ignore // Trigger 제외
            );

            int nearestIndex = -1; // 가장 가까운 충돌 없음 상태
            float nearestDistance = float.MaxValue; // 가장 가까운 거리 초기화

            for (int index = 0; index < hitCount; index++) // 충돌 후보 순회
            {
                Collider hitCollider = hitBuffer[index].collider; // 현재 충돌 Collider 조회

                if (hitCollider == null) // 잘못된 후보 확인
                {
                    continue; // 현재 후보 제외
                }

                ProjectJNetworkExternalGameplay target =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // Player Target 조회

                if (
                    target != null && // Player Collider 확인
                    target.Object != null && // NetworkObject 존재 확인
                    target.Object.IsValid && // NetworkObject 유효 확인
                    target.Object.InputAuthority == NetworkOwner // 투척 사용자 확인
                )
                {
                    continue; // 자기 Collider 제외
                }

                if (hitBuffer[index].distance >= nearestDistance) // 기존 후보보다 먼 충돌 확인
                {
                    continue; // 더 먼 후보 제외
                }

                nearestIndex = index; // 가장 가까운 충돌 Index 저장
                nearestDistance = hitBuffer[index].distance; // 가장 가까운 거리 저장
            }

            if (nearestIndex < 0) // 유효 충돌 없음 확인
            {
                return false; // 계속 이동 허용
            }

            Collider nearestCollider = hitBuffer[nearestIndex].collider; // 가장 가까운 Collider 조회
            ProjectJNetworkExternalGameplay nearestTarget =
                nearestCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // 가장 가까운 Player 조회

            if (nearestTarget != null) // Player 적중 여부 확인
            {
                nearestTarget.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item, // 젤리·부활 보호가 차단하는 적대 아이템 외력
                    NetworkDirection * ProjectJPoolBallPolicy.HitForce // 기획 힘 4 적용
                );
            }

            return true; // Player 또는 지형 첫 충돌 처리 완료
        }

        private void DespawnAuthority() // 서버 투사체 제거
        {
            if (
                Runner == null || // Runner 누락 확인
                Object == null || // NetworkObject 누락 확인
                !Object.IsValid // 이미 제거된 객체 확인
            )
            {
                return; // 중복 Despawn 차단
            }

            Runner.Despawn(Object); // 서버 권한 NetworkObject 제거
        }
    }
}
