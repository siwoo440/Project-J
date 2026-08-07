using System.Collections.Generic; // 테스트 데이터 에셋 목록 기능 참조
using System.Linq; // 검증 문제 코드 검색 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 형식 참조
using UnityEngine; // ScriptableObject 생성과 제거 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class ProjectDataValidatorTests // 공통 데이터 ID와 필수 값 검증 테스트 형식 선언
    {
        private readonly List<ProjectDataAsset> createdAssets = new List<ProjectDataAsset>(); // 테스트 중 생성한 임시 데이터 에셋 목록 저장

        [TearDown] // 각 테스트 실행 후 정리 메서드 지정
        public void TearDown() // 각 테스트에서 생성한 임시 ScriptableObject 제거
        {
            foreach (ProjectDataAsset asset in createdAssets) // 생성된 모든 테스트 데이터 에셋 순회
            {
                if (asset != null) // 현재 테스트 에셋이 존재하는지 확인
                {
                    Object.DestroyImmediate(asset); // 테스트용 데이터 에셋 즉시 제거
                }
            }

            createdAssets.Clear(); // 테스트 데이터 에셋 목록 초기화
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ValidIdsAreAcceptedForAllCategories() // 여섯 데이터 분류의 올바른 ID 형식 검증
        {
            Assert.IsTrue(ProjectDataIdRules.IsValid("PLY-001", ProjectDataCategory.Player, out _)); // 플레이어 ID 형식 검증
            Assert.IsTrue(ProjectDataIdRules.IsValid("MAP-001", ProjectDataCategory.Map, out _)); // 맵 ID 형식 검증
            Assert.IsTrue(ProjectDataIdRules.IsValid("OBS-001", ProjectDataCategory.Obstacle, out _)); // 장애물 ID 형식 검증
            Assert.IsTrue(ProjectDataIdRules.IsValid("ITM-001", ProjectDataCategory.Item, out _)); // 아이템 ID 형식 검증
            Assert.IsTrue(ProjectDataIdRules.IsValid("COS-001", ProjectDataCategory.Cosmetic, out _)); // 꾸미기 ID 형식 검증
            Assert.IsTrue(ProjectDataIdRules.IsValid("AUD-001", ProjectDataCategory.Audio, out _)); // 오디오 ID 형식 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CreateProducesExpectedThreeDigitId() // 데이터 분류와 번호를 이용한 ID 생성 결과 검증
        {
            Assert.AreEqual("ITM-001", ProjectDataIdRules.Create(ProjectDataCategory.Item, 1)); // 첫 번째 아이템 ID 생성 결과 검증
            Assert.AreEqual("MAP-125", ProjectDataIdRules.Create(ProjectDataCategory.Map, 125)); // 125번 맵 ID 생성 결과 검증
            Assert.AreEqual("AUD-999", ProjectDataIdRules.Create(ProjectDataCategory.Audio, 999)); // 마지막 오디오 ID 생성 결과 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void WrongCategoryPrefixIsRejected() // 데이터 분류와 다른 접두사 사용 차단 여부 검증
        {
            bool isValid = ProjectDataIdRules.IsValid("ITM-001", ProjectDataCategory.Player, out string reason); // 아이템 접두사를 플레이어 데이터에 사용한 결과 조회

            Assert.IsFalse(isValid); // 잘못된 분류 접두사 검사 실패 여부 검증
            StringAssert.Contains("PLY-", reason); // 필요한 플레이어 접두사 안내 포함 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ZeroIdNumberIsRejected() // 000 데이터 번호 사용 차단 여부 검증
        {
            bool isValid = ProjectDataIdRules.IsValid("OBS-000", ProjectDataCategory.Obstacle, out _); // 000 장애물 ID 검사 결과 조회

            Assert.IsFalse(isValid); // 000 번호 검사 실패 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void MissingRequiredValuesAreDetected() // 데이터 ID와 표시 이름 누락 자동 감지 여부 검증
        {
            ItemDataDefinition asset = CreateAsset<ItemDataDefinition>(); // 기본값만 가진 아이템 데이터 에셋 생성
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { asset }); // 누락 값이 있는 데이터 에셋 검증 실행
            string[] issueCodes = report.Issues.Select(issue => issue.Code).ToArray(); // 발견된 모든 검증 문제 코드 배열 생성

            CollectionAssert.Contains(issueCodes, ProjectDataValidator.MissingIdCode); // 데이터 ID 누락 오류 감지 여부 검증
            CollectionAssert.Contains(issueCodes, ProjectDataValidator.MissingNameCode); // 데이터 표시 이름 누락 오류 감지 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DuplicateIdsAreDetected() // 같은 데이터 ID 중복 사용 자동 감지 여부 검증
        {
            ItemDataDefinition firstAsset = CreateConfiguredAsset<ItemDataDefinition>("ITM-001", "First Item", new ProjectDataVersion(1, 0, 0)); // 첫 번째 아이템 데이터 에셋 생성
            ItemDataDefinition secondAsset = CreateConfiguredAsset<ItemDataDefinition>("ITM-001", "Second Item", new ProjectDataVersion(1, 0, 0)); // 같은 ID를 사용하는 두 번째 아이템 데이터 에셋 생성
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new ProjectDataAsset[] { firstAsset, secondAsset }); // 중복 ID 데이터 에셋 검증 실행
            int duplicateIssueCount = report.Issues.Count(issue => issue.Code == ProjectDataValidator.DuplicateIdCode); // ID 중복 오류 수 계산

            Assert.AreEqual(2, duplicateIssueCount); // 중복 ID를 사용하는 두 에셋 모두에서 오류 감지 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void InvalidVersionIsDetected() // 잘못된 데이터 버전 자동 감지 여부 검증
        {
            AudioDataDefinition asset = CreateConfiguredAsset<AudioDataDefinition>("AUD-001", "Invalid Version Audio", new ProjectDataVersion(0, 0, 0)); // 0.0.0 버전 오디오 데이터 에셋 생성
            ProjectDataValidationReport report = ProjectDataValidator.Validate(new[] { asset }); // 잘못된 버전 데이터 에셋 검증 실행

            Assert.IsTrue(report.Issues.Any(issue => issue.Code == ProjectDataValidator.InvalidVersionCode)); // 데이터 버전 오류 감지 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void SixValidCategoryAssetsProduceNoErrors() // 여섯 분류의 올바른 데이터 에셋 전체 검증
        {
            List<ProjectDataAsset> assets = new List<ProjectDataAsset> // 여섯 분류의 검증용 데이터 에셋 목록 생성
            {
                CreateConfiguredAsset<PlayerDataDefinition>("PLY-001", "Default Player", new ProjectDataVersion(1, 0, 0)), // 올바른 플레이어 데이터 추가
                CreateConfiguredAsset<MapDataDefinition>("MAP-001", "Default Map", new ProjectDataVersion(1, 0, 0)), // 올바른 맵 데이터 추가
                CreateConfiguredAsset<ObstacleDataDefinition>("OBS-001", "Default Obstacle", new ProjectDataVersion(1, 0, 0)), // 올바른 장애물 데이터 추가
                CreateConfiguredAsset<ItemDataDefinition>("ITM-001", "Spring Shoes", new ProjectDataVersion(1, 0, 0)), // 올바른 아이템 데이터 추가
                CreateConfiguredAsset<CosmeticDataDefinition>("COS-001", "Default Costume", new ProjectDataVersion(1, 0, 0)), // 올바른 꾸미기 데이터 추가
                CreateConfiguredAsset<AudioDataDefinition>("AUD-001", "Default Audio", new ProjectDataVersion(1, 0, 0)) // 올바른 오디오 데이터 추가
            };

            ProjectDataValidationReport report = ProjectDataValidator.Validate(assets); // 여섯 분류 데이터 에셋 전체 검증 실행

            Assert.IsTrue(report.IsValid); // 전체 데이터 검증 성공 여부 검증
            Assert.AreEqual(0, report.ErrorCount); // 검증 오류가 없는지 검증
        }

        private T CreateAsset<T>() where T : ProjectDataAsset // 테스트용 빈 데이터 에셋 생성
        {
            T asset = ScriptableObject.CreateInstance<T>(); // 지정 형식의 ScriptableObject 인스턴스 생성
            createdAssets.Add(asset); // 테스트 후 제거할 에셋 목록에 추가
            return asset; // 생성된 테스트 데이터 에셋 반환
        }

        private T CreateConfiguredAsset<T>(string dataId, string displayName, ProjectDataVersion version) where T : ProjectDataAsset // 식별 정보가 설정된 테스트 데이터 에셋 생성
        {
            T asset = CreateAsset<T>(); // 지정 형식의 빈 테스트 데이터 에셋 생성
            asset.SetEditorIdentity(dataId, displayName, version); // 테스트 데이터 ID와 표시 이름과 버전 설정
            return asset; // 설정된 테스트 데이터 에셋 반환
        }
    }
}
