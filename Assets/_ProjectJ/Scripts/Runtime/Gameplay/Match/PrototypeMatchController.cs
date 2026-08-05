using ProjectJ.Player; // 플레이어 진행과 상태 기능 참조
using UnityEngine; // Unity 기본 기능 참조

namespace ProjectJ.Gameplay // 경기 기능 네임스페이스 선언
{ // 경기 기능 묶음
    public enum PrototypeMatchEndReason // 프로토타입 경기 종료 원인 선언
    { // 경기 종료 원인 묶음
        None, // 경기 진행 중 상태
        CourseTopReached, // 정상 지점 도달 종료
        TimeExpired // 제한 시간 만료 종료
    } // 경기 종료 원인 묶음 종료

    public enum PrototypeMatchOutcome // 로컬 플레이어 승패 결과 선언
    { // 승패 결과 묶음
        None, // 경기 진행 중 상태
        Victory, // 단독 승리
        SharedVictory, // 공동 승리
        Defeat // 패배
    } // 승패 결과 묶음 종료

    public readonly struct PrototypeRankEntry // 프로토타입 순위 한 줄 데이터 선언
    { // 순위 데이터 묶음
        public PrototypeRankEntry(string displayName, float height, float reachedAt, bool isLocalPlayer, int stableOrder, bool hasReachedCourseTop, int rank) // 순위 데이터 생성
        { // 순위 데이터 생성 처리
            DisplayName = displayName; // 참가자 표시 이름 저장
            Height = height; // 비교 높이 저장
            ReachedAt = reachedAt; // 표시 높이 도달 시간 저장
            IsLocalPlayer = isLocalPlayer; // 로컬 플레이어 여부 저장
            StableOrder = stableOrder; // 완전 동점 고정 순서 저장
            HasReachedCourseTop = hasReachedCourseTop; // 정상 지점 도달 여부 저장
            Rank = rank; // 공동 순위 번호 저장
        } // 순위 데이터 생성 종료

        public string DisplayName { get; } // 참가자 표시 이름 반환
        public float Height { get; } // 현재 실제 높이 반환
        public float ReachedAt { get; } // 현재 표시 높이 도달 시간 반환
        public bool IsLocalPlayer { get; } // 로컬 플레이어 여부 반환
        public int StableOrder { get; } // 완전 동점 고정 순서 반환
        public bool HasReachedCourseTop { get; } // 정상 지점 도달 여부 반환
        public int Rank { get; } // 공동 순위 번호 반환

        public PrototypeRankEntry WithRank(int rank) // 공동 순위 번호가 적용된 복사본 생성
        { // 순위 복사 처리
            return new PrototypeRankEntry(DisplayName, Height, ReachedAt, IsLocalPlayer, StableOrder, HasReachedCourseTop, rank); // 새 순위 번호를 가진 데이터 반환
        } // 순위 복사 종료
    } // 순위 데이터 묶음 종료

    [DisallowMultipleComponent] // 경기 관리자 중복 방지
    public sealed class PrototypeMatchController : MonoBehaviour // 단일 플레이 프로토타입 경기 관리자 선언
    { // 경기 관리자 기능 묶음
        [SerializeField, Min(1f)] private float matchDurationSeconds = 600f; // 경기 제한 시간
        [SerializeField, Min(0f)] private float sharedRankHeightTolerance = MatchRankingRules.DefaultHeightTolerance; // 공동 순위 높이 허용 오차
        [SerializeField] private string playerDisplayName = "PLAYER"; // 플레이어 순위표 이름
        [SerializeField] private PlayerStateController playerStateController; // 플레이어 제어 상태 관리자
        [SerializeField] private PlayerRespawnController playerRespawnController; // 플레이어 높이와 부활 정보 제공자
        [SerializeField] private PushableDummy[] dummyOpponents; // 순위에 포함할 더미 목록

        private PrototypeRankEntry[] currentRanking; // 실시간 실제 높이 순위
        private PrototypeRankEntry[] finalRanking; // 경기 종료 시 고정 순위
        private float elapsedMatchTime; // 경기 경과 시간
        private float observedPlayerCurrentHeight; // 마지막으로 관찰한 플레이어 현재 높이
        private float playerCurrentHeightReachedAt; // 현재 표시 높이 도달 시간
        private bool playerReachedCourseTop; // 플레이어 정상 지점 도달 여부

        public float MatchDurationSeconds => matchDurationSeconds; // 전체 경기 제한 시간 반환
        public float RemainingTime { get; private set; } // 남은 경기 시간 반환
        public float ElapsedMatchTime => elapsedMatchTime; // 경기 경과 시간 반환
        public bool IsMatchFinished { get; private set; } // 경기 종료 상태 반환
        public int ParticipantCount => currentRanking == null ? 0 : currentRanking.Length; // 현재 참가자 수 반환
        public int PlayerRank { get; private set; } = 1; // 플레이어 현재 공동 순위 반환
        public int FinalPlayerRank { get; private set; } = 1; // 플레이어 최종 공동 순위 반환
        public PrototypeMatchEndReason EndReason { get; private set; } // 경기 종료 원인 반환
        public PrototypeMatchOutcome PlayerOutcome { get; private set; } // 로컬 플레이어 승패 결과 반환

        private void Awake() // 경기 필수 참조 검증
        { // 경기 참조 준비 처리
            if (playerStateController == null && playerRespawnController != null) // 상태 관리자 자동 검색 조건 확인
            { // 상태 관리자 자동 검색 처리
                playerStateController = playerRespawnController.GetComponent<PlayerStateController>(); // 플레이어 오브젝트에서 상태 관리자 조회
            } // 상태 관리자 자동 검색 종료

            if (playerStateController == null || playerRespawnController == null) // 플레이어 참조 누락 확인
            { // 플레이어 참조 누락 처리
                Debug.LogError("[ProjectJ][Gameplay][PROTOTYPE_MATCH_REFERENCE_MISSING] 플레이어 상태와 부활 참조를 확인합니다.", this); // 필수 참조 누락 오류 출력
                enabled = false; // 경기 관리자 비활성화
                return; // 경기 준비 중단
            } // 플레이어 참조 누락 처리 종료

            int dummyCount = dummyOpponents == null ? 0 : dummyOpponents.Length; // 등록된 더미 수 계산
            currentRanking = new PrototypeRankEntry[dummyCount + 1]; // 현재 순위 배열 생성
            finalRanking = new PrototypeRankEntry[dummyCount + 1]; // 최종 순위 배열 생성
        } // 경기 참조 준비 종료

        private void OnValidate() // Inspector 경기 수치 보정
        { // 경기 수치 보정 처리
            matchDurationSeconds = Mathf.Max(1f, matchDurationSeconds); // 최소 1초 경기 시간 보장
            sharedRankHeightTolerance = MatchRankingRules.ClampHeightTolerance(sharedRankHeightTolerance); // 음수가 없는 공동 순위 허용 오차 보장
        } // 경기 수치 보정 종료

        private void Start() // 경기 타이머와 순위 시작
        { // 경기 시작 처리
            playerStateController.ResetForNewMatch(); // 플레이어 정상 상태 초기화
            RemainingTime = matchDurationSeconds; // 전체 경기 시간 적용
            elapsedMatchTime = 0f; // 경기 경과 시간 초기화
            EndReason = PrototypeMatchEndReason.None; // 경기 종료 원인 초기화
            PlayerOutcome = PrototypeMatchOutcome.None; // 승패 결과 초기화
            playerReachedCourseTop = false; // 정상 도달 상태 초기화
            observedPlayerCurrentHeight = playerRespawnController.CurrentHeight; // 플레이어 최초 현재 높이 저장
            playerCurrentHeightReachedAt = Time.timeSinceLevelLoad; // 플레이어 최초 표시 높이 도달 시간 저장
            BuildRanking(currentRanking); // 최초 현재 순위 생성
            PlayerRank = FindLocalPlayerRank(currentRanking); // 최초 플레이어 순위 저장
        } // 경기 시작 처리 종료

        private void Update() // 경기 시간과 순위 갱신
        { // 경기 프레임 처리
            if (IsMatchFinished) // 경기 종료 상태 확인
            { // 경기 종료 상태 처리
                return; // 경기 갱신 생략
            } // 경기 종료 상태 처리 종료

            float deltaTime = Mathf.Max(0f, Time.unscaledDeltaTime); // 메뉴 중에도 흐르는 현재 프레임 시간 계산
            elapsedMatchTime += deltaTime; // 경기 경과 시간 증가
            RemainingTime = Mathf.Max(0f, matchDurationSeconds - elapsedMatchTime); // 남은 경기 시간 계산
            UpdatePlayerCurrentHeightReachedTime(); // 플레이어 현재 높이 도달 시간 갱신
            BuildRanking(currentRanking); // 현재 실제 높이 순위 갱신
            PlayerRank = FindLocalPlayerRank(currentRanking); // 플레이어 현재 공동 순위 갱신

            if (RemainingTime <= 0f) // 경기 시간 종료 확인
            { // 제한 시간 종료 처리
                FinishMatch(PrototypeMatchEndReason.TimeExpired); // 시간 만료 결과 확정
            } // 제한 시간 종료 처리 종료
        } // 경기 프레임 처리 종료

        public bool TryFinishByCourseTop(PlayerRespawnController reachingPlayer) // 정상 도달 기반 경기 종료 시도
        { // 정상 도달 종료 처리
            if (IsMatchFinished || reachingPlayer == null || reachingPlayer != playerRespawnController) // 종료 상태와 로컬 플레이어 여부 확인
            { // 정상 도달 종료 차단 처리
                return false; // 경기 종료 실패 반환
            } // 정상 도달 종료 차단 처리 종료

            playerReachedCourseTop = true; // 플레이어 정상 도달 상태 저장
            observedPlayerCurrentHeight = playerRespawnController.CurrentHeight; // 종료 순간 플레이어 실제 높이 저장
            playerCurrentHeightReachedAt = Time.timeSinceLevelLoad; // 정상 도달 시간 저장
            FinishMatch(PrototypeMatchEndReason.CourseTopReached); // 정상 도달 결과 확정
            return true; // 경기 종료 성공 반환
        } // 정상 도달 종료 처리 종료

        public PrototypeRankEntry GetCurrentRankEntry(int index) // 현재 순위 항목 반환
        { // 현재 순위 조회 처리
            if (currentRanking == null || index < 0 || index >= currentRanking.Length) // 현재 순위 인덱스 범위 확인
            { // 잘못된 현재 순위 조회 처리
                return default; // 빈 순위 항목 반환
            } // 잘못된 현재 순위 조회 처리 종료

            return currentRanking[index]; // 현재 순위 항목 반환
        } // 현재 순위 조회 종료

        public PrototypeRankEntry GetFinalRankEntry(int index) // 최종 순위 항목 반환
        { // 최종 순위 조회 처리
            if (finalRanking == null || index < 0 || index >= finalRanking.Length) // 최종 순위 인덱스 범위 확인
            { // 잘못된 최종 순위 조회 처리
                return default; // 빈 순위 항목 반환
            } // 잘못된 최종 순위 조회 처리 종료

            return finalRanking[index]; // 최종 순위 항목 반환
        } // 최종 순위 조회 종료

        private void UpdatePlayerCurrentHeightReachedTime() // 플레이어 현재 표시 높이 도달 시간 기록
        { // 플레이어 높이 시간 갱신 처리
            float currentHeight = playerRespawnController.CurrentHeight; // 플레이어 현재 실제 높이 조회

            if (Mathf.Abs(currentHeight - observedPlayerCurrentHeight) <= sharedRankHeightTolerance) // 표시 높이 변경 여부 확인
            { // 표시 높이 유지 처리
                return; // 도달 시간 갱신 생략
            } // 표시 높이 유지 처리 종료

            observedPlayerCurrentHeight = currentHeight; // 관찰한 현재 높이 갱신
            playerCurrentHeightReachedAt = Time.timeSinceLevelLoad; // 현재 표시 높이 도달 시간 저장
        } // 플레이어 높이 시간 갱신 종료

        private void FinishMatch(PrototypeMatchEndReason endReason) // 경기 종료와 최종 결과 확정
        { // 경기 종료 처리
            if (IsMatchFinished) // 중복 경기 종료 확인
            { // 중복 종료 처리
                return; // 결과 중복 확정 생략
            } // 중복 종료 처리 종료

            IsMatchFinished = true; // 경기 종료 상태 적용
            EndReason = endReason; // 경기 종료 원인 저장
            RemainingTime = Mathf.Max(0f, RemainingTime); // 종료 순간 남은 시간 고정
            BuildRanking(finalRanking); // 종료 순간 실제 높이 기반 최종 순위 생성
            FinalPlayerRank = FindLocalPlayerRank(finalRanking); // 플레이어 최종 공동 순위 저장
            PlayerOutcome = DeterminePlayerOutcome(finalRanking, FinalPlayerRank); // 최종 순위 기반 승패 판정
            playerRespawnController.StopRespawnForMatchEnd(); // 부활과 플레이어 입력과 이동 차단
            StopDummyOpponents(); // 경기 종료 순간 더미 이동과 밀치기 중단
        } // 경기 종료 처리 종료

        private void StopDummyOpponents() // 경기 종료 순간 더미 참가자 정지
        { // 더미 참가자 정지 처리
            if (dummyOpponents == null) // 더미 목록 존재 확인
            { // 더미 목록 없음 처리
                return; // 더미 정지 생략
            } // 더미 목록 없음 처리 종료

            for (int index = 0; index < dummyOpponents.Length; index++) // 등록된 더미 순회
            { // 더미 정지 반복 처리
                if (dummyOpponents[index] != null) // 현재 더미 존재 확인
                { // 유효 더미 정지 처리
                    dummyOpponents[index].StopForMatchEnd(); // 더미 이동과 대기 밀치기 종료
                } // 유효 더미 정지 처리 종료
            } // 더미 정지 반복 처리 종료
        } // 더미 참가자 정지 종료

        private void BuildRanking(PrototypeRankEntry[] destination) // 실제 높이 기반 순위 생성
        { // 순위 생성 처리
            if (destination == null || destination.Length == 0) // 순위 배열 유효성 확인
            { // 순위 생성 차단 처리
                return; // 순위 생성 생략
            } // 순위 생성 차단 처리 종료

            float playerHeight = playerRespawnController.CurrentHeight; // 플레이어 현재 실제 높이 조회
            destination[0] = new PrototypeRankEntry(playerDisplayName, playerHeight, playerCurrentHeightReachedAt, true, 0, playerReachedCourseTop, 0); // 플레이어 순위 데이터 생성

            for (int index = 1; index < destination.Length; index++) // 더미 순위 데이터 순회
            { // 더미 순위 데이터 처리
                PushableDummy dummy = dummyOpponents[index - 1]; // 현재 더미 조회

                if (dummy == null) // 더미 참조 누락 확인
                { // 빈 더미 처리
                    destination[index] = new PrototypeRankEntry($"DUMMY {index}", 0f, float.PositiveInfinity, false, index, false, 0); // 빈 더미 순위 데이터 생성
                    continue; // 다음 더미 처리
                } // 빈 더미 처리 종료

                destination[index] = new PrototypeRankEntry(dummy.CompetitorName, dummy.CurrentHeight, dummy.CurrentHeightReachedAt, false, index, false, 0); // 더미 실제 높이 순위 데이터 생성
            } // 더미 순위 데이터 처리 종료

            SortRanking(destination); // 실제 높이와 표시 순서 기준 정렬
            AssignCompetitionRanks(destination); // 공동 순위와 건너뛴 다음 순위 적용
        } // 순위 생성 처리 종료

        private void SortRanking(PrototypeRankEntry[] ranking) // 순위 배열 삽입 정렬
        { // 순위 정렬 처리
            for (int index = 1; index < ranking.Length; index++) // 두 번째 항목부터 순회
            { // 삽입 정렬 반복 처리
                PrototypeRankEntry currentEntry = ranking[index]; // 현재 삽입할 항목 저장
                int previousIndex = index - 1; // 앞쪽 비교 인덱스 준비

                while (previousIndex >= 0 && ShouldComeBefore(currentEntry, ranking[previousIndex])) // 앞 항목보다 먼저 표시할지 확인
                { // 앞 항목 이동 처리
                    ranking[previousIndex + 1] = ranking[previousIndex]; // 앞 항목을 한 칸 뒤로 이동
                    previousIndex--; // 다음 앞 항목으로 이동
                } // 앞 항목 이동 처리 종료

                ranking[previousIndex + 1] = currentEntry; // 현재 항목을 정렬 위치에 삽입
            } // 삽입 정렬 반복 처리 종료
        } // 순위 정렬 처리 종료

        private bool ShouldComeBefore(PrototypeRankEntry left, PrototypeRankEntry right) // 두 순위 항목 표시 우선순위 비교
        { // 표시 우선순위 비교 처리
            return MatchRankingRules.ShouldComeBefore(left.HasReachedCourseTop, left.Height, left.ReachedAt, left.StableOrder, right.HasReachedCourseTop, right.Height, right.ReachedAt, right.StableOrder, sharedRankHeightTolerance); // 공통 표시 순서 규칙 결과 반환
        } // 표시 우선순위 비교 종료

        private void AssignCompetitionRanks(PrototypeRankEntry[] ranking) // 공동 순위와 건너뛰기 규칙 적용
        { // 공동 순위 적용 처리
            for (int index = 0; index < ranking.Length; index++) // 정렬된 참가자 순회
            { // 참가자 순위 번호 처리
                int rank = 1; // 기본 첫 순위 준비

                if (index > 0) // 첫 참가자 이후 확인
                { // 이전 참가자 기반 순위 계산 처리
                    PrototypeRankEntry previousEntry = ranking[index - 1]; // 바로 앞 참가자 조회
                    PrototypeRankEntry currentEntry = ranking[index]; // 현재 참가자 조회
                    rank = MatchRankingRules.CalculateCompetitionRank(index, previousEntry.Rank, previousEntry.HasReachedCourseTop, previousEntry.Height, currentEntry.HasReachedCourseTop, currentEntry.Height, sharedRankHeightTolerance); // 공동 순위 또는 건너뛴 순위 계산
                } // 이전 참가자 기반 순위 계산 종료

                ranking[index] = ranking[index].WithRank(rank); // 계산된 공동 순위 번호 적용
            } // 참가자 순위 번호 처리 종료
        } // 공동 순위 적용 종료

        private int FindLocalPlayerRank(PrototypeRankEntry[] ranking) // 로컬 플레이어 순위 검색
        { // 로컬 순위 검색 처리
            for (int index = 0; index < ranking.Length; index++) // 전체 순위 항목 순회
            { // 로컬 참가자 확인 처리
                if (ranking[index].IsLocalPlayer) // 로컬 플레이어 항목 확인
                { // 로컬 참가자 발견 처리
                    return Mathf.Max(1, ranking[index].Rank); // 공동 순위 번호 반환
                } // 로컬 참가자 발견 처리 종료
            } // 로컬 참가자 확인 처리 종료

            return 1; // 안전 기본 순위 반환
        } // 로컬 순위 검색 종료

        private PrototypeMatchOutcome DeterminePlayerOutcome(PrototypeRankEntry[] ranking, int localRank) // 최종 공동 순위 기반 승패 판정
        { // 승패 판정 처리
            if (localRank != 1) // 로컬 플레이어 1위 여부 확인
            { // 패배 처리
                return PrototypeMatchOutcome.Defeat; // 패배 결과 반환
            } // 패배 처리 종료

            int firstPlaceCount = 0; // 공동 1위 참가자 수 준비

            for (int index = 0; index < ranking.Length; index++) // 최종 순위 항목 순회
            { // 공동 1위 집계 처리
                if (ranking[index].Rank == 1) // 1위 참가자 확인
                { // 1위 참가자 집계 처리
                    firstPlaceCount++; // 공동 1위 수 증가
                } // 1위 참가자 집계 처리 종료
            } // 공동 1위 집계 처리 종료

            return firstPlaceCount > 1 ? PrototypeMatchOutcome.SharedVictory : PrototypeMatchOutcome.Victory; // 공동 승리 또는 단독 승리 반환
        } // 승패 판정 종료
    } // 경기 관리자 기능 묶음 종료
} // 경기 기능 묶음 종료
