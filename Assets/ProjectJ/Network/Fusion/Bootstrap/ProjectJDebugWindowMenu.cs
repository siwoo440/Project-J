using System; // 문자열 비교와 Type 사용
using System.Collections.Generic; // Debug Window 목록 사용
using System.Reflection; // OnGUI와 내부 표시 상태 탐색
using UnityEngine; // MonoBehaviour와 Runtime 초기화 사용
using UnityEngine.InputSystem; // F1, F2 키 입력 사용
using UnityEngine.SceneManagement; // Scene 전환 감지 사용

namespace ProjectJ.Networking.Fusion // Project J Fusion 네임스페이스
{
#if !UNITY_SERVER // 전용 서버에서는 Debug GUI 제외
    public static class ProjectJDebugWindowMenuInstaller // Debug Hotkey 관리자 자동 설치
    {
        [RuntimeInitializeOnLoadMethod( // Scene 로드 전 자동 실행 지정
            RuntimeInitializeLoadType.BeforeSceneLoad // 최초 Scene 이전 설치 시점
        )]
        private static void Install() // Debug Hotkey 관리자 설치
        {
            ProjectJDebugWindowMenu existing = // 기존 관리자 검색
                UnityEngine.Object.FindFirstObjectByType< // 현재 로드 객체에서 검색
                    ProjectJDebugWindowMenu // 대상 관리자 타입
                >();

            if (existing != null) // 기존 관리자 존재 확인
            {
                return; // 중복 설치 방지
            }

            GameObject menuObject = // 관리자 전용 GameObject 생성
                new GameObject( // 새 GameObject 생성
                    "=== Project J Debug Hotkeys ===" // Hierarchy 표시 이름
                );

            UnityEngine.Object.DontDestroyOnLoad( // Scene 전환 유지 설정
                menuObject // 유지할 관리자 객체
            );

            menuObject.AddComponent< // Hotkey 관리자 Component 추가
                ProjectJDebugWindowMenu // 추가할 관리자 타입
            >();
        }
    }

    [DefaultExecutionOrder(-32000)] // 다른 Debug View보다 먼저 입력 처리
    [DisallowMultipleComponent] // 중복 Component 추가 방지
    public sealed class ProjectJDebugWindowMenu : // F1, F2 전용 Debug Hotkey 관리자
        MonoBehaviour // Unity Component 상속
    {
        private const float RefreshInterval = // Debug Window 검색 주기
            0.25f; // 0.25초 간격

        private const int DirectHotkeyWindowCount = // 직접 선택 가능한 Debug Window 수
            2; // F1과 F2 두 개만 사용

        private readonly List<MonoBehaviour> // 관리 대상 Debug Window 목록
            managedWindows = // 목록 필드 선언
                new List<MonoBehaviour>(); // 빈 목록 생성

        private MonoBehaviour selectedWindow; // 현재 표시 중인 Debug Window

        private float nextRefreshTime; // 다음 Debug Window 검색 시각

        private void Awake() // 관리자 초기화
        {
            DontDestroyOnLoad( // Scene 전환 유지 설정
                gameObject // 현재 관리자 객체
            );

            SceneManager.sceneLoaded += // Scene 로드 이벤트 등록
                OnSceneLoaded; // Scene 로드 처리 함수 연결

            RefreshWindows(); // 시작 시 Debug Window 검색
            ApplyVisibility(); // 시작 시 모든 Debug Window 숨김
        }

        private void Update() // 매 프레임 Hotkey와 목록 갱신
        {
            if ( // 목록 갱신 시각 확인
                Time.unscaledTime >= // 현재 비스케일 시간 비교
                nextRefreshTime // 다음 갱신 시각
            )
            {
                nextRefreshTime = // 다음 갱신 시각 계산
                    Time.unscaledTime + // 현재 비스케일 시간
                    RefreshInterval; // 검색 주기 추가

                RefreshWindows(); // 새로 생성된 Debug Window 검색
                ApplyVisibility(); // 현재 선택 상태 다시 적용
            }

            Keyboard keyboard = // 현재 키보드 조회
                Keyboard.current; // Input System 현재 키보드

            if (keyboard == null) // 키보드 미연결 확인
            {
                return; // 입력 처리 중단
            }

            if ( // F1 입력 확인
                keyboard.f1Key // F1 키 참조
                    .wasPressedThisFrame // 현재 프레임 눌림 확인
            )
            {
                ToggleDebugWindow(0); // 첫 번째 Debug Window 전환
                return; // 같은 프레임 추가 입력 방지
            }

            if ( // F2 입력 확인
                keyboard.f2Key // F2 키 참조
                    .wasPressedThisFrame // 현재 프레임 눌림 확인
            )
            {
                ToggleDebugWindow(1); // 두 번째 Debug Window 전환
            }
        }

        private void OnDestroy() // 관리자 제거 처리
        {
            SceneManager.sceneLoaded -= // Scene 로드 이벤트 해제
                OnSceneLoaded; // 등록 함수 제거

            selectedWindow = // 현재 선택 초기화
                null; // 선택 없음 설정

            ApplyVisibility(); // 관리 중인 Debug Window 전부 숨김
            managedWindows.Clear(); // 관리 목록 비우기
        }

        private void OnSceneLoaded( // Scene 로드 완료 처리
            Scene scene, // 로드된 Scene 정보
            LoadSceneMode mode // Scene 로드 방식
        )
        {
            selectedWindow = // Scene 전환 시 선택 초기화
                null; // Debug Window 표시 해제

            RefreshWindows(); // 새 Scene Debug Window 검색
            ApplyVisibility(); // 모든 Debug Window 기본 숨김
        }

        private void ToggleDebugWindow( // F1, F2 대상 Window 전환
            int index // 관리 목록 인덱스
        )
        {
            RefreshWindows(); // Hotkey 입력 직전 목록 최신화

            if ( // 선택 가능한 인덱스 확인
                index < 0 || // 음수 인덱스 차단
                index >= DirectHotkeyWindowCount || // F1, F2 범위 밖 차단
                index >= managedWindows.Count // 실제 목록 범위 밖 차단
            )
            {
                return; // 유효한 Debug Window가 없으면 종료
            }

            MonoBehaviour targetWindow = // Hotkey에 대응하는 Debug Window 조회
                managedWindows[index]; // 정렬된 목록에서 대상 선택

            selectedWindow = // 현재 표시 대상 변경
                selectedWindow == targetWindow // 같은 Window 재입력 여부 확인
                    ? null // 같은 키 재입력 시 닫기
                    : targetWindow; // 다른 상태면 대상 Window 열기

            ApplyVisibility(); // 선택 결과를 모든 Debug Window에 반영
        }

        private void RefreshWindows() // 현재 Scene의 Debug Window 검색
        {
            MonoBehaviour[] allBehaviours = // 모든 로드 MonoBehaviour 조회
                Resources.FindObjectsOfTypeAll< // 비활성 Component 포함 검색
                    MonoBehaviour // 검색 대상 Component 타입
                >();

            managedWindows.Clear(); // 이전 검색 결과 제거

            for ( // 모든 MonoBehaviour 순회
                int index = 0; // 시작 인덱스
                index < allBehaviours.Length; // 배열 끝까지 반복
                index++ // 다음 인덱스 이동
            )
            {
                MonoBehaviour behaviour = // 현재 Component 조회
                    allBehaviours[index]; // 배열 원소 가져오기

                if ( // Debug Window 여부 확인
                    !IsDebugWindow( // Debug Window 판정 실행
                        behaviour // 현재 Component 전달
                    )
                )
                {
                    continue; // 일반 Component 제외
                }

                managedWindows.Add( // 관리 목록에 Debug Window 추가
                    behaviour // 판정된 Debug Window
                );
            }

            managedWindows.Sort( // 일정한 Hotkey 순서를 위한 정렬
                CompareDebugWindows // 타입 이름 기준 비교 함수
            );

            if ( // 현재 선택 Window 유효성 확인
                selectedWindow != null && // 기존 선택이 존재하고
                !managedWindows.Contains( // 현재 목록에 존재하지 않는지 확인
                    selectedWindow // 기존 선택 대상
                )
            )
            {
                selectedWindow = // 사라진 Window 선택 해제
                    null; // 선택 없음 설정
            }
        }

        private void ApplyVisibility() // Debug Window 표시 상태 적용
        {
            for ( // 관리 중인 모든 Window 순회
                int index = 0; // 시작 인덱스
                index < managedWindows.Count; // 목록 끝까지 반복
                index++ // 다음 인덱스 이동
            )
            {
                MonoBehaviour window = // 현재 Debug Window 조회
                    managedWindows[index]; // 목록 원소 가져오기

                if (window == null) // 삭제된 Component 확인
                {
                    continue; // 삭제된 항목 건너뛰기
                }

                bool shouldShow = // 현재 Window 표시 여부 계산
                    window == selectedWindow; // 선택된 Window만 표시

                SetInternalVisibilityState( // 기존 Debug View 내부 표시 변수 동기화
                    window, // 대상 Debug Window
                    shouldShow // 적용할 표시 상태
                );

                window.enabled = // Component 활성 상태 변경
                    shouldShow; // 선택된 Window만 활성화
            }
        }

        private static void SetInternalVisibilityState( // 기존 visible 필드 동기화
            MonoBehaviour window, // 대상 Debug Window
            bool shouldShow // 적용할 표시 상태
        )
        {
            Type type = // 대상 실제 타입 조회
                window.GetType(); // Runtime 타입 가져오기

            TrySetBooleanField( // visible 필드 변경 시도
                type, // 대상 타입
                window, // 대상 객체
                "visible", // 일반 visible 필드 이름
                shouldShow // 적용할 표시 상태
            );

            TrySetBooleanField( // isVisible 필드 변경 시도
                type, // 대상 타입
                window, // 대상 객체
                "isVisible", // isVisible 필드 이름
                shouldShow // 적용할 표시 상태
            );
        }

        private static void TrySetBooleanField( // bool 표시 필드 안전 변경
            Type type, // 대상 타입
            MonoBehaviour window, // 대상 객체
            string fieldName, // 찾을 필드 이름
            bool value // 적용할 bool 값
        )
        {
            FieldInfo field = // 표시 상태 필드 검색
                type.GetField( // Runtime 필드 조회
                    fieldName, // 찾을 필드 이름
                    BindingFlags.Instance | // 인스턴스 필드 포함
                    BindingFlags.Public | // public 필드 포함
                    BindingFlags.NonPublic // private 필드 포함
                );

            if ( // 변경 가능한 bool 필드인지 확인
                field == null || // 필드 미존재 확인
                field.FieldType != typeof(bool) // bool 타입 여부 확인
            )
            {
                return; // 대상 필드가 아니면 종료
            }

            field.SetValue( // 내부 표시 상태 값 적용
                window, // 대상 객체
                value // 새 표시 값
            );
        }

        private bool IsDebugWindow( // Debug GUI Component 판정
            MonoBehaviour behaviour // 검사할 Component
        )
        {
            if ( // 기본 제외 조건 확인
                behaviour == null || // null Component 제외
                behaviour == this // 관리자 자기 자신 제외
            )
            {
                return false; // Debug Window 아님
            }

            GameObject targetObject = // Component 소속 GameObject 조회
                behaviour.gameObject; // 대상 GameObject 가져오기

            if ( // 유효한 로드 Scene 소속인지 확인
                targetObject == null || // GameObject 누락 확인
                !targetObject.scene.IsValid() || // 유효하지 않은 Scene 제외
                !targetObject.scene.isLoaded // 로드되지 않은 Scene 제외
            )
            {
                return false; // 관리 대상 제외
            }

            Type type = // Component 실제 타입 조회
                behaviour.GetType(); // Runtime 타입 가져오기

            string targetNamespace = // 타입 네임스페이스 조회
                type.Namespace ?? // null 여부 확인
                string.Empty; // 네임스페이스 없으면 빈 문자열

            if ( // Project J 타입인지 확인
                !targetNamespace.StartsWith( // 네임스페이스 접두사 비교
                    "ProjectJ", // Project J 접두사
                    StringComparison.Ordinal // 정확한 서수 비교
                )
            )
            {
                return false; // 외부 Debug GUI 제외
            }

            MethodInfo onGuiMethod = // OnGUI 함수 존재 여부 조회
                type.GetMethod( // Runtime 메서드 검색
                    "OnGUI", // Debug GUI 함수 이름
                    BindingFlags.Instance | // 인스턴스 메서드 포함
                    BindingFlags.Public | // public 메서드 포함
                    BindingFlags.NonPublic // private 메서드 포함
                );

            if (onGuiMethod == null) // OnGUI 미존재 확인
            {
                return false; // GUI 없는 Component 제외
            }

            string typeName = // 타입 이름 조회
                type.Name; // 클래스 이름 저장

            return // Debug 관련 이름 판정 결과 반환
                ContainsIgnoreCase( // Debug 문자열 확인
                    typeName, // 타입 이름
                    "Debug" // Debug 키워드
                ) ||
                ContainsIgnoreCase( // Invite 문자열 확인
                    typeName, // 타입 이름
                    "Invite" // Invite 키워드
                ) ||
                ContainsIgnoreCase( // Diagnostic 문자열 확인
                    typeName, // 타입 이름
                    "Diagnostic" // Diagnostic 키워드
                ) ||
                ContainsIgnoreCase( // Gate 문자열 확인
                    typeName, // 타입 이름
                    "Gate" // Gate 키워드
                ) ||
                ContainsIgnoreCase( // Overlay 문자열 확인
                    typeName, // 타입 이름
                    "Overlay" // Overlay 키워드
                );
        }

        private static bool ContainsIgnoreCase( // 대소문자 무시 문자열 포함 확인
            string source, // 원본 문자열
            string value // 찾을 문자열
        )
        {
            return // 포함 여부 반환
                source.IndexOf( // 문자열 위치 검색
                    value, // 찾을 값
                    StringComparison.OrdinalIgnoreCase // 대소문자 무시 서수 비교
                ) >= // 검색 결과 비교
                0; // 0 이상이면 포함
        }

        private static int CompareDebugWindows( // Debug Window 정렬 비교
            MonoBehaviour left, // 왼쪽 비교 대상
            MonoBehaviour right // 오른쪽 비교 대상
        )
        {
            if (left == null) // 왼쪽 null 확인
            {
                return right == null // 양쪽 null 여부 확인
                    ? 0 // 둘 다 null이면 동일
                    : 1; // 왼쪽만 null이면 뒤로 정렬
            }

            if (right == null) // 오른쪽 null 확인
            {
                return -1; // 오른쪽 null이면 왼쪽을 앞으로 정렬
            }

            string leftKey = // 왼쪽 정렬 키 생성
                left.GetType().FullName + // 타입 전체 이름 사용
                "/" + // 타입과 객체 이름 구분자
                left.gameObject.name; // GameObject 이름 추가

            string rightKey = // 오른쪽 정렬 키 생성
                right.GetType().FullName + // 타입 전체 이름 사용
                "/" + // 타입과 객체 이름 구분자
                right.gameObject.name; // GameObject 이름 추가

            return string.Compare( // 두 정렬 키 비교
                leftKey, // 왼쪽 키
                rightKey, // 오른쪽 키
                StringComparison.Ordinal // 고정된 서수 정렬
            );
        }
    }
#endif // 전용 서버 제외 영역 종료
}
