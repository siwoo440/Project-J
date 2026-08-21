using NUnit.Framework; // NUnit 테스트 기능 사용
using ProjectJ.Map; // 맵 시스템 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class MapObstaclePlacementVolumeTests // 장애물 배치 영역 테스트
    {
        [Test] // 테스트 등록
        public void SafeVolume_AllowsContainedBounds() // Safe Volume 내부 허용 테스트
        {
            GameObject moduleObject = CreateModuleWithSafeVolume(out MapModule module, out _); // 테스트 Module 생성

            try // 테스트 정리 보장
            {
                Bounds obstacleBounds = new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f)); // 내부 장애물 Bounds 생성
                MapObstaclePlacementResult result = MapObstaclePlacementValidator.Validate(module, obstacleBounds); // 배치 검사 실행
                Assert.IsTrue(result.IsAllowed); // 배치 허용 확인
                Assert.AreEqual(MapObstaclePlacementRejectReason.None, result.RejectReason); // 거부 사유 없음 확인
            }
            finally // 테스트 오브젝트 정리
            {
                Object.DestroyImmediate(moduleObject); // 테스트 Module 삭제
            }
        }

        [Test] // 테스트 등록
        public void SafeVolume_RejectsOutsideBounds() // Safe Volume 외부 거부 테스트
        {
            GameObject moduleObject = CreateModuleWithSafeVolume(out MapModule module, out _); // 테스트 Module 생성

            try // 테스트 정리 보장
            {
                Bounds obstacleBounds = new Bounds(new Vector3(5.5f, 0f, 0f), new Vector3(2f, 2f, 2f)); // 경계 이탈 장애물 Bounds 생성
                MapObstaclePlacementResult result = MapObstaclePlacementValidator.Validate(module, obstacleBounds); // 배치 검사 실행
                Assert.IsFalse(result.IsAllowed); // 배치 거부 확인
                Assert.AreEqual(MapObstaclePlacementRejectReason.OutsideSafeVolume, result.RejectReason); // 영역 이탈 사유 확인
            }
            finally // 테스트 오브젝트 정리
            {
                Object.DestroyImmediate(moduleObject); // 테스트 Module 삭제
            }
        }

        [Test] // 테스트 등록
        public void NoSpawnVolume_RejectsIntersection() // No Spawn 침범 거부 테스트
        {
            GameObject moduleObject = CreateModuleWithSafeVolume(out MapModule module, out Transform moduleTransform); // 테스트 Module 생성

            try // 테스트 정리 보장
            {
                GameObject noSpawnObject = new GameObject("NoSpawn"); // 금지 영역 오브젝트 생성
                noSpawnObject.transform.SetParent(moduleTransform, false); // Module 하위 배치
                noSpawnObject.transform.localPosition = Vector3.zero; // Module 중심 배치
                noSpawnObject.transform.localScale = new Vector3(2f, 2f, 2f); // 금지 영역 크기 설정
                MapObstaclePlacementVolume noSpawnVolume = noSpawnObject.AddComponent<MapObstaclePlacementVolume>(); // 금지 영역 컴포넌트 추가
                noSpawnVolume.Configure(MapObstacleVolumeType.NoSpawn, true); // 금지 영역 설정
                Bounds obstacleBounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f)); // 금지 영역 침범 Bounds 생성
                MapObstaclePlacementResult result = MapObstaclePlacementValidator.Validate(module, obstacleBounds); // 배치 검사 실행
                Assert.IsFalse(result.IsAllowed); // 배치 거부 확인
                Assert.AreEqual(MapObstaclePlacementRejectReason.IntersectsNoSpawnVolume, result.RejectReason); // 금지 영역 사유 확인
            }
            finally // 테스트 오브젝트 정리
            {
                Object.DestroyImmediate(moduleObject); // 테스트 Module 삭제
            }
        }

        [Test] // 테스트 등록
        public void FutureObstacle_UsesColliderBounds() // 미래 장애물 Collider Bounds 테스트
        {
            GameObject moduleObject = CreateModuleWithSafeVolume(out MapModule module, out _); // 테스트 Module 생성
            GameObject obstacleObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 임시 장애물 생성

            try // 테스트 정리 보장
            {
                obstacleObject.transform.position = Vector3.zero; // 안전 영역 중앙 배치
                obstacleObject.transform.localScale = new Vector3(2f, 2f, 2f); // 장애물 크기 설정
                MapObstaclePlacementResult result = MapObstaclePlacementValidator.Validate(module, obstacleObject); // GameObject 기준 검사 실행
                Assert.IsTrue(result.IsAllowed); // 미래 장애물 공통 허용 확인
            }
            finally // 테스트 오브젝트 정리
            {
                Object.DestroyImmediate(obstacleObject); // 임시 장애물 삭제
                Object.DestroyImmediate(moduleObject); // 테스트 Module 삭제
            }
        }

        private static GameObject CreateModuleWithSafeVolume(out MapModule module, out Transform moduleTransform) // 테스트 Module 생성
        {
            GameObject moduleObject = new GameObject("Module"); // Module 오브젝트 생성
            module = moduleObject.AddComponent<MapModule>(); // Module 컴포넌트 추가
            moduleTransform = moduleObject.transform; // Module Transform 반환
            GameObject safeObject = new GameObject("SafeVolume"); // 설치 가능 영역 생성
            safeObject.transform.SetParent(moduleObject.transform, false); // Module 하위 배치
            safeObject.transform.localPosition = Vector3.zero; // Module 중심 배치
            safeObject.transform.localScale = new Vector3(10f, 10f, 10f); // 설치 가능 영역 크기 설정
            MapObstaclePlacementVolume safeVolume = safeObject.AddComponent<MapObstaclePlacementVolume>(); // 설치 가능 영역 컴포넌트 추가
            safeVolume.Configure(MapObstacleVolumeType.Safe, true); // 설치 가능 영역 설정
            return moduleObject; // 테스트 Module 반환
        }
    }
}
