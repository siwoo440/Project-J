using System.Collections.Generic; // 생성 객체 목록 사용
using NUnit.Framework; // EditMode Test 사용
using ProjectJ.AI; // Bot 지형 센서 사용
using UnityEngine; // Unity Physics 사용

namespace ProjectJ.Tests.EditMode // EditMode Test Namespace
{
    public sealed class ProjectJBotTraversalSensorTests // Bot 지형 센서 Test 모음
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>(); // Test 생성 객체 목록

        [TearDown] // Test 종료 정리
        public void TearDown() // 생성 객체 제거
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--) // 생성 객체 역순 순회
            {
                if (createdObjects[index] != null) // 제거 대상 존재 확인
                {
                    Object.DestroyImmediate(createdObjects[index]); // Test 객체 즉시 제거
                }
            }

            createdObjects.Clear(); // 정리 목록 초기화
        }

        [Test] // 소환 높이 아래 평지 이동 검증
        public void TrySelectTraversal_AcceptsFlatGroundBelowSpawnMarker() // 착지 후 평지 이동 검증
        {
            CreateGround(Vector3.zero, new Vector3(10f, 1f, 10f), -0.5f); // 넓은 평지 생성
            ProjectJBotTraversalSensor sensor = CreateSensor(Vector3.zero); // 바닥 위 Bot 센서 생성
            Physics.SyncTransforms(); // Physics Transform 동기화

            bool selected = sensor.TrySelectTraversal( // 주변 이동 후보 선택
                Vector3.zero, // 현재 발 위치 전달
                Vector3.forward, // 전방 목표 방향 전달
                2f, // 높은 소환 지점 높이 전달
                5f, // 걷기 속도 전달
                7f, // 점프 속도 전달
                -20f, // 중력 전달
                0.4f, // 몸통 반경 전달
                2f, // 몸통 높이 전달
                Vector3.zero, // 실패 방향 없음
                out ProjectJBotTraversalDecision decision // 이동 판단 수신
            );

            Assert.That(selected, Is.True); // 평지 후보 선택 검증
            Assert.That(decision.Action, Is.EqualTo(ProjectJBotTraversalAction.Walk)); // 걷기 행동 검증
        }

        [Test] // 자기 Collider 점프 제외 검증
        public void TrySelectTraversal_IgnoresOwnColliderDuringJumpArc() // 자기 몸통을 장애물로 보지 않는지 검증
        {
            CreateGround(Vector3.zero, new Vector3(10f, 1f, 10f), -0.5f); // 시작 평지 생성
            CreateGround(new Vector3(0f, 0f, 1.4f), new Vector3(1.2f, 0.5f, 1f), 0.25f); // 전방 점프 단차 생성
            ProjectJBotTraversalSensor sensor = CreateSensor(Vector3.zero); // 바닥 위 Bot 센서 생성
            Physics.SyncTransforms(); // Physics Transform 동기화

            bool selected = sensor.TrySelectTraversal( // 점프 후보 선택
                Vector3.zero, // 현재 발 위치 전달
                Vector3.forward, // 전방 목표 방향 전달
                0f, // 현재 안전 높이 전달
                5f, // 걷기 속도 전달
                7f, // 점프 속도 전달
                -20f, // 중력 전달
                0.4f, // 몸통 반경 전달
                2f, // 몸통 높이 전달
                Vector3.zero, // 실패 방향 없음
                out ProjectJBotTraversalDecision decision // 이동 판단 수신
            );

            Assert.That(selected, Is.True); // 점프 후보 선택 검증
            Assert.That(decision.Action, Is.EqualTo(ProjectJBotTraversalAction.Jump)); // 점프 행동 검증
        }

        private ProjectJBotTraversalSensor CreateSensor( // Bot 센서 객체 생성
            Vector3 footPosition // Bot 발 위치
        )
        {
            GameObject botObject = new GameObject("TraversalSensorBot"); // Bot Test 객체 생성
            createdObjects.Add(botObject); // 정리 목록 등록
            botObject.transform.position = footPosition; // Bot 발 위치 설정
            CapsuleCollider collider = botObject.AddComponent<CapsuleCollider>(); // Bot 몸통 Collider 추가
            collider.radius = 0.4f; // 몸통 반경 설정
            collider.height = 2f; // 몸통 높이 설정
            collider.center = Vector3.up; // 발 기준 몸통 중심 설정
            return botObject.AddComponent<ProjectJBotTraversalSensor>(); // 지형 센서 추가 후 반환
        }

        private void CreateGround( // Test 지형 생성
            Vector3 position, // 지형 수평 위치
            Vector3 scale, // 지형 크기
            float centerY // 지형 중심 높이
        )
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube); // Cube 지형 생성
            createdObjects.Add(ground); // 정리 목록 등록
            ground.name = "TraversalGround"; // Test 지형 이름 설정
            ground.transform.position = new Vector3(position.x, centerY, position.z); // 지형 위치 설정
            ground.transform.localScale = scale; // 지형 크기 설정
        }
    }
}
