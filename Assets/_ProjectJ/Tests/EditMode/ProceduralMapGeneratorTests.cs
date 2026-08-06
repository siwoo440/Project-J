using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class ProceduralMapGeneratorTests // 분기 맵 생성기 통합 자동 테스트 선언
    { // 분기 맵 생성기 통합 자동 테스트 묶음
        private readonly List<Object> temporaryObjects = new List<Object>(); // 테스트 종료 시 제거할 임시 오브젝트 목록

        [TearDown] // 각 테스트 종료 정리 항목 표시
        public void TearDown() // 테스트 임시 오브젝트 제거
        { // 테스트 오브젝트 제거 처리
            for (int objectIndex = temporaryObjects.Count - 1; objectIndex >= 0; objectIndex--) // 임시 오브젝트 역순 순회
            { // 임시 오브젝트 제거 처리
                if (temporaryObjects[objectIndex] != null) // 현재 임시 오브젝트 존재 확인
                { // 현재 임시 오브젝트 제거 처리
                    Object.DestroyImmediate(temporaryObjects[objectIndex]); // 현재 임시 오브젝트 즉시 제거
                } // 현재 임시 오브젝트 제거 처리
            } // 임시 오브젝트 제거 처리

            temporaryObjects.Clear(); // 임시 오브젝트 목록 초기화
        } // 테스트 오브젝트 제거 처리

        [Test] // 자동 테스트 항목 표시
        public void FixedSeedCreatesReachableEightModuleBranchGraph() // 고정 시드의 8개 분기 그래프 생성 확인
        { // 고정 시드 분기 생성 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 분기 생성기 구성
            generator.GenerateMap(); // 첫 분기 맵 생성 실행
            Assert.IsTrue(generator.LastGenerationSucceeded); // 전체 생성 성공 확인
            Assert.AreEqual(8, generator.GeneratedModuleCount); // 목표 모듈 8개 확인
            Assert.AreEqual(8, generator.GraphNodes.Count); // 그래프 노드 8개 확인
            Assert.AreEqual(8, generator.GraphEdges.Count); // 분기와 합류를 포함한 간선 8개 확인
            Assert.IsTrue(MapGenerationGraphRules.AreAllNodesReachable(generator.GraphNodes.Count, generator.GraphEdges, 0)); // 시작점 기준 전체 노드 도달 확인
            AssertNoBlockingOverlap(generator.GeneratedModules, 0.05f); // 모든 생성 모듈 Bounds 비겹침 확인
            Assert.IsTrue(generator.LastValidationReport.IsCompleted); // 생성 직후 종합 검사 완료 확인
            Assert.IsTrue(generator.LastValidationReport.IsValid, generator.LastValidationReport.BuildDetailedMessage()); // 생성 결과 종합 검사 통과 확인
            Assert.AreEqual(0, generator.LastValidationReport.IssueCount); // 생성 결과 발견 문제 없음 확인
        } // 고정 시드 분기 생성 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void SameSeedCreatesSameGenerationSignature() // 동일 시드 생성 결과 재현 확인
        { // 동일 시드 재현 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 분기 생성기 구성
            generator.GenerateMap(); // 첫 분기 맵 생성 실행
            string firstSignature = generator.GenerationSignature; // 첫 생성 결과 서명 저장
            generator.GenerateMap(); // 같은 설정으로 둘째 분기 맵 생성 실행
            string secondSignature = generator.GenerationSignature; // 둘째 생성 결과 서명 저장
            Assert.IsNotEmpty(firstSignature); // 첫 생성 결과 서명 존재 확인
            Assert.AreEqual(firstSignature, secondSignature); // 동일 시드 생성 결과 일치 확인
        } // 동일 시드 재현 테스트 처리

        private ProceduralMapGenerator CreateConfiguredGenerator() // 테스트용 생성기와 모듈 데이터 구성
        { // 테스트용 생성기 구성 처리
            MapTraversalProfile traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
            MapModuleDefinition ordinaryModule = CreateOrdinaryModule(traversalProfile); // 테스트 일반 모듈 생성
            MapModuleDefinition branchModule = CreateBranchModule(traversalProfile); // 테스트 분기 모듈 생성
            MapModuleDefinition mergeModule = CreateMergeModule(traversalProfile); // 테스트 합류 모듈 생성
            MapGenerationSettings settings = ScriptableObject.CreateInstance<MapGenerationSettings>(); // 테스트 생성 설정 에셋 생성
            settings.ConfigureForEditor(31001, false, 0, 8, 32, 0.05f, 0.05f, 0.02f, true, 2, new[] { ordinaryModule, branchModule, mergeModule }); // 테스트 분기 생성 수치 적용
            temporaryObjects.Add(settings); // 정리 대상 생성 설정 등록
            GameObject generatorObject = new GameObject("TestProceduralMapGenerator"); // 테스트 생성기 오브젝트 생성
            generatorObject.transform.position = new Vector3(50f, 0f, 0f); // 테스트 생성 원점 적용
            temporaryObjects.Add(generatorObject); // 정리 대상 생성기 오브젝트 등록
            GameObject rootObject = new GameObject("GeneratedMap"); // 테스트 생성 결과 루트 생성
            rootObject.transform.SetParent(generatorObject.transform, false); // 생성기 아래 결과 루트 배치
            ProceduralMapGenerator generator = generatorObject.AddComponent<ProceduralMapGenerator>(); // 테스트 생성기 컴포넌트 추가
            generator.ConfigureForEditor(settings, rootObject.transform, false, false); // 테스트 생성기 참조 연결
            return generator; // 구성된 테스트 생성기 반환
        } // 테스트용 생성기 구성 처리

        private MapModuleDefinition CreateOrdinaryModule(MapTraversalProfile traversalProfile) // 테스트용 직선 일반 모듈 생성
        { // 테스트 일반 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestOrdinary"); // 테스트 일반 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 일반 모듈 정의 조회
            definition.ConfigureForEditor("MAP-T01", MapModuleKind.FixedPlatform, MapTraversalRequirement.Walk, MapRotationOptions.All, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 일반 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 일반 모듈 연결 지점 수집
            return definition; // 테스트 일반 모듈 반환
        } // 테스트 일반 모듈 생성 처리

        private MapModuleDefinition CreateBranchModule(MapTraversalProfile traversalProfile) // 테스트용 분기 모듈 생성
        { // 테스트 분기 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestBranch"); // 테스트 분기 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 중앙 남쪽 입구 생성
            CreateConnection(root.transform, "ExitLeft", new Vector3(-6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 왼쪽 북쪽 출구 생성
            CreateConnection(root.transform, "ExitRight", new Vector3(6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 오른쪽 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 분기 모듈 정의 조회
            definition.ConfigureForEditor("MAP-T02", MapModuleKind.Branch, MapTraversalRequirement.Walk, MapRotationOptions.All, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 분기 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 분기 모듈 연결 지점 수집
            return definition; // 테스트 분기 모듈 반환
        } // 테스트 분기 모듈 생성 처리

        private MapModuleDefinition CreateMergeModule(MapTraversalProfile traversalProfile) // 테스트용 합류 모듈 생성
        { // 테스트 합류 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestMerge"); // 테스트 합류 모듈 루트 생성
            CreateConnection(root.transform, "EntranceLeft", new Vector3(-6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 왼쪽 남쪽 입구 생성
            CreateConnection(root.transform, "EntranceRight", new Vector3(6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 오른쪽 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 중앙 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 합류 모듈 정의 조회
            definition.ConfigureForEditor("MAP-T03", MapModuleKind.Merge, MapTraversalRequirement.Walk, MapRotationOptions.All, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 합류 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 합류 모듈 연결 지점 수집
            return definition; // 테스트 합류 모듈 반환
        } // 테스트 합류 모듈 생성 처리

        private GameObject CreateModuleRoot(string objectName) // 테스트용 모듈 루트 생성
        { // 테스트 모듈 루트 생성 처리
            GameObject root = new GameObject(objectName); // 빈 테스트 모듈 루트 생성
            root.AddComponent<MapModuleDefinition>(); // 맵 모듈 정의 컴포넌트 추가
            temporaryObjects.Add(root); // 정리 대상 테스트 모듈 등록
            return root; // 생성된 테스트 모듈 루트 반환
        } // 테스트 모듈 루트 생성 처리

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 생성된 테스트 연결 지점 반환
        } // 테스트 연결 지점 생성 처리

        private void AssertNoBlockingOverlap(IReadOnlyList<MapModuleDefinition> modules, float tolerance) // 모든 생성 모듈 쌍의 실제 겹침 검사
        { // 전체 모듈 겹침 검사 처리
            for (int firstIndex = 0; firstIndex < modules.Count; firstIndex++) // 첫 비교 모듈 순회
            { // 첫 비교 모듈 처리
                for (int secondIndex = firstIndex + 1; secondIndex < modules.Count; secondIndex++) // 뒤쪽 비교 모듈 순회
                { // 모듈 쌍 영역 비교 처리
                    bool overlaps = MapGenerationRules.BoundsHaveBlockingOverlap(modules[firstIndex].WorldBounds, modules[secondIndex].WorldBounds, tolerance); // 현재 모듈 쌍 실제 겹침 계산
                    Assert.IsFalse(overlaps, $"모듈 Bounds 겹침: {modules[firstIndex].name}, {modules[secondIndex].name}"); // 현재 모듈 쌍 비겹침 확인
                } // 모듈 쌍 영역 비교 처리
            } // 첫 비교 모듈 처리
        } // 전체 모듈 겹침 검사 처리
    } // 분기 맵 생성기 통합 자동 테스트 묶음
} // EditMode 테스트 묶음
