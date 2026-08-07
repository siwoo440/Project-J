using System; // 바인딩 그룹 문자열 분리 기능 참조
using System.Linq; // 액션과 Control Scheme 검색 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Input; // 프로젝트 입력 이름 상수 참조
using UnityEditor; // Unity 에셋 데이터베이스 기능 참조
using UnityEngine.InputSystem; // Unity Input System 액션 에셋 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class InputActionAssetTests // 프로젝트 입력 액션 에셋 구성 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void InputAssetContainsExpectedActionMaps() // 입력 에셋에 Gameplay과 UI 액션 맵 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기

            Assert.IsNotNull(inputActions.FindActionMap(ProjectInputNames.Gameplay.Map, false)); // Gameplay 액션 맵 존재 여부 검증
            Assert.IsNotNull(inputActions.FindActionMap(ProjectInputNames.UI.Map, false)); // UI 액션 맵 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void GameplayMapContainsExpectedActions() // Gameplay 액션 맵의 필수 액션 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기
            string[] requiredActions = // Gameplay 필수 액션 이름 배열 선언
            {
                ProjectInputNames.Gameplay.Move, // 이동 액션 이름 추가
                ProjectInputNames.Gameplay.Look, // 시점 액션 이름 추가
                ProjectInputNames.Gameplay.Jump, // 점프 액션 이름 추가
                ProjectInputNames.Gameplay.Sprint, // 달리기 액션 이름 추가
                ProjectInputNames.Gameplay.Crouch, // 앉기 액션 이름 추가
                ProjectInputNames.Gameplay.Push, // 밀치기 액션 이름 추가
                ProjectInputNames.Gameplay.UseItem, // 아이템 사용 액션 이름 추가
                ProjectInputNames.Gameplay.SelectPreviousItem, // 이전 아이템 선택 액션 이름 추가
                ProjectInputNames.Gameplay.SelectNextItem, // 다음 아이템 선택 액션 이름 추가
                ProjectInputNames.Gameplay.ShowItem, // 아이템 보여주기 액션 이름 추가
                ProjectInputNames.Gameplay.DropItem, // 아이템 버리기 액션 이름 추가
                ProjectInputNames.Gameplay.Interact, // 상호작용 액션 이름 추가
                ProjectInputNames.Gameplay.Scoreboard, // 순위표 액션 이름 추가
                ProjectInputNames.Gameplay.Pause // 일시정지 액션 이름 추가
            };

            AssertActionsExist(inputActions, ProjectInputNames.Gameplay.Map, requiredActions); // Gameplay 필수 액션 전체 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void UIMapContainsExpectedActions() // UI 액션 맵의 필수 액션 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기
            string[] requiredActions = // UI 필수 액션 이름 배열 선언
            {
                ProjectInputNames.UI.Navigate, // UI 이동 액션 이름 추가
                ProjectInputNames.UI.Submit, // UI 확인 액션 이름 추가
                ProjectInputNames.UI.Cancel, // UI 취소 액션 이름 추가
                ProjectInputNames.UI.Point, // UI 포인터 액션 이름 추가
                ProjectInputNames.UI.Click, // UI 클릭 액션 이름 추가
                ProjectInputNames.UI.ScrollWheel // UI 스크롤 액션 이름 추가
            };

            AssertActionsExist(inputActions, ProjectInputNames.UI.Map, requiredActions); // UI 필수 액션 전체 존재 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void KeyboardMouseAndGamepadControlSchemesExist() // 키보드·마우스와 게임패드 Control Scheme 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기
            string[] schemeNames = inputActions.controlSchemes.Select(controlScheme => controlScheme.name).ToArray(); // 등록된 모든 Control Scheme 이름 배열 생성

            CollectionAssert.Contains(schemeNames, ProjectInputNames.KeyboardMouseScheme); // Keyboard&Mouse Control Scheme 존재 여부 검증
            CollectionAssert.Contains(schemeNames, ProjectInputNames.GamepadScheme); // Gamepad Control Scheme 존재 여부 검증
            Assert.AreEqual(2, schemeNames.Length); // 불필요한 추가 Control Scheme이 없는지 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void RequiredKeyboardMouseBindingsExist() // 필수 키보드와 마우스 바인딩 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기

            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Move, "<Keyboard>/w", ProjectInputNames.KeyboardMouseScheme); // W 이동 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Move, "<Keyboard>/a", ProjectInputNames.KeyboardMouseScheme); // A 이동 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Move, "<Keyboard>/s", ProjectInputNames.KeyboardMouseScheme); // S 이동 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Move, "<Keyboard>/d", ProjectInputNames.KeyboardMouseScheme); // D 이동 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Look, "<Mouse>/delta", ProjectInputNames.KeyboardMouseScheme); // 마우스 시점 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Jump, "<Keyboard>/space", ProjectInputNames.KeyboardMouseScheme); // Space 점프 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Sprint, "<Keyboard>/leftShift", ProjectInputNames.KeyboardMouseScheme); // Shift 달리기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Crouch, "<Keyboard>/leftCtrl", ProjectInputNames.KeyboardMouseScheme); // Ctrl 앉기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Push, "<Mouse>/leftButton", ProjectInputNames.KeyboardMouseScheme); // 좌클릭 밀치기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.UseItem, "<Mouse>/rightButton", ProjectInputNames.KeyboardMouseScheme); // 우클릭 아이템 사용 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.SelectPreviousItem, "<Keyboard>/q", ProjectInputNames.KeyboardMouseScheme); // Q 이전 아이템 선택 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.SelectNextItem, "<Keyboard>/e", ProjectInputNames.KeyboardMouseScheme); // E 다음 아이템 선택 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.ShowItem, "<Keyboard>/r", ProjectInputNames.KeyboardMouseScheme); // R 아이템 보여주기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.DropItem, "<Keyboard>/g", ProjectInputNames.KeyboardMouseScheme); // G 아이템 버리기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Interact, "<Keyboard>/f", ProjectInputNames.KeyboardMouseScheme); // F 상호작용 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Scoreboard, "<Keyboard>/tab", ProjectInputNames.KeyboardMouseScheme); // Tab 순위표 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Pause, "<Keyboard>/escape", ProjectInputNames.KeyboardMouseScheme); // ESC 일시정지 바인딩 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void RequiredGamepadBindingsExist() // 필수 게임패드 바인딩 존재 여부 검증
        {
            InputActionAsset inputActions = LoadInputAsset(); // 프로젝트 입력 액션 에셋 불러오기

            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Move, "<Gamepad>/leftStick", ProjectInputNames.GamepadScheme); // 왼쪽 스틱 이동 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Look, "<Gamepad>/rightStick", ProjectInputNames.GamepadScheme); // 오른쪽 스틱 시점 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Jump, "<Gamepad>/buttonSouth", ProjectInputNames.GamepadScheme); // South 버튼 점프 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Sprint, "<Gamepad>/leftStickPress", ProjectInputNames.GamepadScheme); // 왼쪽 스틱 클릭 달리기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Crouch, "<Gamepad>/buttonEast", ProjectInputNames.GamepadScheme); // East 버튼 앉기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Push, "<Gamepad>/rightShoulder", ProjectInputNames.GamepadScheme); // 오른쪽 숄더 밀치기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.UseItem, "<Gamepad>/rightTrigger", ProjectInputNames.GamepadScheme); // 오른쪽 트리거 아이템 사용 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.SelectPreviousItem, "<Gamepad>/dpad/left", ProjectInputNames.GamepadScheme); // D-pad 왼쪽 이전 아이템 선택 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.SelectNextItem, "<Gamepad>/dpad/right", ProjectInputNames.GamepadScheme); // D-pad 오른쪽 다음 아이템 선택 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.ShowItem, "<Gamepad>/buttonWest", ProjectInputNames.GamepadScheme); // West 버튼 아이템 보여주기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.DropItem, "<Gamepad>/dpad/down", ProjectInputNames.GamepadScheme); // D-pad 아래 아이템 버리기 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Interact, "<Gamepad>/buttonNorth", ProjectInputNames.GamepadScheme); // North 버튼 상호작용 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Scoreboard, "<Gamepad>/select", ProjectInputNames.GamepadScheme); // Select 버튼 순위표 바인딩 검증
            AssertBinding(inputActions, ProjectInputNames.Gameplay.Map, ProjectInputNames.Gameplay.Pause, "<Gamepad>/start", ProjectInputNames.GamepadScheme); // Start 버튼 일시정지 바인딩 검증
        }

        private static InputActionAsset LoadInputAsset() // 프로젝트 입력 액션 에셋 불러오기와 존재 검증
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputNames.AssetPath); // 고정 경로에서 입력 액션 에셋 불러오기
            Assert.IsNotNull(inputActions, $"입력 액션 에셋이 없습니다: {ProjectInputNames.AssetPath}"); // 입력 액션 에셋 존재 여부 검증
            return inputActions; // 검증된 입력 액션 에셋 반환
        }

        private static void AssertActionsExist(InputActionAsset inputActions, string mapName, string[] requiredActions) // 지정 액션 맵의 모든 필수 액션 존재 여부 검증
        {
            InputActionMap actionMap = inputActions.FindActionMap(mapName, false); // 입력 에셋에서 지정 액션 맵 검색
            Assert.IsNotNull(actionMap, $"{mapName} 액션 맵이 없습니다."); // 지정 액션 맵 존재 여부 검증

            foreach (string actionName in requiredActions) // 모든 필수 액션 이름 순회
            {
                Assert.IsNotNull(actionMap.FindAction(actionName, false), $"{mapName}/{actionName} 액션이 없습니다."); // 현재 필수 액션 존재 여부 검증
            }
        }

        private static void AssertBinding(InputActionAsset inputActions, string mapName, string actionName, string controlPath, string bindingGroup) // 지정 액션의 필수 바인딩 존재 여부 검증
        {
            InputActionMap actionMap = inputActions.FindActionMap(mapName, false); // 입력 에셋에서 지정 액션 맵 검색
            Assert.IsNotNull(actionMap, $"{mapName} 액션 맵이 없습니다."); // 지정 액션 맵 존재 여부 검증
            InputAction inputAction = actionMap.FindAction(actionName, false); // 지정 액션 맵에서 액션 검색
            Assert.IsNotNull(inputAction, $"{mapName}/{actionName} 액션이 없습니다."); // 지정 액션 존재 여부 검증

            bool bindingExists = inputAction.bindings.Any(binding => // 현재 액션의 모든 바인딩에서 필수 조건 검색
                binding.path == controlPath // 지정 컨트롤 경로 일치 여부 확인
                && ContainsBindingGroup(binding.groups, bindingGroup)); // 지정 Control Scheme 그룹 포함 여부 확인

            Assert.IsTrue(bindingExists, $"{mapName}/{actionName}에 {controlPath} ({bindingGroup}) 바인딩이 없습니다."); // 필수 바인딩 존재 여부 검증
        }

        private static bool ContainsBindingGroup(string groups, string expectedGroup) // 바인딩 그룹 문자열의 지정 그룹 포함 여부 반환
        {
            if (string.IsNullOrWhiteSpace(groups)) // 바인딩 그룹 문자열이 비어 있는지 확인
            {
                return false; // 지정 그룹 미포함 반환
            }

            return groups.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Contains(expectedGroup); // 세미콜론으로 분리한 그룹의 지정 값 포함 여부 반환
        }
    }
}
