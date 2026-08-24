using Fusion; // Networked와 NetworkObject 사용
using ProjectJ.Items; // 눈덩이 공통 정책 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 눈덩이 네트워크 기능
    {
        private const string SnowballProjectileResourcePath =
            "ProjectJNetworkSnowballProjectile"; // 투사체 Resources 경로

        private NetworkObject snowballProjectilePrefab; // 불러온 눈덩이 투사체 Prefab

        [Networked] // 눈덩이 감속 남은 시간 동기화
        private TickTimer NetworkSnowballSlowTimer
        {
            get;
            set;
        }

        public bool IsSnowballSlowed =>
            IsTimerActive(NetworkSnowballSlowTimer); // 눈덩이 감속 활성 여부

        public float SnowballSlowRemaining =>
            GetRemainingTime(NetworkSnowballSlowTimer); // 눈덩이 감속 남은 시간

        private void InitializeSnowballAuthority()
        {
            NetworkSnowballSlowTimer = TickTimer.None; // 최초 감속 상태 초기화
        }

        private bool UseSnowballAuthority()
        {
            if (
                Runner == null ||
                !Runner.IsServer ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false; // 서버 권한과 Runner 누락 처리
            }

            NetworkObject projectilePrefab = ResolveSnowballProjectilePrefab(); // 투사체 Prefab 조회

            if (projectilePrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 109일차 눈덩이 Prefab을 찾을 수 없음",
                    this
                ); // 누락된 Resources Prefab 기록

                return false; // 아이템 소비 차단
            }

            Vector3 forward = transform.forward; // 플레이어 전방 조회
            forward.y = 0f; // 수평 투척 방향 유지

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 잘못된 방향 대체
            }

            forward.Normalize(); // 일정한 투사체 속도 유지

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.9f; // 자기 Collider 앞의 생성 위치 계산

            NetworkObject projectileObject = Runner.Spawn(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(forward),
                Object.InputAuthority
            ); // 서버 권한 눈덩이 생성

            if (projectileObject == null)
            {
                return false; // Spawn 실패 시 아이템 소비 차단
            }

            ProjectJNetworkSnowballProjectile projectile =
                projectileObject.GetComponent<ProjectJNetworkSnowballProjectile>(); // 눈덩이 동작 Component 조회

            if (
                projectile == null ||
                !projectile.ConfigureAuthority(Object.InputAuthority, forward)
            )
            {
                Runner.Despawn(projectileObject); // 잘못된 투사체 제거
                return false; // 구성 실패 시 아이템 소비 차단
            }

            return true; // 투척 성공 반환
        }

        internal bool ApplySnowballSlowAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false; // Target State Authority 누락 처리
            }

            float duration = ProjectJSnowballPolicy.GetRefreshedDuration(
                SnowballSlowRemaining
            ); // 재적중 시 단일 3초 지속 시간 계산

            NetworkSnowballSlowTimer = TickTimer.CreateFromSeconds(
                Runner,
                duration
            ); // 감속 Timer 시작·갱신

            return true; // 감속 적용 성공 반환
        }

        private void ClearSnowballSlowAuthority()
        {
            NetworkSnowballSlowTimer = TickTimer.None; // 부활·초기화 시 감속 제거
        }

        private NetworkObject ResolveSnowballProjectilePrefab()
        {
            if (snowballProjectilePrefab == null)
            {
                GameObject projectilePrefabObject = Resources.Load<GameObject>(
                    SnowballProjectileResourcePath
                ); // Resources에서 투사체 Prefab 조회

                snowballProjectilePrefab = projectilePrefabObject != null
                    ? projectilePrefabObject.GetComponent<NetworkObject>()
                    : null; // Prefab의 NetworkObject Component 조회
            }

            return snowballProjectilePrefab; // 조회된 Prefab 반환
        }
    }
}
