using System.Collections.Generic; // 안전 위치 기록 목록 기능 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.Player; // 플레이어 이동과 부활 상태 기능 참조
using UnityEngine; // Unity 위치와 시간 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public readonly struct PlayerRewindSample // 되감기 위치와 회전 표본 선언
    { // 되감기 표본 묶음
        public PlayerRewindSample(Vector3 position, Quaternion rotation, float recordedAt) // 되감기 표본 생성
        { // 되감기 표본 값 저장 처리
            Position = position; // 안전 위치 저장
            Rotation = rotation; // 안전 회전 저장
            RecordedAt = recordedAt; // 기록 시간 저장
        } // 되감기 표본 값 저장 처리 종료

        public Vector3 Position { get; } // 안전 위치 반환
        public Quaternion Rotation { get; } // 안전 회전 반환
        public float RecordedAt { get; } // 기록 시간 반환
    } // 되감기 표본 묶음 종료

    [DisallowMultipleComponent] // 플레이어당 되감기 기록기 한 개만 허용
    [RequireComponent(typeof(PlayerMovementController))] // 접지 상태 제공자 필수 지정
    public sealed class PlayerRewindRecorder : MonoBehaviour // 최근 안전 위치 기록기 선언
    { // 되감기 기록기 묶음
        [SerializeField, Min(1f)] private float maximumHistorySeconds = 6f; // 최대 보존 기록 시간 저장
        [SerializeField, Min(0.01f)] private float sampleInterval = P2ItemRules.RewindSampleInterval; // 안전 위치 기록 간격 저장
        [SerializeField] private PlayerMovementController movementController; // 접지 상태 제공자 저장
        [SerializeField] private PlayerRespawnController respawnController; // 부활 상태 제공자 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private PlayerP2ItemEffectController p2EffectController; // 되감기 진행 상태 제공자 저장

        private readonly List<PlayerRewindSample> samples = new List<PlayerRewindSample>(); // 시간순 안전 위치 표본 목록 저장
        private float sampleCooldownRemaining; // 다음 기록까지 남은 시간 저장

        public int SampleCount => samples.Count; // 현재 기록 표본 수 반환

        private void Awake() // 실행 시작 시 기록 참조 준비
        { // 기록 참조 준비 처리
            ResolveReferences(); // 같은 플레이어와 Scene 기반 참조 자동 연결
        } // 기록 참조 준비 처리 종료

        private void Update() // 안전 위치 기록 시간 갱신
        { // 안전 위치 기록 프레임 처리
            if (matchController != null && matchController.IsMatchFinished) // 경기 종료 여부 확인
            { // 경기 종료 기록 처리
                samples.Clear(); // 종료 후 오래된 이동 기록 제거
                return; // 안전 위치 기록 종료
            } // 경기 종료 기록 처리 종료

            if (respawnController != null && respawnController.IsRespawning) // 부활 진행 여부 확인
            { // 부활 중 기록 처리
                samples.Clear(); // 사망 전 이동 기록 제거
                sampleCooldownRemaining = 0f; // 다음 기록 대기 시간 초기화
                return; // 안전 위치 기록 종료
            } // 부활 중 기록 처리 종료

            if (p2EffectController != null && p2EffectController.IsRewinding) // 현재 되감기 진행 여부 확인
            { // 되감기 중 기록 처리
                return; // 되감기 위치 재기록 방지
            } // 되감기 중 기록 처리 종료

            sampleCooldownRemaining = Mathf.Max(0f, sampleCooldownRemaining - Time.deltaTime); // 다음 기록 대기 시간 감소

            if (sampleCooldownRemaining > 0f || movementController == null || !movementController.IsGrounded) // 기록 간격과 안전 접지 상태 확인
            { // 안전 위치 기록 대기 처리
                return; // 현재 프레임 기록 생략
            } // 안전 위치 기록 대기 처리 종료

            samples.Add(new PlayerRewindSample(transform.position, transform.rotation, Time.time)); // 현재 접지 위치와 회전과 시간 기록
            sampleCooldownRemaining = Mathf.Max(0.01f, sampleInterval); // 다음 표본 기록 간격 설정
            RemoveExpiredSamples(); // 최대 보존 시간을 벗어난 표본 제거
        } // 안전 위치 기록 프레임 처리 종료

        public bool TryBuildRewindPath(float historySeconds, List<PlayerRewindSample> destination) // 최근 기록을 최신부터 과거 순서로 복사
        { // 되감기 경로 생성 처리
            if (destination == null) // 결과 목록 누락 여부 확인
            { // 결과 목록 누락 처리
                return false; // 되감기 경로 생성 실패 반환
            } // 결과 목록 누락 처리 종료

            destination.Clear(); // 이전 결과 표본 제거
            float minimumRecordedAt = Time.time - Mathf.Max(0.1f, historySeconds); // 사용할 가장 오래된 기록 시간 계산

            for (int index = samples.Count - 1; index >= 0; index--) // 최신 표본부터 역순 순회
            { // 현재 되감기 표본 확인
                PlayerRewindSample sample = samples[index]; // 현재 기록 표본 조회

                if (sample.RecordedAt < minimumRecordedAt && destination.Count > 0) // 요청 시간보다 오래된 표본 도달 여부 확인
                { // 요청 기록 범위 종료 처리
                    break; // 되감기 경로 수집 종료
                } // 요청 기록 범위 종료 처리 종료

                destination.Add(sample); // 최신부터 과거 순서로 표본 추가
            } // 현재 되감기 표본 확인 종료

            return destination.Count >= 2; // 이동 가능한 두 개 이상 표본 존재 여부 반환
        } // 되감기 경로 생성 처리 종료

        public void ClearHistory() // 부활과 외부 초기화용 이동 기록 제거
        { // 이동 기록 제거 처리
            samples.Clear(); // 모든 안전 위치 표본 제거
            sampleCooldownRemaining = 0f; // 다음 기록 대기 시간 초기화
        } // 이동 기록 제거 처리 종료

        private void RemoveExpiredSamples() // 최대 보존 시간을 벗어난 오래된 표본 제거
        { // 오래된 표본 제거 처리
            float minimumRecordedAt = Time.time - Mathf.Max(1f, maximumHistorySeconds); // 보존할 가장 오래된 시간 계산

            while (samples.Count > 0 && samples[0].RecordedAt < minimumRecordedAt) // 목록 앞쪽 만료 표본 존재 여부 확인
            { // 만료 표본 제거 처리
                samples.RemoveAt(0); // 가장 오래된 표본 제거
            } // 만료 표본 제거 처리 종료
        } // 오래된 표본 제거 처리 종료

        private void ResolveReferences() // 플레이어와 Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            movementController = movementController == null ? GetComponent<PlayerMovementController>() : movementController; // 같은 오브젝트 이동 관리자 저장
            respawnController = respawnController == null ? GetComponent<PlayerRespawnController>() : respawnController; // 같은 오브젝트 부활 관리자 저장
            p2EffectController = p2EffectController == null ? GetComponent<PlayerP2ItemEffectController>() : p2EffectController; // 같은 오브젝트 P2 효과 관리자 저장
            matchController = matchController == null ? FindFirstObjectByType<PrototypeMatchController>() : matchController; // 현재 Scene 경기 관리자 저장
        } // 누락 참조 자동 연결 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerMovementController newMovementController, PlayerRespawnController newRespawnController, PrototypeMatchController newMatchController, PlayerP2ItemEffectController newP2EffectController) // 자동 설정 도구용 기록 참조 연결
        { // Editor 기록 참조 연결 처리
            movementController = newMovementController; // 이동 관리자 저장
            respawnController = newRespawnController; // 부활 관리자 저장
            matchController = newMatchController; // 경기 관리자 저장
            p2EffectController = newP2EffectController; // P2 효과 관리자 저장
            maximumHistorySeconds = 6f; // 5초 되감기보다 긴 안전 기록 보존
            sampleInterval = P2ItemRules.RewindSampleInterval; // 공통 안전 위치 기록 간격 적용
        } // Editor 기록 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 되감기 기록기 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
