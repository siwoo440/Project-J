using System.Collections; // 코루틴 기능 참조
using UnityEngine; // Unity 기본 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{
    [DisallowMultipleComponent] // 부활 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader), typeof(PlayerMovementController))] // 필수 컴포넌트 보장
    public sealed class PlayerRespawnController : MonoBehaviour // 높이와 부활 관리 컴포넌트
    {
        [SerializeField] private Transform heightOrigin; // 높이 측정 기준점
        [SerializeField] private float fallLimitY = -5f; // 추락 판정 월드 높이
        [SerializeField, Min(0f)] private float respawnDelay = 0.75f; // 부활 대기 시간
        [SerializeField, Min(0f)] private float respawnVerticalOffset = 0.05f; // 부활 지점 수직 보정값

        private CharacterController characterController; // 캐릭터 충돌 제어기
        private PlayerInputReader inputReader; // 플레이어 입력 제공자
        private PlayerMovementController movementController; // 플레이어 이동 제어기
        private Vector3 respawnPosition; // 현재 부활 위치
        private Quaternion respawnRotation; // 현재 부활 회전
        private float heightOriginY; // 높이 기준 Y 좌표
        private Coroutine respawnRoutine; // 진행 중인 부활 코루틴

        public float CurrentHeight => Mathf.Max(0f, transform.position.y - heightOriginY); // 현재 높이 반환
        public float HighestHeight { get; private set; } // 최고 높이 반환
        public string CurrentCheckpointId { get; private set; } = "START"; // 현재 체크포인트 식별자
        public bool IsRespawning { get; private set; } // 부활 진행 상태
        public int RespawnCount { get; private set; } // 누적 부활 횟수

        private void Awake() // 필수 참조와 최초 부활 지점 준비
        {
            characterController = GetComponent<CharacterController>(); // CharacterController 조회
            inputReader = GetComponent<PlayerInputReader>(); // 입력 컴포넌트 조회
            movementController = GetComponent<PlayerMovementController>(); // 이동 컴포넌트 조회
            respawnPosition = transform.position; // 최초 위치 저장
            respawnRotation = transform.rotation; // 최초 회전 저장
            heightOriginY = heightOrigin != null ? heightOrigin.position.y : transform.position.y; // 높이 기준값 저장
            HighestHeight = CurrentHeight; // 최초 최고 높이 저장
        }

        private void Update() // 높이와 추락 상태 갱신
        {
            if (!IsRespawning) // 정상 플레이 상태 확인
            {
                HighestHeight = Mathf.Max(HighestHeight, CurrentHeight); // 최고 높이 갱신
            }

            if (!IsRespawning && transform.position.y <= fallLimitY) // 추락 기준 통과 확인
            {
                BeginRespawn(); // 부활 처리 시작
            }
        }

        public void ActivateCheckpoint(string checkpointId, Transform respawnPoint) // 새 체크포인트 활성화
        {
            if (string.IsNullOrWhiteSpace(checkpointId) || respawnPoint == null) // 체크포인트 정보 유효성 확인
            {
                Debug.LogWarning("[ProjectJ][Gameplay][CHECKPOINT_INVALID] 체크포인트 ID 또는 부활 지점을 확인합니다.", this); // 잘못된 체크포인트 경고
                return; // 체크포인트 변경 중단
            }

            CurrentCheckpointId = checkpointId; // 체크포인트 ID 저장
            respawnPosition = respawnPoint.position + Vector3.up * respawnVerticalOffset; // 부활 위치 저장
            respawnRotation = respawnPoint.rotation; // 부활 회전 저장
        }

        public void BeginRespawn() // 외부 호출용 부활 시작
        {
            if (IsRespawning || respawnRoutine != null) // 중복 부활 요청 확인
            {
                return; // 중복 처리 생략
            }

            respawnRoutine = StartCoroutine(RespawnRoutine()); // 부활 코루틴 실행
        }

        private IEnumerator RespawnRoutine() // 입력 정지와 위치 복귀 처리
        {
            IsRespawning = true; // 부활 상태 활성화
            inputReader.enabled = false; // 플레이어 입력 비활성화
            movementController.enabled = false; // 플레이어 이동 비활성화
            characterController.enabled = false; // 캐릭터 충돌 비활성화

            if (respawnDelay > 0f) // 부활 대기 시간 확인
            {
                yield return new WaitForSeconds(respawnDelay); // 부활 대기
            }

            transform.SetPositionAndRotation(respawnPosition, respawnRotation); // 체크포인트 위치와 회전 적용
            characterController.enabled = true; // 캐릭터 충돌 활성화
            movementController.ResetAfterRespawn(); // 이동과 자세 상태 초기화
            inputReader.enabled = true; // 플레이어 입력 활성화
            movementController.enabled = true; // 플레이어 이동 활성화
            RespawnCount++; // 부활 횟수 증가
            IsRespawning = false; // 부활 상태 해제
            respawnRoutine = null; // 부활 코루틴 참조 초기화
        }
    }
}