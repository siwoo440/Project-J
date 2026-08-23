using System.Collections.Generic;
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

            UpdateLook();
            UpdateZoom();
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

            cameraRigRoot.transform.position =
                boundPlayer.transform.position +
                Vector3.up *
                targetHeight;

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
