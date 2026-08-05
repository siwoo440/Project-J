using ProjectJ.Core.Services; // 사용자 설정 서비스 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 카메라와 회전 기능 참조

namespace ProjectJ.Player // 플레이어 카메라 기능 네임스페이스 선언
{ // 3인칭 카메라 범위
    [DisallowMultipleComponent] // 동일 카메라 컴포넌트 중복 방지
    [RequireComponent(typeof(Camera))] // 실제 카메라 컴포넌트 보장
    public sealed class ThirdPersonCameraController : MonoBehaviour // 회전과 충돌과 시야각을 관리하는 3인칭 카메라 선언
    { // 3인칭 카메라 기능 범위
        [Header("References")] // 카메라 참조 설정 구역 제목
        [SerializeField] private PlayerInputReader inputReader; // 플레이어 시점 입력 컴포넌트
        [SerializeField] private PlayerMovementController movementController; // 달리기 상태 제공 이동 컴포넌트
        [SerializeField] private Transform target; // 카메라 추적 피벗 대상
        [Header("Orbit")] // 회전 카메라 설정 구역 제목
        [SerializeField, Min(0.1f)] private float distance = 5f; // 카메라와 피벗 사이 기본 거리
        [SerializeField, Min(0.001f)] private float mouseSensitivity = 0.12f; // 설정 서비스 없을 때 마우스 감도
        [SerializeField, Min(1f)] private float gamepadDegreesPerSecond = 180f; // 설정 서비스 없을 때 게임패드 회전 속도
        [SerializeField, Range(-89f, 0f)] private float minimumPitch = -75f; // 카메라 최소 수직 회전 각도
        [SerializeField, Range(0f, 89f)] private float maximumPitch = 80f; // 카메라 최대 수직 회전 각도
        [SerializeField] private float startingPitch = 15f; // 게임 시작 수직 회전 각도
        [SerializeField] private bool lockCursorOnEnable = true; // 활성화 시 마우스 커서 잠금 여부
        [Header("Collision")] // 카메라 충돌 설정 구역 제목
        [SerializeField] private LayerMask cameraCollisionLayers = ~0; // 카메라를 가로막는 지형 레이어
        [SerializeField, Min(0.01f)] private float cameraCollisionRadius = 0.25f; // 벽 관통 방지 구체 반지름
        [SerializeField, Min(0f)] private float cameraCollisionPadding = 0.08f; // 벽과 카메라 사이 추가 여유
        [SerializeField, Min(0.01f)] private float minimumCameraDistance = 0.35f; // 피벗과 카메라의 최소 거리
        [SerializeField, Min(0f)] private float distanceRecoverySpeed = 8f; // 벽 이탈 뒤 초당 거리 복귀 속도
        [Header("Field Of View")] // 카메라 시야각 설정 구역 제목
        [SerializeField, Range(1f, 179f)] private float normalFieldOfView = 60f; // 기본 이동 시야각
        [SerializeField, Range(1f, 179f)] private float sprintFieldOfView = 68f; // 달리기 시야각
        [SerializeField, Min(0f)] private float fieldOfViewBlendSpeed = 24f; // 초당 시야각 전환 속도

        private Camera controlledCamera; // 제어할 실제 카메라
        private ThirdPersonCameraCollisionProbe collisionProbe; // 벽 충돌 거리 탐지기
        private SettingsService settingsService; // 연결된 사용자 설정 서비스
        private float runtimeMouseSensitivity; // 현재 적용된 마우스 감도
        private float runtimeGamepadDegreesPerSecond; // 현재 적용된 게임패드 회전 속도
        private bool runtimeInvertLookY; // 현재 적용된 수직 시점 반전
        private bool isSettingsSubscribed; // 설정 변경 이벤트 구독 여부
        private float currentDistance; // 충돌 보정이 적용된 현재 카메라 거리
        private float yaw; // 현재 수평 회전 각도
        private float pitch; // 현재 수직 회전 각도

        public float CurrentDistance => currentDistance; // 현재 충돌 보정 카메라 거리 반환
        public float CurrentYaw => yaw; // 현재 수평 회전 각도 반환
        public float CurrentPitch => pitch; // 현재 수직 회전 각도 반환

        private void Awake() // 카메라 참조와 기본 조작 설정 준비
        { // 카메라 준비 범위
            controlledCamera = GetComponent<Camera>(); // 같은 오브젝트의 카메라 조회
            runtimeMouseSensitivity = mouseSensitivity; // Inspector 마우스 감도 기본값 적용
            runtimeGamepadDegreesPerSecond = gamepadDegreesPerSecond; // Inspector 게임패드 감도 기본값 적용
            runtimeInvertLookY = false; // 기본 수직 시점 반전 해제
            currentDistance = Mathf.Max(0.1f, distance); // 시작 카메라 거리 적용

            if (inputReader == null) // 입력 읽기 컴포넌트 연결 여부 확인
            { // 입력 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "PlayerInputReader가 연결되지 않았습니다.", "CAMERA_INPUT_MISSING", this); // 카메라 입력 참조 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
                return; // 카메라 준비 중단
            } // 입력 누락 범위 종료

            if (target == null) // 카메라 대상 연결 여부 확인
            { // 대상 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "CameraTarget이 연결되지 않았습니다.", "CAMERA_TARGET_MISSING", this); // 카메라 대상 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
                return; // 카메라 준비 중단
            } // 대상 누락 범위 종료

            if (movementController == null) // 달리기 상태 참조 자동 연결 조건 확인
            { // 이동 컴포넌트 자동 연결 범위
                movementController = inputReader.GetComponent<PlayerMovementController>(); // 입력 오브젝트에서 이동 컴포넌트 조회
            } // 이동 컴포넌트 자동 연결 범위 종료

            int effectiveCollisionLayers = cameraCollisionLayers.value & ~(1 << inputReader.gameObject.layer); // 플레이어 기본 레이어를 제외한 충돌 마스크 계산
            collisionProbe = new ThirdPersonCameraCollisionProbe(inputReader.transform, effectiveCollisionLayers, cameraCollisionRadius); // 현재 설정 기반 카메라 충돌 탐지기 생성
        } // 카메라 준비 범위 종료

        private void Start() // 저장 설정 연결과 시작 카메라 상태 적용
        { // 카메라 시작 범위
            if (!enabled) // 카메라 준비 실패 상태 확인
            { // 준비 실패 범위
                return; // 시작 처리 생략
            } // 준비 실패 범위 종료

            TryConnectSettings(); // Bootstrap에서 준비된 설정 서비스 연결
            yaw = target.eulerAngles.y; // 대상 현재 수평 회전값 적용
            pitch = Mathf.Clamp(startingPitch, minimumPitch, maximumPitch); // 시작 수직 각도 범위 제한
            controlledCamera.fieldOfView = ThirdPersonCameraMath.CalculateTargetFieldOfView(false, normalFieldOfView, sprintFieldOfView); // 기본 시야각 즉시 적용
            ApplyCameraTransform(0f); // 시작 카메라 위치와 회전 적용
        } // 카메라 시작 범위 종료

        private void OnEnable() // 카메라 활성화와 커서 상태 적용
        { // 카메라 활성화 범위
            TryConnectSettings(); // 사용 가능한 설정 서비스 연결 시도

            if (lockCursorOnEnable) // 커서 잠금 설정 여부 확인
            { // 커서 잠금 범위
                Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 화면 중앙 잠금
                Cursor.visible = false; // 잠긴 마우스 커서 숨김
            } // 커서 잠금 범위 종료
        } // 카메라 활성화 범위 종료

        private void OnDisable() // 설정 이벤트와 커서 상태 정리
        { // 카메라 비활성화 범위
            DisconnectSettings(); // 설정 변경 이벤트 구독 해제

            if (!lockCursorOnEnable) // 커서 잠금 기능 사용 여부 확인
            { // 커서 유지 범위
                return; // 커서 상태 변경 생략
            } // 커서 유지 범위 종료

            Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금 해제
            Cursor.visible = true; // 마우스 커서 표시
        } // 카메라 비활성화 범위 종료

        private void LateUpdate() // 플레이어 이동 후 카메라 회전과 충돌과 시야각 갱신
        { // 카메라 프레임 갱신 범위
            float deltaTime = Time.unscaledDeltaTime; // 일시 정지와 무관한 카메라 프레임 시간 조회
            Vector2 lookInput = inputReader.LookValue; // 현재 시점 이동 입력 조회
            float inputScale = inputReader.IsLookFromMouse ? runtimeMouseSensitivity : runtimeGamepadDegreesPerSecond * deltaTime; // 입력 장치별 회전 배율 계산
            float verticalLookInput = runtimeInvertLookY ? -lookInput.y : lookInput.y; // 수직 시점 반전 설정 적용
            yaw += lookInput.x * inputScale; // 수평 시점 입력 누적
            pitch -= verticalLookInput * inputScale; // 수직 시점 입력 누적
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch); // 수직 회전 각도 범위 제한
            ApplyCameraTransform(deltaTime); // 계산된 카메라 위치와 회전 적용
            UpdateFieldOfView(deltaTime); // 달리기 상태 기반 시야각 갱신
        } // 카메라 프레임 갱신 범위 종료

        private void TryConnectSettings() // 설정 서비스 조회와 변경 이벤트 연결
        { // 설정 서비스 연결 범위
            if (isSettingsSubscribed) // 기존 설정 이벤트 연결 여부 확인
            { // 중복 연결 범위
                return; // 중복 이벤트 연결 생략
            } // 중복 연결 범위 종료

            if (!GameServiceRegistry.TryGet(out settingsService)) // 설정 서비스 조회 실패 확인
            { // 설정 서비스 없음 범위
                return; // Inspector 기본 감도 유지
            } // 설정 서비스 없음 범위 종료

            settingsService.SettingsChanged += ApplySettings; // 설정 변경 이벤트 구독
            isSettingsSubscribed = true; // 설정 이벤트 구독 상태 저장
            ApplySettings(settingsService.Current); // 저장된 조작 설정 즉시 적용
        } // 설정 서비스 연결 범위 종료

        private void DisconnectSettings() // 설정 변경 이벤트 연결 해제
        { // 설정 이벤트 해제 범위
            if (!isSettingsSubscribed || settingsService == null) // 이벤트 연결 없음 확인
            { // 이벤트 없음 범위
                return; // 이벤트 해제 생략
            } // 이벤트 없음 범위 종료

            settingsService.SettingsChanged -= ApplySettings; // 설정 변경 이벤트 구독 해제
            isSettingsSubscribed = false; // 설정 이벤트 구독 상태 해제
        } // 설정 이벤트 해제 범위 종료

        private void ApplySettings(ProjectUserSettings settings) // 사용자 조작 설정을 카메라에 적용
        { // 사용자 설정 적용 범위
            runtimeMouseSensitivity = settings.MouseSensitivity; // 저장된 마우스 감도 적용
            runtimeGamepadDegreesPerSecond = settings.GamepadLookDegreesPerSecond; // 저장된 게임패드 감도 적용
            runtimeInvertLookY = settings.InvertLookY; // 저장된 수직 시점 반전 적용
        } // 사용자 설정 적용 범위 종료

        private void ApplyCameraTransform(float deltaTime) // 회전과 벽 충돌을 반영한 카메라 위치 적용
        { // 카메라 위치 적용 범위
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f); // 현재 수평과 수직 각도 회전 생성
            Vector3 backwardDirection = -(cameraRotation * Vector3.forward); // 피벗에서 카메라로 향하는 뒤쪽 방향 계산
            float targetDistance = Mathf.Max(0.1f, distance); // Inspector 기본 목표 거리 보정

            if (collisionProbe != null && collisionProbe.TryGetClosestHitDistance(target.position, backwardDirection, targetDistance, out float hitDistance)) // 카메라 방향 벽 충돌 확인
            { // 벽 충돌 거리 적용 범위
                targetDistance = ThirdPersonCameraMath.CalculateCollisionDistance(hitDistance, cameraCollisionPadding, minimumCameraDistance, distance); // 벽 앞쪽 안전 거리 계산
            } // 벽 충돌 거리 적용 범위 종료

            currentDistance = ThirdPersonCameraMath.CalculateSmoothedDistance(currentDistance, targetDistance, distanceRecoverySpeed, deltaTime); // 관통 방지와 부드러운 거리 복귀 적용
            Vector3 cameraPosition = target.position + backwardDirection * currentDistance; // 충돌 보정 거리 기반 카메라 위치 계산
            transform.SetPositionAndRotation(cameraPosition, cameraRotation); // 카메라 위치와 회전 동시 적용
        } // 카메라 위치 적용 범위 종료

        private void UpdateFieldOfView(float deltaTime) // 달리기 상태 기반 시야각 전환
        { // 시야각 갱신 범위
            bool isSprinting = movementController != null && movementController.IsSprinting; // 현재 실제 달리기 상태 조회
            float targetFieldOfView = ThirdPersonCameraMath.CalculateTargetFieldOfView(isSprinting, normalFieldOfView, sprintFieldOfView); // 달리기 상태 기반 목표 시야각 계산
            controlledCamera.fieldOfView = ThirdPersonCameraMath.CalculateSmoothedFieldOfView(controlledCamera.fieldOfView, targetFieldOfView, fieldOfViewBlendSpeed, deltaTime); // 현재 시야각의 부드러운 전환 적용
        } // 시야각 갱신 범위 종료

        private void OnValidate() // Inspector 값 변경 시 카메라 설정 보정
        { // Inspector 값 보정 범위
            distance = Mathf.Max(0.1f, distance); // 기본 카메라 거리 최소값 보정
            minimumCameraDistance = Mathf.Clamp(minimumCameraDistance, 0.01f, distance); // 최소 카메라 거리 범위 보정
            normalFieldOfView = Mathf.Clamp(normalFieldOfView, 1f, 179f); // 기본 시야각 범위 보정
            sprintFieldOfView = Mathf.Clamp(sprintFieldOfView, normalFieldOfView, 179f); // 달리기 시야각을 기본 시야각 이상으로 보정
        } // Inspector 값 보정 범위 종료
    } // 3인칭 카메라 기능 범위 종료
} // 3인칭 카메라 범위 종료
