using System; // 잘못된 레이어 값 예외 기능 참조
using System.Collections.Generic; // 읽기 전용 레이어 목록 기능 참조
using UnityEngine; // LayerMask와 Unity 레이어 이름 조회 기능 참조

namespace ProjectJ.Core.Physics // 프로젝트 물리 레이어 네임스페이스 선언
{
    public static class ProjectPhysicsLayers // Project J 전용 물리 레이어 이름과 번호 관리 형식 선언
    {
        public const int MinimumUserLayerIndex = 8; // Unity 사용자 레이어 시작 번호 선언
        public const int MaximumLayerIndex = 31; // Unity 레이어 최대 번호 선언

        public const string PlayerName = "Player"; // 플레이어 레이어 이름 선언
        public const string GroundName = "Ground"; // 지면 레이어 이름 선언
        public const string ObstacleName = "Obstacle"; // 장애물 레이어 이름 선언
        public const string CheckpointName = "Checkpoint"; // 체크포인트 레이어 이름 선언
        public const string ItemBoxName = "ItemBox"; // 아이템 상자 레이어 이름 선언
        public const string InteractableName = "Interactable"; // 상호작용 오브젝트 레이어 이름 선언
        public const string PushHitboxName = "PushHitbox"; // 밀치기 판정 레이어 이름 선언
        public const string RespawnProtectionName = "RespawnProtection"; // 부활 보호 레이어 이름 선언

        private static readonly ProjectPhysicsLayer[] LayerValues = // Project J 전용 레이어 전체 배열 선언
        {
            ProjectPhysicsLayer.Player, // 플레이어 레이어 추가
            ProjectPhysicsLayer.Ground, // 지면 레이어 추가
            ProjectPhysicsLayer.Obstacle, // 장애물 레이어 추가
            ProjectPhysicsLayer.Checkpoint, // 체크포인트 레이어 추가
            ProjectPhysicsLayer.ItemBox, // 아이템 상자 레이어 추가
            ProjectPhysicsLayer.Interactable, // 상호작용 오브젝트 레이어 추가
            ProjectPhysicsLayer.PushHitbox, // 밀치기 판정 레이어 추가
            ProjectPhysicsLayer.RespawnProtection // 부활 보호 레이어 추가
        };

        public static IReadOnlyList<ProjectPhysicsLayer> All => LayerValues; // Project J 전용 레이어 전체 목록 반환
        public static int Count => LayerValues.Length; // Project J 전용 레이어 개수 반환

        public static int GetIndex(ProjectPhysicsLayer layer) // 프로젝트 물리 레이어의 Unity 레이어 번호 반환
        {
            return (int)layer; // enum에 지정된 Unity 레이어 번호 반환
        }

        public static string GetName(ProjectPhysicsLayer layer) // 프로젝트 물리 레이어의 Unity 레이어 이름 반환
        {
            switch (layer) // 전달된 프로젝트 물리 레이어 분기
            {
                case ProjectPhysicsLayer.Player: // 플레이어 레이어 처리
                    return PlayerName; // 플레이어 레이어 이름 반환

                case ProjectPhysicsLayer.Ground: // 지면 레이어 처리
                    return GroundName; // 지면 레이어 이름 반환

                case ProjectPhysicsLayer.Obstacle: // 장애물 레이어 처리
                    return ObstacleName; // 장애물 레이어 이름 반환

                case ProjectPhysicsLayer.Checkpoint: // 체크포인트 레이어 처리
                    return CheckpointName; // 체크포인트 레이어 이름 반환

                case ProjectPhysicsLayer.ItemBox: // 아이템 상자 레이어 처리
                    return ItemBoxName; // 아이템 상자 레이어 이름 반환

                case ProjectPhysicsLayer.Interactable: // 상호작용 오브젝트 레이어 처리
                    return InteractableName; // 상호작용 오브젝트 레이어 이름 반환

                case ProjectPhysicsLayer.PushHitbox: // 밀치기 판정 레이어 처리
                    return PushHitboxName; // 밀치기 판정 레이어 이름 반환

                case ProjectPhysicsLayer.RespawnProtection: // 부활 보호 레이어 처리
                    return RespawnProtectionName; // 부활 보호 레이어 이름 반환

                default: // 정의되지 않은 프로젝트 물리 레이어 처리
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, "정의되지 않은 Project J 물리 레이어입니다."); // 잘못된 레이어 값 예외 발생
            }
        }

        public static int GetMask(ProjectPhysicsLayer layer) // 프로젝트 물리 레이어의 단일 비트 마스크 반환
        {
            return 1 << GetIndex(layer); // 레이어 번호를 단일 비트 마스크로 변환하여 반환
        }

        public static bool IsConfigured(ProjectPhysicsLayer layer) // 프로젝트 설정의 레이어 이름과 고정 번호 일치 여부 반환
        {
            int layerIndex = GetIndex(layer); // 검사할 Unity 레이어 번호 조회
            string configuredName = LayerMask.LayerToName(layerIndex); // 현재 프로젝트에 등록된 레이어 이름 조회
            return string.Equals(configuredName, GetName(layer), StringComparison.Ordinal); // 현재 이름과 예상 이름 일치 여부 반환
        }
    }
}
