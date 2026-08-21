using System.Collections.Generic; // 목록 기능 사용
using NUnit.Framework; // NUnit 테스트 사용
using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // ScriptableObject 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ItemDefinitionValidatorTests // 아이템 데이터 검증 테스트
    {
        [Test] // 테스트 등록
        public void Validate_AllowsValidDefinition() // 정상 데이터 허용 테스트
        {
            ItemDefinition definition =
                CreateDefinition("spring_shoes", "Spring Shoes"); // 정상 아이템 생성

            try
            {
                ItemDefinitionValidationResult result =
                    ItemDefinitionValidator.Validate(definition); // 데이터 검사

                Assert.IsTrue(result.IsValid); // 정상 결과 확인
            }
            finally
            {
                Object.DestroyImmediate(definition); // 테스트 데이터 제거
            }
        }

        [Test] // 테스트 등록
        public void Validate_RejectsEmptyId() // ID 누락 거부 테스트
        {
            ItemDefinition definition =
                CreateDefinition(string.Empty, "Spring Shoes"); // 빈 ID 아이템 생성

            try
            {
                ItemDefinitionValidationResult result =
                    ItemDefinitionValidator.Validate(definition); // 데이터 검사

                Assert.IsFalse(result.IsValid); // 오류 결과 확인
            }
            finally
            {
                Object.DestroyImmediate(definition); // 테스트 데이터 제거
            }
        }

        [Test] // 테스트 등록
        public void ValidateCatalog_RejectsDuplicateId() // 중복 ID 거부 테스트
        {
            ItemDefinition first =
                CreateDefinition("water_gun", "Water Gun"); // 첫 아이템 생성

            ItemDefinition second =
                CreateDefinition("WATER_GUN", "Water Gun Copy"); // 대소문자만 다른 중복 아이템 생성

            try
            {
                List<string> errors =
                    ItemDefinitionValidator.ValidateCatalog( // 전체 목록 검사
                        new List<ItemDefinition>
                        {
                            first,
                            second
                        }
                    );

                Assert.AreEqual(1, errors.Count); // 중복 오류 하나 확인
            }
            finally
            {
                Object.DestroyImmediate(first); // 첫 데이터 제거
                Object.DestroyImmediate(second); // 두 번째 데이터 제거
            }
        }

        private static ItemDefinition CreateDefinition(string id, string displayName) // 테스트 아이템 생성
        {
            ItemDefinition definition =
                ScriptableObject.CreateInstance<ItemDefinition>(); // ScriptableObject 생성

            definition.Configure(
                id,
                displayName,
                ItemCategory.Utility,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                0f,
                0f,
                false
            ); // 기본 정상값 설정

            return definition; // 생성 데이터 반환
        }
    }
}
