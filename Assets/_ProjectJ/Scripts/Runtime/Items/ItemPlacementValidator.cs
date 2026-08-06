using UnityEngine; // Unity 물리 검사 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 공통 검사기 중복 방지
    public sealed class ItemPlacementValidator : MonoBehaviour // 설치형 아이템 공통 위치 검사기 선언
    { // 설치형 아이템 공통 위치 검사기 묶음
        [SerializeField] private LayerMask groundMask = ~0; // 설치 가능한 지면 Layer 저장
        [SerializeField] private LayerMask blockingMask = ~0; // 설치를 막는 Collider Layer 저장
        [SerializeField, Min(0.1f)] private float groundProbeHeight = 3f; // 후보 위치 위쪽 지면 검사 시작 높이
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 8f; // 아래 방향 최대 지면 검사 거리
        [SerializeField, Range(0f, 89f)] private float maximumSlopeAngle = 35f; // 허용 최대 지면 경사
        [SerializeField, Min(0f)] private float surfaceClearance = 0.03f; // 지면과 설치 물체 사이 여유 높이
        [SerializeField] private bool drawLastCheckGizmo = true; // 최근 검사 영역 기즈모 표시 여부

        private ItemPlacementResult lastResult; // 최근 설치 위치 검사 결과 저장
        private Vector3 lastHalfExtents = Vector3.one * 0.5f; // 최근 검사 절반 크기 저장
        private Quaternion lastRotation = Quaternion.identity; // 최근 검사 회전값 저장

        public ItemPlacementResult LastResult => lastResult; // 최근 설치 위치 검사 결과 반환

        public bool TryValidate(Vector3 requestedPosition, Vector3 halfExtents, Quaternion rotation, Transform ignoredRoot, out ItemPlacementResult result) // 공통 지면과 경사와 장애물 검사 실행
        { // 설치 위치 공통 검사 처리
            Vector3 safeHalfExtents = new Vector3(Mathf.Max(0.01f, halfExtents.x), Mathf.Max(0.01f, halfExtents.y), Mathf.Max(0.01f, halfExtents.z)); // 검사 크기 양수 보정
            Vector3 rayOrigin = requestedPosition + Vector3.up * groundProbeHeight; // 후보 위치 위쪽 Ray 시작점 계산
            float rayDistance = Mathf.Max(groundProbeHeight + 0.1f, groundProbeDistance); // 최소 지면 검사 거리 보정

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, rayDistance, groundMask, QueryTriggerInteraction.Ignore)) // 아래 방향 지면 검출 시도
            { // 지면 미검출 처리
                result = ItemPlacementResult.CreateFailure(ItemPlacementFailureReason.NoGround, requestedPosition); // 지면 없음 실패 결과 생성
                StoreLastResult(result, safeHalfExtents, rotation); // 최근 검사 정보 저장
                return false; // 설치 불가 반환
            } // 지면 미검출 처리 종료

            if (!ItemPlacementRules.IsSlopeAllowed(groundHit.normal, maximumSlopeAngle)) // 지면 경사 허용 여부 확인
            { // 과도한 경사 처리
                result = new ItemPlacementResult(false, ItemPlacementFailureReason.SlopeTooSteep, groundHit.point, groundHit.normal, groundHit.collider); // 경사 초과 실패 결과 생성
                StoreLastResult(result, safeHalfExtents, rotation); // 최근 검사 정보 저장
                return false; // 설치 불가 반환
            } // 과도한 경사 처리 종료

            Vector3 placementPosition = groundHit.point + groundHit.normal * surfaceClearance; // 지면 위 최종 설치 기준점 계산
            Vector3 overlapCenter = placementPosition + Vector3.up * safeHalfExtents.y; // 설치 물체 중심 검사 위치 계산
            Collider[] overlaps = Physics.OverlapBox(overlapCenter, safeHalfExtents, rotation, blockingMask, QueryTriggerInteraction.Ignore); // 설치 공간 장애물 Collider 수집

            for (int colliderIndex = 0; colliderIndex < overlaps.Length; colliderIndex++) // 감지된 Collider 전체 순회
            { // 현재 Collider 차단 여부 확인 처리
                Collider overlap = overlaps[colliderIndex]; // 현재 감지 Collider 조회

                if (overlap == null || overlap == groundHit.collider) // 누락 Collider와 감지 지면 여부 확인
                { // 무시할 Collider 처리
                    continue; // 현재 Collider 차단 검사 생략
                } // 무시할 Collider 처리 종료

                if (ignoredRoot != null && overlap.transform.IsChildOf(ignoredRoot)) // 검사 제외 루트 하위 Collider 여부 확인
                { // 제외 루트 Collider 처리
                    continue; // 현재 Collider 차단 검사 생략
                } // 제외 루트 Collider 처리 종료

                result = new ItemPlacementResult(false, ItemPlacementFailureReason.Blocked, placementPosition, groundHit.normal, groundHit.collider); // 장애물 점유 실패 결과 생성
                StoreLastResult(result, safeHalfExtents, rotation); // 최근 검사 정보 저장
                return false; // 설치 불가 반환
            } // 현재 Collider 차단 여부 확인 처리 종료

            result = ItemPlacementResult.CreateSuccess(placementPosition, groundHit.normal, groundHit.collider); // 모든 조건을 통과한 성공 결과 생성
            StoreLastResult(result, safeHalfExtents, rotation); // 최근 검사 정보 저장
            return true; // 설치 가능 반환
        } // 설치 위치 공통 검사 처리 종료

        public bool TryValidateInsideBounds(Vector3 requestedPosition, Vector3 halfExtents, Quaternion rotation, Transform ignoredRoot, Bounds allowedBounds, float edgePadding, out ItemPlacementResult result) // 허용 영역을 포함한 공통 설치 검사 실행
        { // 허용 영역 포함 설치 검사 처리
            if (!ItemPlacementRules.IsInsideBounds(requestedPosition, allowedBounds, edgePadding)) // 후보 위치 허용 영역 포함 여부 확인
            { // 허용 영역 이탈 처리
                result = ItemPlacementResult.CreateFailure(ItemPlacementFailureReason.OutsideAllowedArea, requestedPosition); // 영역 이탈 실패 결과 생성
                StoreLastResult(result, halfExtents, rotation); // 최근 검사 정보 저장
                return false; // 설치 불가 반환
            } // 허용 영역 이탈 처리 종료

            return TryValidate(requestedPosition, halfExtents, rotation, ignoredRoot, out result); // 지면과 경사와 장애물 공통 검사 결과 반환
        } // 허용 영역 포함 설치 검사 처리 종료

        private void StoreLastResult(ItemPlacementResult result, Vector3 halfExtents, Quaternion rotation) // 최근 검사 결과와 영역 저장
        { // 최근 검사 정보 저장 처리
            lastResult = result; // 최근 검사 결과 저장
            lastHalfExtents = halfExtents; // 최근 검사 절반 크기 저장
            lastRotation = rotation; // 최근 검사 회전값 저장
        } // 최근 검사 정보 저장 처리 종료

        private void OnDrawGizmosSelected() // Scene 선택 시 최근 설치 검사 영역 표시
        { // 최근 설치 검사 기즈모 처리
            if (!drawLastCheckGizmo) // 기즈모 표시 비활성화 여부 확인
            { // 기즈모 표시 생략 처리
                return; // 최근 검사 기즈모 처리 중단
            } // 기즈모 표시 생략 처리 종료

            Matrix4x4 previousMatrix = Gizmos.matrix; // 기존 기즈모 행렬 저장
            Vector3 center = lastResult.Position + Vector3.up * lastHalfExtents.y; // 최근 설치 검사 중심 계산
            Gizmos.matrix = Matrix4x4.TRS(center, lastRotation, Vector3.one); // 최근 위치와 회전 기반 기즈모 행렬 적용
            Gizmos.color = lastResult.IsValid ? new Color(0.2f, 1f, 0.35f, 0.85f) : new Color(1f, 0.2f, 0.2f, 0.85f); // 성공과 실패 상태별 기즈모 색상 적용
            Gizmos.DrawWireCube(Vector3.zero, lastHalfExtents * 2f); // 최근 설치 검사 Box 영역 표시
            Gizmos.matrix = previousMatrix; // 기존 기즈모 행렬 복원
        } // 최근 설치 검사 기즈모 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(LayerMask newGroundMask, LayerMask newBlockingMask, float newGroundProbeHeight, float newGroundProbeDistance, float newMaximumSlopeAngle, float newSurfaceClearance) // 자동 설정 도구용 공통 검사기 설정
        { // 자동 설정 도구용 검사기 설정 처리
            groundMask = newGroundMask; // 설치 가능 지면 Layer 저장
            blockingMask = newBlockingMask; // 설치 차단 Layer 저장
            groundProbeHeight = Mathf.Max(0.1f, newGroundProbeHeight); // 지면 검사 시작 높이 보정 후 저장
            groundProbeDistance = Mathf.Max(0.1f, newGroundProbeDistance); // 지면 검사 거리 보정 후 저장
            maximumSlopeAngle = Mathf.Clamp(newMaximumSlopeAngle, 0f, 89f); // 허용 경사 안전 범위 보정 후 저장
            surfaceClearance = Mathf.Max(0f, newSurfaceClearance); // 지면 여유 높이 보정 후 저장
        } // 자동 설정 도구용 검사기 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 설치형 아이템 공통 위치 검사기 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
