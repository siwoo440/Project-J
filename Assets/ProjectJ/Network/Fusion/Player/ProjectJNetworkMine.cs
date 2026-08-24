using System.Collections.Generic; // 재사용 Player 목록 사용
using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 지뢰 공통 정책 사용
using UnityEngine; // Scene Player 검색과 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    [DisallowMultipleComponent] // 지뢰 동작 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion 네트워크 객체 보장
    [RequireComponent(typeof(NetworkTransform))] // 설치 위치 동기화 보장
    public sealed class ProjectJNetworkMine : NetworkBehaviour // 서버 권한 지뢰 동작
    {
        private readonly List<ProjectJNetworkExternalGameplay> targetBuffer =
            new List<ProjectJNetworkExternalGameplay>(8); // Tick별 재사용 Player 후보 목록

        [Networked] // 지뢰 초기화 상태 동기화
        private NetworkBool NetworkInitialized
        {
            get;
            set;
        }

        [Networked] // 설치 사용자 동기화
        private PlayerRef NetworkOwner
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

        [Networked] // 지뢰 활성화 대기 시간 동기화
        private TickTimer NetworkArmTimer
        {
            get;
            set;
        }

        [Networked] // 지뢰 유지 시간 동기화
        private TickTimer NetworkLifetimeTimer
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

        public bool IsInitialized => NetworkInitialized; // 설치 완료 여부 조회
        public PlayerRef Owner => NetworkOwner; // 설치 사용자 조회
        public bool HasExploded => NetworkExploded; // 폭발 완료 여부 조회

        public bool ConfigureAuthority( // 서버 지뢰 초기화
            PlayerRef owner, // 설치 사용자
            Vector3 fallbackDirection // 같은 위치 대체 방향
        )
        {
            if (
                Runner == null || // Runner 누락 조건
                Object == null || // NetworkObject 누락 조건
                !Object.IsValid || // NetworkObject 무효 조건
                !Object.HasStateAuthority // 서버 권한 누락 조건
            )
            {
                return false; // 잘못된 초기화 차단
            }

            fallbackDirection.y = 0f; // 대체 방향 수평화

            if (fallbackDirection.sqrMagnitude <= 0.0001f) // 대체 방향 누락 확인
            {
                fallbackDirection = Vector3.forward; // 기본 전방 사용
            }

            NetworkOwner = owner; // 설치 사용자 저장
            NetworkFallbackDirection = fallbackDirection.normalized; // 일정한 대체 방향 저장
            NetworkArmTimer = TickTimer.CreateFromSeconds( // 활성화 Timer 생성
                Runner, // 현재 Runner
                ProjectJMinePolicy.ArmSeconds // 활성화 대기 시간
            );
            NetworkLifetimeTimer = TickTimer.CreateFromSeconds( // 유지 Timer 생성
                Runner, // 현재 Runner
                ProjectJMinePolicy.LifetimeSeconds // Definition 기준 유지 시간
            );
            NetworkExploded = false; // 폭발 상태 초기화
            NetworkInitialized = true; // 서버 Tick 판정 시작
            return true; // 초기화 성공 반환
        }

        public override void FixedUpdateNetwork() // 서버 지뢰 Tick 판정
        {
            if (!Object.HasStateAuthority) // 서버 권한 확인
            {
                return; // Client 판정 차단
            }

            if (!NetworkInitialized || Runner == null) // 초기화와 Runner 확인
            {
                DespawnAuthority(); // 잘못 생성된 지뢰 제거
                return;
            }

            if (NetworkLifetimeTimer.ExpiredOrNotRunning(Runner)) // 25초 유지 시간 확인
            {
                DespawnAuthority(); // 시간 종료 지뢰 제거
                return;
            }

            if (NetworkExploded) // 기존 폭발 상태 확인
            {
                DespawnAuthority(); // 중복 Tick 폭발 차단
                return;
            }

            bool isArmed = NetworkArmTimer.ExpiredOrNotRunning(Runner); // 활성화 완료 여부 계산
            ProjectJNetworkExternalGameplay.CollectActivePlayers( // 현재 Runner Player Registry 조회
                Runner, // 지뢰가 속한 Runner
                targetBuffer // 재사용 후보 목록
            );
            bool hasValidTarget = HasValidTriggerTarget(targetBuffer); // 감지 반경의 유효 상대 확인

            if (!ProjectJMinePolicy.ShouldTrigger(isArmed, hasValidTarget)) // 폭발 시작 조건 확인
            {
                return; // 대기 상태 유지
            }

            NetworkExploded = true; // 외력 적용 전 중복 폭발 차단
            ApplyExplosionAuthority(targetBuffer); // 폭발 반경 다중 Target 처리
            DespawnAuthority(); // 한 번 폭발한 지뢰 제거
        }

        private bool HasValidTriggerTarget( // 감지 반경 유효 상대 확인
            List<ProjectJNetworkExternalGameplay> targets // 현재 Player 후보
        )
        {
            float triggerRadiusSquared = // 제곱 거리 기준 생성
                ProjectJMinePolicy.TriggerRadius * ProjectJMinePolicy.TriggerRadius; // 감지 반경 제곱

            for (int index = 0; index < targets.Count; index++) // 모든 Player 후보 순회
            {
                ProjectJNetworkExternalGameplay target = targets[index]; // 현재 Target 조회

                if (!IsValidTarget(target)) // 보호·소유자 상태 확인
                {
                    continue; // 유효하지 않은 Target 제외
                }

                float distanceSquared = // 지뢰와 Target 제곱 거리 계산
                    (target.transform.position - transform.position).sqrMagnitude; // 3차원 거리 사용

                if (distanceSquared <= triggerRadiusSquared) // 감지 반경 포함 확인
                {
                    return true; // 최초 유효 상대 감지
                }
            }

            return false; // 감지 대상 없음
        }

        private void ApplyExplosionAuthority( // 폭발 반경 다중 Target 처리
            List<ProjectJNetworkExternalGameplay> targets // 현재 Player 후보
        )
        {
            float explosionRadiusSquared = // 제곱 거리 기준 생성
                ProjectJMinePolicy.ExplosionRadius * ProjectJMinePolicy.ExplosionRadius; // 폭발 반경 제곱

            for (int index = 0; index < targets.Count; index++) // 모든 Player 후보 순회
            {
                ProjectJNetworkExternalGameplay target = targets[index]; // 현재 Target 조회

                if (!IsValidTarget(target)) // 보호·소유자 상태 확인
                {
                    continue; // 유효하지 않은 Target 제외
                }

                float distanceSquared = // 지뢰와 Target 제곱 거리 계산
                    (target.transform.position - transform.position).sqrMagnitude; // 3차원 거리 사용

                if (distanceSquared > explosionRadiusSquared) // 폭발 반경 초과 확인
                {
                    continue; // 범위 밖 Target 제외
                }

                Vector3 velocityChange = ProjectJMinePolicy.CreateExplosionVelocityChange( // 위쪽·바깥쪽 외력 계산
                    transform.position, // 지뢰 위치 전달
                    target.transform.position, // Target 위치 전달
                    NetworkFallbackDirection // 같은 위치 대체 방향 전달
                );

                target.TryApplyExternalVelocityChange3D( // 서버 권한 3차원 외력 적용
                    ProjectJExternalForceSource.Item, // 적대 아이템 외력 원인
                    velocityChange // 폭발 속도 변화
                );
            }
        }

        private bool IsValidTarget( // 지뢰 대상 공통 판정
            ProjectJNetworkExternalGameplay target // 검사할 Player
        )
        {
            return
                target != null && // Player 존재 조건
                target.CanReceiveMineExplosionAuthority(NetworkOwner); // 소유자·보호 상태 조건
        }

        private void DespawnAuthority() // 서버 지뢰 제거
        {
            if (
                Runner == null || // Runner 누락 조건
                Object == null || // NetworkObject 누락 조건
                !Object.IsValid // 이미 제거된 객체 조건
            )
            {
                return; // 중복 제거 차단
            }

            Runner.Despawn(Object); // 서버 NetworkObject 제거
        }
    }
}
