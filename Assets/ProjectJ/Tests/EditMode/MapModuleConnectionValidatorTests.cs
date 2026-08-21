using NUnit.Framework; // NUnit 테스트 기능 사용
using ProjectJ.Map; // 맵 시스템 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class MapModuleConnectionValidatorTests // Module 연결 검사 테스트
    {
        [Test] // 테스트 등록
        public void Validate_AllowsAlignedExitEntrance() // 정상 Socket 연결 테스트
        {
            MapModuleSocket exitSocket = CreateSocket("Exit", Vector3.zero, Vector3.forward, MapModuleFaceDirection.North, MapModuleFaceState.Exit); // Exit 생성
            MapModuleSocket entranceSocket = CreateSocket("Entrance", Vector3.zero, Vector3.back, MapModuleFaceDirection.South, MapModuleFaceState.Entrance); // Entrance 생성

            try
            {
                MapModuleConnectionResult result = MapModuleConnectionValidator.Validate(exitSocket, entranceSocket); // 연결 검사
                Assert.IsTrue(result.IsValid); // 정상 연결 확인
                Assert.AreEqual(MapModuleConnectionFailure.None, result.Failure); // 실패 없음 확인
            }
            finally
            {
                Object.DestroyImmediate(exitSocket.gameObject); // Exit 삭제
                Object.DestroyImmediate(entranceSocket.gameObject); // Entrance 삭제
            }
        }

        [Test] // 테스트 등록
        public void Validate_RejectsWrongStateOrder() // 잘못된 상태 순서 테스트
        {
            MapModuleSocket first = CreateSocket("First", Vector3.zero, Vector3.forward, MapModuleFaceDirection.North, MapModuleFaceState.Entrance); // 잘못된 첫 Socket 생성
            MapModuleSocket second = CreateSocket("Second", Vector3.zero, Vector3.back, MapModuleFaceDirection.South, MapModuleFaceState.Exit); // 잘못된 두 번째 Socket 생성

            try
            {
                MapModuleConnectionResult result = MapModuleConnectionValidator.Validate(first, second); // 연결 검사
                Assert.IsFalse(result.IsValid); // 연결 거부 확인
                Assert.AreEqual(MapModuleConnectionFailure.InvalidStateOrder, result.Failure); // 상태 오류 확인
            }
            finally
            {
                Object.DestroyImmediate(first.gameObject); // 첫 Socket 삭제
                Object.DestroyImmediate(second.gameObject); // 두 번째 Socket 삭제
            }
        }

        [Test] // 테스트 등록
        public void Validate_RejectsPositionMismatch() // Socket 위치 불일치 테스트
        {
            MapModuleSocket exitSocket = CreateSocket("Exit", Vector3.zero, Vector3.forward, MapModuleFaceDirection.North, MapModuleFaceState.Exit); // Exit 생성
            MapModuleSocket entranceSocket = CreateSocket("Entrance", Vector3.right, Vector3.back, MapModuleFaceDirection.South, MapModuleFaceState.Entrance); // 어긋난 Entrance 생성

            try
            {
                MapModuleConnectionResult result = MapModuleConnectionValidator.Validate(exitSocket, entranceSocket, 0.05f); // 연결 검사
                Assert.IsFalse(result.IsValid); // 연결 거부 확인
                Assert.AreEqual(MapModuleConnectionFailure.PositionMismatch, result.Failure); // 위치 오류 확인
            }
            finally
            {
                Object.DestroyImmediate(exitSocket.gameObject); // Exit 삭제
                Object.DestroyImmediate(entranceSocket.gameObject); // Entrance 삭제
            }
        }

        [Test] // 테스트 등록
        public void Validate_RejectsFacingMismatch() // Socket 방향 불일치 테스트
        {
            MapModuleSocket exitSocket = CreateSocket("Exit", Vector3.zero, Vector3.forward, MapModuleFaceDirection.North, MapModuleFaceState.Exit); // Exit 생성
            MapModuleSocket entranceSocket = CreateSocket("Entrance", Vector3.zero, Vector3.forward, MapModuleFaceDirection.South, MapModuleFaceState.Entrance); // 같은 방향 Entrance 생성

            try
            {
                MapModuleConnectionResult result = MapModuleConnectionValidator.Validate(exitSocket, entranceSocket); // 연결 검사
                Assert.IsFalse(result.IsValid); // 연결 거부 확인
                Assert.AreEqual(MapModuleConnectionFailure.FacingMismatch, result.Failure); // 방향 오류 확인
            }
            finally
            {
                Object.DestroyImmediate(exitSocket.gameObject); // Exit 삭제
                Object.DestroyImmediate(entranceSocket.gameObject); // Entrance 삭제
            }
        }

        private static MapModuleSocket CreateSocket( // 테스트 Socket 생성
            string objectName, // 오브젝트 이름
            Vector3 position, // 세계 위치
            Vector3 forward, // 외향 방향
            MapModuleFaceDirection direction, // 논리 방향
            MapModuleFaceState state // Socket 상태
        )
        {
            GameObject socketObject = new GameObject(objectName); // Socket 오브젝트 생성
            socketObject.transform.position = position; // 위치 적용
            socketObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up); // 외향 방향 적용
            MapModuleSocket socket = socketObject.AddComponent<MapModuleSocket>(); // Socket 컴포넌트 추가
            socket.Configure(direction, state); // 상태 설정
            return socket; // 생성 Socket 반환
        }
    }
}
