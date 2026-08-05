using System.Collections; // 코루틴 기능 참조
using UnityEngine; // Unity 기본 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    [DisallowMultipleComponent] // 부활 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 컴포넌트 보장
    [RequireComponent(typeof(PlayerMovementController))] // 플레이어 이동 컴포넌트 보장
    [RequireComponent(typeof(PlayerStateController))] // 플레이어 상태 컴포넌트 보장
    public sealed class PlayerRespawnController : MonoBehaviour // 높이와 부활 관리 컴포넌트
    { // 클래스 범위 시작
        [SerializeField] private Transform heightOrigin; // 높이 측정 기준점
        [SerializeField] private float fallLimitY = -5f; // 추락 판정 월드 높이
        [SerializeField, Min(0f)] private float respawnDelay = 0.75f; // 부활 대기 시간
        [SerializeField, Min(0f)] private float respawnVerticalOffset = 0.05f; // 부활 지점 수직 보정값

        private CharacterController characterController; // 캐릭터 충돌 제어기
        private PlayerMovementController movementController; // 플레이어 이동 제어기
        private PlayerStateController stateController; // 플레이어 상태 관리자
        private Vector3 respawnPosition; // 현재 부활 위치
        private Quaternion respawnRotation; // 현재 부활 회전
        private float heightOriginY; // 높이 기준 Y 좌표
        private Coroutine respawnRoutine; // 진행 중인 부활 코루틴

        public float CurrentHeight => Mathf.Max(0f, transform.position.y - heightOriginY); // 현재 높이 반환
        public float HighestHeight { get; private set; } // 최고 높이 반환
        public string CurrentCheckpointId { get; private set; } = "START"; // 현재 체크포인트 식별자
        public bool IsRespawning => stateController != null && stateController.IsRespawning; // 부활 진행 상태
        public int RespawnCount { get; private set; } // 누적 부활 횟수

        private void Awake() // 필수 참조와 최초 부활 지점 준비
        { // 메서드 범위 시작
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            movementController = GetComponent<PlayerMovementController>(); // 이동 컴포넌트 조회
            stateController = GetComponent<PlayerStateController>(); // 상태 컴포넌트 조회
            respawnPosition = transform.position; // 최초 위치 저장
            respawnRotation = transform.rotation; // 최초 회전 저장
            heightOriginY = heightOrigin != null ? heightOrigin.position.y : transform.position.y; // 높이 기준값 저장
            HighestHeight = CurrentHeight; // 최초 최고 높이 저장
        } // 메서드 범위 종료

        private void Update() // 높이와 추락 상태 갱신
        { // 메서드 범위 시작
            if (stateController.IsMatchFinished) // 경기 종료 상태 확인
            { // 조건 범위 시작
                return; // 높이와 부활 갱신 생략
            } // 조건 범위 종료

            if (!stateController.IsRespawning) // 정상 플레이 상태 확인
            { // 조건 범위 시작
                HighestHeight = Mathf.Max(HighestHeight, CurrentHeight); // 최고 높이 갱신
            } // 조건 범위 종료

            if (stateController.CanMove && transform.position.y <= fallLimitY) // 추락 기준 통과 확인
            { // 조건 범위 시작
                BeginRespawn(); // 부활 처리 시작
            } // 조건 범위 종료
        } // 메서드 범위 종료

        public void ActivateCheckpoint(string checkpointId, Transform respawnPoint) // 새 체크포인트 활성화
        { // 메서드 범위 시작
            if (string.IsNullOrWhiteSpace(checkpointId) || respawnPoint == null) // 체크포인트 정보 유효성 확인
            { // 조건 범위 시작
                Debug.LogWarning("[ProjectJ][Gameplay][CHECKPOINT_INVALID] 체크포인트 ID 또는 부활 지점을 확인합니다.", this); // 잘못된 체크포인트 경고
                return; // 체크포인트 변경 중단
            } // 조건 범위 종료

            CurrentCheckpointId = checkpointId; // 체크포인트 ID 저장
            respawnPosition = respawnPoint.position + Vector3.up * respawnVerticalOffset; // 부활 위치 저장
            respawnRotation = respawnPoint.rotation; // 부활 회전 저장
        } // 메서드 범위 종료

        public void BeginRespawn() // 외부 호출용 부활 시작
        { // 메서드 범위 시작
            if (respawnRoutine != null || !stateController.TryBeginRespawn()) // 중복 요청과 상태 전환 가능 여부 확인
            { // 조건 범위 시작
                return; // 부활 처리 생략
            } // 조건 범위 종료

            respawnRoutine = StartCoroutine(RespawnRoutine()); // 부활 코루틴 실행
        } // 메서드 범위 종료

        public void StopRespawnForMatchEnd() // 경기 종료용 부활 중단
        { // 메서드 범위 시작
            stateController.FinishMatch(); // 경기 종료 상태 우선 적용

            if (respawnRoutine != null) // 진행 중인 부활 코루틴 확인
            { // 조건 범위 시작
                StopCoroutine(respawnRoutine); // 부활 코루틴 즉시 중단
                respawnRoutine = null; // 코루틴 참조 초기화
            } // 조건 범위 종료

            if (characterController != null && !characterController.enabled) // 비활성화된 충돌체 확인
            { // 조건 범위 시작
                characterController.enabled = true; // 캐릭터 충돌체 복구
            } // 조건 범위 종료
        } // 메서드 범위 종료

        private IEnumerator RespawnRoutine() // 상태 차단과 위치 복귀 처리
        { // 메서드 범위 시작
            characterController.enabled = false; // 캐릭터 충돌 비활성화

            if (respawnDelay > 0f) // 부활 대기 시간 확인
            { // 조건 범위 시작
                yield return new WaitForSeconds(respawnDelay); // 부활 대기
            } // 조건 범위 종료

            if (stateController.IsMatchFinished) // 대기 중 경기 종료 확인
            { // 조건 범위 시작
                characterController.enabled = true; // 캐릭터 충돌체 복구
                respawnRoutine = null; // 코루틴 참조 초기화
                yield break; // 부활 처리 종료
            } // 조건 범위 종료

            transform.SetPositionAndRotation(respawnPosition, respawnRotation); // 체크포인트 위치와 회전 적용
            characterController.enabled = true; // 캐릭터 충돌 활성화
            movementController.ResetAfterRespawn(); // 이동과 외부 힘 상태 초기화
            RespawnCount++; // 부활 횟수 증가
            stateController.CompleteRespawn(); // 정상 플레이 상태 복구
            respawnRoutine = null; // 코루틴 참조 초기화
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
