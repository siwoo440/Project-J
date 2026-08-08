using System; // 입력 재지정 예외와 문자열 비교 기능 참조
using ProjectJ.Core.Services; // 사용자 설정 서비스 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Input; // 입력 이름 상수 참조
using UnityEngine; // Unity 기본 기능 참조
using UnityEngine.InputSystem; // Input System 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 런타임 입력 제공 기능 구성
    [DisallowMultipleComponent] // 입력 컴포넌트 중복 방지
    public sealed class PlayerInputReader : MonoBehaviour // 플레이어 입력 제공 컴포넌트 선언
    { // 이동과 액션과 저장된 키 재지정 적용 기능 구성
        [SerializeField] private InputActionAsset inputActions; // 원본 입력 액션 에셋
        private InputActionAsset runtimeInputActions; // 런타임 입력 복제본
        private InputActionMap gameplayMap; // Gameplay 액션 맵
        private InputAction moveAction; // 이동 액션
        private InputAction lookAction; // 시점 액션
        private InputAction jumpAction; // 점프 액션
        private InputAction sprintAction; // 달리기 액션
        private InputAction crouchAction; // 앉기 액션
        private InputAction pushAction; // 밀치기 액션
        private SettingsService settingsService; // 사용자 설정 변경 구독 서비스
        private string lastAppliedBindingOverridesJson = string.Empty; // 마지막으로 적용한 입력 재지정 JSON
        private bool isSettingsSubscribed; // SettingsChanged 이벤트 구독 여부
        private bool itemMovementRestricted; // 비눗방울 이동 입력 제한 상태
        private bool p2MovementRestricted; // 되감기와 카트 이동 입력 제한 상태

        public Vector2 MoveValue => !IsItemMovementRestricted && moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero; // 모든 아이템 제한 상태를 반영한 현재 이동 입력
        public Vector2 LookValue => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero; // 현재 시점 입력
        public bool IsSprintPressed => !IsItemMovementRestricted && sprintAction != null && sprintAction.IsPressed(); // 모든 아이템 제한 상태를 반영한 달리기 누름 상태
        public bool IsCrouchPressed => !IsItemMovementRestricted && crouchAction != null && crouchAction.IsPressed(); // 모든 아이템 제한 상태를 반영한 앉기 누름 상태
        public bool IsLookFromMouse => lookAction != null && lookAction.activeControl != null && lookAction.activeControl.device is Mouse; // 마우스 시점 입력 여부
        public bool IsItemMovementRestricted => itemMovementRestricted || p2MovementRestricted; // 비눗방울과 P2 이동 입력 제한 여부 반환

        private void Awake() // 런타임 입력과 저장된 재지정 준비
        { // InputActionAsset 복제와 필수 Gameplay 액션 검색
            if (inputActions == null) // 입력 에셋 누락 확인
            { // 필수 입력 에셋 누락 방어
                ProjectLog.Error(ProjectLogCategory.Input, "InputSystem_Actions 에셋이 연결되지 않았습니다.", "PLAYER_INPUT_ASSET_MISSING", this); // 입력 에셋 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            } // 필수 입력 에셋 누락 방어 마무리

            runtimeInputActions = Instantiate(inputActions); // 입력 에셋 런타임 복제
            gameplayMap = runtimeInputActions.FindActionMap(ProjectInputNames.Gameplay.Map, false); // Gameplay 맵 검색

            if (gameplayMap == null) // Gameplay 맵 누락 확인
            { // 필수 액션 맵 누락 방어
                ProjectLog.Error(ProjectLogCategory.Input, "Gameplay 액션 맵을 찾을 수 없습니다.", "GAMEPLAY_MAP_MISSING", this); // 액션 맵 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 입력 준비 중단
            } // 필수 액션 맵 누락 방어 마무리

            moveAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Move, false); // 이동 액션 검색
            lookAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Look, false); // 시점 액션 검색
            jumpAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Jump, false); // 점프 액션 검색
            sprintAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Sprint, false); // 달리기 액션 검색
            crouchAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Crouch, false); // 앉기 액션 검색
            pushAction = gameplayMap.FindAction(ProjectInputNames.Gameplay.Push, false); // 밀치기 액션 검색

            if (moveAction == null || lookAction == null || jumpAction == null || sprintAction == null || crouchAction == null || pushAction == null) // 필수 액션 누락 확인
            { // 필수 플레이어 액션 구성 오류 방어
                ProjectLog.Error(ProjectLogCategory.Input, "Move, Look, Jump, Sprint, Crouch, Push 액션 구성을 확인합니다.", "PLAYER_ACTION_MISSING", this); // 필수 액션 누락 오류 출력
                enabled = false; // 입력 컴포넌트 비활성화
                return; // 잘못된 액션 구성 후 추가 초기화 방지
            } // 필수 플레이어 액션 구성 오류 방어 마무리

            TryConnectSettings(); // 저장 설정 서비스 연결 시도
            ApplyBindingOverridesJson(settingsService != null ? settingsService.Current.InputBindingOverridesJson : string.Empty); // 현재 저장된 입력 재지정 적용
        } // 런타임 입력과 저장된 재지정 준비 마무리

        private void OnEnable() // 입력 활성화와 설정 서비스 연결 보강
        { // Gameplay 액션 맵과 설정 이벤트 활성화
            TryConnectSettings(); // 늦게 준비된 SettingsService 연결 시도
            gameplayMap?.Enable(); // Gameplay 맵 활성화
        } // 입력 활성화와 설정 서비스 연결 보강 마무리

        private void OnDisable() // 입력 액션 맵 비활성화
        { // 캐릭터 비활성 상태 입력 차단
            gameplayMap?.Disable(); // Gameplay 맵 비활성화
        } // 입력 액션 맵 비활성화 마무리

        private void OnDestroy() // 입력 복제본과 설정 이벤트 정리
        { // Scene 종료 시 런타임 입력 리소스 해제
            DisconnectSettings(); // SettingsChanged 이벤트 구독 해제

            if (runtimeInputActions == null) // 입력 복제본 없음 확인
            { // 정리할 입력 복제본 없음 처리
                return; // 복제본 정리 생략
            } // 정리할 입력 복제본 없음 처리 마무리

            Destroy(runtimeInputActions); // 입력 복제본 제거
            runtimeInputActions = null; // 입력 복제본 참조 초기화
        } // 입력 복제본과 설정 이벤트 정리 마무리

        public bool WasJumpPressedThisFrame() // 점프 시작 입력 반환
        { // 현재 프레임 Jump 액션 상태 조회
            return jumpAction != null && jumpAction.WasPressedThisFrame(); // 현재 프레임 점프 입력 반환
        } // 점프 시작 입력 반환 마무리

        public bool WasPushPressedThisFrame() // 밀치기 시작 입력 반환
        { // 현재 프레임 Push 액션 상태 조회
            return pushAction != null && pushAction.WasPressedThisFrame(); // 현재 프레임 밀치기 입력 반환
        } // 밀치기 시작 입력 반환 마무리

        public void SetItemMovementRestricted(bool isRestricted) // 아이템 효과 기반 이동과 달리기와 앉기 입력 제한 설정
        { // 비눗방울 계열 이동 입력 제한 상태 갱신
            itemMovementRestricted = isRestricted; // 새 입력 제한 상태 저장
        } // 아이템 효과 기반 이동 입력 제한 설정 마무리

        public void SetP2MovementRestricted(bool isRestricted) // 되감기와 카트 기반 이동과 달리기와 앉기 입력 제한 설정
        { // P2 아이템 계열 이동 입력 제한 상태 갱신
            p2MovementRestricted = isRestricted; // 새 P2 입력 제한 상태 저장
        } // 되감기와 카트 기반 이동과 달리기와 앉기 입력 제한 설정 마무리

        public bool TryApplyBindingOverride(string actionName, int bindingIndex, string controlPath) // 지정 액션 바인딩 재지정과 저장 시도
        { // 런타임 PlayerInputReader 직접 재지정 호환 API
            if (gameplayMap == null || string.IsNullOrWhiteSpace(actionName) || string.IsNullOrWhiteSpace(controlPath)) // 재지정 요청 기본값 확인
            { // 잘못된 재지정 요청 방어
                return false; // 잘못된 재지정 요청 반환
            } // 잘못된 재지정 요청 방어 마무리

            InputAction action = gameplayMap.FindAction(actionName, false); // 이름 기준 재지정 대상 액션 검색

            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) // 액션과 바인딩 인덱스 유효성 확인
            { // 잘못된 재지정 대상 방어
                ProjectLog.Warning(ProjectLogCategory.Input, $"입력 재지정 대상을 찾지 못했습니다. Action={actionName}, Index={bindingIndex}", "INPUT_REBIND_TARGET_INVALID", this); // 잘못된 재지정 대상 경고 출력
                return false; // 입력 재지정 실패 반환
            } // 잘못된 재지정 대상 방어 마무리

            action.ApplyBindingOverride(bindingIndex, controlPath); // 선택한 바인딩에 새 제어 경로 적용
            SaveBindingOverrides(); // 변경된 전체 입력 재지정 저장
            return true; // 입력 재지정 성공 반환
        } // 지정 액션 바인딩 재지정과 저장 시도 마무리

        public void ResetBindingOverrides() // 전체 입력 재지정 기본값 복원
        { // 런타임 입력 복제본과 저장 JSON 동시에 초기화
            runtimeInputActions?.RemoveAllBindingOverrides(); // 런타임 입력 재지정 전체 제거
            lastAppliedBindingOverridesJson = string.Empty; // 마지막 입력 재지정 상태 초기화

            if (GameServiceRegistry.TryGet(out SettingsService service)) // 설정 서비스 조회 성공 확인
            { // 기본 키 상태 저장
                service.SetInputBindingOverrides(string.Empty); // 빈 재지정 JSON 저장
            } // 기본 키 상태 저장 마무리
        } // 전체 입력 재지정 기본값 복원 마무리

        private void TryConnectSettings() // SettingsService 조회와 변경 이벤트 연결
        { // 설정 메뉴 Apply 이후 현재 PlayerInputReader에도 키를 즉시 반영하기 위한 구독
            if (isSettingsSubscribed) // 기존 설정 이벤트 연결 여부 확인
            { // 중복 이벤트 연결 방지
                return; // 기존 연결 유지
            } // 중복 이벤트 연결 방지 마무리

            if (!GameServiceRegistry.TryGet(out SettingsService service) || service.State != GameServiceState.Initialized) // 설정 서비스 등록과 초기화 여부 확인
            { // Bootstrap 이전 직접 Scene 실행 처리
                return; // Inspector 기본 입력 상태 유지
            } // Bootstrap 이전 직접 Scene 실행 처리 마무리

            settingsService = service; // 준비 완료 SettingsService 참조 저장
            settingsService.SettingsChanged += HandleSettingsChanged; // 설정 변경 이벤트 구독
            isSettingsSubscribed = true; // 설정 이벤트 구독 상태 저장
        } // SettingsService 조회와 변경 이벤트 연결 마무리

        private void DisconnectSettings() // SettingsService 변경 이벤트 연결 해제
        { // Scene 종료 시 서비스 이벤트 참조 정리
            if (!isSettingsSubscribed || settingsService == null) // 이벤트 연결 없음 여부 확인
            { // 해제할 이벤트 없음 처리
                isSettingsSubscribed = false; // 구독 상태 안전 초기화
                settingsService = null; // 서비스 참조 안전 초기화
                return; // 이벤트 해제 생략
            } // 해제할 이벤트 없음 처리 마무리

            settingsService.SettingsChanged -= HandleSettingsChanged; // 설정 변경 이벤트 구독 해제
            isSettingsSubscribed = false; // 설정 이벤트 구독 상태 초기화
            settingsService = null; // 설정 서비스 참조 초기화
        } // SettingsService 변경 이벤트 연결 해제 마무리

        private void HandleSettingsChanged(ProjectUserSettings settings) // 사용자 설정 Apply 이후 입력 재지정 갱신
        { // 다른 설정 변경에서는 같은 JSON 재적용 생략
            string newOverridesJson = settings != null ? settings.InputBindingOverridesJson ?? string.Empty : string.Empty; // 새 입력 재지정 JSON 안전 조회

            if (string.Equals(lastAppliedBindingOverridesJson, newOverridesJson, StringComparison.Ordinal)) // 기존 적용 JSON과 동일 여부 확인
            { // 입력 재지정 변화 없음 처리
                return; // 불필요한 InputAction 재바인딩 생략
            } // 입력 재지정 변화 없음 처리 마무리

            ApplyBindingOverridesJson(newOverridesJson); // 변경된 입력 재지정 JSON 즉시 적용
        } // 사용자 설정 Apply 이후 입력 재지정 갱신 마무리

        private void ApplyBindingOverridesJson(string bindingOverridesJson) // 지정 JSON을 런타임 InputActionAsset에 안전 적용
        { // 액션 맵 활성 상태 보존과 손상 JSON 예외 처리
            if (runtimeInputActions == null) // 런타임 입력 복제본 누락 여부 확인
            { // 적용할 InputActionAsset 없음 처리
                return; // 입력 재지정 적용 생략
            } // 적용할 InputActionAsset 없음 처리 마무리

            bool wasGameplayEnabled = gameplayMap != null && gameplayMap.enabled; // Gameplay 맵 기존 활성 상태 저장
            gameplayMap?.Disable(); // 재지정 변경 중 Gameplay 입력 일시 비활성화
            runtimeInputActions.RemoveAllBindingOverrides(); // 기존 런타임 재지정 전체 제거

            try // 입력 재지정 JSON 적용 예외 감시
            { // 빈 JSON과 저장 JSON 구분 적용
                if (!string.IsNullOrWhiteSpace(bindingOverridesJson)) // 저장된 입력 재지정 존재 여부 확인
                { // 저장된 Binding Override 복원
                    runtimeInputActions.LoadBindingOverridesFromJson(bindingOverridesJson, true); // 기존 재지정을 제거하고 저장 JSON 적용
                } // 저장된 Binding Override 복원 마무리

                lastAppliedBindingOverridesJson = bindingOverridesJson ?? string.Empty; // 정상 적용된 JSON 상태 저장
            } // 빈 JSON과 저장 JSON 구분 적용 마무리
            catch (Exception exception) // 손상된 입력 재지정 처리
            { // 잘못된 Binding Override 안전 복구
                runtimeInputActions.RemoveAllBindingOverrides(); // 손상 재지정 제거 후 기본 키 복원
                lastAppliedBindingOverridesJson = string.Empty; // 마지막 적용 상태 기본 키로 초기화
                ProjectLog.Warning(ProjectLogCategory.Input, $"저장된 입력 재지정을 적용하지 못했습니다. {exception.Message}", "INPUT_BINDING_LOAD_FAILED", this); // 입력 복원 경고 출력
            } // 잘못된 Binding Override 안전 복구 마무리

            if (wasGameplayEnabled) // 기존 Gameplay 맵 활성 상태 확인
            { // 재지정 완료 뒤 기존 입력 상태 복원
                gameplayMap?.Enable(); // Gameplay 맵 다시 활성화
            } // 재지정 완료 뒤 기존 입력 상태 복원 마무리
        } // 지정 JSON을 런타임 InputActionAsset에 안전 적용 마무리

        private void SaveBindingOverrides() // 현재 입력 재지정 JSON 저장
        { // 런타임 InputActionAsset 전체 Override를 SettingsService에 전달
            if (runtimeInputActions == null) // 런타임 입력 복제본 누락 확인
            { // 저장할 InputActionAsset 없음 처리
                return; // 입력 재지정 저장 생략
            } // 저장할 InputActionAsset 없음 처리 마무리

            if (!GameServiceRegistry.TryGet(out SettingsService service)) // 설정 서비스 조회 실패 확인
            { // 저장 서비스 경로 누락 방어
                ProjectLog.Warning(ProjectLogCategory.Input, "설정 서비스가 없어 입력 재지정을 저장하지 못했습니다.", "INPUT_BINDING_SAVE_SERVICE_MISSING", this); // 설정 서비스 누락 경고 출력
                return; // 입력 재지정 저장 중단
            } // 저장 서비스 경로 누락 방어 마무리

            string bindingOverridesJson = runtimeInputActions.SaveBindingOverridesAsJson(); // 전체 입력 재지정 JSON 생성
            lastAppliedBindingOverridesJson = bindingOverridesJson; // 직접 변경된 재지정 상태 저장
            service.SetInputBindingOverrides(bindingOverridesJson); // 사용자 설정 파일에 입력 재지정 저장
        } // 현재 입력 재지정 JSON 저장 마무리
    } // 플레이어 입력 제공 컴포넌트 마무리
} // 플레이어 기능 네임스페이스 마무리
