using System.Collections.Generic; // 재사용 Player 목록 사용
using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 폭탄 공통 정책 사용
using UnityEngine; // 물리 이동과 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    [DisallowMultipleComponent] // 폭탄 동작 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion 네트워크 객체 보장
    [RequireComponent(typeof(NetworkTransform))] // 위치 동기화 보장
    public sealed class ProjectJNetworkBomb : NetworkBehaviour // 서버 권한 폭탄 동작
    {
        private readonly RaycastHit[] hitBuffer =
            new RaycastHit[24]; // Tick 충돌 후보 재사용 버퍼

        private readonly List<ProjectJNetworkExternalGameplay> targetBuffer =
            new List<ProjectJNetworkExternalGameplay>(8); // 폭발 Player 후보 재사용 목록

        [Networked] // 폭탄 초기화 상태 동기화
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

        [Networked] // 최초 투척 위치 동기화
        private Vector3 NetworkThrowOrigin
        {
            get;
            set;
        }

        [Networked] // 현재 포물선 속도 동기화
        private Vector3 NetworkVelocity
        {
            get;
            set;
        }

        [Networked] // 같은 위치 폭발 대체 방향 동기화
        private Vector3 NetworkFallbackDirection
        {
            get;
            set;
        }

        [Networked] // 2.5초 신관 동기화
        private TickTimer NetworkFuseTimer
        {
            get;
            set;
        }

        [Networked] // 중복 폭발 차단 상태 동기화
        private NetworkBool NetworkExploded
        {
            get;
            set;
        }

        public bool IsInitialized => NetworkInitialized; // 정상 초기화 여부
        public PlayerRef Owner => NetworkOwner; // 투척 사용자 조회
        public bool HasExploded => NetworkExploded; // 폭발 완료 여부 조회

        public bool ConfigureAuthority( // 서버 폭탄 초기화
            PlayerRef owner, // 투척 사용자
            Vector3 forward // 투척 전방
        )
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false; // 서버 권한 누락 처리
            }

            forward.y = 0f; // 투척 방향 수평화

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 잘못된 방향 보정
            }

            forward.Normalize(); // 일정한 전방 저장

            NetworkOwner = owner; // 투척 사용자 저장
            NetworkThrowOrigin = transform.position; // 최초 위치 저장
            NetworkVelocity =
                ProjectJBombPolicy.CreateInitialVelocity(forward); // 포물선 초기 속도 저장
            NetworkFallbackDirection = forward; // 같은 위치 폭발 대체 방향 저장
            NetworkFuseTimer = TickTimer.CreateFromSeconds(
                Runner,
                ProjectJBombPolicy.FuseSeconds
            ); // 2.5초 신관 시작
            NetworkExploded = false; // 폭발 상태 초기화
            NetworkInitialized = true; // 다음 서버 Tick부터 동작
            return true; // 초기화 성공
        }

        public override void FixedUpdateNetwork() // 서버 폭탄 Tick 처리
        {
            if (!Object.HasStateAuthority)
            {
                return; // Proxy 물리 판정 차단
            }

            if (
                !NetworkInitialized ||
                Runner == null
            )
            {
                DespawnAuthority(); // 잘못 생성된 폭탄 제거
                return;
            }

            if (NetworkExploded)
            {
                DespawnAuthority(); // 중복 폭발 객체 정리
                return;
            }

            if (!IsOwnerGameplayActive()) // 경기 종료·Owner 상태 확인
            {
                DespawnAuthority(); // 경기 종료 시 폭탄 제거
                return;
            }

            if (NetworkFuseTimer.ExpiredOrNotRunning(Runner))
            {
                ExplodeAuthority(); // 신관 종료 폭발
                return;
            }

            SimulateThrowAuthority(); // 포물선 이동과 충돌 처리
        }

        private void SimulateThrowAuthority() // 서버 포물선 이동
        {
            float deltaTime = Runner.DeltaTime; // Fusion Tick 시간
            Vector3 velocity = NetworkVelocity; // 현재 속도 복사

            velocity +=
                Vector3.up *
                ProjectJBombPolicy.PrototypeGravity *
                deltaTime; // 프로토타입 중력 적용

            float horizontalDistance =
                ProjectJBombPolicy.GetHorizontalDistance(
                    NetworkThrowOrigin,
                    transform.position
                ); // 현재 수평 투척 거리

            if (
                horizontalDistance >=
                ProjectJBombPolicy.MaximumThrowDistance
            )
            {
                velocity.x = 0f; // 최대 거리 이후 수평 이동 정지
                velocity.z = 0f;
            }

            Vector3 step = velocity * deltaTime; // 이번 Tick 이동량
            float stepDistance = step.magnitude; // 충돌 검사 거리

            if (
                stepDistance > 0.0001f &&
                TryResolveCollision(
                    step / stepDistance,
                    stepDistance
                )
            )
            {
                NetworkVelocity = Vector3.zero; // 최초 충돌 위치에 정지
                return;
            }

            Vector3 nextPosition =
                transform.position + step; // 충돌 없는 다음 위치

            float nextHorizontalDistance =
                ProjectJBombPolicy.GetHorizontalDistance(
                    NetworkThrowOrigin,
                    nextPosition
                ); // 이동 후 수평 거리 예측

            if (
                nextHorizontalDistance >
                ProjectJBombPolicy.MaximumThrowDistance
            )
            {
                Vector3 horizontal =
                    nextPosition - NetworkThrowOrigin; // 최초 위치 기준 이동 벡터
                horizontal.y = 0f; // 수평 성분만 사용

                if (horizontal.sqrMagnitude > 0.0001f)
                {
                    horizontal =
                        horizontal.normalized *
                        ProjectJBombPolicy.MaximumThrowDistance; // 최대 12m로 제한

                    nextPosition.x =
                        NetworkThrowOrigin.x + horizontal.x;
                    nextPosition.z =
                        NetworkThrowOrigin.z + horizontal.z; // XZ 위치 제한
                }

                velocity.x = 0f; // 최대 거리 도달 후 수평 속도 제거
                velocity.z = 0f;
            }

            transform.position = nextPosition; // 서버 위치 갱신
            NetworkVelocity = velocity; // 다음 Tick 속도 저장
        }

        private bool TryResolveCollision( // 이번 Tick 최초 충돌 처리
            Vector3 direction, // 이동 방향
            float stepDistance // 이동 거리
        )
        {
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                ProjectJBombPolicy.CollisionRadius,
                direction,
                hitBuffer,
                stepDistance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            ); // 서버 SphereCast 충돌 후보 조회

            int nearestIndex = -1; // 가장 가까운 유효 충돌 없음
            float nearestDistance = float.MaxValue; // 최근접 거리 초기화

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    hitBuffer[index].collider; // 현재 충돌 Collider

                if (hitCollider == null)
                {
                    continue; // 잘못된 충돌 후보 제외
                }

                ProjectJNetworkExternalGameplay target =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // Player 조회

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

                nearestIndex = index; // 최근접 충돌 저장
                nearestDistance = hitBuffer[index].distance;
            }

            if (nearestIndex < 0)
            {
                return false; // 이번 Tick 유효 충돌 없음
            }

            RaycastHit nearestHit =
                hitBuffer[nearestIndex]; // 최근접 충돌 정보

            transform.position =
                nearestHit.point +
                nearestHit.normal *
                ProjectJBombPolicy.CollisionRadius; // 표면 바깥에 폭탄 정지

            return true; // 충돌 처리 완료
        }

        private bool IsOwnerGameplayActive() // 폭탄 유지 가능한 경기 상태 확인
        {
            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                targetBuffer
            ); // 현재 Runner Player Registry 조회

            for (int index = 0; index < targetBuffer.Count; index++)
            {
                ProjectJNetworkExternalGameplay player =
                    targetBuffer[index]; // 현재 Player 조회

                if (
                    player == null ||
                    player.Object == null ||
                    !player.Object.IsValid ||
                    player.Object.InputAuthority != NetworkOwner
                )
                {
                    continue; // Owner가 아닌 Player 제외
                }

                return player.GameplayInputAllowed; // 경기 진행 중에만 유지
            }

            return false; // Owner를 찾지 못하면 제거
        }

        private void ExplodeAuthority() // 서버 범위 폭발 처리
        {
            if (NetworkExploded)
            {
                return; // 중복 폭발 차단
            }

            NetworkExploded = true; // 외력 처리 전 폭발 상태 고정

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                targetBuffer
            ); // 현재 Runner Player 후보 조회

            for (int index = 0; index < targetBuffer.Count; index++)
            {
                ProjectJNetworkExternalGameplay target =
                    targetBuffer[index]; // 현재 Target 조회

                if (
                    target == null ||
                    target.Object == null ||
                    !target.Object.IsValid ||
                    target.Object.InputAuthority == NetworkOwner
                )
                {
                    continue; // 누락 Player와 투척 사용자 제외
                }

                float distance = Vector3.Distance(
                    transform.position,
                    target.transform.position
                ); // 폭발 중심과 Target 거리

                if (!ProjectJBombPolicy.IsWithinExplosionRadius(distance))
                {
                    continue; // 5m 폭발 반경 밖 제외
                }

                Vector3 velocityChange =
                    ProjectJBombPolicy.CreateExplosionVelocityChange(
                        transform.position,
                        target.transform.position,
                        NetworkFallbackDirection
                    ); // 10→4m/s 거리 감쇠 외력 계산

                if (velocityChange.sqrMagnitude <= 0.0001f)
                {
                    continue; // 유효 외력 없음
                }

                target.TryApplyExternalVelocityChange3D(
                    ProjectJExternalForceSource.Item,
                    velocityChange
                ); // 기존 Jelly·Respawn·Gameplay 보호 흐름 재사용
            }

            DespawnAuthority(); // 한 번 폭발 후 NetworkObject 제거
        }

        private void DespawnAuthority() // 서버 폭탄 제거
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return; // 이미 제거된 폭탄 처리
            }

            Runner.Despawn(Object); // 서버 NetworkObject 제거
        }
    }
}
