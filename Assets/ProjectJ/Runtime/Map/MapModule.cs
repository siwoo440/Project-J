using System.Collections.Generic; // 읽기 전용 Socket 목록 기능 사용
using UnityEngine; // Unity 컴포넌트와 Vector 기능 사용

namespace ProjectJ.Map // Project J 맵 기능 네임스페이스
{
    [DisallowMultipleComponent] // 중복 MapModule 부착 방지
    public sealed class MapModule : MonoBehaviour // 정육면체 맵 모듈 공통 데이터
    {
        public const float DefaultModuleSize = 10f; // 기본 정육면체 한 변 크기
        public const float PlayerHeightReference = 2f; // 플레이어 높이 기준값

        [SerializeField] private string moduleId = "Module"; // 모듈 고유 ID
        [SerializeField, Min(0.1f)] private float moduleSize = DefaultModuleSize; // 모듈 한 변 크기
        [SerializeField] private MapModuleSocket[] sockets; // 6방향 Socket 목록

        public string ModuleId => moduleId; // 모듈 ID 반환
        public float ModuleSize => moduleSize; // 모듈 크기 반환
        public IReadOnlyList<MapModuleSocket> Sockets => sockets; // Socket 목록 반환
        public int EntranceCount => CountState(MapModuleFaceState.Entrance); // Entrance 개수 반환
        public int ExitCount => CountState(MapModuleFaceState.Exit); // Exit 개수 반환

        public void Configure(string newModuleId, float newModuleSize, MapModuleSocket[] newSockets) // Editor 생성 데이터 적용
        {
            moduleId = string.IsNullOrWhiteSpace(newModuleId) ? "Module" : newModuleId; // 빈 ID 보정
            moduleSize = Mathf.Max(0.1f, newModuleSize); // 모듈 크기 최소값 보장
            sockets = newSockets; // Socket 목록 저장
        }

        public bool TryGetSocket(MapModuleFaceDirection direction, out MapModuleSocket socket) // 지정 방향 Socket 검색
        {
            socket = null; // 검색 결과 초기화

            if (sockets == null) // Socket 배열 누락 검사
            {
                return false; // 검색 실패 반환
            }

            for (int index = 0; index < sockets.Length; index++) // Socket 전체 순회
            {
                MapModuleSocket candidate = sockets[index]; // 현재 Socket 조회

                if (candidate != null && candidate.Direction == direction) // 방향 일치 검사
                {
                    socket = candidate; // 검색 결과 저장
                    return true; // 검색 성공 반환
                }
            }

            return false; // 대상 Socket 없음 반환
        }

        public bool IsDefinitionValid() // 모듈 기본 정의 검증
        {
            if (sockets == null || sockets.Length != 6) // 6방향 Socket 규칙 검사
            {
                return false; // 잘못된 Socket 구성 반환
            }

            bool[] directionFound = new bool[6]; // 방향 중복 검사 배열 생성
            int entranceCount = 0; // Entrance 개수 초기화
            int exitCount = 0; // Exit 개수 초기화

            for (int index = 0; index < sockets.Length; index++) // Socket 전체 순회
            {
                MapModuleSocket socket = sockets[index]; // 현재 Socket 조회

                if (socket == null) // Socket 누락 검사
                {
                    return false; // 정의 오류 반환
                }

                int directionIndex = (int)socket.Direction; // 방향 인덱스 계산

                if (directionIndex < 0 || directionIndex >= directionFound.Length || directionFound[directionIndex]) // 범위와 중복 검사
                {
                    return false; // 방향 정의 오류 반환
                }

                directionFound[directionIndex] = true; // 현재 방향 사용 기록

                if (socket.State == MapModuleFaceState.Entrance) // Entrance 상태 검사
                {
                    entranceCount++; // Entrance 개수 증가
                }

                if (socket.State == MapModuleFaceState.Exit) // Exit 상태 검사
                {
                    exitCount++; // Exit 개수 증가
                }
            }

            return entranceCount >= 1 && exitCount >= 1; // 최소 Entrance와 Exit 규칙 반환
        }

        private int CountState(MapModuleFaceState state) // 특정 Face 상태 개수 계산
        {
            if (sockets == null) // Socket 배열 누락 검사
            {
                return 0; // 개수 0 반환
            }

            int count = 0; // 상태 개수 초기화

            for (int index = 0; index < sockets.Length; index++) // Socket 전체 순회
            {
                MapModuleSocket socket = sockets[index]; // 현재 Socket 조회

                if (socket != null && socket.State == state) // 상태 일치 검사
                {
                    count++; // 상태 개수 증가
                }
            }

            return count; // 계산 결과 반환
        }

        public static bool CanConnect(MapModuleFaceDirection fromDirection, MapModuleFaceState fromState, MapModuleFaceDirection toDirection, MapModuleFaceState toState) // 두 Face 연결 가능 여부 검사
        {
            return fromState == MapModuleFaceState.Exit && toState == MapModuleFaceState.Entrance && GetOppositeDirection(fromDirection) == toDirection; // Exit와 반대 Entrance 규칙 반환
        }

        public static bool IsFaceStateSetValid(IReadOnlyList<MapModuleFaceState> states) // 6면 상태 집합 기본 검증
        {
            if (states == null || states.Count != 6) // 6면 개수 검사
            {
                return false; // 잘못된 상태 집합 반환
            }

            int entranceCount = 0; // Entrance 개수 초기화
            int exitCount = 0; // Exit 개수 초기화

            for (int index = 0; index < states.Count; index++) // 상태 전체 순회
            {
                if (states[index] == MapModuleFaceState.Entrance) // Entrance 상태 검사
                {
                    entranceCount++; // Entrance 개수 증가
                }

                if (states[index] == MapModuleFaceState.Exit) // Exit 상태 검사
                {
                    exitCount++; // Exit 개수 증가
                }
            }

            return entranceCount >= 1 && exitCount >= 1; // 최소 진행 연결 규칙 반환
        }

        public static MapModuleFaceDirection GetOppositeDirection(MapModuleFaceDirection direction) // 반대 방향 계산
        {
            switch (direction) // 입력 방향 분기
            {
                case MapModuleFaceDirection.North: return MapModuleFaceDirection.South; // 북쪽 반대 반환
                case MapModuleFaceDirection.South: return MapModuleFaceDirection.North; // 남쪽 반대 반환
                case MapModuleFaceDirection.East: return MapModuleFaceDirection.West; // 동쪽 반대 반환
                case MapModuleFaceDirection.West: return MapModuleFaceDirection.East; // 서쪽 반대 반환
                case MapModuleFaceDirection.Up: return MapModuleFaceDirection.Down; // 위쪽 반대 반환
                case MapModuleFaceDirection.Down: return MapModuleFaceDirection.Up; // 아래쪽 반대 반환
                default: return MapModuleFaceDirection.North; // 예외 기본 방향 반환
            }
        }

        public static Vector3Int GetDirectionCellOffset(MapModuleFaceDirection direction) // 3차원 Grid 이동량 계산
        {
            switch (direction) // 입력 방향 분기
            {
                case MapModuleFaceDirection.North: return new Vector3Int(0, 0, 1); // 북쪽 Cell 이동
                case MapModuleFaceDirection.South: return new Vector3Int(0, 0, -1); // 남쪽 Cell 이동
                case MapModuleFaceDirection.East: return new Vector3Int(1, 0, 0); // 동쪽 Cell 이동
                case MapModuleFaceDirection.West: return new Vector3Int(-1, 0, 0); // 서쪽 Cell 이동
                case MapModuleFaceDirection.Up: return new Vector3Int(0, 1, 0); // 위쪽 Cell 이동
                case MapModuleFaceDirection.Down: return new Vector3Int(0, -1, 0); // 아래쪽 Cell 이동
                default: return Vector3Int.zero; // 예외 이동 없음 반환
            }
        }

        public static Vector3 GetDirectionVector(MapModuleFaceDirection direction) // 방향 Vector 계산
        {
            switch (direction) // 입력 방향 분기
            {
                case MapModuleFaceDirection.North: return Vector3.forward; // 북쪽 Vector 반환
                case MapModuleFaceDirection.South: return Vector3.back; // 남쪽 Vector 반환
                case MapModuleFaceDirection.East: return Vector3.right; // 동쪽 Vector 반환
                case MapModuleFaceDirection.West: return Vector3.left; // 서쪽 Vector 반환
                case MapModuleFaceDirection.Up: return Vector3.up; // 위쪽 Vector 반환
                case MapModuleFaceDirection.Down: return Vector3.down; // 아래쪽 Vector 반환
                default: return Vector3.zero; // 예외 Vector 반환
            }
        }
    }
}
