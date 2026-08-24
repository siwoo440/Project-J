using System.Collections.Generic;
using ProjectJ.CameraSystem; // 카메라 위치 보간 정책 사용
using ProjectJ.Debugging; // 개발용 커서 입력 정책 사용
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class
        ProjectJLocalPlayerPresentationController :
            MonoBehaviour
    {
        private const float MouseSensitivity =
            0.15f;

        private const float MinimumPitch =
            -45f;

        private const float MaximumPitch =
            70f;

        private const float StandingTargetHeight =
            1.5f;

        private const float CrouchTargetHeight =
            0.85f;

        private const float DefaultDistance =
            7.5f;

        private const float MinimumDistance =
            3.5f;

        private const float MaximumDistance =
            10f;

        private const float ZoomStep =
            0.75f;

        private const float NormalFov =
            60f;

        private const float SprintFov =
            68f;

        private const float FovChangeSpeed =
            8f;

        private const float CameraPositionSmoothingSpeed = // 카메라 위치 추적 속도
            18f; // 빠른 반응과 Tick 흔들림 완화 균형값

        private const float CameraSnapDistance = // 순간이동 즉시 추적 거리
            4f; // 부활·Scene 전환 보간 이동 방지 기준

        private readonly List<Camera>
            suspendedCameras =
                new List<Camera>();

        private readonly List<AudioListener>
            suspendedAudioListeners =
                new List<AudioListener>();

        private ProjectJNetworkPlayer boundPlayer;

        private GameObject cameraRigRoot;
        private Transform pitchPivot;
        private Camera gameplayCamera;
        private AudioListener gameplayAudioListener;

        private float yaw;
        private float pitch;
        private float cameraDistance =
            DefaultDistance;

        private Vector3 smoothedCameraTargetPosition; // 현재 보간된 카메라 목표 위치

        private bool hasSmoothedCameraTargetPosition; // 카메라 보간 위치 초기화 여부

        private bool refreshPresentationAfterSceneChange;

        public static
            ProjectJLocalPlayerPresentationController
            Instance
        {
            get;
            private set;
        }

        public ProjectJNetworkPlayer BoundPlayer =>
            boundPlayer;

        public bool IsBound =>
            boundPlayer != null;

        public bool CameraBound =>
            boundPlayer != null &&
            gameplayCamera != null &&
            gameplayCamera.enabled;

        public bool LocalUiBound =>
            boundPlayer != null;

        public bool AudioListenerBound =>
            boundPlayer != null &&
            gameplayAudioListener != null &&
            gameplayAudioListener.enabled;

        public int SuspendedCameraCount =>
            suspendedCameras.Count;

        public float CameraFollowOffset // 카메라와 실제 목표 위치 차이
        {
            get; // 외부 진단 화면 조회
            private set; // 내부 카메라 추적에서만 변경
        }

        public float CameraStepDistance // 최근 프레임 카메라 이동 거리
        {
            get; // 외부 진단 화면 조회
            private set; // 내부 카메라 추적에서만 변경
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void Install()
        {
            EnsureInstance();
        }

        public static void BindLocalPlayer(
            ProjectJNetworkPlayer player
        )
        {
            if (
                player == null ||
                !player.HasLocalInputAuthority
            )
            {
                return;
            }

            EnsureInstance()
                .Bind(
                    player
                );
        }

        public static void UnbindLocalPlayer(
            ProjectJNetworkPlayer player
        )
        {
            if (Instance == null)
            {
                return;
            }

            Instance.Unbind(
                player
            );
        }

        private static
            ProjectJLocalPlayerPresentationController
            EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject root =
                new GameObject(
                    "[ProjectJ] Local Player Presentation"
                );

            DontDestroyOnLoad(
                root
            );

            Instance =
                root.AddComponent<
                    ProjectJLocalPlayerPresentationController
                >();

            return Instance;
        }

        private void Awake()
        {
            if (
                Instance != null &&
                Instance != this
            )
            {
                Destroy(
                    gameObject
                );

                return;
            }

            Instance =
                this;

            DontDestroyOnLoad(
                gameObject
            );
        }

        private void LateUpdate()
        {
            if (refreshPresentationAfterSceneChange)
            {
                refreshPresentationAfterSceneChange =
                    false;

                if (boundPlayer != null)
                {
                    SuspendOtherLocalPresentation();
                }
            }

            if (
                boundPlayer == null ||
                gameplayCamera == null ||
                pitchPivot == null
            )
            {
                return;
            }

            bool canProcessCameraInput = // 카메라 입력 가능 여부 계산
                ProjectJDebugCursorReleasePolicy.CanProcessCameraInput( // 커서 입력 정책 호출
                    ProjectJDebugCursorReleaseController.IsCursorReleased // 현재 커서 해제 상태 전달
                );

            if (canProcessCameraInput) // 게임 카메라 입력 허용 확인
            {
                UpdateLook(); // 마우스 시점 회전 처리
                UpdateZoom(); // 마우스 휠 줌 처리
            }

            UpdateCameraTransform();
            UpdateCameraFov();
        }

        private void Bind(
            ProjectJNetworkPlayer player
        )
        {
            if (boundPlayer == player)
            {
                return;
            }

            if (boundPlayer != null)
            {
                Unbind(
                    boundPlayer
                );
            }

            boundPlayer =
                player;

            EnsureCameraRig();
            SuspendOtherLocalPresentation();

            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            SceneManager.activeSceneChanged +=
                OnActiveSceneChanged;

            cameraRigRoot.SetActive(
                true
            );

            gameplayCamera.enabled =
                true;

            gameplayAudioListener.enabled =
                true;

            yaw =
                NormalizeSignedAngle(
                    player.transform.eulerAngles.y
                );

            pitch =
                10f;

            cameraDistance =
                DefaultDistance;

            ResetCameraPositionSmoothing(); // 새 Player 위치에서 보간 상태 초기화

            UpdateCameraTransform();

            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;

            Debug.Log(
                "[Project J/Fusion] " +
                "68일차 Local Presentation 연결 / P" +
                player.Owner.AsIndex
            );
        }

        private void Unbind(
            ProjectJNetworkPlayer player
        )
        {
            if (
                boundPlayer == null ||
                boundPlayer != player
            )
            {
                return;
            }

            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            refreshPresentationAfterSceneChange =
                false;

            boundPlayer =
                null;

            ResetCameraPositionSmoothing(); // 연결 해제 시 이전 위치 제거

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled =
                    false;
            }

            if (gameplayAudioListener != null)
            {
                gameplayAudioListener.enabled =
                    false;
            }

            if (cameraRigRoot != null)
            {
                cameraRigRoot.SetActive(
                    false
                );
            }

            RestoreSuspendedLocalPresentation();

            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }

        private void OnActiveSceneChanged(
            Scene previousScene,
            Scene newScene
        )
        {
            if (boundPlayer == null)
            {
                return;
            }

            refreshPresentationAfterSceneChange =
                true;

            ResetCameraPositionSmoothing(); // 새 Scene에서 이전 위치 보간 방지
        }

        private void EnsureCameraRig()
        {
            if (cameraRigRoot != null)
            {
                return;
            }

            cameraRigRoot =
                new GameObject(
                    "ProjectJ_LocalGameplayCameraRig"
                );

            cameraRigRoot.transform.SetParent(
                transform,
                false
            );

            GameObject pitchObject =
                new GameObject(
                    "PitchPivot"
                );

            pitchPivot =
                pitchObject.transform;

            pitchPivot.SetParent(
                cameraRigRoot.transform,
                false
            );

            GameObject cameraObject =
                new GameObject(
                    "LocalGameplayCamera"
                );

            cameraObject.transform.SetParent(
                pitchPivot,
                false
            );

            cameraObject.tag =
                "MainCamera";

            gameplayCamera =
                cameraObject.AddComponent<
                    Camera
                >();

            gameplayAudioListener =
                cameraObject.AddComponent<
                    AudioListener
                >();

            gameplayCamera.fieldOfView =
                NormalFov;

            gameplayCamera.nearClipPlane =
                0.1f;

            gameplayCamera.farClipPlane =
                1000f;

            cameraRigRoot.SetActive(
                false
            );
        }

        private void SuspendOtherLocalPresentation()
        {
            RestoreSuspendedLocalPresentation();

            Camera[] cameras =
                FindObjectsByType<Camera>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < cameras.Length;
                i++
            )
            {
                Camera candidate =
                    cameras[i];

                if (
                    candidate == null ||
                    candidate == gameplayCamera ||
                    !candidate.enabled
                )
                {
                    continue;
                }

                candidate.enabled =
                    false;

                suspendedCameras.Add(
                    candidate
                );
            }

            AudioListener[] listeners =
                FindObjectsByType<AudioListener>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < listeners.Length;
                i++
            )
            {
                AudioListener candidate =
                    listeners[i];

                if (
                    candidate == null ||
                    candidate == gameplayAudioListener ||
                    !candidate.enabled
                )
                {
                    continue;
                }

                candidate.enabled =
                    false;

                suspendedAudioListeners.Add(
                    candidate
                );
            }
        }

        private void RestoreSuspendedLocalPresentation()
        {
            for (
                int i = 0;
                i < suspendedCameras.Count;
                i++
            )
            {
                Camera candidate =
                    suspendedCameras[i];

                if (candidate != null)
                {
                    candidate.enabled =
                        true;
                }
            }

            suspendedCameras.Clear();

            for (
                int i = 0;
                i < suspendedAudioListeners.Count;
                i++
            )
            {
                AudioListener candidate =
                    suspendedAudioListeners[i];

                if (candidate != null)
                {
                    candidate.enabled =
                        true;
                }
            }

            suspendedAudioListeners.Clear();
        }

        private void UpdateLook()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Vector2 lookDelta =
                Mouse.current.delta.ReadValue();

            yaw =
                NormalizeSignedAngle(
                    yaw +
                    lookDelta.x *
                    MouseSensitivity
                );

            pitch =
                Mathf.Clamp(
                    pitch -
                    lookDelta.y *
                    MouseSensitivity,
                    MinimumPitch,
                    MaximumPitch
                );
        }

        private void UpdateZoom()
        {
            if (Mouse.current == null)
            {
                return;
            }

            float scrollY =
                Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scrollY) <= 0.01f)
            {
                return;
            }

            float direction =
                scrollY > 0f
                    ? -1f
                    : 1f;

            cameraDistance =
                Mathf.Clamp(
                    cameraDistance +
                    direction *
                    ZoomStep,
                    MinimumDistance,
                    MaximumDistance
                );
        }

        private void UpdateCameraTransform()
        {
            if (
                boundPlayer == null ||
                cameraRigRoot == null ||
                pitchPivot == null ||
                gameplayCamera == null
            )
            {
                return;
            }

            float targetHeight =
                boundPlayer.IsCrouching
                    ? CrouchTargetHeight
                    : StandingTargetHeight;

            Vector3 targetPosition = // Player 기준 카메라 목표 위치
                boundPlayer.transform.position +
                Vector3.up *
                targetHeight;

            Vector3 previousCameraPosition = // 이전 프레임 카메라 위치
                cameraRigRoot.transform.position; // 현재 Rig 위치 저장

            bool hadPreviousPosition = // 이전 보간 위치 존재 여부
                hasSmoothedCameraTargetPosition; // 초기화 상태 저장

            bool shouldSnap = // 즉시 목표 위치 이동 여부
                !hadPreviousPosition || // 최초 연결 상태 확인
                ProjectJCameraSmoothingPolicy.ShouldSnap( // 순간이동 거리 판정
                    smoothedCameraTargetPosition, // 현재 보간 위치 전달
                    targetPosition, // 새 목표 위치 전달
                    CameraSnapDistance // 순간이동 거리 기준 전달
                );

            if (shouldSnap) // 최초 연결·순간이동 확인
            {
                smoothedCameraTargetPosition = // 목표 위치 즉시 적용
                    targetPosition; // 부활·Scene 전환 위치 사용
            }
            else
            {
                smoothedCameraTargetPosition = // 다음 보간 위치 계산
                    ProjectJCameraSmoothingPolicy.CalculateNextPosition( // 프레임 독립 보간 호출
                        smoothedCameraTargetPosition, // 현재 보간 위치 전달
                        targetPosition, // Player 목표 위치 전달
                        CameraPositionSmoothingSpeed, // 카메라 추적 속도 전달
                        Time.deltaTime // 현재 프레임 시간 전달
                    );
            }

            cameraRigRoot.transform.position = // 카메라 Rig 위치 적용
                smoothedCameraTargetPosition; // 계산된 보간 위치 사용

            CameraStepDistance = // 최근 프레임 카메라 이동 거리 갱신
                hadPreviousPosition // 이전 위치 존재 여부 확인
                    ? Vector3.Distance( // 이전 위치와 현재 위치 거리 계산
                        previousCameraPosition, // 이전 카메라 위치
                        smoothedCameraTargetPosition // 현재 보간 위치
                    )
                    : 0f; // 최초 연결 이동량 제외

            CameraFollowOffset = // 실제 목표와 보간 위치 차이 갱신
                Vector3.Distance( // 두 위치 거리 계산
                    smoothedCameraTargetPosition, // 현재 보간 위치
                    targetPosition // Player 기준 목표 위치
                );

            hasSmoothedCameraTargetPosition = // 다음 프레임 보간 활성화
                true; // 현재 위치 초기화 완료

            cameraRigRoot.transform.rotation =
                Quaternion.Euler(
                    0f,
                    yaw,
                    0f
                );

            pitchPivot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f
                );

            gameplayCamera.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -cameraDistance
                );

            gameplayCamera.transform.localRotation =
                Quaternion.identity;
        }

        private void ResetCameraPositionSmoothing() // 카메라 보간 상태 초기화
        {
            smoothedCameraTargetPosition = // 이전 보간 위치 제거
                Vector3.zero; // 기본 위치 사용

            hasSmoothedCameraTargetPosition = // 초기화 상태 해제
                false; // 다음 추적에서 즉시 목표 적용

            CameraFollowOffset = // 목표 위치 차이 초기화
                0f; // 초기 진단값 사용

            CameraStepDistance = // 프레임 이동 거리 초기화
                0f; // 초기 진단값 사용
        }

        private void UpdateCameraFov()
        {
            float targetFov =
                boundPlayer != null &&
                boundPlayer.IsSprinting
                    ? SprintFov
                    : NormalFov;

            gameplayCamera.fieldOfView =
                Mathf.MoveTowards(
                    gameplayCamera.fieldOfView,
                    targetFov,
                    FovChangeSpeed *
                    Time.deltaTime
                );
        }

        private static float NormalizeSignedAngle(
            float angle
        )
        {
            return
                Mathf.Repeat(
                    angle + 180f,
                    360f
                ) -
                180f;
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ProjectJDebugOverlayController.IsVisible) // 통합 패널 선택 상태 확인
            {
                return; // 독립 진단창 출력 차단
            }

            if (boundPlayer == null)
            {
                return;
            }

            const float width =
                300f;

            const float height =
                74f;

            Rect rect =
                new Rect(
                    Screen.width - width - 20f,
                    20f,
                    width,
                    height
                );

            GUI.Box(
                rect,
                string.Empty
            );

            GUI.Label(
                new Rect(
                    rect.x + 12f,
                    rect.y + 8f,
                    width - 24f,
                    24f
                ),
                "LOCAL PLAYER  P" +
                boundPlayer.Owner.AsIndex
            );

            GUI.Label(
                new Rect(
                    rect.x + 12f,
                    rect.y + 34f,
                    width - 24f,
                    24f
                ),
                "Camera ON  |  UI LOCAL  |  Audio ON"
            );
#endif
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            refreshPresentationAfterSceneChange =
                false;

            RestoreSuspendedLocalPresentation();

            if (Instance == this)
            {
                Instance =
                    null;
            }
        }
    }
}
