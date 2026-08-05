using ProjectJ.Input; // 프로젝트 입력 이름 상수 참조
using UnityEngine; // Unity 컴포넌트와 벡터 기능 참조
using UnityEngine.InputSystem; // Unity Input System 기능 참조
namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 코드 묶음
    [DisallowMultipleComponent] // 동일 오브젝트의 입력 읽기 컴포넌트 중복 방지
    public sealed class PlayerInputReader : MonoBehaviour // 플레이어 입력 값 제공 컴포넌트 선언
    { // 플레이어 입력 읽기 코드 묶음
        [SerializeField] private InputActionAsset inputActions; // 원본 입력 액션 에셋 참조
        private InputActionAsset runtimeInputActions; // 런타임 입력 액션 복제본 저장
        private InputActionMap gameplayMap; // Gameplay 액션 맵 저장
        private InputAction moveAction; // 이동 액션 저장
        private InputAction lookAction; // 시점 액션 저장
        private InputAction jumpAction; // 점프 액션 저장
        public Vector2 MoveValue => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero; // 현재 이동 입력 값 반환
        public Vector2 LookValue => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero; // 현재 시점 입력 값 반환
        public bool IsLookFromMouse => lookAction != null && lookAction.activeControl != null && lookAction.activeControl.device is Mouse; // 현재 시점 입력의 마우스 여부 반환
        private void Awake() // 런타임 입력 액션 준비
        { // 입력 준비 처리 묶음
            if (inputActions == null) // 입력 액션 에셋 연결 여부 확인
            { // 입력 에셋 누락 처리 묶음
                Debug.LogError("[ProjectJ][Input][PLAYER_INPUT_ASSET_MISSING] InputSystem_Actions 에셋이 연결되지 않았습니다.", this); // 입력 에셋 누락 오류 출력
                enabled = false; // 입력 읽기 컴포넌트 비활성화
                return; // 입력 준비 처리 중단
            } // 입력 에셋 누락 처리 종료
            runtimeInputActions = Instantiate(inputActions); // 원본 보호용 입력 액션 복제본 생성
            gameplayMap = runtimeInputActions.FindActionMap(ProjectInputNames.Gameplay.Map, false); // Gameplay 액션 맵 검색
            if (gameplayMap == null) // Gameplay 액션 맵 존재 여부 확인
            { // 액션 맵 누락 처리 묶음
                Debug.LogError("[ProjectJ][Input][GAMEPLAY_MAP_MISSING] Gameplay 액션 맵을 찾을 수 없습니다.", this); // 액션 맵 누락 오류 출력
                enabled = false; // 입력 읽기 컴포넌트 비활성화
                return; // 입력 준비 처리 중단
            } // 액션 맵 누락 처리 종료
            moveAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Move, false); // 이동 액션 검색
            lookAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Look, false); // 시점 액션 검색
            jumpAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Jump, false); // 점프 액션 검색
            if (moveAction == null || lookAction == null || jumpAction == null) // 필수 입력 액션 존재 여부 확인
            { // 필수 액션 누락 처리 묶음
                Debug.LogError("[ProjectJ][Input][PLAYER_ACTION_MISSING] Move, Look, Jump 액션 구성을 확인합니다.", this); // 필수 입력 액션 누락 오류 출력
                enabled = false; // 입력 읽기 컴포넌트 비활성화
            } // 필수 액션 누락 처리 종료
        } // 입력 준비 처리 종료
        private void OnEnable() // 컴포넌트 활성화 시 입력 시작
        { // 입력 활성화 처리 묶음
            gameplayMap?.Enable(); // Gameplay 액션 맵 활성화
        } // 입력 활성화 처리 종료
        private void OnDisable() // 컴포넌트 비활성화 시 입력 중지
        { // 입력 비활성화 처리 묶음
            gameplayMap?.Disable(); // Gameplay 액션 맵 비활성화
        } // 입력 비활성화 처리 종료
        private void OnDestroy() // 컴포넌트 제거 시 입력 복제본 정리
        { // 입력 정리 처리 묶음
            if (runtimeInputActions == null) // 입력 복제본 존재 여부 확인
            { // 입력 복제본 없음 처리 묶음
                return; // 입력 복제본 제거 생략
            } // 입력 복제본 없음 처리 종료
            Destroy(runtimeInputActions); // 런타임 입력 액션 복제본 제거
            runtimeInputActions = null; // 입력 복제본 참조 초기화
        } // 입력 정리 처리 종료
        public bool WasJumpPressedThisFrame() // 현재 프레임 점프 입력 여부 반환
        { // 점프 입력 확인 처리 묶음
            return jumpAction != null && jumpAction.WasPressedThisFrame(); // 점프 버튼의 현재 프레임 입력 결과 반환
        } // 점프 입력 확인 처리 종료
    } // 플레이어 입력 읽기 코드 종료
} // 플레이어 기능 코드 종료
