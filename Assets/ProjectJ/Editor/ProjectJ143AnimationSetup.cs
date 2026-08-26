using System; // 문자열 비교 기능
using UnityEditor; // Unity Editor 자동 설정 기능
using UnityEditor.Animations; // Animator Controller 편집 기능
using UnityEngine; // AnimationClip과 Vector3 기능

namespace ProjectJ.EditorTools // Project J Editor 전용 네임스페이스
{
    [InitializeOnLoad] // Unity Editor 로드 시 자동 실행
    public static class ProjectJ143AnimationSetup // 143일차 Animator 자동 구성
    {
        private const string ControllerPath = "Assets/ProjectJ/Art/Characters/Animations/Imported/char_AC.controller"; // 기존 Animator Controller 경로
        private const string CrouchIdlePath = "Assets/ProjectJ/Crouching Idle.fbx"; // 앉기 대기 FBX 경로
        private const string CrouchMovePath = "Assets/ProjectJ/Crouched Walking.fbx"; // 앉기 이동 FBX 경로
        private const string PushPath = "Assets/ProjectJ/Push.fbx"; // 밀치기 FBX 경로

        private const string CrouchIdleStateName = "Crouch Idle"; // 앉기 대기 상태 이름
        private const string CrouchMoveStateName = "Crouch Move"; // 앉기 이동 상태 이름
        private const string PushStateName = "Push"; // 밀치기 상태 이름
        private const string JumpStateName = "jump"; // 기존 점프 상태 이름
        private const string RunStateName = "running"; // 기존 달리기 상태 이름
        private const string IdleStateName = "Idle"; // 기존 대기 상태 이름

        private const string CrouchIdleTriggerName = "crouchIdle"; // 앉기 대기 Trigger 이름
        private const string CrouchMoveTriggerName = "crouchMove"; // 앉기 이동 Trigger 이름
        private const string PushTriggerName = "push"; // 밀치기 Trigger 이름
        private const string RunTriggerName = "run"; // 달리기 Trigger 이름

        static ProjectJ143AnimationSetup() // Editor 자동 적용 예약
        {
            EditorApplication.delayCall += ApplySetup; // Asset Import 완료 뒤 구성 실행
        }

        [MenuItem("Project J/143/Apply Animation Setup")] // 수동 재적용 메뉴 등록
        private static void ApplySetup() // Animator Controller 안전 구성
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath); // 기존 Controller 로드

            if (controller == null || controller.layers.Length == 0) // Controller 유효성 검사
            {
                return; // 구성 중단
            }

            AnimationClip crouchIdleClip = LoadAnimationClip(CrouchIdlePath); // 앉기 대기 Clip 로드
            AnimationClip crouchMoveClip = LoadAnimationClip(CrouchMovePath); // 앉기 이동 Clip 로드
            AnimationClip pushClip = LoadAnimationClip(PushPath); // 밀치기 Clip 로드

            if (crouchIdleClip == null || crouchMoveClip == null || pushClip == null) // 필수 Clip Import 완료 여부 검사
            {
                return; // Clip 준비 전 구성 보류
            }

            EnsureTrigger(controller, CrouchIdleTriggerName); // 앉기 대기 Trigger 보장
            EnsureTrigger(controller, CrouchMoveTriggerName); // 앉기 이동 Trigger 보장
            EnsureTrigger(controller, PushTriggerName); // 밀치기 Trigger 보장
            EnsureTrigger(controller, RunTriggerName); // 달리기 Trigger 보장

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine; // Base Layer 상태 머신 조회
            AnimatorState crouchIdleState = EnsureState(stateMachine, CrouchIdleStateName, crouchIdleClip, new Vector3(-430f, 360f, 0f)); // 앉기 대기 상태 보장
            AnimatorState crouchMoveState = EnsureState(stateMachine, CrouchMoveStateName, crouchMoveClip, new Vector3(-430f, 470f, 0f)); // 앉기 이동 상태 보장
            AnimatorState pushState = EnsureState(stateMachine, PushStateName, pushClip, new Vector3(420f, 360f, 0f)); // Push 상태 보장

            EnsureRunTransition(stateMachine); // 달리기 전환 재구성 보장
            EnsureAnyStateTransition(stateMachine, crouchIdleState, CrouchIdleTriggerName); // 앉기 대기 전환 보장
            EnsureAnyStateTransition(stateMachine, crouchMoveState, CrouchMoveTriggerName); // 앉기 이동 전환 보장
            EnsureAnyStateTransition(stateMachine, pushState, PushTriggerName); // Push 전환 보장
            RemoveAutomaticJumpExit(stateMachine); // 점프 강제 Run 전환 제거
            EnsurePushFallback(stateMachine, pushState); // Push 종료 대기 복귀 보장

            EditorUtility.SetDirty(controller); // Controller 변경 표시
            AssetDatabase.SaveAssets(); // Controller 변경 저장
        }

        private static AnimationClip LoadAnimationClip(string assetPath) // FBX 내부 실제 Clip 검색
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath); // Import된 모든 하위 Asset 조회

            foreach (UnityEngine.Object asset in assets) // 모든 하위 Asset 순회
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.Ordinal)) // 실제 AnimationClip 검사
                {
                    return clip; // 실제 Clip 반환
                }
            }

            return null; // Clip 누락 반환
        }

        private static void EnsureTrigger(AnimatorController controller, string triggerName) // Trigger 파라미터 보장
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters) // 기존 파라미터 순회
            {
                if (parameter.name == triggerName && parameter.type == AnimatorControllerParameterType.Trigger) // 동일 Trigger 존재 검사
                {
                    return; // 중복 추가 방지
                }
            }

            controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger); // 새 Trigger 추가
        }

        private static AnimatorState EnsureState(AnimatorStateMachine stateMachine, string stateName, Motion motion, Vector3 position) // Animator 상태 보장
        {
            foreach (ChildAnimatorState childState in stateMachine.states) // 기존 상태 순회
            {
                if (childState.state != null && childState.state.name == stateName) // 동일 상태 존재 검사
                {
                    childState.state.motion = motion; // 최신 Motion 연결
                    return childState.state; // 기존 상태 반환
                }
            }

            AnimatorState state = stateMachine.AddState(stateName, position); // 새 상태 생성
            state.motion = motion; // 실제 Motion 연결
            state.writeDefaultValues = true; // 기존 Controller 기록 방식 유지
            return state; // 새 상태 반환
        }

        private static void EnsureRunTransition(AnimatorStateMachine stateMachine) // Any State 달리기 전환 재구성 보장
        {
            AnimatorState runState = FindState(stateMachine, RunStateName); // 기존 달리기 상태 조회

            if (runState == null) // 달리기 상태 누락 검사
            {
                return; // 전환 구성 생략
            }

            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions; // 기존 Any State 전환 복사

            foreach (AnimatorStateTransition transition in transitions) // 모든 Any State 전환 순회
            {
                bool targetsRun = transition.destinationState == runState; // 달리기 목적 상태 검사
                bool usesRunTrigger = HasCondition(transition, RunTriggerName); // 달리기 Trigger 조건 검사

                if (targetsRun || usesRunTrigger) // 기존 달리기 전환 검사
                {
                    stateMachine.RemoveAnyStateTransition(transition); // 기존 달리기 전환 제거
                }
            }

            AnimatorStateTransition newTransition = stateMachine.AddAnyStateTransition(runState); // 새 달리기 전환 생성
            newTransition.hasExitTime = false; // Exit Time 비활성화
            newTransition.hasFixedDuration = false; // Fixed Duration 비활성화
            newTransition.duration = 0f; // 즉시 전환
            newTransition.offset = 0f; // Clip 처음부터 재생
            newTransition.interruptionSource = TransitionInterruptionSource.None; // 전환 중단 비활성화
            newTransition.orderedInterruption = false; // 순서 기반 중단 비활성화
            newTransition.canTransitionToSelf = false; // 자기 자신 재전환 방지
            newTransition.AddCondition(AnimatorConditionMode.If, 0f, RunTriggerName); // run Trigger 연결
        }

        private static bool HasCondition(AnimatorStateTransition transition, string parameterName) // 전환 조건 존재 여부 검사
        {
            foreach (AnimatorCondition condition in transition.conditions) // 모든 조건 순회
            {
                if (condition.parameter == parameterName) // 조건 이름 일치 검사
                {
                    return true; // 조건 존재 반환
                }
            }

            return false; // 조건 누락 반환
        }

        private static void EnsureAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState destinationState, string triggerName) // Any State 전환 보장
        {
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions) // 기존 Any State 전환 순회
            {
                if (transition.destinationState != destinationState) // 목적 상태 불일치 검사
                {
                    continue; // 다음 전환 검사
                }

                foreach (AnimatorCondition condition in transition.conditions) // 전환 조건 순회
                {
                    if (condition.parameter == triggerName) // 동일 Trigger 조건 검사
                    {
                        return; // 중복 전환 방지
                    }
                }
            }

            AnimatorStateTransition newTransition = stateMachine.AddAnyStateTransition(destinationState); // 새 Any State 전환 생성
            newTransition.hasExitTime = false; // 즉시 Trigger 전환 설정
            newTransition.duration = 0.08f; // 짧은 시각 블렌딩 적용
            newTransition.canTransitionToSelf = true; // 비반복 Clip 재시작 허용
            newTransition.AddCondition(AnimatorConditionMode.If, 0f, triggerName); // Trigger 조건 연결
        }

        private static void RemoveAutomaticJumpExit(AnimatorStateMachine stateMachine) // 기존 점프 자동 Run 전환 제거
        {
            AnimatorState jumpState = FindState(stateMachine, JumpStateName); // 점프 상태 조회

            if (jumpState == null) // 점프 상태 누락 검사
            {
                return; // 제거 생략
            }

            AnimatorStateTransition[] transitions = jumpState.transitions; // 점프 전환 복사

            foreach (AnimatorStateTransition transition in transitions) // 점프 전환 순회
            {
                if (transition.hasExitTime && transition.conditions.Length == 0) // 무조건 자동 종료 전환 검사
                {
                    jumpState.RemoveTransition(transition); // 자동 Run 전환 제거
                }
            }
        }

        private static void EnsurePushFallback(AnimatorStateMachine stateMachine, AnimatorState pushState) // Push 종료 대기 복귀 보장
        {
            AnimatorState idleState = FindState(stateMachine, IdleStateName); // 기존 대기 상태 조회

            if (idleState == null) // 대기 상태 누락 검사
            {
                return; // 복귀 전환 생략
            }

            foreach (AnimatorStateTransition transition in pushState.transitions) // 기존 Push 전환 순회
            {
                if (transition.destinationState == idleState && transition.hasExitTime) // 기존 자동 대기 복귀 검사
                {
                    return; // 중복 전환 방지
                }
            }

            AnimatorStateTransition fallback = pushState.AddTransition(idleState); // Push 종료 대기 전환 생성
            fallback.hasExitTime = true; // Clip 종료 비율 사용
            fallback.exitTime = 0.95f; // Push 대부분 재생 후 복귀
            fallback.duration = 0.08f; // 짧은 복귀 블렌딩 적용
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName) // 이름 기반 Animator 상태 검색
        {
            foreach (ChildAnimatorState childState in stateMachine.states) // 기존 상태 순회
            {
                if (childState.state != null && childState.state.name == stateName) // 상태 이름 일치 검사
                {
                    return childState.state; // 일치 상태 반환
                }
            }

            return null; // 상태 누락 반환
        }
    }
}
