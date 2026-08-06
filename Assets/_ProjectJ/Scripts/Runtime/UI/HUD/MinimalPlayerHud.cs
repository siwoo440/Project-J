using System.Collections.Generic; // 동적 순위 텍스트 목록 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Gameplay; // 경기 관리자와 순위 데이터 기능 참조
using ProjectJ.Items; // 플레이어 아이템 인벤토리 기능 참조
using ProjectJ.Player; // 플레이어 진행과 이동 기능 참조
using TMPro; // TextMeshPro 텍스트 기능 참조
using UnityEngine; // Unity 컴포넌트와 수치 기능 참조
using UnityEngine.UI; // Canvas Image 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    [DisallowMultipleComponent] // HUD 컴포넌트 중복 방지
    public sealed class MinimalPlayerHud : MonoBehaviour // 기존 이름을 유지한 Canvas 기반 전체 HUD 선언
    { // Canvas 플레이어 HUD 기능 묶음
        [Header("데이터 제공자")] // 데이터 제공자 Inspector 구분
        [SerializeField] private PlayerRespawnController respawnController; // 체크포인트와 부활 정보 제공자
        [SerializeField] private PlayerHeightProgressController heightProgressController; // 높이와 구간 정보 제공자
        [SerializeField] private PlayerMovementController movementController; // 스태미나 정보 제공자
        [SerializeField] private PrototypeMatchController matchController; // 경기 시간과 순위 제공자
        [SerializeField] private PlayerItemInventory itemInventory; // 두 슬롯 아이템 정보 제공자

        [Header("상단 중앙")] // 상단 중앙 UI Inspector 구분
        [SerializeField] private TMP_Text timerText; // 남은 시간 텍스트 참조
        [SerializeField] private TMP_Text currentRankText; // 현재 순위 텍스트 참조

        [Header("진행 정보")] // 진행 정보 UI Inspector 구분
        [SerializeField] private TMP_Text heightText; // 현재와 최고 높이 텍스트 참조
        [SerializeField] private TMP_Text sectionText; // 현재 구간과 전체 진행 텍스트 참조
        [SerializeField] private Image courseProgressFill; // 전체 코스 진행 막대 채움 이미지 참조
        [SerializeField] private TMP_Text checkpointText; // 체크포인트 텍스트 참조
        [SerializeField] private TMP_Text courseTopText; // 정상 도달 상태 텍스트 참조

        [Header("스태미나")] // 스태미나 UI Inspector 구분
        [SerializeField] private TMP_Text staminaText; // 스태미나 백분율 텍스트 참조
        [SerializeField] private Image staminaFill; // 스태미나 막대 채움 이미지 참조

        [Header("실시간 순위")] // 실시간 순위 UI Inspector 구분
        [SerializeField] private RectTransform rankingEntryRoot; // 실시간 순위 항목 부모 참조
        [SerializeField] private TMP_Text rankingEntryTemplate; // 실시간 순위 항목 복제 원본 참조

        [Header("아이템 슬롯")] // 아이템 슬롯 UI Inspector 구분
        [SerializeField] private CanvasItemSlotView[] itemSlotViews = new CanvasItemSlotView[PlayerItemInventory.Capacity]; // 두 아이템 슬롯 표시 참조

        [Header("중앙 안내와 결과")] // 중앙 안내와 결과 UI Inspector 구분
        [SerializeField] private GameObject respawnNoticePanel; // 부활 안내 패널 참조
        [SerializeField] private GameObject resultPanel; // 경기 결과 전체 화면 패널 참조
        [SerializeField] private TMP_Text resultTitleText; // 경기 결과 제목 텍스트 참조
        [SerializeField] private TMP_Text resultReasonText; // 경기 종료 원인 텍스트 참조
        [SerializeField] private TMP_Text finalRankText; // 플레이어 최종 순위 텍스트 참조
        [SerializeField] private RectTransform resultEntryRoot; // 최종 순위 항목 부모 참조
        [SerializeField] private TMP_Text resultEntryTemplate; // 최종 순위 항목 복제 원본 참조

        private readonly List<TMP_Text> rankingEntries = new List<TMP_Text>(); // 생성된 실시간 순위 텍스트 목록
        private readonly List<TMP_Text> resultEntries = new List<TMP_Text>(); // 생성된 최종 순위 텍스트 목록
        private bool inventoryEventSubscribed; // 인벤토리 변경 이벤트 연결 여부

        private void Awake() // Canvas HUD 필수 참조 자동 연결과 검증
        { // Canvas HUD 준비 처리
            ResolvePlayerReferences(); // 같은 플레이어 기반 누락 참조 자동 연결

            if (!HasRequiredReferences()) // 필수 데이터와 Canvas 참조 유효 여부 확인
            { // Canvas HUD 참조 누락 처리
                ProjectLog.Error(ProjectLogCategory.Gameplay, "40일차 Canvas HUD 데이터와 UI 참조 연결을 확인합니다.", "CANVAS_HUD_REFERENCE_MISSING", this); // Canvas HUD 참조 누락 오류 출력
                enabled = false; // 잘못 구성된 Canvas HUD 비활성화
                return; // Canvas HUD 준비 중단
            } // Canvas HUD 참조 누락 처리 종료

            rankingEntryTemplate.gameObject.SetActive(false); // 실시간 순위 복제 원본 숨김
            resultEntryTemplate.gameObject.SetActive(false); // 최종 순위 복제 원본 숨김
            respawnNoticePanel.SetActive(false); // 최초 부활 안내 숨김
            resultPanel.SetActive(false); // 최초 경기 결과 숨김
        } // Canvas HUD 준비 처리 종료

        private void OnEnable() // Canvas HUD 활성화 시 이벤트 연결과 최초 표시 갱신
        { // Canvas HUD 활성화 처리
            SubscribeInventoryEvent(); // 인벤토리 변경 이벤트 연결
            RefreshInventorySlots(); // 현재 두 슬롯 표시 즉시 갱신
        } // Canvas HUD 활성화 처리 종료

        private void OnDisable() // Canvas HUD 비활성화 시 이벤트 해제
        { // Canvas HUD 비활성화 처리
            UnsubscribeInventoryEvent(); // 인벤토리 변경 이벤트 해제
        } // Canvas HUD 비활성화 처리 종료

        private void Update() // Canvas HUD 모든 실시간 표시 갱신
        { // Canvas HUD 프레임 갱신 처리
            RefreshTopCenter(); // 남은 시간과 현재 순위 갱신
            RefreshProgressPanel(); // 높이와 구간과 체크포인트 갱신
            RefreshStaminaPanel(); // 스태미나 표시 갱신
            RefreshCurrentRanking(); // 실시간 순위 목록 갱신
            RefreshRespawnNotice(); // 부활 안내 표시 상태 갱신
            RefreshResultPanel(); // 경기 결과 표시 상태 갱신
        } // Canvas HUD 프레임 갱신 처리 종료

        private void ResolvePlayerReferences() // 플레이어 컴포넌트 기반 누락 참조 자동 연결
        { // 플레이어 참조 자동 연결 처리
            if (heightProgressController == null && respawnController != null) // 높이 진행 참조 누락 여부 확인
            { // 높이 진행 참조 자동 연결 처리
                heightProgressController = respawnController.GetComponent<PlayerHeightProgressController>(); // 플레이어에서 높이 진행 관리자 조회
            } // 높이 진행 참조 자동 연결 처리 종료

            if (movementController == null && respawnController != null) // 이동 참조 누락 여부 확인
            { // 이동 참조 자동 연결 처리
                movementController = respawnController.GetComponent<PlayerMovementController>(); // 플레이어에서 이동 관리자 조회
            } // 이동 참조 자동 연결 처리 종료

            if (itemInventory == null && respawnController != null) // 아이템 인벤토리 참조 누락 여부 확인
            { // 아이템 인벤토리 참조 자동 연결 처리
                itemInventory = respawnController.GetComponent<PlayerItemInventory>(); // 플레이어에서 아이템 인벤토리 조회
            } // 아이템 인벤토리 참조 자동 연결 처리 종료
        } // 플레이어 참조 자동 연결 처리 종료

        private bool HasRequiredReferences() // Canvas HUD 필수 참조 연결 여부 확인
        { // Canvas HUD 필수 참조 검사 처리
            bool hasSources = respawnController != null && heightProgressController != null && movementController != null && matchController != null && itemInventory != null; // 모든 데이터 제공자 연결 여부 계산
            bool hasPrimaryViews = timerText != null && currentRankText != null && heightText != null && sectionText != null && courseProgressFill != null && checkpointText != null && courseTopText != null; // 상단과 진행 UI 연결 여부 계산
            bool hasSecondaryViews = staminaText != null && staminaFill != null && rankingEntryRoot != null && rankingEntryTemplate != null && respawnNoticePanel != null; // 스태미나와 순위와 안내 UI 연결 여부 계산
            bool hasResultViews = resultPanel != null && resultTitleText != null && resultReasonText != null && finalRankText != null && resultEntryRoot != null && resultEntryTemplate != null; // 결과 UI 연결 여부 계산
            return hasSources && hasPrimaryViews && hasSecondaryViews && hasResultViews && itemSlotViews != null && itemSlotViews.Length == PlayerItemInventory.Capacity; // 전체 Canvas HUD 참조 검사 결과 반환
        } // Canvas HUD 필수 참조 검사 처리 종료

        private void RefreshTopCenter() // 남은 시간과 현재 순위 표시 갱신
        { // 상단 중앙 표시 갱신 처리
            timerText.text = CanvasUiTextRules.FormatTimer(matchController.RemainingTime); // 남은 시간 두 자리 문구 적용
            int displayedRank = matchController.IsMatchFinished ? matchController.FinalPlayerRank : matchController.PlayerRank; // 경기 상태 기반 표시 순위 선택
            currentRankText.text = CanvasUiTextRules.FormatRank(displayedRank, matchController.ParticipantCount); // 현재 순위와 참가자 수 문구 적용
        } // 상단 중앙 표시 갱신 처리 종료

        private void RefreshProgressPanel() // 높이와 구간과 체크포인트 표시 갱신
        { // 진행 정보 표시 갱신 처리
            int courseProgressPercent = Mathf.RoundToInt(heightProgressController.CourseProgress01 * 100f); // 전체 코스 진행 백분율 계산
            heightText.text = $"현재 {heightProgressController.CurrentHeight:0.0} m  |  최고 {heightProgressController.HighestHeight:0.0} m"; // 현재와 최고 높이 문구 적용
            sectionText.text = $"구간 {heightProgressController.CurrentSectionIndex}/{heightProgressController.SectionCount}  |  전체 {courseProgressPercent}%"; // 현재 구간과 전체 진행 문구 적용
            courseProgressFill.fillAmount = Mathf.Clamp01(heightProgressController.CourseProgress01); // 전체 코스 진행 막대 비율 적용
            checkpointText.text = $"체크포인트 {respawnController.CurrentCheckpointIndex}/{respawnController.CheckpointCount}  |  {respawnController.CurrentCheckpointId}"; // 체크포인트 순서와 ID 문구 적용
            courseTopText.text = respawnController.HasReachedCourseTop ? "정상 지점 : 도달" : "정상 지점 : 미도달"; // 정상 도달 상태 문구 적용
        } // 진행 정보 표시 갱신 처리 종료

        private void RefreshStaminaPanel() // 스태미나 텍스트와 막대 표시 갱신
        { // 스태미나 표시 갱신 처리
            float normalizedStamina = Mathf.Clamp01(movementController.StaminaNormalized); // 스태미나 비율 안전 범위 보정
            staminaText.text = $"스태미나  {Mathf.RoundToInt(normalizedStamina * 100f)}%"; // 스태미나 백분율 문구 적용
            staminaFill.fillAmount = normalizedStamina; // 스태미나 막대 채움 비율 적용
        } // 스태미나 표시 갱신 처리 종료

        private void RefreshCurrentRanking() // 현재 참가자 순위 목록 표시 갱신
        { // 현재 참가자 순위 목록 갱신 처리
            int participantCount = matchController.ParticipantCount; // 현재 참가자 수 조회
            EnsureEntryCount(rankingEntries, rankingEntryTemplate, rankingEntryRoot, participantCount); // 참가자 수와 같은 실시간 항목 수 보장

            for (int index = 0; index < participantCount; index++) // 현재 참가자 전체 순회
            { // 현재 순위 항목 표시 처리
                PrototypeRankEntry entry = matchController.GetCurrentRankEntry(index); // 현재 순위 데이터 조회
                bool isSharedRank = CountEntriesWithRank(false, entry.Rank) > 1; // 현재 공동 순위 여부 계산
                TMP_Text entryText = rankingEntries[index]; // 현재 순위 텍스트 조회
                entryText.text = CanvasUiTextRules.FormatRankingEntry(entry, isSharedRank, false); // 현재 순위 한 줄 문구 적용
                entryText.color = entry.IsLocalPlayer ? new Color(0.2f, 1f, 0.9f, 1f) : Color.white; // 로컬 플레이어 강조 색상 적용
                entryText.fontStyle = entry.IsLocalPlayer ? FontStyles.Bold : FontStyles.Normal; // 로컬 플레이어 굵은 글꼴 적용
            } // 현재 순위 항목 표시 처리 종료
        } // 현재 참가자 순위 목록 갱신 처리 종료

        private void RefreshRespawnNotice() // 부활 안내 패널 표시 상태 갱신
        { // 부활 안내 패널 표시 처리
            respawnNoticePanel.SetActive(respawnController.IsRespawning && !matchController.IsMatchFinished); // 부활 중이며 경기 진행 중일 때만 안내 표시
        } // 부활 안내 패널 표시 처리 종료

        private void RefreshResultPanel() // 경기 결과 패널과 최종 순위 갱신
        { // 경기 결과 패널 갱신 처리
            if (!matchController.IsMatchFinished) // 경기 진행 중 여부 확인
            { // 경기 결과 숨김 처리
                resultPanel.SetActive(false); // 경기 결과 전체 화면 숨김
                return; // 최종 결과 갱신 생략
            } // 경기 결과 숨김 처리 종료

            resultPanel.SetActive(true); // 경기 종료 결과 전체 화면 표시
            resultTitleText.text = CanvasUiTextRules.GetOutcomeText(matchController.PlayerOutcome); // 승리와 공동 승리와 패배 제목 적용
            resultReasonText.text = CanvasUiTextRules.GetEndReasonText(matchController.EndReason); // 경기 종료 원인 문구 적용
            finalRankText.text = $"최종 순위 : {CanvasUiTextRules.FormatRank(matchController.FinalPlayerRank, matchController.ParticipantCount)}"; // 최종 순위와 참가자 수 문구 적용
            int participantCount = matchController.ParticipantCount; // 최종 참가자 수 조회
            EnsureEntryCount(resultEntries, resultEntryTemplate, resultEntryRoot, participantCount); // 참가자 수와 같은 최종 결과 항목 수 보장

            for (int index = 0; index < participantCount; index++) // 최종 참가자 전체 순회
            { // 최종 순위 항목 표시 처리
                PrototypeRankEntry entry = matchController.GetFinalRankEntry(index); // 최종 순위 데이터 조회
                bool isSharedRank = CountEntriesWithRank(true, entry.Rank) > 1; // 최종 공동 순위 여부 계산
                TMP_Text entryText = resultEntries[index]; // 최종 순위 텍스트 조회
                entryText.text = CanvasUiTextRules.FormatRankingEntry(entry, isSharedRank, true); // 정상 도달을 포함한 최종 순위 문구 적용
                entryText.color = entry.IsLocalPlayer ? new Color(0.2f, 1f, 0.9f, 1f) : Color.white; // 로컬 플레이어 강조 색상 적용
                entryText.fontStyle = entry.IsLocalPlayer ? FontStyles.Bold : FontStyles.Normal; // 로컬 플레이어 굵은 글꼴 적용
            } // 최종 순위 항목 표시 처리 종료
        } // 경기 결과 패널 갱신 처리 종료

        private void RefreshInventorySlots() // 플레이어 두 아이템 슬롯 표시 갱신
        { // 아이템 슬롯 표시 갱신 처리
            if (itemInventory == null || itemSlotViews == null) // 인벤토리와 슬롯 표시 참조 확인
            { // 아이템 슬롯 갱신 차단 처리
                return; // 잘못된 참조의 슬롯 갱신 생략
            } // 아이템 슬롯 갱신 차단 처리 종료

            for (int slotIndex = 0; slotIndex < PlayerItemInventory.Capacity; slotIndex++) // 두 아이템 슬롯 순회
            { // 현재 아이템 슬롯 갱신 처리
                if (slotIndex < itemSlotViews.Length && itemSlotViews[slotIndex] != null) // 현재 슬롯 표시 참조 존재 여부 확인
                { // 현재 아이템 슬롯 표시 처리
                    itemSlotViews[slotIndex].Refresh(slotIndex, itemInventory.GetItemAt(slotIndex)); // 현재 인벤토리 데이터로 슬롯 표시 갱신
                } // 현재 아이템 슬롯 표시 처리 종료
            } // 현재 아이템 슬롯 갱신 처리 종료
        } // 아이템 슬롯 표시 갱신 처리 종료

        private void SubscribeInventoryEvent() // 인벤토리 변경 이벤트 안전 연결
        { // 인벤토리 변경 이벤트 연결 처리
            if (inventoryEventSubscribed || itemInventory == null) // 중복 연결과 인벤토리 누락 확인
            { // 이벤트 연결 생략 처리
                return; // 이벤트 중복 연결 방지
            } // 이벤트 연결 생략 처리 종료

            itemInventory.InventoryChanged += RefreshInventorySlots; // 인벤토리 변경 시 슬롯 갱신 메서드 연결
            inventoryEventSubscribed = true; // 이벤트 연결 상태 저장
        } // 인벤토리 변경 이벤트 연결 처리 종료

        private void UnsubscribeInventoryEvent() // 인벤토리 변경 이벤트 안전 해제
        { // 인벤토리 변경 이벤트 해제 처리
            if (!inventoryEventSubscribed || itemInventory == null) // 연결 상태와 인벤토리 존재 여부 확인
            { // 이벤트 해제 생략 처리
                return; // 연결되지 않은 이벤트 해제 방지
            } // 이벤트 해제 생략 처리 종료

            itemInventory.InventoryChanged -= RefreshInventorySlots; // 인벤토리 변경 이벤트에서 슬롯 갱신 메서드 해제
            inventoryEventSubscribed = false; // 이벤트 연결 상태 초기화
        } // 인벤토리 변경 이벤트 해제 처리 종료

        private int CountEntriesWithRank(bool useFinalRanking, int rank) // 같은 공동 순위 참가자 수 계산
        { // 공동 순위 참가자 수 계산 처리
            int count = 0; // 같은 순위 참가자 수 초기화

            for (int index = 0; index < matchController.ParticipantCount; index++) // 전체 참가자 순회
            { // 현재 참가자 순위 비교 처리
                PrototypeRankEntry entry = useFinalRanking ? matchController.GetFinalRankEntry(index) : matchController.GetCurrentRankEntry(index); // 현재 또는 최종 순위 데이터 선택

                if (entry.Rank == rank) // 같은 순위 번호 여부 확인
                { // 같은 순위 집계 처리
                    count++; // 같은 순위 참가자 수 증가
                } // 같은 순위 집계 처리 종료
            } // 현재 참가자 순위 비교 처리 종료

            return count; // 같은 순위 참가자 수 반환
        } // 공동 순위 참가자 수 계산 처리 종료

        private static void EnsureEntryCount(List<TMP_Text> entries, TMP_Text template, RectTransform parent, int requiredCount) // 동적 순위 텍스트 개수 보장
        { // 동적 순위 텍스트 개수 보장 처리
            while (entries.Count < requiredCount) // 필요한 항목보다 생성 항목이 적은 동안 반복
            { // 새 순위 항목 생성 처리
                TMP_Text newEntry = Instantiate(template, parent); // 복제 원본 기반 새 TextMeshPro 항목 생성
                newEntry.name = $"Entry_{entries.Count + 1:00}"; // Hierarchy 확인용 순번 이름 적용
                newEntry.gameObject.SetActive(true); // 새 순위 항목 표시
                entries.Add(newEntry); // 생성된 순위 텍스트 목록 등록
            } // 새 순위 항목 생성 처리 종료

            for (int index = 0; index < entries.Count; index++) // 생성된 모든 순위 항목 순회
            { // 순위 항목 활성 상태 처리
                entries[index].gameObject.SetActive(index < requiredCount); // 현재 참가자 수 범위 항목만 표시
            } // 순위 항목 활성 상태 처리 종료
        } // 동적 순위 텍스트 개수 보장 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerRespawnController newRespawnController, PlayerHeightProgressController newHeightProgressController, PlayerMovementController newMovementController, PrototypeMatchController newMatchController, PlayerItemInventory newItemInventory, TMP_Text newTimerText, TMP_Text newCurrentRankText, TMP_Text newHeightText, TMP_Text newSectionText, Image newCourseProgressFill, TMP_Text newCheckpointText, TMP_Text newCourseTopText, TMP_Text newStaminaText, Image newStaminaFill, RectTransform newRankingEntryRoot, TMP_Text newRankingEntryTemplate, CanvasItemSlotView[] newItemSlotViews, GameObject newRespawnNoticePanel, GameObject newResultPanel, TMP_Text newResultTitleText, TMP_Text newResultReasonText, TMP_Text newFinalRankText, RectTransform newResultEntryRoot, TMP_Text newResultEntryTemplate) // 자동 설정 도구용 전체 HUD 참조 연결
        { // 자동 설정 도구용 전체 HUD 참조 연결 처리
            respawnController = newRespawnController; // 부활 관리자 참조 저장
            heightProgressController = newHeightProgressController; // 높이 진행 관리자 참조 저장
            movementController = newMovementController; // 이동 관리자 참조 저장
            matchController = newMatchController; // 경기 관리자 참조 저장
            itemInventory = newItemInventory; // 아이템 인벤토리 참조 저장
            timerText = newTimerText; // 남은 시간 텍스트 참조 저장
            currentRankText = newCurrentRankText; // 현재 순위 텍스트 참조 저장
            heightText = newHeightText; // 높이 텍스트 참조 저장
            sectionText = newSectionText; // 구간 텍스트 참조 저장
            courseProgressFill = newCourseProgressFill; // 코스 진행 막대 참조 저장
            checkpointText = newCheckpointText; // 체크포인트 텍스트 참조 저장
            courseTopText = newCourseTopText; // 정상 도달 텍스트 참조 저장
            staminaText = newStaminaText; // 스태미나 텍스트 참조 저장
            staminaFill = newStaminaFill; // 스태미나 막대 참조 저장
            rankingEntryRoot = newRankingEntryRoot; // 실시간 순위 항목 부모 참조 저장
            rankingEntryTemplate = newRankingEntryTemplate; // 실시간 순위 복제 원본 참조 저장
            itemSlotViews = newItemSlotViews; // 두 아이템 슬롯 표시 참조 저장
            respawnNoticePanel = newRespawnNoticePanel; // 부활 안내 패널 참조 저장
            resultPanel = newResultPanel; // 경기 결과 패널 참조 저장
            resultTitleText = newResultTitleText; // 경기 결과 제목 참조 저장
            resultReasonText = newResultReasonText; // 경기 종료 원인 참조 저장
            finalRankText = newFinalRankText; // 최종 순위 참조 저장
            resultEntryRoot = newResultEntryRoot; // 최종 순위 항목 부모 참조 저장
            resultEntryTemplate = newResultEntryTemplate; // 최종 순위 복제 원본 참조 저장
        } // 자동 설정 도구용 전체 HUD 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // Canvas 플레이어 HUD 기능 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료
