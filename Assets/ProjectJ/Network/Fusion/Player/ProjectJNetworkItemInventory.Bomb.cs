using Fusion; // NetworkObject와 PlayerRef 사용
using ProjectJ.Items; // 폭탄 공통 정책 사용
using UnityEngine; // Resources와 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 폭탄 네트워크 사용 기능
    {
        private const string BombResourcePath =
            "ProjectJNetworkBomb"; // 폭탄 Resources 경로

        private NetworkObject bombPrefab; // 불러온 폭탄 Network Prefab

        private bool UseBombAuthority() // 서버 권한 폭탄 투척
        {
            bool runnerReady =
                Runner != null &&
                Runner.IsServer &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority; // 서버 권한 준비 상태

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed; // 경기 입력 허용 상태

            bool hasActiveBomb =
                runnerReady &&
                HasActiveBombAuthority(); // 사용자 활성 폭탄 존재 여부

            if (!ProjectJBombPolicy.CanThrow(
                runnerReady,
                gameplayAllowed,
                hasActiveBomb
            ))
            {
                return false; // 권한·경기·1개 제한 실패 시 소비 차단
            }

            NetworkObject resolvedBombPrefab = ResolveBombPrefab(); // 폭탄 Prefab 조회

            if (resolvedBombPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 114일차 폭탄 Prefab을 찾을 수 없음",
                    this
                ); // Resources Prefab 누락 기록

                return false; // 아이템 소비 차단
            }

            Vector3 forward = transform.forward; // 사용자 전방 조회
            forward.y = 0f; // 수평 투척 방향 유지

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 잘못된 전방 보정
            }

            forward.Normalize(); // 일정한 투척 방향 유지

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.9f; // 자기 Collider 앞 생성 위치

            NetworkObject bombObject = Runner.Spawn(
                resolvedBombPrefab,
                spawnPosition,
                Quaternion.LookRotation(forward),
                Object.InputAuthority
            ); // 서버 권한 폭탄 생성

            if (bombObject == null)
            {
                return false; // Spawn 실패 시 아이템 소비 차단
            }

            ProjectJNetworkBomb bomb =
                bombObject.GetComponent<ProjectJNetworkBomb>(); // 폭탄 동작 Component 조회

            if (
                bomb == null ||
                !bomb.ConfigureAuthority(
                    Object.InputAuthority,
                    forward
                )
            )
            {
                Runner.Despawn(bombObject); // 잘못 생성된 폭탄 제거
                return false; // 구성 실패 시 아이템 소비 차단
            }

            return true; // 폭탄 투척 성공
        }

        private bool HasActiveBombAuthority() // 사용자당 폭탄 1개 제한 검사
        {
            ProjectJNetworkBomb[] bombs =
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkBomb>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 활성 폭탄 검색

            for (int index = 0; index < bombs.Length; index++)
            {
                ProjectJNetworkBomb bomb = bombs[index]; // 현재 폭탄 조회

                if (
                    bomb == null ||
                    bomb.Runner != Runner ||
                    !bomb.IsInitialized ||
                    bomb.HasExploded
                )
                {
                    continue; // 다른 Runner·비활성 폭탄 제외
                }

                if (bomb.Owner == Object.InputAuthority)
                {
                    return true; // 같은 사용자의 활성 폭탄 발견
                }
            }

            return false; // 활성 폭탄 없음
        }

        private NetworkObject ResolveBombPrefab() // Resources 폭탄 Prefab 조회
        {
            if (bombPrefab == null)
            {
                GameObject bombPrefabObject = Resources.Load<GameObject>(
                    BombResourcePath
                ); // Resources에서 폭탄 Prefab 조회

                bombPrefab = bombPrefabObject != null
                    ? bombPrefabObject.GetComponent<NetworkObject>()
                    : null; // NetworkObject Component 저장
            }

            return bombPrefab; // 조회된 Prefab 반환
        }
    }
}
