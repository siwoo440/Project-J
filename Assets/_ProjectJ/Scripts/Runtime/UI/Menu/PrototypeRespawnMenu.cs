using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Player; // 플레이어 입력과 부활 기능 참조
using UnityEngine; // Unity 화면과 커서 기능 참조
using UnityEngine.InputSystem; // Input System 키보드 기능 참조

namespace ProjectJ.UI // 사용자 인터페이스 네임스페이스 선언
{ // 경기 메뉴 기능 범위
    [DisallowMultipleComponent] // 경기 메뉴 컴포넌트 중복 방지
    public sealed class PrototypeRespawnMenu : MonoBehaviour // ESC 직접 부활 경기 메뉴 선언
    { // 직접 부활 메뉴 범위
        [SerializeField] private PlayerRespawnController respawnController; // 직접 부활 요청 대상
        [SerializeField] private PlayerInputReader inputReader; // 메뉴 중 게임 입력 차단 대상
        [SerializeField] private Vector2 menuSize = new Vector2(420f, 230f); // 경기 메뉴 크기
        [SerializeField, Range(0f, 1f)] private float respawnFadeAlpha = 0.65f; // 부활 화면 암전 투명도

        private bool isMenuOpen; // 경기 메뉴 열림 여부
        private bool wasInputReaderEnabled; // 메뉴 열기 전 입력 컴포넌트 상태
        private CursorLockMode previousCursorLockMode; // 메뉴 열기 전 커서 잠금 상태
        private bool previousCursorVisible; // 메뉴 열기 전 커서 표시 상태

        public bool IsMenuOpen => isMenuOpen; // 경기 메뉴 열림 상태 반환

        private void Awake() // 경기 메뉴 필수 참조 준비
        { // 경기 메뉴 준비 범위
            if (respawnController == null) // 부활 관리자 Inspector 연결 확인
            { // 부활 관리자 자동 연결 범위
                respawnController = GetComponentInParent<PlayerRespawnController>(); // 부모 플레이어에서 부활 관리자 조회
            } // 부활 관리자 자동 연결 범위 종료

            if (inputReader == null && respawnController != null) // 입력 제공자 Inspector 연결 확인
            { // 입력 제공자 자동 연결 범위
                inputReader = respawnController.GetComponent<PlayerInputReader>(); // 플레이어에서 입력 제공자 조회
            } // 입력 제공자 자동 연결 범위 종료

            if (respawnController == null) // 필수 부활 관리자 누락 확인
            { // 부활 관리자 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "직접 부활 메뉴의 PlayerRespawnController 연결을 확인합니다.", "RESPAWN_MENU_SOURCE_MISSING", this); // 부활 관리자 누락 오류 출력
                enabled = false; // 잘못된 경기 메뉴 비활성화
            } // 부활 관리자 누락 범위 종료
        } // 경기 메뉴 준비 범위 종료

        private void OnValidate() // Inspector 경기 메뉴 수치 보정
        { // 경기 메뉴 수치 보정 범위
            menuSize.x = Mathf.Max(320f, menuSize.x); // 최소 메뉴 너비 보장
            menuSize.y = Mathf.Max(200f, menuSize.y); // 최소 메뉴 높이 보장
            respawnFadeAlpha = Mathf.Clamp01(respawnFadeAlpha); // 화면 암전 투명도 범위 제한
        } // 경기 메뉴 수치 보정 범위 종료

        private void Update() // ESC 경기 메뉴 입력 처리
        { // 경기 메뉴 입력 갱신 범위
            if (respawnController == null) // 부활 관리자 존재 확인
            { // 부활 관리자 누락 범위
                return; // 경기 메뉴 입력 처리 생략
            } // 부활 관리자 누락 범위 종료

            if (respawnController.IsRespawning) // 부활 진행 상태 확인
            { // 부활 진행 범위
                CloseMenu(); // 부활 중 열린 경기 메뉴 닫기
                return; // ESC 입력 처리 생략
            } // 부활 진행 범위 종료

            if (respawnController.IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 종료 범위
                CloseMenu(); // 경기 종료 뒤 열린 경기 메뉴 닫기
                return; // 경기 종료 뒤 ESC 입력 처리 생략
            } // 경기 종료 범위 종료

            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) // ESC 시작 입력 확인
            { // ESC 미입력 범위
                return; // 메뉴 전환 생략
            } // ESC 미입력 범위 종료

            if (isMenuOpen) // 현재 메뉴 열림 확인
            { // 메뉴 닫기 범위
                CloseMenu(); // 경기 메뉴 닫기
            } // 메뉴 닫기 범위 종료
            else // 현재 메뉴 닫힘 확인
            { // 메뉴 열기 범위
                OpenMenu(); // 경기 메뉴 열기
            } // 메뉴 열기 범위 종료
        } // 경기 메뉴 입력 갱신 범위 종료

        private void OnDisable() // 컴포넌트 비활성화 시 입력과 커서 복구
        { // 경기 메뉴 비활성화 범위
            CloseMenu(); // 열린 경기 메뉴 안전 종료
        } // 경기 메뉴 비활성화 범위 종료

        private void OnGUI() // 부활 화면 전환과 경기 메뉴 출력
        { // 경기 메뉴 화면 출력 범위
            DrawRespawnFade(); // 부활 중 화면 암전 출력

            if (!isMenuOpen) // 경기 메뉴 열림 상태 확인
            { // 경기 메뉴 닫힘 범위
                return; // 경기 메뉴 출력 생략
            } // 경기 메뉴 닫힘 범위 종료

            Rect menuRect = new Rect((Screen.width - menuSize.x) * 0.5f, (Screen.height - menuSize.y) * 0.5f, menuSize.x, menuSize.y); // 화면 중앙 메뉴 영역 계산
            GUI.Box(menuRect, GUIContent.none); // 경기 메뉴 배경 출력
            GUI.Label(new Rect(menuRect.x + 20f, menuRect.y + 16f, menuRect.width - 40f, 28f), "경기 메뉴"); // 경기 메뉴 제목 출력
            GUI.Label(new Rect(menuRect.x + 20f, menuRect.y + 50f, menuRect.width - 40f, 24f), $"현재 체크포인트 : {respawnController.CurrentCheckpointId}"); // 현재 부활 지점 출력
            GUI.Label(new Rect(menuRect.x + 20f, menuRect.y + 78f, menuRect.width - 40f, 42f), "직접 부활은 확인과 재사용 대기시간 없이 즉시 실행됩니다."); // 직접 부활 규칙 안내

            Rect respawnButtonRect = new Rect(menuRect.x + 20f, menuRect.y + 126f, menuRect.width - 40f, 38f); // 직접 부활 버튼 영역 계산

            if (GUI.Button(respawnButtonRect, "마지막 체크포인트에서 부활")) // 직접 부활 버튼 입력 확인
            { // 직접 부활 버튼 범위
                if (respawnController.TryRequestManualRespawn()) // 직접 부활 승인 결과 확인
                { // 직접 부활 승인 범위
                    CloseMenu(); // 승인 뒤 경기 메뉴 닫기
                } // 직접 부활 승인 범위 종료
            } // 직접 부활 버튼 범위 종료

            Rect closeButtonRect = new Rect(menuRect.x + 20f, menuRect.y + 174f, menuRect.width - 40f, 34f); // 경기 복귀 버튼 영역 계산

            if (GUI.Button(closeButtonRect, "경기로 돌아가기")) // 경기 복귀 버튼 입력 확인
            { // 경기 복귀 버튼 범위
                CloseMenu(); // 경기 메뉴 닫기
            } // 경기 복귀 버튼 범위 종료
        } // 경기 메뉴 화면 출력 범위 종료

        private void OpenMenu() // 경기 메뉴 열기와 게임 입력 차단
        { // 경기 메뉴 열기 범위
            if (isMenuOpen || respawnController.IsRespawning || respawnController.IsMatchFinished) // 중복 열기와 부활과 경기 종료 상태 확인
            { // 메뉴 열기 차단 범위
                return; // 경기 메뉴 열기 생략
            } // 메뉴 열기 차단 범위 종료

            isMenuOpen = true; // 경기 메뉴 열림 상태 저장
            previousCursorLockMode = Cursor.lockState; // 기존 커서 잠금 상태 저장
            previousCursorVisible = Cursor.visible; // 기존 커서 표시 상태 저장
            wasInputReaderEnabled = inputReader != null && inputReader.enabled; // 기존 입력 컴포넌트 활성 상태 저장

            if (inputReader != null) // 입력 컴포넌트 존재 확인
            { // 게임 입력 차단 범위
                inputReader.enabled = false; // 이동과 시점과 행동 입력 비활성화
            } // 게임 입력 차단 범위 종료

            Cursor.lockState = CursorLockMode.None; // 메뉴 조작용 커서 잠금 해제
            Cursor.visible = true; // 메뉴 조작용 커서 표시
        } // 경기 메뉴 열기 범위 종료

        private void CloseMenu() // 경기 메뉴 닫기와 입력과 커서 복구
        { // 경기 메뉴 닫기 범위
            if (!isMenuOpen) // 경기 메뉴 닫힘 상태 확인
            { // 메뉴 중복 닫기 범위
                return; // 입력과 커서 중복 복구 생략
            } // 메뉴 중복 닫기 범위 종료

            isMenuOpen = false; // 경기 메뉴 닫힘 상태 저장

            if (inputReader != null && wasInputReaderEnabled) // 기존 입력 활성 상태 확인
            { // 게임 입력 복구 범위
                inputReader.enabled = true; // 이동과 시점과 행동 입력 활성화
            } // 게임 입력 복구 범위 종료

            Cursor.lockState = previousCursorLockMode; // 기존 커서 잠금 상태 복구
            Cursor.visible = previousCursorVisible; // 기존 커서 표시 상태 복구
        } // 경기 메뉴 닫기 범위 종료

        private void DrawRespawnFade() // 부활 중 전체 화면 암전 출력
        { // 부활 화면 암전 범위
            if (respawnController == null || !respawnController.IsRespawning) // 부활 진행 상태 확인
            { // 부활 미진행 범위
                return; // 화면 암전 출력 생략
            } // 부활 미진행 범위 종료

            Color previousColor = GUI.color; // 기존 GUI 색상 저장
            GUI.color = new Color(0f, 0f, 0f, respawnFadeAlpha); // 반투명 검은색 적용
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture); // 전체 화면 암전 출력
            GUI.color = previousColor; // 기존 GUI 색상 복구
        } // 부활 화면 암전 범위 종료
    } // 직접 부활 메뉴 범위 종료
} // 경기 메뉴 기능 범위 종료
