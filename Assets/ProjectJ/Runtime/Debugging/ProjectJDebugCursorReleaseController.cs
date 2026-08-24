using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Debugging
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDebugCursorReleaseController :
        MonoBehaviour
    {
        private CursorLockMode previousLockState;
        private bool previousVisible;

        public static bool IsCursorReleased // 개발용 커서 해제 상태
        {
            get; // 외부 카메라 입력 정책 조회
            private set; // 컨트롤러 내부 상태 변경 제한
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectJDebugCursorReleaseController existing =
                FindFirstObjectByType<
                    ProjectJDebugCursorReleaseController
                >();

            if (existing != null)
            {
                return;
            }

            GameObject controllerObject =
                new GameObject(
                    "=== Project J Debug Cursor ==="
                );

            controllerObject.AddComponent<
                ProjectJDebugCursorReleaseController
            >();

            DontDestroyOnLoad(
                controllerObject
            );
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard =
                Keyboard.current;

            bool wasAltPressedThisFrame =
                keyboard != null &&
                (
                    keyboard.leftAltKey.wasPressedThisFrame ||
                    keyboard.rightAltKey.wasPressedThisFrame
                );

            if (wasAltPressedThisFrame)
            {
                bool nextReleasedState = // 다음 커서 토글 상태 계산
                    ProjectJDebugCursorReleasePolicy.GetNextReleasedState( // 토글 정책 호출
                        IsCursorReleased // 현재 커서 해제 상태 전달
                    );

                if (nextReleasedState) // 커서 해제 전환 확인
                {
                    ReleaseCursor(); // 커서 표시와 잠금 해제
                }
                else
                {
                    RestoreCursor(); // 이전 커서 상태 복구
                }

                return;
            }

            if (IsCursorReleased) // 커서 해제 상태 유지 확인
            {
                ApplyReleasedCursorState(); // 외부 재잠금 이후 해제 상태 재적용
            }
#endif
        }

        private void OnApplicationFocus(
            bool hasFocus
        )
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (
                hasFocus &&
                IsCursorReleased
            )
            {
                ApplyReleasedCursorState(); // 포커스 복귀 후 커서 해제 유지
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsCursorReleased)
            {
                RestoreCursor();
            }
#endif
        }

        private void ReleaseCursor()
        {
            previousLockState =
                Cursor.lockState;

            previousVisible =
                Cursor.visible;

            ApplyReleasedCursorState(); // 커서 해제 상태 적용

            IsCursorReleased = true; // 개발용 커서 해제 상태 저장
        }

        private static void ApplyReleasedCursorState() // 커서 해제 상태 강제 적용
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }

        private void RestoreCursor()
        {
            Cursor.lockState =
                previousLockState;

            Cursor.visible =
                previousVisible;

            IsCursorReleased = false; // 개발용 커서 잠금 상태 저장
        }
    }
}
