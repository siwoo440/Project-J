using System; // 입력 재지정 예외 기능 참조
using ProjectJ.Core.Services; // 사용자 설정 서비스 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Input; // 입력 이름 상수 참조
using UnityEngine; // Unity 기본 기능 참조
using UnityEngine.InputSystem; // Input System 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위
    [DisallowMultipleComponent] // 입력 컴포넌트 중복 방지
    public sealed class PlayerInputReader : MonoBehaviour // 플레이어 입력 제공 컴포넌트
    { // 클래스 범위
        [SerializeField] private InputActionAsset inputActions; // 원본 입력 액션 에셋
        private InputActionAsset runtimeInputActions; // 런타임 입력 복제본
        private InputActionMap gameplayMap; // Gameplay 액션 맵
        private InputAction moveAction; // 이동 액션
        private InputAction lookAction; // 시점 액션
        private InputAction jumpAction; // 점프 액션
        private InputAction sprintAction; // 달리기 액션
        private InputAction crouchAction; // 앉기 액션
        private InputAction pushAction; // 밀치기 액션

        public Vector2 MoveValue => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero; // 현재 이동 입력
        public Vector2 LookValue => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero; // 현재 시점 입력
        public bool IsSprintPressed => sprintAction != null && sprintAction.IsPressed(); // 달리기 누름 상태
        public bool IsCrouchPressed => crouchAction != null && crouchAction.IsPressed(); // 앉기 누름 상태
        public bool IsLookFromMouse => lookAction != null && lookAction.activeControl != null && lookAction.activeControl.device is Mouse; // 마우스 시점 입력 여부

        private void Awake() // 런타임 입력과 저장된 재지정 준비
        { // 메서드 범위
            if (inputActions == null) // 입력 에셋 누락 확인
            { // 조건 범위
                ProjectLog.Error(ProjectLogCategory.Input, "InputSystem_Actions 에셋이 연결되지 않았습니다.", "PLAYER_INPUT_ASSET_MISSING", this); // 입력 에셋 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            } // 조건 범위

            runtimeInputActions = Instantiate(inputActions); // 입력 에셋 런타임 복제
            ApplySavedBindingOverrides(); // 저장된 입력 재지정 적용
            gameplayMap = runtimeInputActions.FindActionMap(ProjectInputNames.Gameplay.Map, false); // Gameplay 맵 검색

            if (gameplayMap == null) // Gameplay 맵 누락 확인
            { // 조건 범위
                ProjectLog.Error(ProjectLogCategory.Input, "Gameplay 액션 맵을 찾을 수 없습니다.", "GAMEPLAY_MAP_MISSING", this); // 액션 맵 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            } // 조건 범위

            moveAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Move, false); // 이동 액션 검색
            lookAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Look, false); // 시점 액션 검색
            jumpAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Jump, false); // 점프 액션 검색
            sprintAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Sprint, false); // 달리기 액션 검색
            crouchAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Crouch, false); // 앉기 액션 검색
            pushAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Push, false); // 밀치기 액션 검색

            if (moveAction == null || lookAction == null || jumpAction == null || sprintAction == null || crouchAction == null || pushAction == null) // 필수 액션 누락 확인
            { // 조건 범위
                ProjectLog.Error(ProjectLogCategory.Input, "Move, Look, Jump, Sprint, Crouch, Push 액션 구성을 확인합니다.", "PLAYER_ACTION_MISSING", this); // 필수 액션 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
            } // 조건 범위
        } // 메서드 범위

        private void OnEnable() // 입력 활성화
        { // 메서드 범위
            gameplayMap?.Enable(); // Gameplay 맵 활성화
        } // 메서드 범위

        private void OnDisable() // 입력 비활성화
        { // 메서드 범위
            gameplayMap?.Disable(); // Gameplay 맵 비활성화
        } // 메서드 범위

        private void OnDestroy() // 입력 복제본 정리
        { // 메서드 범위
            if (runtimeInputActions == null) // 입력 복제본 없음 확인
            { // 조건 범위
                return; // 복제본 정리 생략
            } // 조건 범위

            Destroy(runtimeInputActions); // 입력 복제본 제거
            runtimeInputActions = null; // 입력 복제본 참조 초기화
        } // 메서드 범위

        public bool WasJumpPressedThisFrame() // 점프 시작 입력 반환
        { // 메서드 범위
            return jumpAction != null && jumpAction.WasPressedThisFrame(); // 현재 프레임 점프 입력 반환
        } // 메서드 범위

        public bool WasPushPressedThisFrame() // 밀치기 시작 입력 반환
        { // 메서드 범위
            return pushAction != null && pushAction.WasPressedThisFrame(); // 현재 프레임 밀치기 입력 반환
        } // 메서드 범위

        public bool TryApplyBindingOverride(string actionName, int bindingIndex, string controlPath) // 지정 액션 바인딩 재지정과 저장 시도
        { // 메서드 범위
            if (gameplayMap == null || string.IsNullOrWhiteSpace(actionName) || string.IsNullOrWhiteSpace(controlPath)) // 재지정 요청 기본값 확인
            { // 조건 범위
                return false; // 잘못된 재지정 요청 반환
            } // 조건 범위

            InputAction action = gameplayMap.FindAction(actionName, false); // 이름 기준 재지정 대상 액션 검색

            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) // 액션과 바인딩 인덱스 유효성 확인
            { // 조건 범위
                ProjectLog.Warning(ProjectLogCategory.Input, $"입력 재지정 대상을 찾지 못했습니다. Action={actionName}, Index={bindingIndex}", "INPUT_REBIND_TARGET_INVALID", this); // 잘못된 재지정 대상 경고 출력
                return false; // 입력 재지정 실패 반환
            } // 조건 범위

            action.ApplyBindingOverride(bindingIndex, controlPath); // 선택한 바인딩에 새 제어 경로 적용
            SaveBindingOverrides(); // 변경된 전체 입력 재지정 저장
            return true; // 입력 재지정 성공 반환
        } // 메서드 범위

        public void ResetBindingOverrides() // 전체 입력 재지정 기본값 복원
        { // 메서드 범위
            runtimeInputActions?.RemoveAllBindingOverrides(); // 런타임 입력 재지정 전체 제거

            if (GameServiceRegistry.TryGet(out SettingsService settingsService)) // 설정 서비스 조회 성공 확인
            { // 조건 범위
                settingsService.SetInputBindingOverrides(string.Empty); // 빈 재지정 JSON 저장
            } // 조건 범위
        } // 메서드 범위

        private void ApplySavedBindingOverrides() // 설정 서비스의 입력 재지정 JSON 적용
        { // 메서드 범위
            if (!GameServiceRegistry.TryGet(out SettingsService settingsService)) // 설정 서비스 조회 실패 확인
            { // 조건 범위
                return; // 저장 입력 적용 생략
            } // 조건 범위

            string bindingOverridesJson = settingsService.Current.InputBindingOverridesJson; // 저장된 입력 재지정 JSON 조회

            if (string.IsNullOrWhiteSpace(bindingOverridesJson)) // 저장된 입력 재지정 없음 확인
            { // 조건 범위
                return; // 입력 재지정 적용 생략
            } // 조건 범위

            try // 입력 재지정 JSON 적용 예외 감시
            { // 예외 감시 범위
                runtimeInputActions.LoadBindingOverridesFromJson(bindingOverridesJson, true); // 기존 재지정을 제거하고 저장 JSON 적용
            } // 예외 감시 범위
            catch (Exception exception) // 손상된 입력 재지정 처리
            { // 예외 처리 범위
                ProjectLog.Warning(ProjectLogCategory.Input, $"저장된 입력 재지정을 적용하지 못했습니다. {exception.Message}", "INPUT_BINDING_LOAD_FAILED", this); // 입력 복원 경고 출력
            } // 예외 처리 범위
        } // 메서드 범위

        private void SaveBindingOverrides() // 현재 입력 재지정 JSON 저장
        { // 메서드 범위
            if (runtimeInputActions == null) // 런타임 입력 복제본 누락 확인
            { // 조건 범위
                return; // 입력 재지정 저장 생략
            } // 조건 범위

            if (!GameServiceRegistry.TryGet(out SettingsService settingsService)) // 설정 서비스 조회 실패 확인
            { // 조건 범위
                ProjectLog.Warning(ProjectLogCategory.Input, "설정 서비스가 없어 입력 재지정을 저장하지 못했습니다.", "INPUT_BINDING_SAVE_SERVICE_MISSING", this); // 설정 서비스 누락 경고 출력
                return; // 입력 재지정 저장 중단
            } // 조건 범위

            string bindingOverridesJson = runtimeInputActions.SaveBindingOverridesAsJson(); // 전체 입력 재지정 JSON 생성
            settingsService.SetInputBindingOverrides(bindingOverridesJson); // 사용자 설정 파일에 입력 재지정 저장
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
