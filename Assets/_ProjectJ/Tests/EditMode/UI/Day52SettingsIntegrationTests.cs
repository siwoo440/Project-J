using NUnit.Framework; // Unity EditMode 테스트 기능 참조
using ProjectJ.Core.Services; // 밝기 변환 기능 참조
using ProjectJ.Input; // 키 재지정 중복 검사 기능 참조
using UnityEngine.InputSystem; // 순수 InputAction 테스트 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 52일차 밝기와 키 재지정 규칙 테스트 구성
    public sealed class Day52SettingsIntegrationTests // 설정 실사용 완성 핵심 규칙 테스트 선언
    { // 디스크와 Scene에 의존하지 않는 순수 단위 테스트 구성
        [Test] // Unity Test Runner 테스트 지정
        public void BrightnessMapsToExpectedPostExposure() // 50·100·150퍼센트 밝기가 -1·0·+1 Exposure로 변환되는지 검증
        { // URP Post Exposure 변환 규칙 확인
            Assert.AreEqual(-1f, DisplayBrightnessApplier.CalculatePostExposure(0.5f), 0.0001f); // 50퍼센트 밝기 -1 Exposure 검증
            Assert.AreEqual(0f, DisplayBrightnessApplier.CalculatePostExposure(1f), 0.0001f); // 100퍼센트 밝기 0 Exposure 검증
            Assert.AreEqual(1f, DisplayBrightnessApplier.CalculatePostExposure(1.5f), 0.0001f); // 150퍼센트 밝기 +1 Exposure 검증
        } // 50·100·150퍼센트 밝기가 -1·0·+1 Exposure로 변환되는지 검증 마무리

        [Test] // Unity Test Runner 테스트 지정
        public void DuplicateKeyboardBindingIsDetected() // 다른 Gameplay 조작과 같은 Keyboard 경로를 재지정하면 충돌로 판단하는지 검증
        { // 설정 메뉴 중복 키 거부 규칙 확인
            InputActionMap map = new InputActionMap("Gameplay"); // 검사용 Gameplay 액션 맵 생성
            InputAction jump = map.AddAction("Jump", InputActionType.Button); // 검사용 Jump 액션 생성
            jump.AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse"); // Jump 기본 Space 바인딩 추가
            InputAction crouch = map.AddAction("Crouch", InputActionType.Button); // 검사용 Crouch 액션 생성
            crouch.AddBinding("<Keyboard>/leftCtrl", groups: "Keyboard&Mouse"); // Crouch 기본 Ctrl 바인딩 추가
            int crouchBindingIndex = InputBindingConflictRules.FindKeyboardMouseBindingIndex(crouch); // Crouch Keyboard 바인딩 인덱스 검색
            crouch.ApplyBindingOverride(crouchBindingIndex, "<Keyboard>/space"); // Crouch를 Jump와 같은 Space로 임시 재지정
            bool hasDuplicate = InputBindingConflictRules.HasDuplicateEffectivePath(map, crouch, crouchBindingIndex); // Gameplay 내부 중복 경로 검사
            Assert.IsTrue(hasDuplicate); // Space 중복 키 감지 검증
        } // 다른 Gameplay 조작과 같은 Keyboard 경로를 재지정하면 충돌로 판단하는지 검증 마무리

        [Test] // Unity Test Runner 테스트 지정
        public void DifferentKeyboardBindingIsAccepted() // 다른 Gameplay 조작과 겹치지 않는 새 Keyboard 경로는 허용되는지 검증
        { // 중복 키 오탐 방지 확인
            InputActionMap map = new InputActionMap("Gameplay"); // 검사용 Gameplay 액션 맵 생성
            InputAction jump = map.AddAction("Jump", InputActionType.Button); // 검사용 Jump 액션 생성
            jump.AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse"); // Jump 기본 Space 바인딩 추가
            InputAction crouch = map.AddAction("Crouch", InputActionType.Button); // 검사용 Crouch 액션 생성
            crouch.AddBinding("<Keyboard>/leftCtrl", groups: "Keyboard&Mouse"); // Crouch 기본 Ctrl 바인딩 추가
            int crouchBindingIndex = InputBindingConflictRules.FindKeyboardMouseBindingIndex(crouch); // Crouch Keyboard 바인딩 인덱스 검색
            crouch.ApplyBindingOverride(crouchBindingIndex, "<Keyboard>/c"); // Crouch를 사용되지 않은 C 키로 임시 재지정
            bool hasDuplicate = InputBindingConflictRules.HasDuplicateEffectivePath(map, crouch, crouchBindingIndex); // Gameplay 내부 중복 경로 검사
            Assert.IsFalse(hasDuplicate); // 사용되지 않은 C 키 허용 검증
        } // 다른 Gameplay 조작과 겹치지 않는 새 Keyboard 경로는 허용되는지 검증 마무리
    } // 설정 실사용 완성 핵심 규칙 테스트 마무리
} // 프로젝트 EditMode 테스트 네임스페이스 마무리
