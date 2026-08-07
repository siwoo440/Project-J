using System.Collections.Generic; // 테스트 데이터 에셋 목록 기능 참조
using System.Linq; // 검증 문제 코드 검색 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 형식 참조
using UnityEngine; // ScriptableObject 생성과 제거 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class ProjectDataCatalogTests // 런타임 데이터 카탈로그 통합 검증 테스트 선언
    {
        private readonly List<ScriptableObject> createdObjects = new List<ScriptableObject>(); // 테스트 중 생성한 임시 Unity 객체 목록 저장

        [TearDown] // 각 테스트 실행 후 정리 메서드 지정
        public void TearDown() // 각 테스트에서 생성한 임시 ScriptableObject 제거
        {
            foreach (ScriptableObject createdObject in createdObjects) // 생성된 모든 테스트 Unity 객체 순회
            {
                if (createdObject != null) // 현재 테스트 객체 존재 여부 확인
                {
                    Object.DestroyImmediate(createdObject); // 테스트용 Unity 객체 즉시 제거
                }
            }

            createdObjects.Clear(); // 테스트 Unity 객체 목록 초기화
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CompleteCatalogPassesValidation() // 여섯 필수 분류가 있는 카탈로그 검증 성공 여부 확인
        {
            List<ProjectDataAsset> assets = CreateCompleteAssetList(); // 여섯 필수 분류 테스트 데이터 생성
            ProjectDataValidationReport report = ProjectDataValidator.ValidateCatalog(assets); // 런타임 카탈로그 전체 검증 실행

            Assert.IsTrue(report.IsValid); // 완전한 카탈로그 검증 성공 여부 확인
            Assert.AreEqual(0, report.ErrorCount); // 완전한 카탈로그 오류 없음 확인
        }

        [Test] // Unity Test Runner 테스트 지정
        public void MissingCategoryBlocksCatalog() // 필수 데이터 분류 누락 감지 여부 확인
        {
            List<ProjectDataAsset> assets = CreateCompleteAssetList(); // 여섯 필수 분류 테스트 데이터 생성
            assets.RemoveAt(assets.Count - 1); // 오디오 데이터 분류 제거
            ProjectDataValidationReport report = ProjectDataValidator.ValidateCatalog(assets); // 불완전한 런타임 카탈로그 검증 실행

            Assert.IsTrue(report.HasErrors); // 필수 분류 누락 오류 존재 여부 확인
            Assert.IsTrue(report.Issues.Any(issue => issue.Code == ProjectDataValidator.MissingCategoryCode)); // 필수 분류 누락 오류 코드 확인
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CatalogStoresEditorAssetList() // Editor 카탈로그 목록 교체 기능 확인
        {
            List<ProjectDataAsset> assets = CreateCompleteAssetList(); // 여섯 필수 분류 테스트 데이터 생성
            ProjectDataCatalog catalog = CreateObject<ProjectDataCatalog>(); // 테스트용 런타임 데이터 카탈로그 생성
            catalog.SetEditorAssets(assets); // 테스트 데이터 목록 카탈로그 등록

            Assert.AreEqual(assets.Count, catalog.Count); // 카탈로그 등록 데이터 수 확인
            Assert.AreSame(assets[0], catalog.Assets[0]); // 첫 번째 데이터 에셋 참조 보존 여부 확인
        }

        private List<ProjectDataAsset> CreateCompleteAssetList() // 여섯 필수 분류의 올바른 테스트 데이터 생성
        {
            return new List<ProjectDataAsset> // 완전한 테스트 데이터 목록 생성
            {
                CreateAsset<PlayerDataDefinition>("PLY-001", "Default Player"), // 올바른 플레이어 데이터 추가
                CreateAsset<MapDataDefinition>("MAP-001", "Default Map"), // 올바른 맵 데이터 추가
                CreateAsset<ObstacleDataDefinition>("OBS-001", "Default Obstacle"), // 올바른 장애물 데이터 추가
                CreateAsset<ItemDataDefinition>("ITM-001", "Spring Shoes"), // 올바른 아이템 데이터 추가
                CreateAsset<CosmeticDataDefinition>("COS-001", "Default Costume"), // 올바른 꾸미기 데이터 추가
                CreateAsset<AudioDataDefinition>("AUD-001", "Default Audio") // 올바른 오디오 데이터 추가
            };
        }

        private T CreateAsset<T>(string dataId, string displayName) where T : ProjectDataAsset // 식별 정보가 설정된 테스트 데이터 에셋 생성
        {
            T asset = CreateObject<T>(); // 지정 형식의 빈 테스트 데이터 에셋 생성
            asset.SetEditorIdentity(dataId, displayName, new ProjectDataVersion(1, 0, 0)); // 테스트 데이터 ID와 표시 이름과 버전 설정
            return asset; // 설정된 테스트 데이터 에셋 반환
        }

        private T CreateObject<T>() where T : ScriptableObject // 테스트용 ScriptableObject 생성과 정리 목록 등록
        {
            T createdObject = ScriptableObject.CreateInstance<T>(); // 지정 형식의 테스트 Unity 객체 생성
            createdObjects.Add(createdObject); // 테스트 후 제거할 Unity 객체 목록 추가
            return createdObject; // 생성된 테스트 Unity 객체 반환
        }
    }
}
