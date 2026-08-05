using UnityEngine; // Unity 컴포넌트와 개발용 GUI 기능 참조
using UnityEngine.InputSystem; // Unity Input System 액션 기능 참조

namespace ProjectJ.Input // 프로젝트 입력 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 중복 입력 모니터 추가 방지
    public sealed class InputActionDebugMonitor : MonoBehaviour // 개발용 입력 액션 동작 확인 컴포넌트 선언
    {
        [SerializeField] private InputActionAsset inputActions; // 검증할 프로젝트 입력 액션 에셋 참조
        [SerializeField] private InputDebugMap mapToTest = InputDebugMap.Gameplay; // 검증할 액션 맵 선택
        [SerializeField] private bool logPerformedActions = true; // 수행된 버튼 액션의 Console 출력 여부 설정
        private InputActionAsset runtimeInputActions; // 원본 에셋을 변경하지 않기 위한 런타임 복제본 저장
        private InputActionMap activeMap; // 현재 활성화된 검증용 액션 맵 저장
        private string lastAction = "None"; // 최근 수행된 액션 정보 저장
        private Rect windowRect = new Rect(20f, 290f, 360f, 190f); // 입력 검증 창 위치와 크기 저장

        public void Configure(InputActionAsset inputActionAsset, InputDebugMap debugMap) // 입력 에셋과 검증 대상 액션 맵 설정
        {
            inputActions = inputActionAsset; // 전달된 입력 액션 에셋 저장
            mapToTest = debugMap; // 전달된 검증 대상 액션 맵 저장
        }

        private void OnEnable() // 컴포넌트 활성화 시 선택된 액션 맵 준비
        {
            EnableSelectedMap(); // 선택된 액션 맵 복제와 활성화 실행
        }

        private void OnDisable() // 컴포넌트 비활성화 시 런타임 입력 에셋 정리
        {
            DisableRuntimeAsset(); // 액션 맵 비활성화와 런타임 복제본 제거
        }

        private void EnableSelectedMap() // 선택된 입력 액션 맵 복제와 활성화
        {
            if (inputActions == null) // 입력 액션 에셋 연결 여부 확인
            {
                Debug.LogError("[Input] InputActionAsset이 연결되지 않았습니다.", this); // 입력 에셋 누락 오류 출력
                return; // 입력 모니터 활성화 중단
            }

            runtimeInputActions = Instantiate(inputActions); // 원본 에셋 보호를 위한 런타임 입력 에셋 복제
            string mapName = GetMapName(mapToTest); // 검증 대상 enum에서 실제 액션 맵 이름 조회
            activeMap = runtimeInputActions.FindActionMap(mapName, false); // 런타임 복제본에서 대상 액션 맵 검색

            if (activeMap == null) // 대상 액션 맵 검색 실패 여부 확인
            {
                Debug.LogError($"[Input] {mapName} 액션 맵을 찾을 수 없습니다.", this); // 대상 액션 맵 누락 오류 출력
                DisableRuntimeAsset(); // 생성된 런타임 입력 에셋 정리
                return; // 입력 모니터 활성화 중단
            }

            activeMap.actionTriggered += OnActionTriggered; // 현재 액션 맵의 모든 액션 이벤트 구독
            activeMap.Enable(); // 현재 검증 대상 액션 맵 활성화
            Debug.Log($"[Input] {mapName} 액션 맵 검증을 시작합니다.", this); // 입력 검증 시작 로그 출력
        }

        private void DisableRuntimeAsset() // 활성 액션 맵과 런타임 입력 에셋 정리
        {
            if (activeMap != null) // 활성 액션 맵 존재 여부 확인
            {
                activeMap.actionTriggered -= OnActionTriggered; // 현재 액션 맵의 모든 액션 이벤트 구독 해제
                activeMap.Disable(); // 현재 액션 맵 비활성화
                activeMap = null; // 활성 액션 맵 참조 초기화
            }

            if (runtimeInputActions != null) // 런타임 입력 에셋 복제본 존재 여부 확인
            {
                Destroy(runtimeInputActions); // 런타임 입력 에셋 복제본 제거
                runtimeInputActions = null; // 런타임 입력 에셋 참조 초기화
            }
        }

        private void OnActionTriggered(InputAction.CallbackContext context) // 현재 액션 맵의 수행된 입력 정보 기록
        {
            if (context.phase != InputActionPhase.Performed) // 액션 수행 단계인지 확인
            {
                return; // 시작 또는 취소 단계 기록 생략
            }

            string controlName = context.control != null ? context.control.displayName : "Unknown"; // 입력을 발생시킨 실제 컨트롤 이름 조회
            lastAction = $"{context.action.name} / {controlName}"; // 최근 수행 액션과 컨트롤 정보 저장

            if (logPerformedActions) // Console 로그 출력 설정 확인
            {
                Debug.Log($"[Input] {GetMapName(mapToTest)} / {lastAction}", this); // 수행된 입력 액션 정보 출력
            }
        }

        private void OnGUI() // 에디터와 개발 빌드에서 입력 검증 창 표시
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Project J Input Debug"); // 이동 가능한 입력 검증 창 표시
#endif
        }

        private void DrawWindow(int windowId) // 입력 검증 창 내부 정보와 현재 값 표시
        {
            GUILayout.Label($"Action Map: {GetMapName(mapToTest)}"); // 현재 검증 중인 액션 맵 이름 표시
            GUILayout.Label($"Last Action: {lastAction}"); // 최근 수행된 액션 정보 표시
            GUILayout.Space(8f); // 기본 정보와 축 값 사이 간격 추가

            if (mapToTest == InputDebugMap.Gameplay) // Gameplay 액션 맵 검증 여부 확인
            {
                DrawVector2Value(ProjectInputNames.Gameplay.Move); // 현재 이동 입력 Vector2 값 표시
                DrawVector2Value(ProjectInputNames.Gameplay.Look); // 현재 시점 입력 Vector2 값 표시
            }
            else // UI 액션 맵 검증인 경우 처리
            {
                DrawVector2Value(ProjectInputNames.UI.Navigate); // 현재 UI 이동 입력 Vector2 값 표시
                DrawVector2Value(ProjectInputNames.UI.Point); // 현재 포인터 위치 Vector2 값 표시
                DrawVector2Value(ProjectInputNames.UI.ScrollWheel); // 현재 스크롤 입력 Vector2 값 표시
            }

            GUILayout.Space(8f); // 축 값과 안내 문구 사이 간격 추가
            GUILayout.Label("Inspector의 Map To Test를 변경한 뒤 Play Mode를 다시 시작합니다."); // 액션 맵 변경 방법 안내 표시
            GUI.DragWindow(); // 입력 검증 창 드래그 이동 허용
        }

        private void DrawVector2Value(string actionName) // 지정한 Vector2 액션의 현재 값 표시
        {
            if (activeMap == null) // 활성 액션 맵 존재 여부 확인
            {
                GUILayout.Label($"{actionName}: Map Disabled"); // 액션 맵 비활성 상태 표시
                return; // 현재 값 읽기 중단
            }

            InputAction inputAction = activeMap.FindAction(actionName, false); // 현재 액션 맵에서 지정 액션 검색

            if (inputAction == null) // 지정 액션 검색 실패 여부 확인
            {
                GUILayout.Label($"{actionName}: Missing"); // 지정 액션 누락 상태 표시
                return; // 현재 값 읽기 중단
            }

            Vector2 value = inputAction.ReadValue<Vector2>(); // 지정 액션의 현재 Vector2 값 읽기
            GUILayout.Label($"{actionName}: ({value.x:0.00}, {value.y:0.00})"); // 지정 액션의 현재 Vector2 값 표시
        }

        private static string GetMapName(InputDebugMap debugMap) // 검증 대상 enum에 대응하는 액션 맵 이름 반환
        {
            return debugMap == InputDebugMap.Gameplay // Gameplay 검증 값 여부 확인
                ? ProjectInputNames.Gameplay.Map // Gameplay 액션 맵 이름 반환
                : ProjectInputNames.UI.Map; // UI 액션 맵 이름 반환
        }
    }
}
