using System.Collections.Generic; // 목록 기능 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.InputSystem; // Input System 사용

namespace ProjectJ.Interaction // 상호작용 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 컴포넌트 방지
    [RequireComponent(typeof(PlayerInput))] // PlayerInput 필수 지정
    public sealed class PlayerInteractionController : MonoBehaviour // 플레이어 상호작용 제어
    {
        private const string InteractActionName = "Interact"; // 상호작용 Action 이름
        private const int OverlapBufferSize = 64; // 주변 Collider 최대 검색 수

        [SerializeField] // 인스펙터 직렬화
        private Transform interactionOrigin; // 상호작용 탐색 기준 위치

        [SerializeField] // 인스펙터 직렬화
        [Min(0.1f)] // 최소 거리 제한
        private float interactionRange = 3f; // 기본 상호작용 거리

        [SerializeField] // 인스펙터 직렬화
        private LayerMask interactionLayers = ~0; // 탐색 대상 Layer

        [SerializeField] // 인스펙터 직렬화
        private bool drawInteractionRange = true; // Scene 범위 표시 여부

        private readonly Collider[] overlapBuffer = new Collider[OverlapBufferSize]; // Collider 검색 버퍼
        private readonly List<IInteractable> candidates = new List<IInteractable>(OverlapBufferSize); // 상호작용 후보 목록
        private readonly HashSet<int> candidateInstanceIds = new HashSet<int>(); // 중복 후보 방지 목록
        private PlayerInput playerInput; // 플레이어 입력 컴포넌트
        private InputAction interactAction; // 상호작용 입력 Action

        public IInteractable CurrentTarget { get; private set; } // 현재 선택된 Target

        public bool HasTarget // Target 존재 여부
        {
            get
            {
                Component targetComponent = CurrentTarget as Component; // Target 컴포넌트 변환
                return targetComponent != null; // 유효 Target 여부 반환
            }
        }

        public float InteractionRange // 현재 상호작용 거리
        {
            get
            {
                return interactionRange; // 거리 값 반환
            }
        }

        private void Awake() // 초기 컴포넌트 준비
        {
            playerInput = GetComponent<PlayerInput>(); // PlayerInput 저장
        }

        private void OnEnable() // 입력 이벤트 연결
        {
            if (playerInput == null) // PlayerInput 초기화 검사
            {
                playerInput = GetComponent<PlayerInput>(); // PlayerInput 다시 탐색
            }

            if (playerInput == null || playerInput.actions == null) // 입력 에셋 누락 검사
            {
                Debug.LogError("PlayerInteractionController: PlayerInput 또는 InputActionAsset이 없습니다.", this); // 입력 누락 오류 출력
                return; // 이벤트 연결 중단
            }

            interactAction = playerInput.actions.FindAction(InteractActionName, false); // Interact Action 탐색

            if (interactAction == null) // Interact Action 누락 검사
            {
                Debug.LogError("PlayerInteractionController: Interact Action을 찾을 수 없습니다.", this); // Action 누락 오류 출력
                return; // 이벤트 연결 중단
            }

            interactAction.performed += OnInteractPerformed; // 상호작용 입력 이벤트 등록
        }

        private void OnDisable() // 입력 이벤트 해제
        {
            if (interactAction != null) // Action 존재 검사
            {
                interactAction.performed -= OnInteractPerformed; // 상호작용 입력 이벤트 해제
            }

            interactAction = null; // Action 참조 초기화
            CurrentTarget = null; // 현재 Target 초기화
        }

        private void Update() // 매 프레임 Target 갱신
        {
            RefreshTarget(); // 최근접 유효 Target 검색
        }

        public void Configure(Transform newInteractionOrigin, float newRange, LayerMask newLayers) // Editor 초기 설정
        {
            interactionOrigin = newInteractionOrigin; // 탐색 기준 위치 저장
            interactionRange = Mathf.Max(0.1f, newRange); // 상호작용 거리 저장
            interactionLayers = newLayers; // 탐색 Layer 저장
        }

        public void RefreshTarget() // 현재 Target 다시 검색
        {
            Vector3 originPosition = GetOriginPosition(); // 탐색 기준 위치 계산
            candidates.Clear(); // 이전 후보 목록 초기화
            candidateInstanceIds.Clear(); // 이전 중복 검사 초기화

            int hitCount = Physics.OverlapSphereNonAlloc( // 주변 Collider 검색
                originPosition, // 탐색 중심 위치
                interactionRange, // 탐색 반경
                overlapBuffer, // 결과 저장 버퍼
                interactionLayers, // 탐색 Layer
                QueryTriggerInteraction.Collide // Trigger 대상 포함
            );

            for (int i = 0; i < hitCount; i++) // 검색된 Collider 반복
            {
                Collider currentCollider = overlapBuffer[i]; // 현재 Collider 저장
                overlapBuffer[i] = null; // 사용한 버퍼 위치 초기화

                if (currentCollider == null) // Collider 유효성 검사
                {
                    continue; // 다음 Collider 진행
                }

                IInteractable interactable = FindInteractable(currentCollider); // 상호작용 대상 탐색
                Component interactableComponent = interactable as Component; // 대상 컴포넌트 변환

                if (interactable == null || interactableComponent == null) // 대상 존재 검사
                {
                    continue; // 비상호작용 Collider 제외
                }

                if (interactableComponent.transform.IsChildOf(transform)) // 자기 자신 하위 대상 검사
                {
                    continue; // 자기 자신 대상 제외
                }

                int instanceId = interactableComponent.GetInstanceID(); // 대상 고유 ID 저장

                if (!candidateInstanceIds.Add(instanceId)) // 중복 대상 검사
                {
                    continue; // 중복 후보 제외
                }

                candidates.Add(interactable); // 유효 후보 추가
            }

            CurrentTarget = InteractionTargetRules.SelectNearest( // 최근접 Target 결정
                gameObject, // 상호작용 실행자
                originPosition, // 탐색 기준 위치
                interactionRange, // 최대 상호작용 거리
                candidates // 유효 후보 목록
            );
        }

        public bool TryInteract() // 현재 Target 상호작용 시도
        {
            RefreshTarget(); // 실행 직전 Target 재검증

            Component targetComponent = CurrentTarget as Component; // 현재 Target 컴포넌트 변환

            if (CurrentTarget == null || targetComponent == null) // Target 존재 검사
            {
                return false; // 상호작용 실패 반환
            }

            Vector3 originPosition = GetOriginPosition(); // 현재 기준 위치 계산

            if (!InteractionTargetRules.IsWithinRange(originPosition, CurrentTarget, interactionRange)) // 거리 재검증
            {
                CurrentTarget = null; // 범위 이탈 Target 해제
                return false; // 상호작용 실패 반환
            }

            if (!CurrentTarget.CanInteract(gameObject)) // 사용 가능 상태 재검증
            {
                CurrentTarget = null; // 사용 불가 Target 해제
                return false; // 상호작용 실패 반환
            }

            CurrentTarget.Interact(gameObject); // Target 상호작용 실행
            return true; // 상호작용 성공 반환
        }

        private Vector3 GetOriginPosition() // 탐색 기준 위치 계산
        {
            if (interactionOrigin != null) // 전용 기준 Transform 검사
            {
                return interactionOrigin.position; // 전용 기준 위치 반환
            }

            return transform.position + Vector3.up; // 기본 몸통 높이 위치 반환
        }

        private static IInteractable FindInteractable(Collider sourceCollider) // Collider에서 상호작용 대상 찾기
        {
            MonoBehaviour[] behaviours = sourceCollider.GetComponentsInParent<MonoBehaviour>(true); // 부모 Behaviour 수집

            for (int i = 0; i < behaviours.Length; i++) // 부모 Behaviour 반복
            {
                MonoBehaviour behaviour = behaviours[i]; // 현재 Behaviour 저장

                if (behaviour == null) // Behaviour 유효성 검사
                {
                    continue; // 다음 Behaviour 진행
                }

                IInteractable interactable = behaviour as IInteractable; // 인터페이스 변환

                if (interactable != null) // 상호작용 대상 확인
                {
                    return interactable; // 첫 상호작용 대상 반환
                }
            }

            return null; // 상호작용 대상 없음 반환
        }

        private void OnInteractPerformed(InputAction.CallbackContext context) // Interact 입력 처리
        {
            TryInteract(); // 현재 Target 상호작용 시도
        }

        private void OnValidate() // 인스펙터 값 보정
        {
            interactionRange = Mathf.Max(0.1f, interactionRange); // 최소 상호작용 거리 보정
        }

        private void OnDrawGizmosSelected() // Scene 탐색 범위 표시
        {
            if (!drawInteractionRange) // 표시 여부 검사
            {
                return; // Gizmo 표시 중단
            }

            Color previousColor = Gizmos.color; // 기존 Gizmo 색상 저장
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 1f); // 탐색 범위 색상 지정
            Gizmos.DrawWireSphere(GetOriginPosition(), interactionRange); // 상호작용 범위 선 표시
            Gizmos.color = previousColor; // 기존 Gizmo 색상 복원
        }
    }
}
