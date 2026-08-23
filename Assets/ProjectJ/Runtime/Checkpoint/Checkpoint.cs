using UnityEngine; // Unity 컴포넌트 사용

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent] // 체크포인트 중복 컴포넌트 방지
    [RequireComponent(typeof(Collider))] // Trigger Collider 보장
    public sealed class Checkpoint : MonoBehaviour // 체크포인트 Trigger
    {
        [SerializeField] // Inspector 체크포인트 ID
        private CheckpointId checkpointId =
            CheckpointId.CP1; // 기본 CP1

        [SerializeField] // Inspector 부활 지점
        private Transform respawnPoint; // 부활 위치 Transform

        public CheckpointId Id // 체크포인트 ID 조회
        {
            get
            {
                return checkpointId; // 현재 ID 반환
            }
        }

        public Vector3 RespawnPosition // 부활 위치 조회
        {
            get
            {
                if (respawnPoint != null) // 별도 부활 지점 확인
                {
                    return respawnPoint.position; // 별도 위치 반환
                }

                return transform.position; // 체크포인트 위치 반환
            }
        }

        public Quaternion RespawnRotation // 부활 회전 조회
        {
            get
            {
                if (respawnPoint != null) // 별도 부활 지점 확인
                {
                    return respawnPoint.rotation; // 별도 회전 반환
                }

                return transform.rotation; // 체크포인트 회전 반환
            }
        }

        private void Awake() // Trigger 물리 설정 보정
        {
            EnsureTriggerPhysics(); // Trigger 이벤트 발생 조건 보장
        }

        private void OnTriggerEnter( // 플레이어 접촉 처리
            Collider other
        )
        {
            if (other == null) // 잘못된 Collider 방지
            {
                return; // 처리 중단
            }

            MonoBehaviour[] behaviours = // 부모 계층 컴포넌트 조회
                other.GetComponentsInParent<MonoBehaviour>(
                    true
                );

            for (int index = 0; index < behaviours.Length; index++) // 네트워크 수신자 검색
            {
                if (behaviours[index] is ICheckpointReceiver receiver) // 공통 수신자 확인
                {
                    receiver.ReceiveCheckpoint( // 체크포인트 전달
                        this
                    );

                    return; // 네트워크 대상은 로컬 Tracker 처리 차단
                }
            }

            PlayerCheckpointTracker tracker = // 기존 오프라인 Tracker 조회
                other.GetComponentInParent<
                    PlayerCheckpointTracker
                >();

            if (tracker == null) // 오프라인 Tracker 존재 확인
            {
                return; // 처리 중단
            }

            tracker.ActivateCheckpoint( // 기존 오프라인 체크포인트 활성화
                this
            );
        }

        public void Configure( // 런타임 설정 적용
            CheckpointId id,
            Transform targetRespawnPoint
        )
        {
            checkpointId = id; // 체크포인트 ID 저장
            respawnPoint = targetRespawnPoint; // 부활 지점 저장
        }

        private void EnsureTriggerPhysics() // Trigger 충돌 이벤트 조건 보장
        {
            Collider trigger = // 현재 Collider 조회
                GetComponent<Collider>();

            if (trigger != null) // Collider 존재 확인
            {
                trigger.isTrigger = true; // Trigger 강제 설정
            }

            Rigidbody triggerBody = // Trigger Rigidbody 조회
                GetComponent<Rigidbody>();

            if (triggerBody == null) // Rigidbody 누락 확인
            {
                triggerBody =
                    gameObject.AddComponent<Rigidbody>(); // 런타임 Rigidbody 추가
            }

            triggerBody.isKinematic = true; // Trigger 위치를 물리로 움직이지 않음
            triggerBody.useGravity = false; // Trigger 중력 비활성화
            triggerBody.detectCollisions = true; // Trigger 충돌 감지 활성화
        }
    }
}
