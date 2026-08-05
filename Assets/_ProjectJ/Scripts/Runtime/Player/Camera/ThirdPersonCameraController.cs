using UnityEngine; // Unity 카메라와 회전 기능 참조
namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 코드 묶음
    [DisallowMultipleComponent] // 동일 카메라의 회전 컴포넌트 중복 방지
    public sealed class ThirdPersonCameraController : MonoBehaviour // 기본 3인칭 회전 카메라 컴포넌트 선언
    { // 3인칭 카메라 코드 묶음
        [SerializeField] private PlayerInputReader inputReader; // 플레이어 시점 입력 읽기 컴포넌트 참조
        [SerializeField] private Transform target; // 카메라가 따라갈 피벗 대상 참조
        [SerializeField, Min(0.1f)] private float distance = 5f; // 카메라와 피벗 사이 거리 설정
        [SerializeField, Min(0.001f)] private float mouseSensitivity = 0.12f; // 마우스 이동량 회전 감도 설정
        [SerializeField, Min(1f)] private float gamepadDegreesPerSecond = 180f; // 게임패드 초당 회전 각도 설정
        [SerializeField, Range(-89f, 0f)] private float minimumPitch = -75f; // 카메라 최소 수직 회전 각도 설정
        [SerializeField, Range(0f, 89f)] private float maximumPitch = 80f; // 카메라 최대 수직 회전 각도 설정
        [SerializeField] private float startingPitch = 15f; // 게임 시작 수직 회전 각도 설정
        [SerializeField] private bool lockCursorOnEnable = true; // 활성화 시 마우스 커서 잠금 여부 설정
        private float yaw; // 현재 수평 회전 각도 저장
        private float pitch; // 현재 수직 회전 각도 저장
        private void Awake() // 카메라 필수 참조 검증
        { // 카메라 준비 처리 묶음
            if (inputReader == null) // 입력 읽기 컴포넌트 연결 여부 확인
            { // 입력 참조 누락 처리 묶음
                Debug.LogError("[ProjectJ][Gameplay][CAMERA_INPUT_MISSING] PlayerInputReader가 연결되지 않았습니다.", this); // 카메라 입력 참조 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
                return; // 카메라 준비 처리 중단
            } // 입력 참조 누락 처리 종료
            if (target == null) // 카메라 대상 연결 여부 확인
            { // 카메라 대상 누락 처리 묶음
                Debug.LogError("[ProjectJ][Gameplay][CAMERA_TARGET_MISSING] CameraTarget이 연결되지 않았습니다.", this); // 카메라 대상 누락 오류 출력
                enabled = false; // 카메라 컴포넌트 비활성화
            } // 카메라 대상 누락 처리 종료
        } // 카메라 준비 처리 종료
        private void Start() // 카메라 시작 회전 상태 준비
        { // 카메라 시작 처리 묶음
            yaw = target.eulerAngles.y; // 대상의 현재 수평 회전값으로 시작 각도 설정
            pitch = Mathf.Clamp(startingPitch, minimumPitch, maximumPitch); // 시작 수직 각도를 허용 범위로 제한
            ApplyCameraTransform(); // 시작 위치와 회전 즉시 적용
        } // 카메라 시작 처리 종료
        private void OnEnable() // 카메라 활성화 시 커서 상태 적용
        { // 커서 활성화 처리 묶음
            if (lockCursorOnEnable) // 커서 잠금 설정 여부 확인
            { // 커서 잠금 처리 묶음
                Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 게임 화면 중앙에 잠금
                Cursor.visible = false; // 잠긴 마우스 커서 숨김
            } // 커서 잠금 처리 종료
        } // 커서 활성화 처리 종료
        private void OnDisable() // 카메라 비활성화 시 커서 상태 복원
        { // 커서 복원 처리 묶음
            if (!lockCursorOnEnable) // 커서 잠금 기능 사용 여부 확인
            { // 커서 복원 생략 처리 묶음
                return; // 커서 상태 변경 생략
            } // 커서 복원 생략 처리 종료
            Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금 해제
            Cursor.visible = true; // 마우스 커서 표시
        } // 커서 복원 처리 종료
        private void LateUpdate() // 플레이어 이동 후 카메라 회전과 위치 갱신
        { // 카메라 프레임 갱신 처리 묶음
            Vector2 lookInput = inputReader.LookValue; // 현재 시점 이동 입력 조회
            float inputScale = inputReader.IsLookFromMouse ? mouseSensitivity : gamepadDegreesPerSecond * Time.unscaledDeltaTime; // 입력 장치에 따른 회전 배율 계산
            yaw += lookInput.x * inputScale; // 수평 시점 입력 누적
            pitch -= lookInput.y * inputScale; // 수직 시점 입력을 반전하여 누적
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch); // 수직 회전 각도를 허용 범위로 제한
            ApplyCameraTransform(); // 계산된 카메라 위치와 회전 적용
        } // 카메라 프레임 갱신 처리 종료
        private void ApplyCameraTransform() // 현재 회전값으로 카메라 위치와 방향 적용
        { // 카메라 변환 적용 처리 묶음
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f); // 현재 수평과 수직 각도로 카메라 회전 생성
            Vector3 cameraPosition = target.position - cameraRotation * Vector3.forward * distance; // 피벗 뒤쪽 카메라 위치 계산
            transform.SetPositionAndRotation(cameraPosition, cameraRotation); // 카메라 위치와 회전 동시 적용
        } // 카메라 변환 적용 처리 종료
    } // 3인칭 카메라 코드 종료
} // 플레이어 기능 코드 종료
