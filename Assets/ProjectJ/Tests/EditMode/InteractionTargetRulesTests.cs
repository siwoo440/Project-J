using System.Collections.Generic; // 목록 기능 사용
using NUnit.Framework; // NUnit 테스트 기능 사용
using ProjectJ.Interaction; // 상호작용 시스템 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class InteractionTargetRulesTests // 공통 상호작용 규칙 테스트
    {
        [Test] // 테스트 등록
        public void SelectNearest_ReturnsClosestValidTarget() // 최근접 Target 선택 테스트
        {
            GameObject interactor = new GameObject("Interactor"); // 테스트 실행자 생성
            TestInteractableButton nearTarget = CreateButton("Near", new Vector3(1f, 0f, 0f), true); // 가까운 Target 생성
            TestInteractableButton farTarget = CreateButton("Far", new Vector3(2f, 0f, 0f), true); // 먼 Target 생성
            List<IInteractable> candidates = new List<IInteractable> { farTarget, nearTarget }; // 후보 목록 생성

            try
            {
                IInteractable selected = InteractionTargetRules.SelectNearest( // 최근접 Target 선택
                    interactor, // 테스트 실행자
                    Vector3.zero, // 탐색 원점
                    3f, // 상호작용 범위
                    candidates // 후보 목록
                );

                Assert.AreSame(nearTarget, selected); // 가까운 Target 선택 확인
            }
            finally
            {
                Object.DestroyImmediate(nearTarget.gameObject); // 가까운 Target 삭제
                Object.DestroyImmediate(farTarget.gameObject); // 먼 Target 삭제
                Object.DestroyImmediate(interactor); // 테스트 실행자 삭제
            }
        }

        [Test] // 테스트 등록
        public void SelectNearest_SkipsUnavailableTarget() // 사용 불가 Target 제외 테스트
        {
            GameObject interactor = new GameObject("Interactor"); // 테스트 실행자 생성
            TestInteractableButton disabledTarget = CreateButton("Disabled", new Vector3(0.5f, 0f, 0f), false); // 가까운 사용 불가 Target 생성
            TestInteractableButton validTarget = CreateButton("Valid", new Vector3(1.5f, 0f, 0f), true); // 먼 사용 가능 Target 생성
            List<IInteractable> candidates = new List<IInteractable> { disabledTarget, validTarget }; // 후보 목록 생성

            try
            {
                IInteractable selected = InteractionTargetRules.SelectNearest( // 최근접 유효 Target 선택
                    interactor, // 테스트 실행자
                    Vector3.zero, // 탐색 원점
                    3f, // 상호작용 범위
                    candidates // 후보 목록
                );

                Assert.AreSame(validTarget, selected); // 사용 가능한 Target 선택 확인
            }
            finally
            {
                Object.DestroyImmediate(disabledTarget.gameObject); // 사용 불가 Target 삭제
                Object.DestroyImmediate(validTarget.gameObject); // 사용 가능 Target 삭제
                Object.DestroyImmediate(interactor); // 테스트 실행자 삭제
            }
        }

        [Test] // 테스트 등록
        public void SelectNearest_RejectsOutOfRangeTargets() // 범위 밖 Target 제외 테스트
        {
            GameObject interactor = new GameObject("Interactor"); // 테스트 실행자 생성
            TestInteractableButton farTarget = CreateButton("Far", new Vector3(4f, 0f, 0f), true); // 범위 밖 Target 생성
            List<IInteractable> candidates = new List<IInteractable> { farTarget }; // 후보 목록 생성

            try
            {
                IInteractable selected = InteractionTargetRules.SelectNearest( // Target 선택 시도
                    interactor, // 테스트 실행자
                    Vector3.zero, // 탐색 원점
                    3f, // 상호작용 범위
                    candidates // 후보 목록
                );

                Assert.IsNull(selected); // 범위 밖 Target 미선택 확인
            }
            finally
            {
                Object.DestroyImmediate(farTarget.gameObject); // 범위 밖 Target 삭제
                Object.DestroyImmediate(interactor); // 테스트 실행자 삭제
            }
        }

        [Test] // 테스트 등록
        public void Interaction_ExecutesOnlySelectedTarget() // 단일 Target 실행 테스트
        {
            GameObject interactor = new GameObject("Interactor"); // 테스트 실행자 생성
            TestInteractableButton nearTarget = CreateButton("Near", new Vector3(1f, 0f, 0f), true); // 가까운 Target 생성
            TestInteractableButton farTarget = CreateButton("Far", new Vector3(2f, 0f, 0f), true); // 먼 Target 생성
            List<IInteractable> candidates = new List<IInteractable> { nearTarget, farTarget }; // 후보 목록 생성

            try
            {
                IInteractable selected = InteractionTargetRules.SelectNearest( // 최근접 Target 선택
                    interactor, // 테스트 실행자
                    Vector3.zero, // 탐색 원점
                    3f, // 상호작용 범위
                    candidates // 후보 목록
                );

                selected.Interact(interactor); // 선택 Target 상호작용 실행
                Assert.AreEqual(1, nearTarget.InteractionCount); // 가까운 Target 1회 실행 확인
                Assert.AreEqual(0, farTarget.InteractionCount); // 먼 Target 미실행 확인
            }
            finally
            {
                Object.DestroyImmediate(nearTarget.gameObject); // 가까운 Target 삭제
                Object.DestroyImmediate(farTarget.gameObject); // 먼 Target 삭제
                Object.DestroyImmediate(interactor); // 테스트 실행자 삭제
            }
        }

        private static TestInteractableButton CreateButton(string objectName, Vector3 position, bool canInteract) // 테스트 버튼 생성
        {
            GameObject buttonObject = new GameObject(objectName); // 버튼 오브젝트 생성
            buttonObject.transform.position = position; // 버튼 위치 적용
            TestInteractableButton button = buttonObject.AddComponent<TestInteractableButton>(); // 테스트 버튼 컴포넌트 추가
            button.Configure(canInteract, null); // 테스트 사용 가능 상태 적용
            return button; // 생성 버튼 반환
        }
    }
}
