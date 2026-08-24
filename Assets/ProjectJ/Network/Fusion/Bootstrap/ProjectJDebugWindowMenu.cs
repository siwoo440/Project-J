using System; // 타입과 문자열 비교 기능
using System.Collections.Generic; // 진단창 목록 기능
using System.Reflection; // OnGUI와 표시 필드 호출 기능
using System.Text; // 표시 이름 조합 기능
using ProjectJ.Debugging; // 통합 패널 공통 정책
using UnityEngine; // Unity Component와 IMGUI 기능
using UnityEngine.InputSystem; // F1 입력 기능
using UnityEngine.SceneManagement; // Scene 전환 감지 기능

namespace ProjectJ.Networking.Fusion // Project J Fusion 네임스페이스
{
#if !UNITY_SERVER // 전용 서버의 진단 UI 제외
    public static class ProjectJDebugWindowMenuInstaller // 통합 디버그 패널 자동 설치
    {
        [RuntimeInitializeOnLoadMethod( // Runtime 자동 실행 지정
            RuntimeInitializeLoadType.BeforeSceneLoad // 최초 Scene 이전 설치
        )]
        private static void Install() // 통합 패널 설치
        {
            ProjectJDebugWindowMenu existing = // 기존 패널 검색
                UnityEngine.Object.FindFirstObjectByType<ProjectJDebugWindowMenu>(); // 로드 객체 검색

            if (existing != null) // 기존 패널 존재 확인
            {
                return; // 중복 설치 방지
            }

            GameObject menuObject = // 패널 전용 객체 생성
                new GameObject("=== Project J Unified Debug Panel ==="); // 새 GameObject 생성

            UnityEngine.Object.DontDestroyOnLoad( // Scene 전환 유지 설정
                menuObject // 유지할 패널 객체
            );

            menuObject.AddComponent<ProjectJDebugWindowMenu>(); // 통합 패널 Component 추가
        }
    }

    [DefaultExecutionOrder(-32000)] // 다른 진단창보다 먼저 입력 처리
    [DisallowMultipleComponent] // 중복 Component 방지
    public sealed class ProjectJDebugWindowMenu : MonoBehaviour // F1 통합 디버그 패널
    {
        private const float RefreshInterval = 0.25f; // 진단창 검색 주기
        private const int PanelWindowId = 10500; // IMGUI Window 고유 번호
        private const float OuterMargin = 16f; // 화면 가장자리 여백
        private const float NavigationWidth = 220f; // 좌측 창 목록 너비
        private const float HeaderHeight = 66f; // 상단 탭 영역 높이
        private const float MinimumPanelWidth = 640f; // 패널 권장 최소 너비
        private const float MinimumPanelHeight = 360f; // 패널 권장 최소 높이
        private const float MaximumPanelWidth = 1440f; // 패널 최대 너비
        private const float MaximumPanelHeight = 1000f; // 패널 최대 높이

        private static readonly ProjectJDebugPanelCategory[] Categories = // 탭 표시 순서
        {
            ProjectJDebugPanelCategory.Overview, // 개요 탭
            ProjectJDebugPanelCategory.Network, // 네트워크 탭
            ProjectJDebugPanelCategory.Player, // 플레이어 탭
            ProjectJDebugPanelCategory.Session, // 세션 탭
            ProjectJDebugPanelCategory.Gameplay // 게임 상태 탭
        };

        private readonly List<DebugWindowEntry> managedWindows = // 관리 대상 진단창 목록
            new List<DebugWindowEntry>(); // 빈 목록 생성

        private ProjectJDebugPanelCategory selectedCategory = // 현재 선택 탭
            ProjectJDebugPanelCategory.Overview; // 개요 탭 기본 선택

        private DebugWindowEntry selectedWindow; // 현재 선택 진단창
        private bool panelVisible = ProjectJUnifiedDebugPanelPolicy.DefaultVisibility; // 통합 패널 표시 상태
        private float nextRefreshTime; // 다음 진단창 검색 시각
        private Vector2 navigationScroll; // 좌측 목록 스크롤 위치
        private Vector2 contentScroll; // 우측 내용 스크롤 위치
        private Rect panelRect; // 현재 패널 영역
        private string lastDrawError = string.Empty; // 최근 진단창 출력 오류
        private GUIStyle titleStyle; // 상단 제목 스타일
        private GUIStyle tabStyle; // 탭 버튼 스타일
        private GUIStyle selectedTabStyle; // 선택 탭 버튼 스타일
        private GUIStyle windowButtonStyle; // 진단창 선택 버튼 스타일
        private GUIStyle selectedWindowButtonStyle; // 선택 진단창 버튼 스타일
        private GUIStyle helpStyle; // 안내 문구 스타일

        private void Awake() // 통합 패널 초기화
        {
            DontDestroyOnLoad( // Scene 전환 유지 설정
                gameObject // 현재 패널 객체
            );

            SceneManager.sceneLoaded += OnSceneLoaded; // Scene 로드 이벤트 등록
            ResetPanelState(); // 기본 숨김 상태 적용
            RefreshWindows(); // 시작 진단창 검색
            HideStandaloneWindows(); // 개별 진단창 숨김
        }

        private void Update() // F1 입력과 목록 갱신
        {
            if (Time.unscaledTime >= nextRefreshTime) // 목록 갱신 시각 확인
            {
                nextRefreshTime = Time.unscaledTime + RefreshInterval; // 다음 갱신 시각 계산
                RefreshWindows(); // 새 진단창 검색
            }

            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null || !keyboard.f1Key.wasPressedThisFrame) // F1 입력 여부 확인
            {
                return; // 표시 전환 생략
            }

            panelVisible = ProjectJUnifiedDebugPanelPolicy.ToggleVisibility( // 패널 표시 상태 전환
                panelVisible // 현재 표시 상태 전달
            );

            if (panelVisible) // 패널 열림 확인
            {
                EnsureSelectedWindow(); // 현재 탭의 첫 진단창 선택
            }
            else // 패널 닫힘 처리
            {
                lastDrawError = string.Empty; // 출력 오류 초기화
            }
        }

        private void LateUpdate() // 개별 창의 단축키 처리 후 숨김
        {
            HideStandaloneWindows(); // 독립 출력 상태 강제 해제
        }

        private void OnGUI() // 통합 패널 출력
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD // 개발 환경에서만 패널 출력
            if (!panelVisible) // 패널 닫힘 확인
            {
                return; // GUI 출력 생략
            }

            EnsureStyles(); // 통합 패널 스타일 준비
            UpdatePanelRect(); // 현재 해상도 패널 영역 계산

            panelRect = GUI.Window( // 통합 패널 Window 출력
                PanelWindowId, // Window 고유 번호
                panelRect, // 출력 영역
                DrawPanelWindow, // 내부 내용 출력 함수
                string.Empty // 기본 제목 미사용
            );
#endif // 개발 환경 출력 종료
        }

        private void OnDestroy() // 통합 패널 제거 처리
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Scene 로드 이벤트 해제
            ResetPanelState(); // 표시 상태 초기화
            HideStandaloneWindows(); // 남은 진단창 숨김
            managedWindows.Clear(); // 관리 목록 비우기
        }

        private void OnSceneLoaded( // Scene 로드 완료 처리
            Scene scene, // 로드된 Scene 정보
            LoadSceneMode mode // Scene 로드 방식
        )
        {
            ResetPanelState(); // Scene 전환 시 패널 숨김
            RefreshWindows(); // 새 Scene 진단창 검색
            HideStandaloneWindows(); // 새 진단창 기본 숨김
        }

        private void ResetPanelState() // 패널 상태 초기화
        {
            panelVisible = ProjectJUnifiedDebugPanelPolicy.DefaultVisibility; // 기본 숨김 적용
            selectedCategory = ProjectJDebugPanelCategory.Overview; // 개요 탭 선택
            selectedWindow = null; // 선택 없음 설정
            navigationScroll = Vector2.zero; // 목록 스크롤 초기화
            contentScroll = Vector2.zero; // 내용 스크롤 초기화
            lastDrawError = string.Empty; // 출력 오류 초기화
            ProjectJDebugOverlayController.SetVisible(false); // 레거시 Overlay 숨김
        }

        private void DrawPanelWindow( // 통합 패널 내부 출력
            int windowId // IMGUI Window 번호
        )
        {
            GUI.Label( // 패널 제목 출력
                new Rect(16f, 8f, 360f, 28f), // 제목 영역
                "PROJECT J - 통합 디버그 패널", // 제목 문구
                titleStyle // 제목 스타일
            );

            GUI.Label( // 단축키 안내 출력
                new Rect(panelRect.width - 260f, 10f, 244f, 24f), // 안내 영역
                "F1 닫기  ·  ALT 커서", // 단축키 안내 문구
                helpStyle // 안내 스타일
            );

            DrawCategoryTabs(); // 상단 탭 출력
            DrawWindowNavigation(); // 좌측 진단창 목록 출력
            DrawSelectedWindow(); // 우측 선택 내용 출력
        }

        private void DrawCategoryTabs() // 상단 카테고리 탭 출력
        {
            float tabStartX = 16f; // 탭 시작 위치
            float tabY = 36f; // 탭 위쪽 위치
            float availableWidth = panelRect.width - 32f; // 탭 사용 가능 너비
            float tabWidth = availableWidth / Categories.Length; // 개별 탭 너비

            for (int index = 0; index < Categories.Length; index++) // 모든 탭 순회
            {
                ProjectJDebugPanelCategory category = Categories[index]; // 현재 탭 분류 조회
                GUIStyle style = category == selectedCategory ? selectedTabStyle : tabStyle; // 탭 스타일 선택

                if (GUI.Button( // 탭 버튼 출력과 입력 확인
                    new Rect(tabStartX + tabWidth * index, tabY, tabWidth - 4f, 28f), // 탭 영역
                    ProjectJUnifiedDebugPanelPolicy.GetCategoryLabel(category), // 한글 탭 이름
                    style // 선택된 버튼 스타일
                ))
                {
                    SelectCategory(category); // 선택 탭 변경
                }
            }
        }

        private void DrawWindowNavigation() // 좌측 진단창 목록 출력
        {
            Rect navigationRect = new Rect( // 목록 표시 영역
                12f, // 왼쪽 위치
                HeaderHeight + 10f, // 탭 아래 위치
                NavigationWidth, // 목록 너비
                panelRect.height - HeaderHeight - 22f // 목록 높이
            );

            GUI.Box(navigationRect, string.Empty); // 목록 배경 출력

            int matchingCount = CountWindowsInCategory(selectedCategory); // 현재 탭 진단창 개수
            Rect viewRect = new Rect( // 목록 스크롤 내부 영역
                0f, // 내부 왼쪽 위치
                0f, // 내부 위쪽 위치
                NavigationWidth - 28f, // 스크롤바 여백 제외 너비
                Mathf.Max(navigationRect.height - 16f, matchingCount * 38f + 12f) // 항목 전체 높이
            );

            navigationScroll = GUI.BeginScrollView( // 목록 스크롤 시작
                navigationRect, // 화면 표시 영역
                navigationScroll, // 현재 스크롤 위치
                viewRect // 전체 내부 영역
            );

            float buttonY = 8f; // 첫 버튼 위쪽 위치

            for (int index = 0; index < managedWindows.Count; index++) // 모든 진단창 순회
            {
                DebugWindowEntry entry = managedWindows[index]; // 현재 진단창 조회

                if (entry.Category != selectedCategory) // 현재 탭 소속 확인
                {
                    continue; // 다른 탭 항목 제외
                }

                GUIStyle style = entry == selectedWindow // 진단창 버튼 스타일 선택
                    ? selectedWindowButtonStyle // 선택 스타일 사용
                    : windowButtonStyle; // 일반 스타일 사용

                if (GUI.Button( // 진단창 선택 버튼 출력
                    new Rect(8f, buttonY, viewRect.width - 12f, 30f), // 버튼 영역
                    entry.DisplayName, // 정리된 진단창 이름
                    style // 선택된 버튼 스타일
                ))
                {
                    selectedWindow = entry; // 선택 진단창 변경
                    contentScroll = Vector2.zero; // 내용 스크롤 초기화
                    lastDrawError = string.Empty; // 이전 출력 오류 초기화
                }

                buttonY += 38f; // 다음 버튼 위치 이동
            }

            GUI.EndScrollView(); // 목록 스크롤 종료
        }

        private void DrawSelectedWindow() // 선택 진단창 내용 출력
        {
            Rect contentRect = new Rect( // 우측 내용 표시 영역
                NavigationWidth + 22f, // 목록 오른쪽 위치
                HeaderHeight + 10f, // 탭 아래 위치
                panelRect.width - NavigationWidth - 34f, // 내용 너비
                panelRect.height - HeaderHeight - 22f // 내용 높이
            );

            GUI.Box(contentRect, string.Empty); // 내용 배경 출력
            EnsureSelectedWindow(); // 현재 탭 선택 항목 보장

            if (selectedWindow == null) // 표시할 진단창 없음 확인
            {
                GUI.Label( // 빈 탭 안내 출력
                    new Rect(contentRect.x + 18f, contentRect.y + 18f, contentRect.width - 36f, 40f), // 안내 영역
                    "현재 Scene에서 사용할 수 있는 진단창이 없습니다.", // 빈 탭 안내 문구
                    helpStyle // 안내 스타일
                );

                return; // 진단창 출력 생략
            }

            Rect scrollRect = new Rect( // 내용 스크롤 표시 영역
                contentRect.x + 8f, // 배경 내부 왼쪽 위치
                contentRect.y + 8f, // 배경 내부 위쪽 위치
                contentRect.width - 16f, // 내부 너비
                contentRect.height - 16f // 내부 높이
            );

            Rect viewRect = new Rect( // 레거시 진단창 가상 화면
                0f, // 가상 왼쪽 위치
                0f, // 가상 위쪽 위치
                Mathf.Max(Screen.width, 1280f), // 기존 고정 좌표 수용 너비
                Mathf.Max(Screen.height, 1080f) // 기존 고정 좌표 수용 높이
            );

            contentScroll = GUI.BeginScrollView( // 내용 스크롤 시작
                scrollRect, // 화면 표시 영역
                contentScroll, // 현재 스크롤 위치
                viewRect // 전체 가상 영역
            );

            InvokeSelectedWindow(); // 선택 진단창 OnGUI 호출
            GUI.EndScrollView(); // 내용 스크롤 종료

            if (!string.IsNullOrEmpty(lastDrawError)) // 출력 오류 존재 확인
            {
                GUI.Label( // 오류 문구 출력
                    new Rect(contentRect.x + 18f, contentRect.y + 18f, contentRect.width - 36f, 80f), // 오류 영역
                    lastDrawError, // 최근 오류 문구
                    helpStyle // 안내 스타일
                );
            }
        }

        private void InvokeSelectedWindow() // 선택 진단창 수동 출력
        {
            if (selectedWindow == null || selectedWindow.Behaviour == null || selectedWindow.OnGuiMethod == null) // 선택 항목 유효성 확인
            {
                return; // 출력 호출 생략
            }

            bool previousGuiEnabled = GUI.enabled; // 기존 GUI 입력 상태 저장
            Color previousGuiColor = GUI.color; // 기존 GUI 색상 저장
            ProjectJDebugOverlayController.SetVisible(true); // 레거시 Overlay 임시 표시
            SetInternalVisibilityState(selectedWindow, true); // 선택 진단창 임시 표시

            try // 수동 OnGUI 안전 호출
            {
                selectedWindow.OnGuiMethod.Invoke( // 레거시 OnGUI 호출
                    selectedWindow.Behaviour, // 대상 Component 전달
                    null // 매개변수 없음 전달
                );

                lastDrawError = string.Empty; // 이전 오류 초기화
            }
            catch (Exception exception) // 진단창 출력 예외 처리
            {
                lastDrawError = selectedWindow.DisplayName + " 출력 실패: " + GetBaseExceptionMessage(exception); // 오류 문구 생성
            }
            finally // 임시 GUI 상태 복구
            {
                SetInternalVisibilityState(selectedWindow, false); // 선택 진단창 다시 숨김
                ProjectJDebugOverlayController.SetVisible(false); // 레거시 Overlay 다시 숨김
                GUI.enabled = previousGuiEnabled; // 기존 GUI 입력 상태 복구
                GUI.color = previousGuiColor; // 기존 GUI 색상 복구
            }
        }

        private void SelectCategory( // 탭 선택 처리
            ProjectJDebugPanelCategory category // 새 탭 분류
        )
        {
            if (selectedCategory == category) // 같은 탭 재선택 확인
            {
                return; // 불필요한 초기화 방지
            }

            selectedCategory = category; // 현재 탭 변경
            selectedWindow = null; // 기존 진단창 선택 해제
            navigationScroll = Vector2.zero; // 목록 스크롤 초기화
            contentScroll = Vector2.zero; // 내용 스크롤 초기화
            lastDrawError = string.Empty; // 출력 오류 초기화
            EnsureSelectedWindow(); // 새 탭 첫 진단창 선택
        }

        private void EnsureSelectedWindow() // 현재 탭 선택 항목 보장
        {
            if (selectedWindow != null && selectedWindow.Behaviour != null && selectedWindow.Category == selectedCategory) // 기존 선택 유효성 확인
            {
                return; // 기존 선택 유지
            }

            selectedWindow = null; // 선택 상태 초기화

            for (int index = 0; index < managedWindows.Count; index++) // 모든 진단창 순회
            {
                DebugWindowEntry entry = managedWindows[index]; // 현재 진단창 조회

                if (entry.Category != selectedCategory) // 현재 탭 소속 확인
                {
                    continue; // 다른 탭 항목 제외
                }

                selectedWindow = entry; // 첫 진단창 자동 선택
                return; // 추가 검색 종료
            }
        }

        private int CountWindowsInCategory( // 탭별 진단창 개수 계산
            ProjectJDebugPanelCategory category // 계산할 탭 분류
        )
        {
            int count = 0; // 진단창 개수 초기화

            for (int index = 0; index < managedWindows.Count; index++) // 모든 진단창 순회
            {
                if (managedWindows[index].Category == category) // 현재 탭 소속 확인
                {
                    count++; // 진단창 개수 증가
                }
            }

            return count; // 계산된 개수 반환
        }

        private void RefreshWindows() // 현재 Scene 진단창 검색
        {
            MonoBehaviour[] allBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>(); // 모든 MonoBehaviour 조회
            MonoBehaviour previouslySelected = selectedWindow != null ? selectedWindow.Behaviour : null; // 기존 선택 Component 저장
            managedWindows.Clear(); // 이전 검색 결과 제거

            for (int index = 0; index < allBehaviours.Length; index++) // 모든 Component 순회
            {
                MonoBehaviour behaviour = allBehaviours[index]; // 현재 Component 조회

                if (!TryCreateEntry(behaviour, out DebugWindowEntry entry)) // 진단창 항목 생성 시도
                {
                    continue; // 일반 Component 제외
                }

                managedWindows.Add(entry); // 관리 목록에 추가
            }

            managedWindows.Sort(CompareEntries); // 탭과 이름 기준 정렬
            selectedWindow = null; // 기존 선택 연결 초기화

            for (int index = 0; index < managedWindows.Count; index++) // 새 목록 순회
            {
                DebugWindowEntry entry = managedWindows[index]; // 현재 항목 조회

                if (entry.Behaviour != previouslySelected) // 기존 Component 일치 확인
                {
                    continue; // 다른 Component 건너뛰기
                }

                selectedWindow = entry; // 기존 선택 항목 복구
                break; // 검색 종료
            }

            EnsureSelectedWindow(); // 유효한 선택 항목 보장
        }

        private bool TryCreateEntry( // Component의 진단창 항목 생성
            MonoBehaviour behaviour, // 검사할 Component
            out DebugWindowEntry entry // 생성된 항목
        )
        {
            entry = null; // 생성 실패 기본값

            if (behaviour == null || behaviour == this) // 기본 제외 조건 확인
            {
                return false; // 진단창 아님 반환
            }

            GameObject targetObject = behaviour.gameObject; // Component 소속 객체 조회

            if (targetObject == null || !targetObject.scene.IsValid() || !targetObject.scene.isLoaded) // 로드 Scene 소속 여부 확인
            {
                return false; // 관리 대상 제외
            }

            Type type = behaviour.GetType(); // Component 실제 타입 조회
            string targetNamespace = type.Namespace ?? string.Empty; // 타입 네임스페이스 조회

            if (!targetNamespace.StartsWith("ProjectJ", StringComparison.Ordinal)) // Project J 타입 여부 확인
            {
                return false; // 외부 Component 제외
            }

            MethodInfo onGuiMethod = type.GetMethod( // OnGUI 함수 검색
                "OnGUI", // 찾을 함수 이름
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic // 인스턴스 접근 범위
            );

            if (onGuiMethod == null || !HasDebugName(type.Name)) // 진단용 OnGUI 여부 확인
            {
                return false; // 일반 게임 UI 제외
            }

            FieldInfo visibilityField = FindVisibilityField(type); // 내부 표시 필드 검색

            entry = new DebugWindowEntry( // 새 진단창 항목 생성
                behaviour, // 대상 Component
                onGuiMethod, // OnGUI 함수
                visibilityField, // 내부 표시 필드
                ProjectJUnifiedDebugPanelPolicy.GetCategory(type.Name), // 탭 분류
                BuildDisplayName(type.Name) // 화면 표시 이름
            );

            return true; // 항목 생성 성공 반환
        }

        private void HideStandaloneWindows() // 모든 독립 진단창 숨김
        {
            ProjectJDebugOverlayController.SetVisible(false); // 공통 Overlay 숨김

            for (int index = 0; index < managedWindows.Count; index++) // 모든 진단창 순회
            {
                DebugWindowEntry entry = managedWindows[index]; // 현재 항목 조회

                if (entry.Behaviour == null) // 삭제된 Component 확인
                {
                    continue; // 다음 항목 처리
                }

                SetInternalVisibilityState(entry, false); // 내부 표시 상태 숨김
            }
        }

        private static void SetInternalVisibilityState( // 내부 표시 필드 변경
            DebugWindowEntry entry, // 대상 진단창 항목
            bool shouldShow // 적용할 표시 상태
        )
        {
            if (entry == null || entry.Behaviour == null || entry.VisibilityField == null) // 필드 변경 가능 여부 확인
            {
                return; // 필드 변경 생략
            }

            entry.VisibilityField.SetValue( // bool 필드 값 적용
                entry.Behaviour, // 대상 Component
                shouldShow // 새 표시 상태
            );
        }

        private static FieldInfo FindVisibilityField( // 내부 표시 필드 검색
            Type type // 검사할 Component 타입
        )
        {
            FieldInfo visibleField = FindBooleanField(type, "visible"); // visible 필드 검색

            if (visibleField != null) // visible 필드 존재 확인
            {
                return visibleField; // 찾은 필드 반환
            }

            return FindBooleanField(type, "isVisible"); // isVisible 필드 검색 결과 반환
        }

        private static FieldInfo FindBooleanField( // 이름으로 bool 필드 검색
            Type type, // 검사할 Component 타입
            string fieldName // 찾을 필드 이름
        )
        {
            FieldInfo field = type.GetField( // Runtime 필드 검색
                fieldName, // 찾을 필드 이름
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic // 인스턴스 접근 범위
            );

            if (field == null || field.FieldType != typeof(bool)) // 유효한 bool 필드 여부 확인
            {
                return null; // 표시 필드 없음 반환
            }

            return field; // 찾은 bool 필드 반환
        }

        private static bool HasDebugName( // 진단창 타입 이름 판정
            string typeName // 검사할 타입 이름
        )
        {
            return ContainsIgnoreCase(typeName, "Debug") || // Debug 이름 포함 확인
                ContainsIgnoreCase(typeName, "Invite") || // Invite 이름 포함 확인
                ContainsIgnoreCase(typeName, "Diagnostic") || // Diagnostic 이름 포함 확인
                ContainsIgnoreCase(typeName, "Gate") || // Gate 이름 포함 확인
                ContainsIgnoreCase(typeName, "Overlay") || // Overlay 이름 포함 확인
                ProjectJUnifiedDebugPanelPolicy.IsKnownDiagnosticWindow(typeName); // 기능 Component 진단창 포함 확인
        }

        private static bool ContainsIgnoreCase( // 대소문자 무시 포함 검사
            string source, // 원본 문자열
            string value // 찾을 문자열
        )
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0; // 포함 여부 반환
        }

        private static string BuildDisplayName( // 타입 이름을 화면 이름으로 변환
            string typeName // 원본 타입 이름
        )
        {
            string cleanedName = typeName // 불필요 접두사와 접미사 제거
                .Replace("ProjectJ", string.Empty) // 프로젝트 접두사 제거
                .Replace("DebugView", string.Empty) // DebugView 접미사 제거
                .Replace("Debug", string.Empty); // Debug 단어 제거

            StringBuilder builder = new StringBuilder(); // 읽기 쉬운 이름 조합기

            for (int index = 0; index < cleanedName.Length; index++) // 모든 문자 순회
            {
                char current = cleanedName[index]; // 현재 문자 조회

                if (index > 0 && char.IsUpper(current) && !char.IsUpper(cleanedName[index - 1])) // 단어 구분 공백 확인
                {
                    builder.Append(' '); // 단어 사이 공백 추가
                }

                builder.Append(current); // 현재 문자 추가
            }

            return builder.Length > 0 ? builder.ToString() : typeName; // 정리된 화면 이름 반환
        }

        private static string GetBaseExceptionMessage( // 실제 예외 문구 추출
            Exception exception // 발생 예외
        )
        {
            Exception current = exception; // 현재 예외 초기화

            while (current.InnerException != null) // 내부 예외 존재 확인
            {
                current = current.InnerException; // 실제 원인 예외로 이동
            }

            return current.Message; // 최종 예외 문구 반환
        }

        private static int CompareEntries( // 진단창 정렬 비교
            DebugWindowEntry left, // 왼쪽 항목
            DebugWindowEntry right // 오른쪽 항목
        )
        {
            int categoryComparison = left.Category.CompareTo(right.Category); // 탭 순서 비교

            if (categoryComparison != 0) // 서로 다른 탭 확인
            {
                return categoryComparison; // 탭 순서 결과 반환
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal); // 화면 이름 정렬
        }

        private void UpdatePanelRect() // 현재 해상도 패널 영역 계산
        {
            float availableWidth = Mathf.Max(320f, Screen.width - OuterMargin * 2f); // 화면 사용 가능 너비
            float availableHeight = Mathf.Max(240f, Screen.height - OuterMargin * 2f); // 화면 사용 가능 높이
            float width = Mathf.Min(availableWidth, MaximumPanelWidth); // 화면을 넘지 않는 패널 너비
            float height = Mathf.Min(availableHeight, MaximumPanelHeight); // 화면을 넘지 않는 패널 높이

            if (availableWidth >= MinimumPanelWidth) // 권장 너비 적용 가능 여부 확인
            {
                width = Mathf.Max(width, MinimumPanelWidth); // 권장 최소 너비 적용
            }

            if (availableHeight >= MinimumPanelHeight) // 권장 높이 적용 가능 여부 확인
            {
                height = Mathf.Max(height, MinimumPanelHeight); // 권장 최소 높이 적용
            }

            panelRect = new Rect( // 화면 중앙 패널 영역 생성
                (Screen.width - width) * 0.5f, // 가로 중앙 위치
                (Screen.height - height) * 0.5f, // 세로 중앙 위치
                width, // 계산된 너비
                height // 계산된 높이
            );
        }

        private void EnsureStyles() // 통합 패널 GUI 스타일 준비
        {
            if (titleStyle != null) // 기존 스타일 생성 확인
            {
                return; // 중복 생성 방지
            }

            titleStyle = new GUIStyle(GUI.skin.label) // 제목 스타일 생성
            {
                fontSize = 18, // 제목 글자 크기
                fontStyle = FontStyle.Bold, // 제목 굵게 표시
                alignment = TextAnchor.MiddleLeft // 제목 왼쪽 정렬
            };

            tabStyle = new GUIStyle(GUI.skin.button) // 일반 탭 스타일 생성
            {
                fontSize = 13, // 탭 글자 크기
                alignment = TextAnchor.MiddleCenter // 탭 가운데 정렬
            };

            selectedTabStyle = new GUIStyle(tabStyle) // 선택 탭 스타일 생성
            {
                fontStyle = FontStyle.Bold // 선택 탭 굵게 표시
            };

            windowButtonStyle = new GUIStyle(GUI.skin.button) // 일반 진단창 버튼 스타일 생성
            {
                alignment = TextAnchor.MiddleLeft, // 버튼 왼쪽 정렬
                fontSize = 12 // 버튼 글자 크기
            };

            selectedWindowButtonStyle = new GUIStyle(windowButtonStyle) // 선택 진단창 버튼 스타일 생성
            {
                fontStyle = FontStyle.Bold // 선택 버튼 굵게 표시
            };

            helpStyle = new GUIStyle(GUI.skin.label) // 안내 문구 스타일 생성
            {
                fontSize = 12, // 안내 글자 크기
                wordWrap = true, // 긴 문구 자동 줄바꿈
                alignment = TextAnchor.MiddleLeft // 안내 왼쪽 정렬
            };
        }

        private sealed class DebugWindowEntry // 관리 대상 진단창 정보
        {
            public DebugWindowEntry( // 진단창 정보 생성
                MonoBehaviour behaviour, // 대상 Component
                MethodInfo onGuiMethod, // OnGUI 함수
                FieldInfo visibilityField, // 내부 표시 필드
                ProjectJDebugPanelCategory category, // 소속 탭
                string displayName // 화면 표시 이름
            )
            {
                Behaviour = behaviour; // 대상 Component 저장
                OnGuiMethod = onGuiMethod; // OnGUI 함수 저장
                VisibilityField = visibilityField; // 표시 필드 저장
                Category = category; // 소속 탭 저장
                DisplayName = displayName; // 표시 이름 저장
            }

            public MonoBehaviour Behaviour // 대상 Component 속성
            {
                get; // 외부 읽기 허용
            }

            public MethodInfo OnGuiMethod // OnGUI 함수 속성
            {
                get; // 외부 읽기 허용
            }

            public FieldInfo VisibilityField // 표시 필드 속성
            {
                get; // 외부 읽기 허용
            }

            public ProjectJDebugPanelCategory Category // 소속 탭 속성
            {
                get; // 외부 읽기 허용
            }

            public string DisplayName // 화면 표시 이름 속성
            {
                get; // 외부 읽기 허용
            }
        }
    }
#endif // 전용 서버 제외 종료
}
