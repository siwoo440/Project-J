using System.Collections; // 코루틴 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 기본 기능 참조
using UnityEngine.Serialization; // 기존 Inspector 값 이전 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 부활 기능 묶음
    public enum PlayerRespawnReason // 플레이어 부활 원인 종류 선언
    { // 부활 원인 항목 묶음
        None, // 부활 요청 없음
        FallLimit, // 추락 한계 통과
        Manual, // 경기 메뉴 직접 부활
        ExternalRequest // 기존 외부 부활 요청
    } // 부활 원인 항목 묶음 종료

    [DisallowMultipleComponent] // 부활 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 컴포넌트 보장
    [RequireComponent(typeof(PlayerMovementController))] // 플레이어 이동 컴포넌트 보장
    [RequireComponent(typeof(PlayerStateController))] // 플레이어 상태 컴포넌트 보장
    [RequireComponent(typeof(PlayerHeightProgressController))] // 플레이어 높이 진행 컴포넌트 보장
    [RequireComponent(typeof(PlayerRespawnProtectionController))] // 부활 보호 컴포넌트 보장
    public sealed class PlayerRespawnController : MonoBehaviour // 체크포인트와 추락과 부활 관리 컴포넌트 선언
    { // 플레이어 부활 기능 묶음
        [Header("Checkpoint Progress")] // 체크포인트 진행 설정 구역 제목
        [SerializeField, Min(1)] private int checkpointCount = 4; // 전체 코스 체크포인트 개수
        [Header("Fall Detection")] // 추락 판정 설정 구역 제목
        [FormerlySerializedAs("fallLimitY")] // 기존 추락 한계 Inspector 값 보존
        [SerializeField] private float minimumWorldFallLimitY = -5f; // 시작 구간 월드 최저 추락 한계
        [SerializeField, Min(0.1f)] private float fallDistanceBelowCheckpoint = 25f; // 현재 체크포인트 아래 허용 추락 거리
        [Header("Respawn")] // 부활 설정 구역 제목
        [SerializeField, Min(0f)] private float respawnDelay = 0.75f; // 부활 화면 전환 대기 시간
        [SerializeField, Min(0f)] private float respawnVerticalOffset = 0.05f; // 부활 지점 수직 보정값

        private CharacterController characterController; // 캐릭터 충돌 제어기
        private PlayerMovementController movementController; // 플레이어 이동 제어기
        private PlayerStateController stateController; // 플레이어 상태 관리자
        private PlayerHeightProgressController heightProgressController; // 플레이어 높이 진행 관리자
        private PlayerRespawnProtectionController protectionController; // 부활 직후 보호 관리자
        private Vector3 respawnPosition; // 현재 부활 위치
        private Quaternion respawnRotation; // 현재 부활 회전
        private float checkpointFallReferenceY; // 현재 체크포인트 기준 높이
        private Coroutine respawnRoutine; // 진행 중인 부활 코루틴
        private PlayerRespawnReason pendingRespawnReason; // 처리 중인 부활 원인

        public float CurrentHeight => heightProgressController != null ? heightProgressController.CurrentHeight : 0f; // 호환용 현재 높이 반환
        public float HighestHeight => heightProgressController != null ? heightProgressController.HighestHeight : 0f; // 호환용 최고 높이 반환
        public PlayerHeightProgressController HeightProgressController => heightProgressController; // 높이 진행 관리자 반환
        public string CurrentCheckpointId { get; private set; } = "START"; // 현재 체크포인트 식별자
        public int CurrentCheckpointIndex { get; private set; } // 현재 활성화한 가장 높은 체크포인트 번호
        public int CheckpointCount => checkpointCount; // 전체 체크포인트 개수 반환
        public float CheckpointProgress01 => CheckpointProgressRules.CalculateProgress01(CurrentCheckpointIndex, checkpointCount); // 체크포인트 진행 비율 반환
        public float CurrentFallLimitY => RespawnFallLimitRules.CalculateFallLimitY(minimumWorldFallLimitY, checkpointFallReferenceY, fallDistanceBelowCheckpoint); // 현재 체크포인트 기준 추락 한계 반환
        public bool HasReachedCourseTop { get; private set; } // 정상 지점 도달 여부
        public bool IsRespawning => stateController != null && stateController.IsRespawning; // 부활 진행 상태 반환
        public bool IsRespawnProtected => protectionController != null && protectionController.IsProtected; // 부활 직후 보호 상태 반환
        public float RespawnProtectionRemaining => protectionController != null ? protectionController.ProtectionRemaining : 0f; // 남은 부활 보호 시간 반환
        public bool IsMatchFinished => stateController != null && stateController.IsMatchFinished; // 경기 종료 상태 반환
        public int RespawnCount { get; private set; } // 누적 부활 횟수
        public int ManualRespawnCount { get; private set; } // 누적 직접 부활 횟수
        public PlayerRespawnReason LastRespawnReason { get; private set; } // 마지막 완료 부활 원인

        private void Awake() // 필수 참조와 최초 부활 지점 준비
        { // 플레이어 부활 준비 처리
            checkpointCount = CheckpointProgressRules.ClampCheckpointCount(checkpointCount); // 전체 체크포인트 개수 안전 보정
            fallDistanceBelowCheckpoint = RespawnFallLimitRules.ClampFallDistance(fallDistanceBelowCheckpoint); // 체크포인트 아래 추락 거리 안전 보정
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            movementController = GetComponent<PlayerMovementController>(); // 이동 컴포넌트 조회
            stateController = GetComponent<PlayerStateController>(); // 상태 컴포넌트 조회
            heightProgressController = GetComponent<PlayerHeightProgressController>(); // 높이 진행 컴포넌트 조회
            protectionController = GetComponent<PlayerRespawnProtectionController>(); // 부활 보호 컴포넌트 조회
            respawnPosition = transform.position; // 최초 위치를 시작 부활 위치로 저장
            respawnRotation = transform.rotation; // 최초 회전을 시작 부활 회전으로 저장
            checkpointFallReferenceY = transform.position.y; // 시작 지점 높이를 최초 추락 기준으로 저장
        } // 플레이어 부활 준비 종료

        private void OnValidate() // Inspector 부활 설정값 보정
        { // Inspector 설정 보정 처리
            checkpointCount = CheckpointProgressRules.ClampCheckpointCount(checkpointCount); // 최소 한 개의 체크포인트 보장
            fallDistanceBelowCheckpoint = RespawnFallLimitRules.ClampFallDistance(fallDistanceBelowCheckpoint); // 최소 추락 거리 보장
            respawnDelay = Mathf.Max(0f, respawnDelay); // 음수가 없는 부활 대기 시간 보장
            respawnVerticalOffset = Mathf.Max(0f, respawnVerticalOffset); // 음수가 없는 부활 수직 보정값 보장
        } // Inspector 설정 보정 종료

        private void Update() // 추락 상태 갱신
        { // 추락 판정 프레임 처리
            if (stateController == null || stateController.IsMatchFinished) // 상태 누락과 경기 종료 상태 확인
            { // 추락 판정 중단 처리
                return; // 추락과 부활 갱신 생략
            } // 추락 판정 중단 처리 종료

            if (!stateController.CanMove) // 플레이어 조작 가능 상태 확인
            { // 조작 불가 처리
                return; // 중복 추락 판정 생략
            } // 조작 불가 처리 종료

            if (RespawnFallLimitRules.HasReachedFallLimit(transform.position.y, CurrentFallLimitY)) // 현재 체크포인트 기준 추락 한계 통과 확인
            { // 추락 한계 통과 처리
                TryBeginRespawn(PlayerRespawnReason.FallLimit); // 추락 원인 부활 처리 시작
            } // 추락 한계 통과 처리 종료
        } // 추락 판정 프레임 처리 종료

        public bool TryActivateCheckpoint(int checkpointIndex, string checkpointId, Transform respawnPoint) // 더 높은 새 체크포인트 활성화 시도
        { // 체크포인트 활성화 처리
            if (string.IsNullOrWhiteSpace(checkpointId) || respawnPoint == null) // 체크포인트 정보 유효성 확인
            { // 잘못된 체크포인트 정보 처리
                ProjectLog.Warning(ProjectLogCategory.Gameplay, "체크포인트 ID 또는 부활 지점을 확인합니다.", "CHECKPOINT_INVALID", this); // 복구 가능한 체크포인트 경고 출력
                return false; // 체크포인트 활성화 실패 반환
            } // 잘못된 체크포인트 정보 처리 종료

            if (!CheckpointProgressRules.IsCheckpointIndexInRange(checkpointIndex, checkpointCount)) // 체크포인트 번호 범위 확인
            { // 잘못된 체크포인트 번호 처리
                ProjectLog.Warning(ProjectLogCategory.Gameplay, "체크포인트 번호가 전체 체크포인트 범위를 벗어났습니다.", "CHECKPOINT_INDEX_INVALID", this); // 체크포인트 번호 경고 출력
                return false; // 체크포인트 활성화 실패 반환
            } // 잘못된 체크포인트 번호 처리 종료

            if (!CheckpointProgressRules.CanActivateCheckpoint(CurrentCheckpointIndex, checkpointIndex, checkpointCount)) // 현재보다 높은 체크포인트 여부 확인
            { // 기존 또는 낮은 체크포인트 처리
                return false; // 부활 지점 하향 변경 차단
            } // 기존 또는 낮은 체크포인트 처리 종료

            ApplyCheckpoint(checkpointIndex, checkpointId, respawnPoint); // 새 체크포인트 정보와 부활 위치 저장
            return true; // 체크포인트 활성화 성공 반환
        } // 체크포인트 활성화 처리 종료

        public void ActivateCheckpoint(string checkpointId, Transform respawnPoint) // 기존 호출 호환용 체크포인트 활성화
        { // 기존 체크포인트 활성화 처리
            int nextCheckpointIndex = Mathf.Clamp(CurrentCheckpointIndex + 1, 1, checkpointCount); // 다음 체크포인트 번호 계산
            TryActivateCheckpoint(nextCheckpointIndex, checkpointId, respawnPoint); // 순서가 있는 체크포인트 활성화 시도
        } // 기존 체크포인트 활성화 처리 종료

        public bool MarkCourseTopReached() // 정상 지점 최초 도달 기록
        { // 정상 지점 도달 처리
            if (HasReachedCourseTop) // 기존 정상 도달 여부 확인
            { // 정상 중복 도달 처리
                return false; // 중복 정상 도달 처리 생략
            } // 정상 중복 도달 처리 종료

            HasReachedCourseTop = true; // 정상 지점 도달 상태 저장
            return true; // 최초 정상 도달 성공 반환
        } // 정상 지점 도달 처리 종료

        public bool TryRequestManualRespawn() // 경기 메뉴 직접 부활 요청
        { // 직접 부활 요청 처리
            if (stateController == null || !stateController.CanMove || stateController.IsMatchFinished) // 직접 부활 가능 상태 확인
            { // 직접 부활 차단 처리
                return false; // 직접 부활 실패 반환
            } // 직접 부활 차단 처리 종료

            return TryBeginRespawn(PlayerRespawnReason.Manual); // 직접 부활 처리 시작 결과 반환
        } // 직접 부활 요청 처리 종료

        public void BeginRespawn() // 기존 외부 호출용 부활 시작
        { // 기존 부활 요청 처리
            TryBeginRespawn(PlayerRespawnReason.ExternalRequest); // 기존 외부 요청 원인 부활 처리 시작
        } // 기존 부활 요청 처리 종료

        public void StopRespawnForMatchEnd() // 경기 종료용 부활 중단
        { // 경기 종료 부활 정리 처리
            stateController.FinishMatch(); // 경기 종료 상태 우선 적용

            if (respawnRoutine != null) // 진행 중인 부활 코루틴 확인
            { // 부활 코루틴 중단 처리
                StopCoroutine(respawnRoutine); // 부활 코루틴 즉시 중단
                respawnRoutine = null; // 코루틴 참조 초기화
            } // 부활 코루틴 중단 처리 종료

            pendingRespawnReason = PlayerRespawnReason.None; // 처리 중인 부활 원인 초기화
            protectionController.StopProtection(); // 진행 중인 부활 보호 즉시 종료

            if (characterController != null && !characterController.enabled) // 비활성화된 충돌체 확인
            { // 충돌체 복구 처리
                characterController.enabled = true; // 캐릭터 충돌체 복구
            } // 충돌체 복구 처리 종료
        } // 경기 종료 부활 정리 종료

        private bool TryBeginRespawn(PlayerRespawnReason respawnReason) // 원인을 포함한 부활 시작 시도
        { // 부활 시작 시도 처리
            if (respawnRoutine != null || stateController == null || !stateController.TryBeginRespawn()) // 중복 요청과 상태 전환 가능 여부 확인
            { // 부활 시작 차단 처리
                return false; // 부활 시작 실패 반환
            } // 부활 시작 차단 처리 종료

            pendingRespawnReason = respawnReason; // 현재 부활 원인 저장
            protectionController.StopProtection(); // 이전 부활 보호 상태와 레이어 정리
            movementController.ResetAfterRespawn(); // 승인 즉시 이동과 낙하와 외부 힘 초기화
            respawnRoutine = StartCoroutine(RespawnRoutine()); // 부활 코루틴 실행
            return true; // 부활 시작 성공 반환
        } // 부활 시작 시도 처리 종료

        private void ApplyCheckpoint(int checkpointIndex, string checkpointId, Transform respawnPoint) // 체크포인트 진행과 부활 위치 저장
        { // 체크포인트 정보 저장 처리
            CurrentCheckpointIndex = checkpointIndex; // 가장 높은 체크포인트 번호 저장
            CurrentCheckpointId = checkpointId; // 체크포인트 ID 저장
            respawnPosition = respawnPoint.position + Vector3.up * respawnVerticalOffset; // 부활 위치 저장
            respawnRotation = respawnPoint.rotation; // 부활 회전 저장
            checkpointFallReferenceY = respawnPoint.position.y; // 체크포인트 원본 높이를 추락 기준으로 저장
        } // 체크포인트 정보 저장 처리 종료

        private IEnumerator RespawnRoutine() // 상태 차단과 체크포인트 위치 복귀 처리
        { // 부활 코루틴 처리
            characterController.enabled = false; // 부활 이동 중 캐릭터 충돌 비활성화

            if (respawnDelay > 0f) // 부활 대기 시간 확인
            { // 부활 대기 처리
                yield return new WaitForSeconds(respawnDelay); // 설정된 화면 전환 시간 대기
            } // 부활 대기 처리 종료

            if (stateController.IsMatchFinished) // 대기 중 경기 종료 확인
            { // 부활 중 경기 종료 처리
                characterController.enabled = true; // 캐릭터 충돌체 복구
                protectionController.StopProtection(); // 부활 보호 상태 정리
                pendingRespawnReason = PlayerRespawnReason.None; // 처리 중인 부활 원인 초기화
                respawnRoutine = null; // 코루틴 참조 초기화
                yield break; // 부활 처리 종료
            } // 부활 중 경기 종료 처리 종료

            transform.SetPositionAndRotation(respawnPosition, respawnRotation); // 체크포인트 위치와 회전 적용
            protectionController.BeginProtection(); // 부활 위치에서 3초 보호와 통과 레이어 적용
            characterController.enabled = true; // 보호 레이어 상태의 캐릭터 충돌 활성화
            heightProgressController.RefreshProgress(); // 부활 위치 기준 현재 높이 즉시 갱신
            RespawnCount++; // 전체 부활 횟수 증가

            if (pendingRespawnReason == PlayerRespawnReason.Manual) // 직접 부활 원인 확인
            { // 직접 부활 횟수 처리
                ManualRespawnCount++; // 직접 부활 횟수 증가
            } // 직접 부활 횟수 처리 종료

            LastRespawnReason = pendingRespawnReason; // 완료된 마지막 부활 원인 저장
            pendingRespawnReason = PlayerRespawnReason.None; // 처리 중인 부활 원인 초기화
            stateController.CompleteRespawn(); // 보호 중 이동 가능한 정상 플레이 상태 복구
            respawnRoutine = null; // 부활 코루틴 참조 초기화
        } // 부활 코루틴 처리 종료
    } // 플레이어 부활 기능 묶음 종료
} // 플레이어 기능 묶음 종료
