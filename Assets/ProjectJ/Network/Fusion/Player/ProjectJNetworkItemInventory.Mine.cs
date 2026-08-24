using Fusion; // NetworkObject와 PlayerRef 사용
using ProjectJ.Checkpoint; // 시작 체크포인트 ID 사용
using ProjectJ.Finish; // FINISH Trigger 설치 차단 사용
using ProjectJ.Items; // 지뢰 공통 정책 사용
using ProjectJ.Items.Placement; // 공통 설치 금지 구역 검사 사용
using UnityEngine; // 물리 Raycast와 Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 지뢰 네트워크 기능
    {
        private const string MineResourcePath = "ProjectJNetworkMine"; // 지뢰 Resources 경로

        private NetworkObject minePrefab; // 불러온 지뢰 Network Prefab

        private bool UseMineAuthority() // 서버 권한 지뢰 설치
        {
            if (
                Runner == null || // Runner 누락 조건
                !Runner.IsServer || // Host·Server 실행 조건
                Object == null || // 사용자 NetworkObject 누락 조건
                !Object.IsValid || // 사용자 NetworkObject 무효 조건
                !Object.HasStateAuthority || // 서버 권한 누락 조건
                externalGameplay == null || // 경기 상태 누락 조건
                !externalGameplay.GameplayInputAllowed // 경기 입력 잠금 조건
            )
            {
                return false; // 잘못된 설치 요청 차단
            }

            NetworkObject resolvedMinePrefab = ResolveMinePrefab(); // 지뢰 Prefab 조회

            if (resolvedMinePrefab == null) // Prefab 누락 확인
            {
                Debug.LogError( // Prefab 누락 로그 출력
                    "[Project J/Fusion] 110일차 지뢰 Prefab을 찾을 수 없음", // 오류 내용
                    this // 로그 대상
                );

                return false; // 아이템 소비 차단
            }

            Vector3 forward = transform.forward; // 사용자 전방 조회
            forward.y = 0f; // 설치 방향 수평화

            if (forward.sqrMagnitude <= 0.0001f) // 잘못된 전방 확인
            {
                forward = Vector3.forward; // 기본 전방 사용
            }

            forward.Normalize(); // 일정한 설치 거리 유지

            Vector3 rayOrigin =
                transform.position + // 사용자 현재 위치
                forward * ProjectJMinePolicy.PlacementForwardDistance + // 전방 설치 거리
                Vector3.up * ProjectJMinePolicy.PlacementRayStartHeight; // Ray 시작 높이

            bool groundFound = Physics.Raycast( // 설치 지면 검색
                rayOrigin, // Ray 시작점
                Vector3.down, // 아래 방향
                out RaycastHit hit, // 지면 충돌 정보
                ProjectJMinePolicy.PlacementRayDistance, // Ray 최대 거리
                Physics.DefaultRaycastLayers, // 기본 물리 Layer
                QueryTriggerInteraction.Ignore // Trigger 제외
            );

            bool groundIsWorld =
                groundFound && // 지면 존재 조건
                hit.collider != null && // Collider 존재 조건
                hit.collider.attachedRigidbody == null && // 이동 물체·Pickup 위 설치 차단
                hit.collider.GetComponentInParent<ProjectJNetworkExternalGameplay>() == null && // Player 위 설치 차단
                hit.collider.GetComponentInParent<ProjectJNetworkMine>() == null; // 지뢰 위 설치 차단

            float groundDot = groundFound // 지면 각도 계산 조건
                ? Vector3.Dot(hit.normal, Vector3.up) // 지면 위쪽 각도 값
                : -1f; // 지면 없음 값

            Vector3 placementPosition = groundFound // 설치 위치 계산 조건
                ? hit.point + hit.normal * 0.1f // 지면 위쪽 보정 위치
                : Vector3.zero; // 지면 없음 대체 위치

            Vector3 placementSize = new Vector3( // 공통 금지 구역 검사 크기
                ProjectJMinePolicy.PlacementWidth, // 설치 공간 X 크기
                ProjectJMinePolicy.PlacementHeight, // 설치 공간 Y 크기
                ProjectJMinePolicy.PlacementWidth // 설치 공간 Z 크기
            );

            Bounds placementBounds = new Bounds( // 공통 설치 영역 생성
                placementPosition + Vector3.up * (placementSize.y * 0.5f), // 영역 중심 위치
                placementSize // 영역 크기
            );

            bool commonPlacementAllowed =
                groundIsWorld && // World 지면 조건
                ItemPlacementValidator.CanPlace(placementBounds) && // Checkpoint·Respawn·금지 구역 검사
                !IntersectsFinishTrigger(placementBounds) && // FINISH 구역 검사
                !IsNearFusionStartPosition(placementPosition); // Fusion 시작 부활 위치 검사

            bool separatedFromMines =
                groundFound && // 거리 계산 가능 조건
                IsSeparatedFromExistingMines(placementPosition); // 기존 지뢰 최소 간격 검사

            bool canPlace = ProjectJMinePolicy.CanPlace( // 최종 설치 정책 계산
                groundFound, // 지면 검색 결과 전달
                groundDot, // 지면 각도 전달
                commonPlacementAllowed, // 공통 구역 결과 전달
                separatedFromMines // 지뢰 간격 결과 전달
            );

            if (!canPlace) // 최종 설치 불가 확인
            {
                return false; // 아이템 소비 차단
            }

            Quaternion placementRotation = Quaternion.FromToRotation( // 지면 법선 정렬 회전 계산
                Vector3.up, // Prefab 기본 위쪽 방향
                hit.normal.normalized // 설치 지면 법선
            );

            NetworkObject mineObject = Runner.Spawn( // 서버 지뢰 NetworkObject 생성
                resolvedMinePrefab, // 지뢰 Prefab
                placementPosition, // 확정 설치 위치
                placementRotation, // 지면 정렬 회전
                Object.InputAuthority // 설치 사용자 Input Authority
            );

            if (mineObject == null) // Spawn 실패 확인
            {
                return false; // 아이템 소비 차단
            }

            ProjectJNetworkMine mine = mineObject.GetComponent<ProjectJNetworkMine>(); // 지뢰 동작 Component 조회

            if (
                mine == null || // 지뢰 Component 누락 조건
                !mine.ConfigureAuthority(Object.InputAuthority, forward) // 서버 상태 초기화 실패 조건
            )
            {
                Runner.Despawn(mineObject); // 잘못 생성된 NetworkObject 제거
                return false; // 아이템 소비 차단
            }

            return true; // 설치 성공 반환
        }

        private bool IsSeparatedFromExistingMines( // 기존 지뢰 최소 간격 검사
            Vector3 placementPosition // 새 지뢰 설치 위치
        )
        {
            ProjectJNetworkMine[] existingMines = // Scene 지뢰 후보 조회
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkMine>( // 활성 지뢰 검색
                    FindObjectsInactive.Exclude, // 비활성 지뢰 제외
                    FindObjectsSortMode.None // 불필요한 정렬 제외
                );

            for (int index = 0; index < existingMines.Length; index++) // 모든 기존 지뢰 순회
            {
                ProjectJNetworkMine existingMine = existingMines[index]; // 현재 지뢰 조회

                if (
                    existingMine == null || // 지뢰 누락 조건
                    existingMine.Runner != Runner || // 다른 Runner 지뢰 제외
                    !existingMine.IsInitialized || // 설치 미완료 조건
                    existingMine.HasExploded // 폭발 완료 조건
                )
                {
                    continue; // 간격 대상 제외
                }

                float distance = Vector3.Distance( // 두 지뢰 거리 계산
                    placementPosition, // 새 지뢰 위치
                    existingMine.transform.position // 기존 지뢰 위치
                );

                if (!ProjectJMinePolicy.IsSeparatedFromMine(distance)) // 최소 간격 미달 확인
                {
                    return false; // 설치 중첩 차단
                }
            }

            return true; // 모든 지뢰 간격 통과
        }

        private bool IsNearFusionStartPosition( // Fusion 시작 부활 위치 검사
            Vector3 placementPosition // 새 지뢰 설치 위치
        )
        {
            ProjectJNetworkExternalGameplay[] players = // Scene Network Player 후보 조회
                UnityEngine.Object.FindObjectsByType<ProjectJNetworkExternalGameplay>( // 활성 Player 검색
                    FindObjectsInactive.Exclude, // 비활성 Player 제외
                    FindObjectsSortMode.None // 불필요한 정렬 제외
                );

            for (int index = 0; index < players.Length; index++) // 모든 Player 후보 순회
            {
                ProjectJNetworkExternalGameplay player = players[index]; // 현재 Player 조회

                if (
                    player == null || // Player 누락 조건
                    player.Runner != Runner || // 다른 Runner Player 제외
                    player.CurrentCheckpointId != CheckpointId.Start // 시작 체크포인트 외 제외
                )
                {
                    continue; // 보호 위치 후보 제외
                }

                if (ProjectJMinePolicy.IsInsideProtectedStartRadius( // 시작 부활 위치 보호 범위 계산
                    placementPosition, // 설치 위치 전달
                    player.RespawnPosition // Fusion 시작 부활 위치 전달
                ))
                {
                    return true; // 시작 위치 근접 설치 차단
                }
            }

            return false; // Fusion 시작 위치와 충분한 거리
        }

        private bool IntersectsFinishTrigger( // FINISH 구역 설치 차단 검사
            Bounds placementBounds // 새 지뢰 설치 영역
        )
        {
            FinishTrigger[] finishTriggers = // Scene FINISH 후보 조회
                UnityEngine.Object.FindObjectsByType<FinishTrigger>( // 활성 FINISH Trigger 검색
                    FindObjectsInactive.Exclude, // 비활성 Trigger 제외
                    FindObjectsSortMode.None // 불필요한 정렬 제외
                );

            for (int index = 0; index < finishTriggers.Length; index++) // 모든 FINISH 후보 순회
            {
                FinishTrigger finishTrigger = finishTriggers[index]; // 현재 FINISH Trigger 조회
                Collider finishCollider = finishTrigger != null // Trigger 존재 조건
                    ? finishTrigger.GetComponent<Collider>() // FINISH Collider 조회
                    : null; // Trigger 누락 상태

                if (finishCollider == null) // Collider 누락 확인
                {
                    continue; // 설치 차단 후보 제외
                }

                Bounds protectedBounds = finishCollider.bounds; // FINISH 실제 영역 복사
                protectedBounds.Expand(new Vector3(2.5f, 1f, 2.5f)); // 주변 설치 여유 공간 추가

                if (protectedBounds.Intersects(placementBounds)) // 설치 영역 침범 확인
                {
                    return true; // FINISH 근처 설치 차단
                }
            }

            return false; // FINISH 영역과 겹치지 않음
        }

        private NetworkObject ResolveMinePrefab() // Resources 지뢰 Prefab 조회
        {
            if (minePrefab == null) // 기존 Cache 확인
            {
                GameObject minePrefabObject = Resources.Load<GameObject>( // Resources Prefab 불러오기
                    MineResourcePath // Resources 내부 경로
                );

                minePrefab = minePrefabObject != null // Prefab 존재 조건
                    ? minePrefabObject.GetComponent<NetworkObject>() // NetworkObject Component 저장
                    : null; // Prefab 누락 상태 저장
            }

            return minePrefab; // 조회된 Prefab 반환
        }
    }
}
