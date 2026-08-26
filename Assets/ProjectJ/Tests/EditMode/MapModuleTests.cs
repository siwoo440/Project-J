using NUnit.Framework; // NUnit 테스트 기능 사용
using ProjectJ.Map; // 맵 모듈 기능 사용
using UnityEngine; // Vector3Int 기능 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class MapModuleTests // 맵 모듈 규칙 테스트
    {
        [Test] // 기본 정의 테스트 등록
        public void Definition_RequiresAtLeastOneEntranceAndExit() // Entrance와 Exit 최소 조건 검사
        {
            MapModuleFaceState[] states = { MapModuleFaceState.Exit, MapModuleFaceState.Entrance, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed }; // 정상 상태 집합 생성
            Assert.That(MapModule.IsFaceStateSetValid(states), Is.True); // 정상 정의 확인
        }

        [Test] // Drop 규칙 테스트 등록
        public void Definition_DropDoesNotReplaceExit() // Drop이 Exit를 대체하지 않는지 검사
        {
            MapModuleFaceState[] states = { MapModuleFaceState.Drop, MapModuleFaceState.Entrance, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed }; // Exit 없는 상태 집합 생성
            Assert.That(MapModule.IsFaceStateSetValid(states), Is.False); // 정의 실패 확인
        }

        [Test] // Entrance 누락 테스트 등록
        public void Definition_RejectsMissingEntrance() // Entrance 없는 정의 거부 검사
        {
            MapModuleFaceState[] states = { MapModuleFaceState.Exit, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed, MapModuleFaceState.Closed }; // Entrance 없는 상태 집합 생성
            Assert.That(MapModule.IsFaceStateSetValid(states), Is.False); // 정의 실패 확인
        }

        [Test] // 수평 연결 테스트 등록
        public void NorthExit_ConnectsToSouthEntrance() // North Exit와 South Entrance 연결 검사
        {
            bool canConnect = MapModule.CanConnect(MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.South, MapModuleFaceState.Entrance); // 정상 연결 검사
            Assert.That(canConnect, Is.True); // 연결 성공 확인
        }

        [Test] // 수직 연결 테스트 등록
        public void UpExit_ConnectsToDownEntrance() // Up Exit와 Down Entrance 연결 검사
        {
            bool canConnect = MapModule.CanConnect(MapModuleFaceDirection.Up, MapModuleFaceState.Exit, MapModuleFaceDirection.Down, MapModuleFaceState.Entrance); // 정상 수직 연결 검사
            Assert.That(canConnect, Is.True); // 연결 성공 확인
        }

        [Test] // 잘못된 상태 연결 테스트 등록
        public void Exit_DoesNotConnectToExit() // Exit끼리 연결되지 않는지 검사
        {
            bool canConnect = MapModule.CanConnect(MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.South, MapModuleFaceState.Exit); // 잘못된 연결 검사
            Assert.That(canConnect, Is.False); // 연결 거부 확인
        }

        [Test] // Drop 연결 테스트 등록
        public void Drop_IsNotNormalProgressConnection() // Drop이 정상 진행 연결이 아닌지 검사
        {
            bool canConnect = MapModule.CanConnect(MapModuleFaceDirection.North, MapModuleFaceState.Drop, MapModuleFaceDirection.South, MapModuleFaceState.Entrance); // Drop 연결 검사
            Assert.That(canConnect, Is.False); // 연결 거부 확인
        }

        [Test] // 반대 방향 테스트 등록
        public void OppositeDirection_IsCorrectForAllAxes() // 3축 반대 방향 검사
        {
            Assert.That(MapModule.GetOppositeDirection(MapModuleFaceDirection.North), Is.EqualTo(MapModuleFaceDirection.South)); // 남북 반대 확인
            Assert.That(MapModule.GetOppositeDirection(MapModuleFaceDirection.East), Is.EqualTo(MapModuleFaceDirection.West)); // 동서 반대 확인
            Assert.That(MapModule.GetOppositeDirection(MapModuleFaceDirection.Up), Is.EqualTo(MapModuleFaceDirection.Down)); // 상하 반대 확인
        }

        [Test] // Grid Offset 테스트 등록
        public void UpDirection_MovesOneGridCellUp() // 위쪽 Cell 이동량 검사
        {
            Vector3Int offset = MapModule.GetDirectionCellOffset(MapModuleFaceDirection.Up); // 위쪽 이동량 계산
            Assert.That(offset, Is.EqualTo(new Vector3Int(0, 1, 0))); // 위쪽 한 Cell 확인
        }

        [Test] // 모듈 크기 테스트 등록
        public void ModuleSize_UsesTenMeterCubeStandard() // 10x10x10 규격 검사
        {
            Assert.That(MapModule.DefaultModuleSize, Is.EqualTo(10f)); // 기본 크기 10 확인
            Assert.That(MapModule.PlayerHeightReference, Is.EqualTo(2f)); // 플레이어 기준 높이 확인
        }
    }
}
