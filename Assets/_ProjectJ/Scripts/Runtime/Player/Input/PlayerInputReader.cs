using ProjectJ.Input; // 입력 이름 상수 참조
using UnityEngine; // Unity 기본 기능 참조
using UnityEngine.InputSystem; // Input System 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{
    [DisallowMultipleComponent] // 입력 컴포넌트 중복 방지
    public sealed class PlayerInputReader : MonoBehaviour // 플레이어 입력 제공 컴포넌트
    {
        [SerializeField] private InputActionAsset inputActions; // 원본 입력 액션 에셋
        private InputActionAsset runtimeInputActions; // 런타임 입력 복제본
        private InputActionMap gameplayMap; // Gameplay 액션 맵
        private InputAction moveAction; // 이동 액션
        private InputAction lookAction; // 시점 액션
        private InputAction jumpAction; // 점프 액션
        private InputAction sprintAction; // 달리기 액션
        private InputAction crouchAction; // 앉기 액션

        public Vector2 MoveValue => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero; // 현재 이동 입력
        public Vector2 LookValue => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero; // 현재 시점 입력
        public bool IsSprintPressed => sprintAction != null && sprintAction.IsPressed(); // 달리기 누름 상태
        public bool IsCrouchPressed => crouchAction != null && crouchAction.IsPressed(); // 앉기 누름 상태
        public bool IsLookFromMouse => lookAction != null && lookAction.activeControl != null && lookAction.activeControl.device is Mouse; // 마우스 시점 입력 여부

        private void Awake() // 런타임 입력 준비
        {
            if (inputActions == null) // 입력 에셋 누락 확인
            {
                Debug.LogError("[ProjectJ][Input][PLAYER_INPUT_ASSET_MISSING] InputSystem_Actions 에셋이 연결되지 않았습니다.", this); // 입력 에셋 누락 오류
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            }

            runtimeInputActions = Instantiate(inputActions); // 입력 에셋 복제
            gameplayMap = runtimeInputActions.FindActionMap(ProjectInputNames.Gameplay.Map, false); // Gameplay 맵 검색

            if (gameplayMap == null) // Gameplay 맵 누락 확인
            {
                Debug.LogError("[ProjectJ][Input][GAMEPLAY_MAP_MISSING] Gameplay 액션 맵을 찾을 수 없습니다.", this); // 액션 맵 누락 오류
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            }

            moveAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Move, false); // 이동 액션 검색
            lookAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Look, false); // 시점 액션 검색
            jumpAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Jump, false); // 점프 액션 검색
            sprintAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Sprint, false); // 달리기 액션 검색
            crouchAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Crouch, false); // 앉기 액션 검색

            if (moveAction == null || lookAction == null || jumpAction == null || sprintAction == null || crouchAction == null) // 필수 액션 누락 확인
            {
                Debug.LogError("[ProjectJ][Input][PLAYER_ACTION_MISSING] Move, Look, Jump, Sprint, Crouch 액션 구성을 확인합니다.", this); // 필수 액션 누락 오류
                enabled = false; // 입력 컴포넌트 비활성화
            }
        }

        private void OnEnable() // 입력 활성화
        {
            gameplayMap?.Enable(); // Gameplay 맵 활성화
        }

        private void OnDisable() // 입력 비활성화
        {
            gameplayMap?.Disable(); // Gameplay 맵 비활성화
        }

        private void OnDestroy() // 입력 복제본 정리
        {
            if (runtimeInputActions == null) // 입력 복제본 없음 확인
            {
                return; // 복제본 정리 생략
            }

            Destroy(runtimeInputActions); // 입력 복제본 제거
            runtimeInputActions = null; // 입력 복제본 참조 초기화
        }

        public bool WasJumpPressedThisFrame() // 점프 시작 입력 반환
        {
            return jumpAction != null && jumpAction.WasPressedThisFrame(); // 현재 프레임 점프 입력
        }
    }
}
