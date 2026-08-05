using ProjectJ.Player; // 플레이어 진행과 입력 기능 참조
using UnityEngine; // Unity 기본 기능 참조

namespace ProjectJ.Gameplay // 경기 기능 네임스페이스
{
    public readonly struct PrototypeRankEntry // 프로토타입 순위 한 줄 데이터
    {
        public PrototypeRankEntry(string displayName, float height, float tieBreakTime, bool isLocalPlayer, int stableOrder) // 순위 데이터 생성
        {
            DisplayName = displayName; // 참가자 표시 이름 저장
            Height = height; // 비교 높이 저장
            TieBreakTime = tieBreakTime; // 동점 비교 시간 저장
            IsLocalPlayer = isLocalPlayer; // 로컬 플레이어 여부 저장
            StableOrder = stableOrder; // 최종 고정 순서 저장
        }

        public string DisplayName { get; } // 참가자 표시 이름
        public float Height { get; } // 현재 또는 최고 높이
        public float TieBreakTime { get; } // 동일 높이 도달 시간
        public bool IsLocalPlayer { get; } // 로컬 플레이어 여부
        public int StableOrder { get; } // 완전 동점 고정 순서
    }

    [DisallowMultipleComponent] // 경기 관리자 중복 방지
    public sealed class PrototypeMatchController : MonoBehaviour // 14일차 단일 플레이 프로토타입 경기 관리자
    {
        private const float HeightComparisonTolerance = 0.001f; // 높이 동점 비교 오차

        [SerializeField, Min(1f)] private float matchDurationSeconds = 600f; // 경기 제한 시간
        [SerializeField] private string playerDisplayName = "PLAYER"; // 플레이어 순위표 이름
        [SerializeField] private PlayerInputReader playerInputReader; // 경기 종료 시 차단할 입력
        [SerializeField] private PlayerMovementController playerMovementController; // 경기 종료 시 차단할 이동
        [SerializeField] private PlayerRespawnController playerRespawnController; // 플레이어 높이 정보
        [SerializeField] private PlayerPushController playerPushController; // 경기 종료 시 차단할 밀치기
        [SerializeField] private PushableDummy[] dummyOpponents; // 순위에 포함할 더미 목록

        private PrototypeRankEntry[] currentRanking; // 실시간 현재 높이 순위
        private PrototypeRankEntry[] finalRanking; // 경기 종료 최고 높이 순위
        private float elapsedMatchTime; // 경기 경과 시간
        private float observedPlayerHighestHeight; // 관찰된 플레이어 최고 높이
        private float playerHighestHeightReachedAt; // 플레이어 최고 높이 도달 시간

        public float RemainingTime { get; private set; } // 남은 경기 시간
        public float ElapsedMatchTime => elapsedMatchTime; // 경기 경과 시간 반환
        public bool IsMatchFinished { get; private set; } // 경기 종료 상태
        public int ParticipantCount => currentRanking == null ? 0 : currentRanking.Length; // 현재 참가자 수
        public int PlayerRank { get; private set; } = 1; // 플레이어 현재 순위
        public int FinalPlayerRank { get; private set; } = 1; // 플레이어 최종 순위

        private void Awake() // 경기 필수 참조 검증
        {
            if (playerInputReader == null || playerMovementController == null || playerRespawnController == null || playerPushController == null) // 플레이어 참조 누락 확인
            {
                Debug.LogError("[ProjectJ][Gameplay][PROTOTYPE_MATCH_REFERENCE_MISSING] 입력, 이동, 부활, 밀치기 참조를 확인합니다.", this); // 필수 참조 누락 오류
                enabled = false; // 경기 관리자 비활성화
                return; // 경기 준비 중단
            }

            int dummyCount = dummyOpponents == null ? 0 : dummyOpponents.Length; // 등록된 더미 수 계산
            currentRanking = new PrototypeRankEntry[dummyCount + 1]; // 현재 순위 배열 생성
            finalRanking = new PrototypeRankEntry[dummyCount + 1]; // 최종 순위 배열 생성
        }

        private void Start() // 경기 타이머와 순위 시작
        {
            RemainingTime = matchDurationSeconds; // 전체 경기 시간 적용
            elapsedMatchTime = 0f; // 경기 경과 시간 초기화
            observedPlayerHighestHeight = playerRespawnController.HighestHeight; // 플레이어 최초 최고 높이 저장
            playerHighestHeightReachedAt = 0f; // 플레이어 최초 도달 시간 저장
            BuildRanking(false, currentRanking); // 최초 현재 순위 생성
            PlayerRank = FindLocalPlayerRank(currentRanking); // 최초 플레이어 순위 저장
        }

        private void Update() // 경기 시간과 순위 갱신
        {
            if (IsMatchFinished) // 경기 종료 상태 확인
            {
                return; // 경기 갱신 생략
            }

            float deltaTime = Time.deltaTime; // 현재 프레임 시간
            elapsedMatchTime += deltaTime; // 경기 경과 시간 증가
            RemainingTime = Mathf.Max(0f, matchDurationSeconds - elapsedMatchTime); // 남은 경기 시간 계산
            UpdatePlayerHighestReachedTime(); // 플레이어 최고 높이 도달 시간 갱신
            BuildRanking(false, currentRanking); // 현재 높이 순위 갱신
            PlayerRank = FindLocalPlayerRank(currentRanking); // 플레이어 현재 순위 갱신

            if (RemainingTime <= 0f) // 경기 시간 종료 확인
            {
                FinishMatch(); // 최종 결과 확정
            }
        }

        public PrototypeRankEntry GetCurrentRankEntry(int index) // 현재 순위 항목 반환
        {
            if (currentRanking == null || index < 0 || index >= currentRanking.Length) // 현재 순위 인덱스 범위 확인
            {
                return default; // 빈 순위 항목 반환
            }

            return currentRanking[index]; // 현재 순위 항목 반환
        }

        public PrototypeRankEntry GetFinalRankEntry(int index) // 최종 순위 항목 반환
        {
            if (finalRanking == null || index < 0 || index >= finalRanking.Length) // 최종 순위 인덱스 범위 확인
            {
                return default; // 빈 순위 항목 반환
            }

            return finalRanking[index]; // 최종 순위 항목 반환
        }

        private void UpdatePlayerHighestReachedTime() // 플레이어 최고 높이 도달 시간 기록
        {
            float currentHighestHeight = playerRespawnController.HighestHeight; // 현재 저장된 플레이어 최고 높이 조회

            if (currentHighestHeight <= observedPlayerHighestHeight + HeightComparisonTolerance) // 새 최고 높이 여부 확인
            {
                return; // 최고 높이 시간 갱신 생략
            }

            observedPlayerHighestHeight = currentHighestHeight; // 관찰된 최고 높이 갱신
            playerHighestHeightReachedAt = Time.timeSinceLevelLoad; // 최고 높이 도달 시간 저장
        }

        private void FinishMatch() // 경기 종료와 최종 순위 확정
        {
            IsMatchFinished = true; // 경기 종료 상태 적용
            RemainingTime = 0f; // 남은 시간 제거
            BuildRanking(true, finalRanking); // 최고 높이 기준 최종 순위 생성
            FinalPlayerRank = FindLocalPlayerRank(finalRanking); // 플레이어 최종 순위 저장
            playerRespawnController.StopRespawnForMatchEnd(); // 진행 중인 부활 중단
            playerInputReader.enabled = false; // 플레이어 입력 차단
            playerMovementController.enabled = false; // 플레이어 이동 차단
            playerPushController.enabled = false; // 플레이어 밀치기 차단
        }

        private void BuildRanking(bool useHighestHeight, PrototypeRankEntry[] destination) // 현재 또는 최고 높이 순위 생성
        {
            float playerHeight = useHighestHeight ? playerRespawnController.HighestHeight : playerRespawnController.CurrentHeight; // 플레이어 비교 높이 선택
            destination[0] = new PrototypeRankEntry(playerDisplayName, playerHeight, playerHighestHeightReachedAt, true, 0); // 플레이어 순위 데이터 생성

            for (int index = 1; index < destination.Length; index++) // 더미 순위 데이터 순회
            {
                PushableDummy dummy = dummyOpponents[index - 1]; // 현재 더미 조회

                if (dummy == null) // 더미 참조 누락 확인
                {
                    destination[index] = new PrototypeRankEntry($"DUMMY {index}", 0f, float.PositiveInfinity, false, index); // 빈 더미 순위 데이터 생성
                    continue; // 다음 더미 처리
                }

                float dummyHeight = useHighestHeight ? dummy.HighestHeight : dummy.CurrentHeight; // 더미 비교 높이 선택
                destination[index] = new PrototypeRankEntry(dummy.CompetitorName, dummyHeight, dummy.HighestHeightReachedAt, false, index); // 더미 순위 데이터 생성
            }

            SortRanking(destination); // 높이와 도달 시간 기준 정렬
        }

        private void SortRanking(PrototypeRankEntry[] ranking) // 순위 배열 삽입 정렬
        {
            for (int index = 1; index < ranking.Length; index++) // 두 번째 항목부터 순회
            {
                PrototypeRankEntry currentEntry = ranking[index]; // 현재 삽입할 항목 저장
                int previousIndex = index - 1; // 앞쪽 비교 인덱스 준비

                while (previousIndex >= 0 && ShouldComeBefore(currentEntry, ranking[previousIndex])) // 앞 항목보다 높은 순위 여부 확인
                {
                    ranking[previousIndex + 1] = ranking[previousIndex]; // 앞 항목을 한 칸 뒤로 이동
                    previousIndex--; // 다음 앞 항목으로 이동
                }

                ranking[previousIndex + 1] = currentEntry; // 현재 항목을 정렬 위치에 삽입
            }
        }

        private bool ShouldComeBefore(PrototypeRankEntry left, PrototypeRankEntry right) // 두 순위 항목 우선순위 비교
        {
            float heightDifference = left.Height - right.Height; // 두 높이 차이 계산

            if (Mathf.Abs(heightDifference) > HeightComparisonTolerance) // 높이 차이 존재 확인
            {
                return heightDifference > 0f; // 더 높은 항목 우선 반환
            }

            if (!Mathf.Approximately(left.TieBreakTime, right.TieBreakTime)) // 도달 시간 차이 확인
            {
                return left.TieBreakTime < right.TieBreakTime; // 먼저 도달한 항목 우선 반환
            }

            return left.StableOrder < right.StableOrder; // 완전 동점 고정 순서 반환
        }

        private int FindLocalPlayerRank(PrototypeRankEntry[] ranking) // 로컬 플레이어 순위 검색
        {
            for (int index = 0; index < ranking.Length; index++) // 전체 순위 항목 순회
            {
                if (ranking[index].IsLocalPlayer) // 로컬 플레이어 항목 확인
                {
                    return index + 1; // 1부터 시작하는 순위 반환
                }
            }

            return 1; // 안전 기본 순위 반환
        }
    }
}
