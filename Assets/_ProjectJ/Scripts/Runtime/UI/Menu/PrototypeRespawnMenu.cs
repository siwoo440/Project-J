using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Player; // 플레이어 입력과 부활 기능 참조
using TMPro; // TextMeshPro 텍스트 기능 참조
using UnityEngine; // Unity 커서와 컴포넌트 기능 참조
using UnityEngine.InputSystem; // Input System 키보드 기능 참조
using UnityEngine.UI; // Canvas Button 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    [DisallowMultipleComponent] // 경기 메뉴 컴포넌트 중복 방지
    public sealed class PrototypeRespawnMenu : MonoBehaviour // 기존 이름을 유지한 Canvas 경기 메뉴 선언
    { // Canvas 경기 메뉴 기능 묶음
        [Header("데이터 제공자")] // 데이터 제공자 Inspector 구분
        [SerializeField] private PlayerRespawnController respawnController; // 직접 부활 요청 대상 참조
        [SerializeField] private PlayerInputReader inputReader; // 메뉴 중 게임 입력 차단 대상 참조

        [Header("Canvas 요소")] // Canvas 요소 Inspector 구분
        [SerializeField] private GameObject fadePanel; // 메뉴와 부활 공통 전체 화면 암전 패널 참조
        [SerializeField] private GameObject menuPanel; // ESC 경기 메뉴 패널 참조
        [SerializeField] private TMP_Text checkpointText; // 현재 체크포인트 텍스트 참조
        [SerializeField] private Button respawnButton; // 마지막 체크포인트 부활 버튼 참조
        [SerializeField] private Button closeButton; // 경기 복귀 버튼 참조

        private bool isMenuOpen; // 경기 메뉴 열림 여부
        private bool buttonsBound; // Canvas 버튼 이벤트 연결 여부
        private bool wasInputReaderEnabled; // 메뉴 열기 전 입력 컴포넌트 상태
        private CursorLockMode previousCursorLockMode; // 메뉴 열기 전 커서 잠금 상태
        private bool previousCursorVisible; // 메뉴 열기 전 커서 표시 상태

        public bool IsMenuOpen => isMenuOpen; // 경기 메뉴 열림 상태 반환

        private void Awake() // Canvas 경기 메뉴 필수 참조 준비
        { // Canvas 경기 메뉴 준비 처리
            ResolvePlayerReferences(); // 같은 플레이어 기반 누락 참조 자동 연결

            if (!HasRequiredReferences()) // 필수 데이터와 Canvas 참조 연결 여부 확인
            { // Canvas 경기 메뉴 참조 누락 처리
                ProjectLog.Error(ProjectLogCategory.Gameplay, "40일차 Canvas 경기 메뉴 데이터와 UI 참조 연결을 확인합니다.", "CANVAS_RESPAWN_MENU_REFERENCE_MISSING", this); // Canvas 경기 메뉴 참조 누락 오류 출력
                enabled = false; // 잘못 구성된 Canvas 경기 메뉴 비활성화
                return; // Canvas 경기 메뉴 준비 중단
            } // Canvas 경기 메뉴 참조 누락 처리 종료

            menuPanel.SetActive(false); // 최초 ESC 경기 메뉴 숨김
            fadePanel.SetActive(false); // 최초 전체 화면 암전 숨김
        } // Canvas 경기 메뉴 준비 처리 종료

        private void OnEnable() // Canvas 경기 메뉴 활성화 시 버튼 이벤트 연결
        { // Canvas 경기 메뉴 활성화 처리
            BindButtons(); // 부활과 복귀 버튼 이벤트 연결
        } // Canvas 경기 메뉴 활성화 처리 종료

        private void OnDisable() // Canvas 경기 메뉴 비활성화 시 상태와 이벤트 정리
        { // Canvas 경기 메뉴 비활성화 처리
            CloseMenu(); // 열린 메뉴와 입력 상태 안전 복구
            UnbindButtons(); // 부활과 복귀 버튼 이벤트 해제
        } // Canvas 경기 메뉴 비활성화 처리 종료

        private void Update() // ESC 입력과 Canvas 표시 상태 갱신
        { // Canvas 경기 메뉴 프레임 갱신 처리
            if (respawnController == null) // 부활 관리자 존재 여부 확인
            { // 부활 관리자 누락 처리
                return; // 메뉴 입력과 표시 갱신 생략
            } // 부활 관리자 누락 처리 종료

            if (respawnController.IsRespawning || respawnController.IsMatchFinished) // 부활 또는 경기 종료 상태 확인
            { // 경기 메뉴 강제 닫기 처리
                CloseMenu(); // 게임 상태가 메뉴를 허용하지 않을 때 닫기
            } // 경기 메뉴 강제 닫기 처리 종료
            else if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) // ESC 새 입력 여부 확인
            { // ESC 메뉴 전환 처리
                if (isMenuOpen) // 현재 메뉴 열림 여부 확인
                { // 메뉴 닫기 처리
                    CloseMenu(); // ESC 입력으로 메뉴 닫기
                } // 메뉴 닫기 처리 종료
                else // 현재 메뉴 닫힘 여부 확인
                { // 메뉴 열기 처리
                    OpenMenu(); // ESC 입력으로 메뉴 열기
                } // 메뉴 열기 처리 종료
            } // ESC 메뉴 전환 처리 종료

            RefreshCanvasState(); // 메뉴와 부활 기반 Canvas 요소 표시 갱신
        } // Canvas 경기 메뉴 프레임 갱신 처리 종료

        private void ResolvePlayerReferences() // 플레이어 기반 누락 참조 자동 연결
        { // 플레이어 참조 자동 연결 처리
            if (respawnController == null) // 부활 관리자 Inspector 연결 여부 확인
            { // 부활 관리자 자동 연결 처리
                respawnController = GetComponentInParent<PlayerRespawnController>(); // 부모 플레이어에서 부활 관리자 조회
            } // 부활 관리자 자동 연결 처리 종료

            if (inputReader == null && respawnController != null) // 입력 제공자 Inspector 연결 여부 확인
            { // 입력 제공자 자동 연결 처리
                inputReader = respawnController.GetComponent<PlayerInputReader>(); // 플레이어에서 입력 제공자 조회
            } // 입력 제공자 자동 연결 처리 종료
        } // 플레이어 참조 자동 연결 처리 종료

        private bool HasRequiredReferences() // Canvas 경기 메뉴 필수 참조 연결 여부 확인
        { // Canvas 경기 메뉴 참조 검사 처리
            return respawnController != null && fadePanel != null && menuPanel != null && checkpointText != null && respawnButton != null && closeButton != null; // 필수 데이터와 UI 참조 검사 결과 반환
        } // Canvas 경기 메뉴 참조 검사 처리 종료

        private void OpenMenu() // 경기 메뉴 열기와 게임 입력 차단
        { // 경기 메뉴 열기 처리
            if (isMenuOpen || respawnController.IsRespawning || respawnController.IsMatchFinished) // 중복 열기와 부활과 경기 종료 상태 확인
            { // 경기 메뉴 열기 차단 처리
                return; // 허용되지 않은 경기 메뉴 열기 생략
            } // 경기 메뉴 열기 차단 처리 종료

            isMenuOpen = true; // 경기 메뉴 열림 상태 저장
            previousCursorLockMode = Cursor.lockState; // 기존 커서 잠금 상태 저장
            previousCursorVisible = Cursor.visible; // 기존 커서 표시 상태 저장
            wasInputReaderEnabled = inputReader != null && inputReader.enabled; // 기존 게임 입력 활성 상태 저장

            if (inputReader != null) // 게임 입력 컴포넌트 존재 여부 확인
            { // 게임 입력 차단 처리
                inputReader.enabled = false; // 이동과 시점과 행동 입력 비활성화
            } // 게임 입력 차단 처리 종료

            Cursor.lockState = CursorLockMode.None; // 메뉴 조작용 커서 잠금 해제
            Cursor.visible = true; // 메뉴 조작용 커서 표시
            RefreshCanvasState(); // 열린 메뉴 Canvas 표시 즉시 갱신
        } // 경기 메뉴 열기 처리 종료

        private void CloseMenu() // 경기 메뉴 닫기와 게임 입력 복구
        { // 경기 메뉴 닫기 처리
            if (!isMenuOpen) // 경기 메뉴 닫힘 상태 확인
            { // 중복 닫기 처리
                RefreshCanvasState(); // 부활 암전 상태만 최신화
                return; // 입력과 커서 중복 복구 생략
            } // 중복 닫기 처리 종료

            isMenuOpen = false; // 경기 메뉴 닫힘 상태 저장

            if (inputReader != null && wasInputReaderEnabled) // 메뉴 전 게임 입력 활성 상태 확인
            { // 게임 입력 복구 처리
                inputReader.enabled = true; // 이동과 시점과 행동 입력 활성화
            } // 게임 입력 복구 처리 종료

            Cursor.lockState = previousCursorLockMode; // 메뉴 전 커서 잠금 상태 복구
            Cursor.visible = previousCursorVisible; // 메뉴 전 커서 표시 상태 복구
            RefreshCanvasState(); // 닫힌 메뉴 Canvas 표시 즉시 갱신
        } // 경기 메뉴 닫기 처리 종료

        private void RefreshCanvasState() // 메뉴와 부활 기반 Canvas 요소 표시 갱신
        { // Canvas 요소 표시 갱신 처리
            if (menuPanel != null) // 경기 메뉴 패널 존재 여부 확인
            { // 경기 메뉴 패널 표시 처리
                menuPanel.SetActive(isMenuOpen); // 메뉴 열림 상태와 패널 표시 동기화
            } // 경기 메뉴 패널 표시 처리 종료

            if (fadePanel != null) // 전체 화면 암전 패널 존재 여부 확인
            { // 전체 화면 암전 표시 처리
                bool shouldShowFade = isMenuOpen || (respawnController != null && respawnController.IsRespawning); // 메뉴 또는 부활 기반 암전 표시 여부 계산
                fadePanel.SetActive(shouldShowFade); // 계산된 전체 화면 암전 표시 적용
            } // 전체 화면 암전 표시 처리 종료

            if (checkpointText != null && respawnController != null) // 체크포인트 텍스트와 데이터 존재 여부 확인
            { // 체크포인트 텍스트 갱신 처리
                checkpointText.text = $"현재 체크포인트 : {respawnController.CurrentCheckpointId}"; // 현재 체크포인트 ID 문구 적용
            } // 체크포인트 텍스트 갱신 처리 종료
        } // Canvas 요소 표시 갱신 처리 종료

        private void OnRespawnButtonClicked() // 마지막 체크포인트 부활 버튼 입력 처리
        { // 부활 버튼 입력 처리
            if (respawnController != null && respawnController.TryRequestManualRespawn()) // 직접 부활 승인 여부 확인
            { // 직접 부활 승인 처리
                CloseMenu(); // 승인 직후 경기 메뉴 닫기
            } // 직접 부활 승인 처리 종료
        } // 부활 버튼 입력 처리 종료

        private void OnCloseButtonClicked() // 경기 복귀 버튼 입력 처리
        { // 경기 복귀 버튼 입력 처리
            CloseMenu(); // 경기 메뉴 닫기와 입력 복구
        } // 경기 복귀 버튼 입력 처리 종료

        private void BindButtons() // Canvas 버튼 이벤트 안전 연결
        { // Canvas 버튼 이벤트 연결 처리
            if (buttonsBound || respawnButton == null || closeButton == null) // 중복 연결과 버튼 누락 확인
            { // 버튼 이벤트 연결 생략 처리
                return; // 잘못된 버튼 이벤트 연결 방지
            } // 버튼 이벤트 연결 생략 처리 종료

            respawnButton.onClick.AddListener(OnRespawnButtonClicked); // 부활 버튼에 직접 부활 처리 연결
            closeButton.onClick.AddListener(OnCloseButtonClicked); // 복귀 버튼에 메뉴 닫기 처리 연결
            buttonsBound = true; // 버튼 이벤트 연결 상태 저장
        } // Canvas 버튼 이벤트 연결 처리 종료

        private void UnbindButtons() // Canvas 버튼 이벤트 안전 해제
        { // Canvas 버튼 이벤트 해제 처리
            if (!buttonsBound) // 버튼 이벤트 미연결 여부 확인
            { // 버튼 이벤트 해제 생략 처리
                return; // 연결되지 않은 버튼 이벤트 해제 방지
            } // 버튼 이벤트 해제 생략 처리 종료

            if (respawnButton != null) // 부활 버튼 존재 여부 확인
            { // 부활 버튼 이벤트 해제 처리
                respawnButton.onClick.RemoveListener(OnRespawnButtonClicked); // 부활 버튼 직접 부활 처리 해제
            } // 부활 버튼 이벤트 해제 처리 종료

            if (closeButton != null) // 복귀 버튼 존재 여부 확인
            { // 복귀 버튼 이벤트 해제 처리
                closeButton.onClick.RemoveListener(OnCloseButtonClicked); // 복귀 버튼 메뉴 닫기 처리 해제
            } // 복귀 버튼 이벤트 해제 처리 종료

            buttonsBound = false; // 버튼 이벤트 연결 상태 초기화
        } // Canvas 버튼 이벤트 해제 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerRespawnController newRespawnController, PlayerInputReader newInputReader, GameObject newFadePanel, GameObject newMenuPanel, TMP_Text newCheckpointText, Button newRespawnButton, Button newCloseButton) // 자동 설정 도구용 경기 메뉴 참조 연결
        { // 자동 설정 도구용 경기 메뉴 참조 연결 처리
            respawnController = newRespawnController; // 부활 관리자 참조 저장
            inputReader = newInputReader; // 게임 입력 제공자 참조 저장
            fadePanel = newFadePanel; // 전체 화면 암전 패널 참조 저장
            menuPanel = newMenuPanel; // ESC 경기 메뉴 패널 참조 저장
            checkpointText = newCheckpointText; // 체크포인트 텍스트 참조 저장
            respawnButton = newRespawnButton; // 부활 버튼 참조 저장
            closeButton = newCloseButton; // 경기 복귀 버튼 참조 저장
        } // 자동 설정 도구용 경기 메뉴 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // Canvas 경기 메뉴 기능 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료
