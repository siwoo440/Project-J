using ProjectJ.Finish; // 밀치기 대상 상태 타입
using ProjectJ.Push; // 밀치기 이벤트 타입
using UnityEngine; // Unity 런타임 기능

namespace ProjectJ.Player // Player Visual 네임스페이스
{
    [DisallowMultipleComponent] // Visual Controller 중복 방지
    public sealed class ProjectJPlayerVisualController : MonoBehaviour // Player와 Bot 공통 외형 애니메이션 제어
    {
        public const string ChefVisualName = "Character_4c4e64"; // 기본 외형 이름

        private const string IdleStateName = "Idle"; // 대기 상태 이름
        private const string RunStateName = "running"; // 달리기 상태 이름
        private const string JumpStateName = "jump"; // 점프 상태 이름
        private const string FallStateName = "fall"; // 낙하 상태 이름
        private const string CrouchIdleStateName = "Crouch Idle"; // 앉기 대기 상태 이름
        private const string CrouchMoveStateName = "Crouch Move"; // 앉기 이동 상태 이름
        private const string PushStateName = "Push"; // 밀치기 상태 이름

        private const string IdleTriggerName = "idle"; // 대기 전환 파라미터
        private const string RunTriggerName = "run"; // 달리기 전환 파라미터
        private const string JumpTriggerName = "jump"; // 점프 전환 파라미터
        private const string FallTriggerName = "fall"; // 낙하 전환 파라미터
        private const string CrouchIdleTriggerName = "crouchIdle"; // 앉기 대기 전환 파라미터
        private const string CrouchMoveTriggerName = "crouchMove"; // 앉기 이동 전환 파라미터
        private const string PushTriggerName = "push"; // 밀치기 전환 파라미터

        [SerializeField] // Visual Root 직렬화
        private Transform visualRoot; // 런타임 모델 부모

        [SerializeField] // 외형 Prefab 목록 직렬화
        private GameObject[] visualPrefabs = new GameObject[0]; // 꾸미기 후보 목록

        [SerializeField] // 기본 외형 이름 직렬화
        private string defaultVisualName = ChefVisualName; // 최초 외형 이름

        [SerializeField] // 모델 위치 직렬화
        private Vector3 modelLocalPosition = new Vector3(0f, -1f, 0f); // 모델 위치 보정값

        [SerializeField] // 모델 회전 직렬화
        private Vector3 modelLocalEulerAngles = Vector3.zero; // 모델 방향 보정값

        [SerializeField] // 모델 크기 직렬화
        private Vector3 modelLocalScale = Vector3.one; // 모델 크기 보정값

        [SerializeField] // Visual Collider 차단 여부 직렬화
        private bool disableVisualColliders = true; // Gameplay 물리 분리 기본값

        [SerializeField] // 이동 판정 임계값 직렬화
        private float moveSpeedThreshold = 0.1f; // 이동 애니메이션 시작 속도

        [SerializeField] // 점프 판정 임계값 직렬화
        private float upwardSpeedThreshold = 0.05f; // 점프와 낙하 구분 속도

        [SerializeField] // Push 종료 판정 직렬화
        private float pushReleaseNormalizedTime = 0.9f; // Push 재생 유지 비율

        private GameObject activeVisual; // 현재 생성된 외형
        private Animator[] activeAnimators = new Animator[0]; // 현재 외형 Animator 목록
        private PlayerCameraRelativeMovement movementController; // 공통 이동 상태 공급자
        private PlayerPushController pushController; // 공통 밀치기 상태 공급자
        private Rigidbody body; // 공통 실제 속도 공급자

        public int CurrentVisualIndex { get; private set; } = -1; // 현재 외형 인덱스
        public string CurrentVisualName { get; private set; } = string.Empty; // 현재 외형 이름
        public string DefaultVisualName => defaultVisualName; // 기본 외형 이름 조회

        private enum VisualLocomotionState // 공통 시각 이동 상태
        {
            Idle, // 대기 상태
            Run, // 이동 상태
            Jump, // 점프 상태
            Fall, // 낙하 상태
            CrouchIdle, // 앉기 대기 상태
            CrouchMove // 앉기 이동 상태
        }

        private void Awake() // 공통 Gameplay 구성요소 연결
        {
            movementController = GetComponent<PlayerCameraRelativeMovement>(); // 이동 상태 연결
            pushController = GetComponent<PlayerPushController>(); // 밀치기 이벤트 연결
            body = GetComponent<Rigidbody>(); // 실제 속도 연결
        }

        private void OnEnable() // 밀치기 이벤트 구독
        {
            if (pushController == null) // 밀치기 Controller 누락 검사
            {
                pushController = GetComponent<PlayerPushController>(); // 밀치기 Controller 재검색
            }

            if (pushController != null) // 밀치기 Controller 존재 검사
            {
                pushController.PushAttempted += HandlePushAttempted; // 밀치기 시각 이벤트 연결
            }
        }

        private void OnDisable() // 밀치기 이벤트 해제
        {
            if (pushController != null) // 밀치기 Controller 존재 검사
            {
                pushController.PushAttempted -= HandlePushAttempted; // 밀치기 시각 이벤트 해제
            }
        }

        private void Start() // 런타임 최초 외형 적용
        {
            ApplyVisualByName(defaultVisualName); // 기본 외형 적용
        }

        private void Update() // 공통 시각 상태 갱신
        {
            if (activeAnimators == null || activeAnimators.Length == 0) // Animator 누락 검사
            {
                return; // 애니메이션 갱신 생략
            }

            VisualLocomotionState desiredState = ResolveLocomotionState(); // 현재 Gameplay 상태 해석

            foreach (Animator animator in activeAnimators) // 모든 Animator 순회
            {
                UpdateAnimatorState(animator, desiredState); // 공통 시각 상태 적용
            }
        }

        public void Configure(Transform root, GameObject[] candidates, string defaultName) // Editor 자동 설정
        {
            visualRoot = root; // Visual Root 연결
            visualPrefabs = candidates ?? new GameObject[0]; // 후보 목록 연결
            defaultVisualName = string.IsNullOrEmpty(defaultName) ? ChefVisualName : defaultName; // 기본 외형 이름 정리
        }

        public bool ApplyVisualByName(string requestedVisualName) // 꾸미기 이름 기반 외형 교체
        {
            if (visualRoot == null || visualPrefabs == null || visualPrefabs.Length == 0) // 외형 설정 누락 검사
            {
                CurrentVisualIndex = -1; // 선택 상태 초기화
                CurrentVisualName = string.Empty; // 외형 이름 초기화
                activeAnimators = new Animator[0]; // Animator 목록 초기화
                return false; // 외형 적용 실패 반환
            }

            string[] candidateNames = BuildCandidateNames(); // 등록 외형 이름 목록 생성
            int resolvedIndex = ProjectJPlayerVisualIndex.ResolveByName(requestedVisualName, defaultVisualName, candidateNames); // 이름 기반 외형 선택

            if (resolvedIndex < 0 || resolvedIndex >= visualPrefabs.Length || visualPrefabs[resolvedIndex] == null) // 선택 결과 유효성 검사
            {
                CurrentVisualIndex = -1; // 선택 상태 초기화
                CurrentVisualName = string.Empty; // 외형 이름 초기화
                activeAnimators = new Animator[0]; // Animator 목록 초기화
                return false; // 외형 적용 실패 반환
            }

            ClearActiveVisual(); // 기존 외형 제거

            GameObject prefab = visualPrefabs[resolvedIndex]; // 선택 Prefab 조회
            activeVisual = Instantiate(prefab, visualRoot); // 새 외형 생성
            activeVisual.name = $"Visual_{prefab.name}"; // 런타임 이름 정리

            Transform modelTransform = activeVisual.transform; // 모델 Transform 조회
            modelTransform.localPosition = modelLocalPosition; // 발 위치 보정
            modelTransform.localRotation = Quaternion.Euler(modelLocalEulerAngles); // 모델 회전 적용
            modelTransform.localScale = modelLocalScale; // 모델 크기 적용

            PrepareVisualHierarchy(activeVisual); // Gameplay 물리와 Animator 준비

            CurrentVisualIndex = resolvedIndex; // 현재 외형 인덱스 저장
            CurrentVisualName = prefab.name; // 현재 외형 이름 저장
            return true; // 외형 적용 성공 반환
        }

        private VisualLocomotionState ResolveLocomotionState() // Gameplay 상태를 시각 상태로 변환
        {
            Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero; // 실제 Rigidbody 속도 조회
            bool isGrounded = movementController != null ? movementController.IsGrounded : Mathf.Abs(velocity.y) <= upwardSpeedThreshold; // 지상 상태 조회
            bool isCrouching = movementController != null && movementController.IsCrouching; // 앉기 상태 조회
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z); // 수평 속도 분리
            bool isMoving = horizontalVelocity.sqrMagnitude > moveSpeedThreshold * moveSpeedThreshold; // 이동 여부 계산

            if (!isGrounded) // 공중 상태 검사
            {
                return velocity.y > upwardSpeedThreshold ? VisualLocomotionState.Jump : VisualLocomotionState.Fall; // 점프 또는 낙하 선택
            }

            if (isCrouching) // 앉기 상태 검사
            {
                return isMoving ? VisualLocomotionState.CrouchMove : VisualLocomotionState.CrouchIdle; // 앉기 이동 상태 선택
            }

            return isMoving ? VisualLocomotionState.Run : VisualLocomotionState.Idle; // 기본 이동 상태 선택
        }

        private void UpdateAnimatorState(Animator animator, VisualLocomotionState desiredState) // 단일 Animator 상태 동기화
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) // Animator 사용 가능 여부 검사
            {
                return; // 상태 갱신 생략
            }

            if (animator.IsInTransition(0)) // 진행 중 전환 검사
            {
                return; // 중복 전환 방지
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 현재 Animator 상태 조회

            if (IsState(stateInfo, PushStateName) && stateInfo.normalizedTime < pushReleaseNormalizedTime) // Push 재생 유지 구간 검사
            {
                return; // Push 시각 동작 우선 유지
            }

            string desiredStateName = GetStateName(desiredState); // 목표 상태 이름 조회
            string desiredTriggerName = GetTriggerName(desiredState); // 목표 Trigger 이름 조회

            if (IsState(stateInfo, desiredStateName)) // 이미 목표 상태인지 검사
            {
                if (ShouldRestartNonLoopingLocomotion(stateInfo, desiredState)) // 비반복 이동 Clip 종료 검사
                {
                    SetTriggerIfAvailable(animator, desiredTriggerName); // 동일 이동 상태 재생
                }

                return; // 불필요한 전환 생략
            }

            SetTriggerIfAvailable(animator, desiredTriggerName); // 목표 이동 상태 전환
        }

        private static bool IsState(AnimatorStateInfo stateInfo, string stateName) // Animator 짧은 상태 이름 비교
        {
            return stateInfo.shortNameHash == Animator.StringToHash(stateName); // 현재 상태 이름 일치 결과 반환
        }

        private static bool ShouldRestartNonLoopingLocomotion(AnimatorStateInfo stateInfo, VisualLocomotionState state) // 비반복 이동 Clip 재생 여부 계산
        {
            bool supportsRestart = state == VisualLocomotionState.Idle || state == VisualLocomotionState.Run || state == VisualLocomotionState.CrouchIdle || state == VisualLocomotionState.CrouchMove; // 반복 가능한 상태 검사
            return supportsRestart && !stateInfo.loop && stateInfo.normalizedTime >= 1f; // 비반복 Clip 종료 결과 반환
        }

        private static string GetStateName(VisualLocomotionState state) // 시각 상태 이름 조회
        {
            switch (state) // 시각 상태 분기
            {
                case VisualLocomotionState.Run: // 이동 상태 검사
                    return RunStateName; // 이동 상태 이름 반환
                case VisualLocomotionState.Jump: // 점프 상태 검사
                    return JumpStateName; // 점프 상태 이름 반환
                case VisualLocomotionState.Fall: // 낙하 상태 검사
                    return FallStateName; // 낙하 상태 이름 반환
                case VisualLocomotionState.CrouchIdle: // 앉기 대기 상태 검사
                    return CrouchIdleStateName; // 앉기 대기 상태 이름 반환
                case VisualLocomotionState.CrouchMove: // 앉기 이동 상태 검사
                    return CrouchMoveStateName; // 앉기 이동 상태 이름 반환
                default: // 기본 대기 상태 처리
                    return IdleStateName; // 대기 상태 이름 반환
            }
        }

        private static string GetTriggerName(VisualLocomotionState state) // 시각 상태 Trigger 조회
        {
            switch (state) // 시각 상태 분기
            {
                case VisualLocomotionState.Run: // 이동 상태 검사
                    return RunTriggerName; // 이동 Trigger 반환
                case VisualLocomotionState.Jump: // 점프 상태 검사
                    return JumpTriggerName; // 점프 Trigger 반환
                case VisualLocomotionState.Fall: // 낙하 상태 검사
                    return FallTriggerName; // 낙하 Trigger 반환
                case VisualLocomotionState.CrouchIdle: // 앉기 대기 상태 검사
                    return CrouchIdleTriggerName; // 앉기 대기 Trigger 반환
                case VisualLocomotionState.CrouchMove: // 앉기 이동 상태 검사
                    return CrouchMoveTriggerName; // 앉기 이동 Trigger 반환
                default: // 기본 대기 상태 처리
                    return IdleTriggerName; // 대기 Trigger 반환
            }
        }

        private void HandlePushAttempted(PushAttemptResult result, PlayerFinishState target, Vector3 direction) // 실제 밀치기 시도 시각화
        {
            if (result == PushAttemptResult.Cooldown || result == PushAttemptResult.InvalidState) // 실행되지 않은 입력 결과 검사
            {
                return; // Push 애니메이션 생략
            }

            foreach (Animator animator in activeAnimators) // 모든 Animator 순회
            {
                SetTriggerIfAvailable(animator, PushTriggerName); // Push 애니메이션 실행
            }
        }

        private static void SetTriggerIfAvailable(Animator animator, string triggerName) // 존재하는 Trigger만 안전하게 실행
        {
            AnimatorControllerParameter[] parameters = animator.parameters; // Animator 파라미터 목록 조회

            for (int index = 0; index < parameters.Length; index++) // 모든 파라미터 순회
            {
                AnimatorControllerParameter parameter = parameters[index]; // 현재 파라미터 조회

                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName) // 요청 Trigger 일치 검사
                {
                    animator.SetTrigger(triggerName); // Animator Trigger 실행
                    return; // 검색 종료
                }
            }
        }

        private string[] BuildCandidateNames() // 등록 외형 이름 목록 생성
        {
            string[] candidateNames = new string[visualPrefabs.Length]; // 이름 배열 생성

            for (int index = 0; index < visualPrefabs.Length; index++) // 모든 후보 순회
            {
                GameObject prefab = visualPrefabs[index]; // 현재 Prefab 조회
                candidateNames[index] = prefab != null ? prefab.name : string.Empty; // 실제 외형 이름 저장
            }

            return candidateNames; // 외형 이름 배열 반환
        }

        private void ClearActiveVisual() // 기존 외형 제거
        {
            if (activeVisual == null) // 기존 외형 누락 검사
            {
                return; // 제거 생략
            }

            Destroy(activeVisual); // 이전 외형 제거 예약
            activeVisual = null; // 기존 참조 초기화
            activeAnimators = new Animator[0]; // Animator 목록 초기화
        }

        private void PrepareVisualHierarchy(GameObject visual) // Visual 물리와 Animator 준비
        {
            SetLayerRecursively(visual.transform, gameObject.layer); // Player Layer 통일
            activeAnimators = visual.GetComponentsInChildren<Animator>(true); // Animator 목록 저장

            foreach (Animator animator in activeAnimators) // 모든 Animator 순회
            {
                animator.applyRootMotion = false; // Root Motion 차단
            }

            if (!disableVisualColliders) // Collider 유지 설정 검사
            {
                return; // 물리 차단 생략
            }

            Collider[] colliders = visual.GetComponentsInChildren<Collider>(true); // Visual Collider 수집

            foreach (Collider collider in colliders) // 모든 Collider 순회
            {
                collider.enabled = false; // Visual 충돌 차단
            }

            Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true); // Visual Rigidbody 수집

            foreach (Rigidbody rigidbody in rigidbodies) // 모든 Rigidbody 순회
            {
                rigidbody.useGravity = false; // Visual 중력 차단
                rigidbody.isKinematic = true; // Visual 물리 이동 차단
                rigidbody.detectCollisions = false; // Visual 충돌 차단
            }
        }

        private static void SetLayerRecursively(Transform current, int layer) // Visual Layer 재귀 적용
        {
            current.gameObject.layer = layer; // 현재 GameObject Layer 적용

            for (int index = 0; index < current.childCount; index++) // 모든 자식 순회
            {
                SetLayerRecursively(current.GetChild(index), layer); // 자식 Layer 재귀 적용
            }
        }
    }
}
