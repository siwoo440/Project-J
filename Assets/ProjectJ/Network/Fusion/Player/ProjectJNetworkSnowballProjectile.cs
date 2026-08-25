using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 눈덩이 공통 정책 사용
using UnityEngine; // 물리 판정과 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    [DisallowMultipleComponent] // 투사체 동작 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion 네트워크 객체 보장
    [RequireComponent(typeof(NetworkTransform))] // 위치 동기화 보장
    public sealed class ProjectJNetworkSnowballProjectile :
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
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false; // State Authority 누락 처리
            }

            direction.y = 0f; // 수평 투사체 이동 유지

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false; // 잘못된 방향 차단
            }

            NetworkOwner = owner; // 투척 사용자 저장
            NetworkDirection = direction.normalized; // 일정한 이동 방향 저장
            NetworkTravelledDistance = 0f; // 누적 거리 초기화
            NetworkInitialized = true; // 다음 Fusion Tick 이동 허용
            return true; // 초기화 성공 반환
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return; // 서버 외 투사체 판정 차단
            }

            if (!NetworkInitialized)
            {
                DespawnAuthority(); // 잘못 생성된 투사체 제거
                return;
            }

            if (ProjectJSnowballPolicy.HasReachedTravelLimit(NetworkTravelledDistance))
            {
                DespawnAuthority(); // 최대 거리 도달 투사체 제거
                return;
            }

            float remainingDistance =
                ProjectJSnowballPolicy.MaximumTravelDistance -
                NetworkTravelledDistance; // 남은 이동 거리 계산

            float stepDistance = Mathf.Min(
                ProjectJSnowballPolicy.ProjectileSpeed * Runner.DeltaTime,
                remainingDistance
            ); // 이번 Tick 이동 거리 제한

            if (TryResolveCollision(stepDistance))
            {
                DespawnAuthority(); // 최초 유효 충돌 뒤 제거
                return;
            }

            transform.position += NetworkDirection * stepDistance; // 서버 투사체 위치 이동
            NetworkTravelledDistance += stepDistance; // 누적 이동 거리 갱신

            if (ProjectJSnowballPolicy.HasReachedTravelLimit(NetworkTravelledDistance))
            {
                DespawnAuthority(); // 15m 도달 직후 제거
            }
        }

        private bool TryResolveCollision( // 이번 Tick 최초 충돌 판정
            float stepDistance // 이번 Tick 이동 거리
        )
        {
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                ProjectJSnowballPolicy.CollisionRadius,
                NetworkDirection,
                hitBuffer,
                stepDistance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            ); // 서버 충돌 후보 조회

            int nearestIndex = -1; // 가장 가까운 충돌 없음 상태
            float nearestDistance = float.MaxValue; // 가장 가까운 거리 초기화

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = hitBuffer[index].collider; // 현재 충돌 Collider 조회

                if (hitCollider == null)
                {
                    continue; // 잘못된 충돌 후보 제외
                }

                ProjectJNetworkExternalGameplay target =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // Player Target 조회

                if (
                    target != null &&
                    target.Object != null &&
                    target.Object.IsValid &&
                    target.Object.InputAuthority == NetworkOwner
                )
                {
                    continue; // 투척 사용자 자기 Collider 제외
                }

                if (hitBuffer[index].distance >= nearestDistance)
                {
                    continue; // 더 먼 충돌 후보 제외
                }

                nearestIndex = index; // 가장 가까운 충돌 Index 저장
                nearestDistance = hitBuffer[index].distance; // 가장 가까운 거리 저장
            }

            if (nearestIndex < 0)
            {
                return false; // 이번 Tick 충돌 없음
            }

            Collider nearestCollider = hitBuffer[nearestIndex].collider; // 가장 가까운 Collider 조회
            ProjectJNetworkExternalGameplay nearestTarget =
                nearestCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // 가장 가까운 Player 조회

            ProjectJNetworkItemInventory mirrorInventory =
                nearestTarget != null
                    ? nearestTarget.GetComponent<ProjectJNetworkItemInventory>()
                    : null;

            if (
                mirrorInventory != null &&
                mirrorInventory.TryReflectHandMirrorProjectileAuthority(
                    NetworkOwner,
                    NetworkDirection,
                    out PlayerRef reflectedOwner,
                    out Vector3 reflectedDirection
                )
            )
            {
                NetworkOwner =
                    reflectedOwner;

                NetworkDirection =
                    reflectedDirection;

                transform.position =
                    ProjectJHandMirrorPolicy.ResolveSeparatedPosition(
                        hitBuffer[nearestIndex].point,
                        reflectedDirection
                    );

                return false;
            }

            if (nearestTarget != null)
            {
                nearestTarget.TryApplySnowballSlowAuthority(NetworkOwner); // 보호 상태를 포함한 감속 적용 시도
            }

            return true; // Player·지형 첫 충돌 처리 완료
        }

        private void DespawnAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return; // 이미 제거된 투사체 처리
            }

            Runner.Despawn(Object); // 서버 투사체 제거
        }
    }
}
