using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class ProceduralVerticalMapGeneratorTests // 수직 선형 맵 생성기 통합 자동 테스트 선언
    { // 수직 선형 맵 생성기 통합 자동 테스트 묶음
        private readonly List<Object> temporaryObjects = new List<Object>(); // 테스트 종료 시 제거할 임시 오브젝트 목록

        [TearDown] // 각 테스트 종료 정리 항목 표시
        public void TearDown() // 테스트 임시 오브젝트 제거
        { // 테스트 오브젝트 제거 처리
            for (int objectIndex = temporaryObjects.Count - 1; objectIndex >= 0; objectIndex--) // 임시 오브젝트 역순 순회
            { // 임시 오브젝트 제거 처리
                if (temporaryObjects[objectIndex] != null) // 현재 임시 오브젝트 존재 확인
                { // 현재 임시 오브젝트 제거 처리
                    Object.DestroyImmediate(temporaryObjects[objectIndex]); // 현재 임시 오브젝트 즉시 제거
                } // 현재 임시 오브젝트 제거 처리 종료
            } // 임시 오브젝트 제거 처리 종료

            temporaryObjects.Clear(); // 임시 오브젝트 목록 초기화
        } // 테스트 오브젝트 제거 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void FixedSeedReachesVerticalTargetAndAscendingCount() // 고정 시드의 목표 높이와 상승 모듈 수 달성 확인
        { // 고정 시드 수직 생성 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 생성기 구성
            generator.GenerateMap(); // 수직 맵 생성 실행
            Assert.IsTrue(generator.LastGenerationSucceeded, generator.LastValidationReport.BuildDetailedMessage()); // 전체 생성과 종합 검사 성공 확인
            Assert.AreEqual(5, generator.GeneratedModuleCount); // 목표 모듈 5개 생성 확인
            Assert.GreaterOrEqual(generator.GeneratedHeight + MapVerticalGenerationRules.HeightEpsilon, generator.EffectiveTargetHeight); // 최종 생성 높이 목표 이상 확인
            Assert.GreaterOrEqual(generator.AscendingModuleCount, 4); // 최소 상승 모듈 4개 확인
            Assert.LessOrEqual(generator.MaximumObservedConsecutiveFlatModules, 1); // 연속 평지 1개 이하 확인
            Assert.AreEqual(4, generator.GraphEdges.Count); // 선형 경로 간선 4개 확인
            MapModuleConnectionPoint firstEntrance = FindConnection(generator.GeneratedModules[0], "Entrance"); // 첫 모듈 시작 입구 조회
            MapModuleConnectionPoint lastExit = FindConnection(generator.GeneratedModules[generator.GeneratedModuleCount - 1], "Exit"); // 마지막 모듈 종료 출구 조회
            float actualHeightGain = lastExit.transform.position.y - firstEntrance.transform.position.y; // 실제 시작과 종료 연결 지점 높이 차이 계산
            Assert.AreEqual(generator.GeneratedHeight, actualHeightGain, 0.02f); // 데이터 누적 높이와 실제 XYZ 배치 높이 일치 확인
        } // 고정 시드 수직 생성 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void EveryVerticalConnectionMatchesInXYZSpace() // 모든 수직 연결 지점의 XYZ 일치 확인
        { // 수직 연결 지점 XYZ 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 생성기 구성
            generator.GenerateMap(); // 수직 맵 생성 실행

            for (int edgeIndex = 0; edgeIndex < generator.GraphEdges.Count; edgeIndex++) // 모든 생성 간선 순회
            { // 단일 간선 연결 위치 검사 처리
                MapGenerationGraphEdge edge = generator.GraphEdges[edgeIndex]; // 현재 그래프 간선 조회
                MapModuleDefinition sourceModule = generator.GeneratedModules[edge.FromNodeIndex]; // 출발 모듈 조회
                MapModuleDefinition targetModule = generator.GeneratedModules[edge.ToNodeIndex]; // 도착 모듈 조회
                MapModuleConnectionPoint sourceExit = FindConnection(sourceModule, edge.ExitConnectionId); // 출발 출구 조회
                MapModuleConnectionPoint targetEntrance = FindConnection(targetModule, edge.EntranceConnectionId); // 도착 입구 조회
                float distance = Vector3.Distance(sourceExit.transform.position, targetEntrance.transform.position); // 두 연결 지점 XYZ 거리 계산
                Assert.LessOrEqual(distance, 0.02f, $"{edgeIndex}번 간선의 XYZ 연결 위치가 다릅니다."); // 연결 지점 위치 허용 오차 확인
            } // 단일 간선 연결 위치 검사 처리 종료
        } // 수직 연결 지점 XYZ 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SameSeedCreatesSameVerticalSignature() // 동일 시드 수직 생성 결과 재현 확인
        { // 동일 시드 수직 재현 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 생성기 구성
            generator.GenerateMap(); // 첫 수직 맵 생성 실행
            string firstSignature = generator.GenerationSignature; // 첫 수직 생성 서명 저장
            generator.GenerateMap(); // 동일 설정으로 둘째 수직 맵 생성 실행
            string secondSignature = generator.GenerationSignature; // 둘째 수직 생성 서명 저장
            Assert.IsNotEmpty(firstSignature); // 첫 수직 생성 서명 존재 확인
            Assert.AreEqual(firstSignature, secondSignature); // 동일 시드의 모듈과 높이 구조 일치 확인
        } // 동일 시드 수직 재현 테스트 처리 종료

        private ProceduralMapGenerator CreateConfiguredGenerator() // 테스트용 수직 생성기와 모듈 구성
        { // 테스트용 수직 생성기 구성 처리
            MapTraversalProfile traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
            MapModuleDefinition flatModule = CreateFlatModule(traversalProfile); // 테스트 평지 모듈 생성
            MapModuleDefinition riseModule = CreateRiseModule(traversalProfile); // 테스트 2미터 상승 모듈 생성
            MapGenerationSettings settings = ScriptableObject.CreateInstance<MapGenerationSettings>(); // 테스트 생성 설정 에셋 생성
            settings.ConfigureVerticalForEditor(35001, false, 0, 5, 64, 0.05f, 0.05f, 0.02f, 8f, 8f, 4, 1, false, new[] { flatModule, riseModule }); // 고정 목표 수직 생성 설정 적용
            temporaryObjects.Add(settings); // 정리 대상 생성 설정 등록
            GameObject generatorObject = new GameObject("TestVerticalMapGenerator"); // 테스트 수직 생성기 오브젝트 생성
            temporaryObjects.Add(generatorObject); // 정리 대상 수직 생성기 오브젝트 등록
            GameObject generatedRoot = new GameObject("GeneratedMap"); // 테스트 생성 결과 루트 생성
            generatedRoot.transform.SetParent(generatorObject.transform, false); // 생성기 아래 결과 루트 배치
            ProceduralMapGenerator generator = generatorObject.AddComponent<ProceduralMapGenerator>(); // 테스트 맵 생성기 컴포넌트 추가
            generator.ConfigureForEditor(settings, generatedRoot.transform, false, false); // 테스트 생성기 참조 연결
            return generator; // 구성된 테스트 수직 생성기 반환
        } // 테스트용 수직 생성기 구성 처리 종료

        private MapModuleDefinition CreateFlatModule(MapTraversalProfile traversalProfile) // 테스트용 평지 모듈 생성
        { // 테스트 평지 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestFlat", false); // 평지 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 0미터 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 평지 모듈 정의 조회
            definition.ConfigureForEditor("MAP-V00", MapModuleKind.FixedPlatform, MapTraversalRequirement.Walk, MapRotationOptions.Degrees0, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 평지 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 평지 모듈 연결 지점 수집
            return definition; // 테스트 평지 모듈 반환
        } // 테스트 평지 모듈 생성 처리 종료

        private MapModuleDefinition CreateRiseModule(MapTraversalProfile traversalProfile) // 테스트용 2미터 상승 모듈 생성
        { // 테스트 상승 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestRise", true); // 상승 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 2f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 2미터 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 상승 모듈 정의 조회
            definition.ConfigureForEditor("MAP-V02", MapModuleKind.JumpRise, MapTraversalRequirement.Jump, MapRotationOptions.Degrees0, new Vector3(0f, 1.25f, 0f), new Vector3(4f, 2.5f, 8f), 2.2f, 1f, 1f, traversalProfile); // 상승 모듈 기본 데이터 적용
            definition.RefreshConnectionPoints(); // 상승 모듈 연결 지점 수집
            MapVerticalTraversalSegment[] segments = // 테스트 수직 이동 구간 배열 선언
            { // 테스트 수직 이동 구간 묶음
                new MapVerticalTraversalSegment("Jump_01", MapTraversalRequirement.Jump, 1f, 1f), // 첫 1미터 상승 점프 구간
                new MapVerticalTraversalSegment("Jump_02", MapTraversalRequirement.Jump, 1f, 1f) // 둘째 1미터 상승 점프 구간
            }; // 테스트 수직 이동 구간 묶음 종료
            MapVerticalModuleData verticalData = root.GetComponent<MapVerticalModuleData>(); // 상승 모듈 수직 데이터 조회
            verticalData.ConfigureForEditor(MapVerticalLayoutKind.JumpRise, "Entrance", "Exit", 2f, segments, traversalProfile); // 2미터 수직 데이터 적용
            return definition; // 테스트 상승 모듈 반환
        } // 테스트 상승 모듈 생성 처리 종료

        private GameObject CreateModuleRoot(string objectName, bool includeVerticalData) // 테스트용 모듈 루트 생성
        { // 테스트 모듈 루트 생성 처리
            GameObject root = new GameObject(objectName); // 빈 테스트 모듈 루트 생성
            root.AddComponent<MapModuleDefinition>(); // 맵 모듈 정의 컴포넌트 추가

            if (includeVerticalData) // 수직 데이터 포함 여부 확인
            { // 수직 데이터 포함 처리
                root.AddComponent<MapVerticalModuleData>(); // 수직 모듈 데이터 컴포넌트 추가
            } // 수직 데이터 포함 처리 종료

            temporaryObjects.Add(root); // 정리 대상 테스트 모듈 등록
            return root; // 생성된 테스트 모듈 루트 반환
        } // 테스트 모듈 루트 생성 처리 종료

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 생성된 테스트 연결 지점 반환
        } // 테스트 연결 지점 생성 처리 종료

        private MapModuleConnectionPoint FindConnection(MapModuleDefinition module, string connectionId) // 모듈에서 ID가 같은 연결 지점 조회
        { // 연결 지점 조회 처리
            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 모듈 연결 지점 목록 조회

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 연결 지점 순회
            { // 연결 지점 ID 비교 처리
                if (points[pointIndex] != null && points[pointIndex].ConnectionId == connectionId) // 요청 ID 일치 확인
                { // 요청 ID 일치 처리
                    return points[pointIndex]; // 일치 연결 지점 반환
                } // 요청 ID 일치 처리 종료
            } // 연결 지점 ID 비교 처리 종료

            Assert.Fail($"{module.name}에서 {connectionId} 연결 지점을 찾지 못했습니다."); // 연결 지점 누락 테스트 실패 출력
            return null; // 컴파일러용 빈 연결 지점 반환
        } // 연결 지점 조회 처리 종료
    } // 수직 선형 맵 생성기 통합 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
