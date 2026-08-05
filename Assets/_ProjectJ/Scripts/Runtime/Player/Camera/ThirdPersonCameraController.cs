using ProjectJ.Core.Services; // 사용자 설정 서비스 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 카메라와 회전 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위
    [DisallowMultipleComponent] // 동일 카메라 회전 컴포넌트 중복 방지
    public sealed class ThirdPersonCameraController : MonoBehaviour // 기본 3인칭 회전 카메라 컴포넌트
    { // 클래스 범위
        [SerializeField] private PlayerInputReader inputReader; // 플레이어 시점 입력 컴포넌트
        [SerializeField] private Transform target; // 카메라 추적 피벗 대상
        [SerializeField, Min(0.1f)] private float distance = 5f; // 카메라와 피벗 사이 거리
        [SerializeField, Min(0.001f)] private float mouseSensitivity = 0.12f; // 설정 서비스 없을 때 마우스 감도
        [SerializeField, Min(1f)] private float gamepadDegreesPerSecond = 180f; // 설정 서비스 없을 때 게임패드 회전 속도
        [SerializeField, Range(-89f, 0f)] private float minimumPitch = -75f; // 카메라 최소 수직 회전 각도
        [SerializeField, Range(0f, 89f)] private float maximumPitch = 80f; // 카메라 최대 수직 회전 각도
        [SerializeField] private float startingPitch = 15f; // 게임 시작 수직 회전 각도
        [SerializeField] private bool lockCursorOnEnable = true; // 활성화 시 마우스 커서 잠금 여부

        private SettingsService settingsService; // 연결된 사용자 설정 서비스
        private float runtimeMouseSensitivity; // 현재 적용된 마우스 감도
        private float runtimeGamepadDegreesPerSecond; // 현재 적용된 게임패드 회전 속도
        private bool runtimeInvertLookY; // 현재 적용된 수직 시점 반전
        private bool isSettingsSubscribed; // 설정 변경 이벤트 구독 여부
        private float yaw; // 현재 수평 회전 각도
        private float pitch; // 현재 수직 회전 각도

        private void Awake() // 카메라 참조와 기본 조작 설정 준비
        { // 메서드 범위
            runtimeMouseSensitivity = mouseSensitivity; // Inspector 마우스 감도 기본값 적용
            runtimeGamepadDegreesPerSecond = gamepadDegreesPerSecond; // Inspector 게임패드 감도 기본값 적용
            runtimeInvertLookY = false; // 기본 수직 시점 반전 해제

            if (inputReader == null) // 입력 읽기 컴포넌트 연결 여부 확인
            { // 조건 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "PlayerInputReader가 연결되지 않았습니다.", "CAMERA_INPUT_MISSING", this); // 카메라 입력 참조 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
                return; // 카메라 준비 중단
            } // 조건 범위

            if (target == null) // 카메라 대상 연결 여부 확인
            { // 조건 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "CameraTarget이 연결되지 않았습니다.", "CAMERA_TARGET_MISSING", this); // 카메라 대상 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
            } // 조건 범위
        } // 메서드 범위

        private void Start() // 저장 설정 연결과 시작 회전 적용
        { // 메서드 범위
            TryConnectSettings(); // Bootstrap에서 준비된 설정 서비스 연결
            yaw = target.eulerAngles.y; // 대상 현재 수평 회전값 적용
            pitch = Mathf.Clamp(startingPitch, minimumPitch, maximumPitch); // 시작 수직 각도 범위 제한
            ApplyCameraTransform(); // 시작 카메라 위치와 회전 적용
        } // 메서드 범위

        private void OnEnable() // 카메라 활성화와 커서 상태 적용
        { // 메서드 범위
            TryConnectSettings(); // 사용 가능한 설정 서비스 연결 시도

            if (lockCursorOnEnable) // 커서 잠금 설정 여부 확인
            { // 조건 범위
                Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 화면 중앙 잠금
                Cursor.visible = false; // 잠긴 마우스 커서 숨김
            } // 조건 범위
        } // 메서드 범위

        private void OnDisable() // 설정 이벤트와 커서 상태 정리
        { // 메서드 범위
            DisconnectSettings(); // 설정 변경 이벤트 구독 해제

            if (!lockCursorOnEnable) // 커서 잠금 기능 사용 여부 확인
            { // 조건 범위
                return; // 커서 상태 변경 생략
            } // 조건 범위

            Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금 해제
            Cursor.visible = true; // 마우스 커서 표시
        } // 메서드 범위

        private void LateUpdate() // 플레이어 이동 후 카메라 회전과 위치 갱신
        { // 메서드 범위
            Vector2 lookInput = inputReader.LookValue; // 현재 시점 이동 입력 조회
            float inputScale = inputReader.IsLookFromMouse ? runtimeMouseSensitivity : runtimeGamepadDegreesPerSecond * Time.unscaledDeltaTime; // 입력 장치별 회전 배율 계산
            float verticalLookInput = runtimeInvertLookY ? -lookInput.y : lookInput.y; // 수직 시점 반전 설정 적용
            yaw += lookInput.x * inputScale; // 수평 시점 입력 누적
            pitch -= verticalLookInput * inputScale; // 수직 시점 입력 누적
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch); // 수직 회전 각도 범위 제한
            ApplyCameraTransform(); // 계산된 카메라 위치와 회전 적용
        } // 메서드 범위

        private void TryConnectSettings() // 설정 서비스 조회와 변경 이벤트 연결
        { // 메서드 범위
            if (isSettingsSubscribed) // 기존 설정 이벤트 연결 여부 확인
            { // 조건 범위
                return; // 중복 이벤트 연결 생략
            } // 조건 범위

            if (!GameServiceRegistry.TryGet(out settingsService)) // 설정 서비스 조회 실패 확인
            { // 조건 범위
                return; // Inspector 기본 감도 유지
            } // 조건 범위

            settingsService.SettingsChanged += ApplySettings; // 설정 변경 이벤트 구독
            isSettingsSubscribed = true; // 설정 이벤트 구독 상태 저장
            ApplySettings(settingsService.Current); // 저장된 조작 설정 즉시 적용
        } // 메서드 범위

        private void DisconnectSettings() // 설정 변경 이벤트 연결 해제
        { // 메서드 범위
            if (!isSettingsSubscribed || settingsService == null) // 이벤트 연결 없음 확인
            { // 조건 범위
                return; // 이벤트 해제 생략
            } // 조건 범위

            settingsService.SettingsChanged -= ApplySettings; // 설정 변경 이벤트 구독 해제
            isSettingsSubscribed = false; // 설정 이벤트 구독 상태 해제
        } // 메서드 범위

        private void ApplySettings(ProjectUserSettings settings) // 사용자 조작 설정을 카메라에 적용
        { // 메서드 범위
            runtimeMouseSensitivity = settings.MouseSensitivity; // 저장된 마우스 감도 적용
            runtimeGamepadDegreesPerSecond = settings.GamepadLookDegreesPerSecond; // 저장된 게임패드 감도 적용
            runtimeInvertLookY = settings.InvertLookY; // 저장된 수직 시점 반전 적용
        } // 메서드 범위

        private void ApplyCameraTransform() // 현재 회전값으로 카메라 위치와 방향 적용
        { // 메서드 범위
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f); // 현재 수평과 수직 각도 회전 생성
            Vector3 cameraPosition = target.position - cameraRotation * Vector3.forward * distance; // 피벗 뒤쪽 카메라 위치 계산
            transform.SetPositionAndRotation(cameraPosition, cameraRotation); // 카메라 위치와 회전 동시 적용
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
