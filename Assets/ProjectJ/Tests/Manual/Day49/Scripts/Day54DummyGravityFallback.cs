using ProjectJ.Player; // 플레이어 이동 컴포넌트 확인
using UnityEngine; // Rigidbody와 Runtime 초기화 사용
using UnityEngine.SceneManagement; // Day49 테스트 씬 확인

namespace ProjectJ.Tests.Manual
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Day54DummyGravityFallback :
        MonoBehaviour
    {
        private const string TargetSceneName =
            "Day49_AllSystemsTest"; // 적용할 통합 테스트 씬

        private const float Gravity =
            -22f; // 실제 플레이어 이동 중력과 동일한 값

        private Rigidbody body;
        private PlayerCameraRelativeMovement movement;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void InstallForDisabledTestPlayers()
        {
            if (
                SceneManager.GetActiveScene().name !=
                TargetSceneName
            )
            {
                return;
            }

            PlayerCameraRelativeMovement[] movements =
                FindObjectsByType<
                    PlayerCameraRelativeMovement
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < movements.Length;
                i++
            )
            {
                PlayerCameraRelativeMovement movement =
                    movements[i];

                if (
                    movement == null ||
                    movement.enabled
                )
                {
                    continue;
                }

                Rigidbody body =
                    movement.GetComponent<Rigidbody>();

                if (
                    body == null ||
                    body.isKinematic
                )
                {
                    continue;
                }

                Day54DummyGravityFallback existing =
                    movement.GetComponent<
                        Day54DummyGravityFallback
                    >();

                if (existing == null)
                {
                    movement.gameObject.AddComponent<
                        Day54DummyGravityFallback
                    >();
                }
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            movement =
                GetComponent<
                    PlayerCameraRelativeMovement
                >();

            if (body != null)
            {
                body.useGravity = false; // 플레이어와 동일하게 Unity 기본 중력은 사용하지 않음
                body.WakeUp();
            }
        }

        private void FixedUpdate()
        {
            if (
                body == null ||
                body.isKinematic
            )
            {
                return;
            }

            if (
                movement != null &&
                movement.enabled
            )
            {
                return; // 실제 이동 스크립트가 켜지면 중복 중력 방지
            }

            body.AddForce(
                Vector3.up * Gravity,
                ForceMode.Acceleration
            ); // 비활성 플레이어 더미에 플레이어와 같은 중력 적용
        }
    }
}
