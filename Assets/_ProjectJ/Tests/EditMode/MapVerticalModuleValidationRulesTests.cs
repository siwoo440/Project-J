using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapVerticalModuleValidationRulesTests // 수직 모듈 검증 규칙 자동 테스트 선언
    { // 수직 모듈 검증 규칙 자동 테스트 묶음
        private readonly List<Object> temporaryObjects = new List<Object>(); // 테스트 종료 시 제거할 임시 오브젝트 목록
        private MapTraversalProfile traversalProfile; // 테스트 공통 이동 능력 에셋

        [SetUp] // 각 테스트 시작 준비 항목 표시
        public void SetUp() // 테스트 공통 이동 능력 준비
        { // 테스트 공통 이동 능력 준비 처리
            traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 프로젝트 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
        } // 테스트 공통 이동 능력 준비 처리 종료

        [TearDown] // 각 테스트 종료 정리 항목 표시
        public void TearDown() // 테스트 임시 오브젝트 제거
        { // 테스트 임시 오브젝트 제거 처리
            for (int objectIndex = temporaryObjects.Count - 1; objectIndex >= 0; objectIndex--) // 임시 오브젝트 역순 순회
            { // 임시 오브젝트 제거 처리
                if (temporaryObjects[objectIndex] != null) // 현재 임시 오브젝트 존재 확인
                { // 현재 임시 오브젝트 제거 처리
                    Object.DestroyImmediate(temporaryObjects[objectIndex]); // 현재 임시 오브젝트 즉시 제거
                } // 현재 임시 오브젝트 제거 처리 종료
            } // 임시 오브젝트 제거 처리 종료

            temporaryObjects.Clear(); // 임시 오브젝트 목록 초기화
        } // 테스트 임시 오브젝트 제거 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void StepRiseAcceptsTwoMeterConnectionGain() // 2미터 계단 상승 데이터 허용 확인
        { // 2미터 계단 상승 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("Step", 8, MapTraversalRequirement.Walk, 0.25f, 1.5f); // 여덟 계단 이동 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(2f, MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, MapVerticalLayoutKind.StepRise, 2f, segments, 1.5f, 0.25f); // 정상 계단 상승 모듈 생성
            bool result = verticalData.TryValidate(out string reason); // 계단 상승 모듈 검사
            Assert.IsTrue(result, reason); // 정상 계단 상승 데이터 허용 확인
        } // 2미터 계단 상승 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ConnectionGainMismatchIsRejected() // 실제와 예상 연결 상승량 불일치 차단 확인
        { // 연결 상승량 불일치 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("Step", 8, MapTraversalRequirement.Walk, 0.25f, 1.5f); // 2미터 합계 이동 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(1.5f, MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, MapVerticalLayoutKind.StepRise, 2f, segments, 1.5f, 0.25f); // 실제 1.5미터와 예상 2미터 모듈 생성
            bool result = verticalData.TryValidate(out string reason); // 상승량 불일치 모듈 검사
            Assert.IsFalse(result); // 상승량 불일치 차단 확인
            StringAssert.Contains("실제 상승량", reason); // 상승량 불일치 사유 포함 확인
        } // 연결 상승량 불일치 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void WalkStepHigherThanThirtyCentimetersIsRejected() // 30센티미터 초과 걷기 계단 차단 확인
        { // 걷기 계단 높이 초과 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("HighStep", 5, MapTraversalRequirement.Walk, 0.4f, 1.5f); // 40센티미터 계단 이동 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(2f, MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, MapVerticalLayoutKind.StepRise, 2f, segments, 1.5f, 0.4f); // 높은 계단 상승 모듈 생성
            bool result = verticalData.TryValidate(out string reason); // 높은 계단 모듈 검사
            Assert.IsFalse(result); // 높은 걷기 계단 차단 확인
            StringAssert.Contains("0.30m", reason); // 걷기 계단 제한 사유 포함 확인
        } // 걷기 계단 높이 초과 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void UnsafeJumpRiseIsRejected() // 안전 높이 초과 점프 상승 차단 확인
        { // 점프 상승 높이 초과 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("UnsafeJump", 1, MapTraversalRequirement.Jump, 3f, 1.5f); // 3미터 단일 점프 이동 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(3f, MapModuleKind.JumpRise, MapTraversalRequirement.Jump, MapVerticalLayoutKind.JumpRise, 3f, segments, 1.5f, 3f); // 과도한 점프 상승 모듈 생성
            bool result = verticalData.TryValidate(out string reason); // 과도한 점프 상승 모듈 검사
            Assert.IsFalse(result); // 안전 높이 초과 점프 차단 확인
            StringAssert.Contains("안전 범위", reason); // 안전 범위 초과 사유 포함 확인
        } // 점프 상승 높이 초과 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SegmentHeightSumMustMatchExpectedGain() // 구간 상승량 합계 일치 요구 확인
        { // 구간 상승량 합계 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("ShortJump", 2, MapTraversalRequirement.Jump, 1f, 1.5f); // 2미터 합계 점프 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(3f, MapModuleKind.JumpRise, MapTraversalRequirement.Jump, MapVerticalLayoutKind.JumpRise, 3f, segments, 1.5f, 1f); // 예상 3미터 모듈 생성
            bool result = verticalData.TryValidate(out string reason); // 구간 합계 불일치 모듈 검사
            Assert.IsFalse(result); // 구간 상승량 합계 불일치 차단 확인
            StringAssert.Contains("구간 합계", reason); // 구간 합계 오류 사유 포함 확인
        } // 구간 상승량 합계 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ConnectionHeightGainUsesModuleLocalSpace() // 모듈 로컬 기준 상승량 계산 확인
        { // 로컬 기준 상승량 테스트 처리
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("Jump", 2, MapTraversalRequirement.Jump, 1.5f, 1.5f); // 정상 3미터 점프 구간 생성
            MapVerticalModuleData verticalData = CreateVerticalModule(3f, MapModuleKind.JumpRise, MapTraversalRequirement.Jump, MapVerticalLayoutKind.JumpRise, 3f, segments, 1.5f, 1.5f); // 정상 점프 상승 모듈 생성
            verticalData.transform.position = new Vector3(10f, 20f, 30f); // 모듈 월드 위치 이동
            MapModuleDefinition definition = verticalData.GetComponent<MapModuleDefinition>(); // 기본 모듈 정의 조회
            MapVerticalModuleValidationRules.TryFindConnectionPoint(definition, "Entrance", out MapModuleConnectionPoint entrancePoint); // 기준 입구 조회
            MapVerticalModuleValidationRules.TryFindConnectionPoint(definition, "Exit", out MapModuleConnectionPoint exitPoint); // 기준 출구 조회
            float result = MapVerticalModuleValidationRules.CalculateConnectionHeightGain(definition, entrancePoint, exitPoint); // 이동된 모듈의 로컬 상승량 계산
            Assert.That(result, Is.EqualTo(3f).Within(0.001f)); // 월드 위치와 무관한 3미터 상승량 확인
        } // 로컬 기준 상승량 테스트 처리 종료

        private MapVerticalModuleData CreateVerticalModule(float exitHeight, MapModuleKind moduleKind, MapTraversalRequirement moduleRequirement, MapVerticalLayoutKind layoutKind, float expectedHeightGain, MapVerticalTraversalSegment[] segments, float jumpDistance, float jumpRise) // 테스트용 수직 모듈 생성
        { // 테스트용 수직 모듈 생성 처리
            GameObject root = new GameObject("VerticalModuleTest"); // 빈 테스트 수직 모듈 루트 생성
            temporaryObjects.Add(root); // 정리 대상 테스트 루트 등록
            MapModuleDefinition definition = root.AddComponent<MapModuleDefinition>(); // 기본 모듈 정의 컴포넌트 추가
            MapVerticalModuleData verticalData = root.AddComponent<MapVerticalModuleData>(); // 수직 모듈 데이터 컴포넌트 추가
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, exitHeight, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 요청 높이 북쪽 출구 생성
            definition.ConfigureForEditor("MAP-VERTICAL-TEST", moduleKind, moduleRequirement, MapRotationOptions.All, new Vector3(0f, exitHeight * 0.5f + 1f, 0f), new Vector3(4f, exitHeight + 2f, 8f), 2.2f, jumpDistance, jumpRise, traversalProfile); // 테스트 기본 모듈 데이터 설정
            definition.RefreshConnectionPoints(); // 테스트 연결 지점 목록 수집
            verticalData.ConfigureForEditor(layoutKind, "Entrance", "Exit", expectedHeightGain, segments, traversalProfile); // 테스트 수직 모듈 데이터 설정
            return verticalData; // 완성된 테스트 수직 모듈 반환
        } // 테스트용 수직 모듈 생성 처리 종료

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트용 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 완성된 테스트 연결 지점 반환
        } // 테스트용 연결 지점 생성 처리 종료

        private MapVerticalTraversalSegment[] CreateRepeatedSegments(string idPrefix, int count, MapTraversalRequirement requirement, float heightGain, float horizontalDistance) // 테스트용 동일 규격 이동 구간 생성
        { // 테스트용 이동 구간 생성 처리
            MapVerticalTraversalSegment[] segments = new MapVerticalTraversalSegment[count]; // 요청 개수 이동 구간 배열 생성

            for (int segmentIndex = 0; segmentIndex < count; segmentIndex++) // 모든 테스트 이동 구간 순회
            { // 단일 테스트 이동 구간 생성 처리
                segments[segmentIndex] = new MapVerticalTraversalSegment($"{idPrefix}_{segmentIndex + 1:00}", requirement, heightGain, horizontalDistance); // 순번 기반 테스트 이동 구간 저장
            } // 단일 테스트 이동 구간 생성 처리 종료

            return segments; // 완성된 테스트 이동 구간 배열 반환
        } // 테스트용 이동 구간 생성 처리 종료
    } // 수직 모듈 검증 규칙 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
