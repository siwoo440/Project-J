using UnityEngine; // Unity 물리와 벡터 기능 참조

namespace ProjectJ.Player // 플레이어 카메라 기능 네임스페이스 선언
{ // 카메라 충돌 탐지 범위
    public sealed class ThirdPersonCameraCollisionProbe // 카메라 벽 충돌 탐지 도구 선언
    { // 카메라 충돌 탐지 기능 범위
        private const int HitBufferCapacity = 16; // 카메라 충돌 결과 최대 개수
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferCapacity]; // 할당 없는 충돌 결과 버퍼
        private readonly Transform ignoredRoot; // 충돌 검사에서 제외할 플레이어 루트
        private readonly int collisionLayers; // 카메라 충돌 대상 레이어 마스크
        private readonly float probeRadius; // 카메라 충돌 구체 반지름

        public ThirdPersonCameraCollisionProbe(Transform ignoredRoot, int collisionLayers, float probeRadius) // 카메라 충돌 탐지 설정 생성
        { // 카메라 충돌 탐지 생성 범위
            this.ignoredRoot = ignoredRoot; // 제외할 플레이어 루트 저장
            this.collisionLayers = collisionLayers; // 충돌 대상 레이어 저장
            this.probeRadius = Mathf.Max(0.01f, probeRadius); // 유효한 충돌 반지름 저장
        } // 카메라 충돌 탐지 생성 범위 종료

        public bool TryGetClosestHitDistance(Vector3 origin, Vector3 direction, float maximumDistance, out float hitDistance) // 피벗에서 카메라 방향의 가장 가까운 벽 거리 탐지
        { // 가장 가까운 충돌 탐지 범위
            hitDistance = Mathf.Max(0f, maximumDistance); // 충돌 없음 기본 거리 저장

            if (direction.sqrMagnitude <= 0.0001f || maximumDistance <= 0f) // 방향과 거리 유효성 확인
            { // 잘못된 탐지 요청 범위
                return false; // 충돌 없음 반환
            } // 잘못된 탐지 요청 범위 종료

            Vector3 normalizedDirection = direction.normalized; // 카메라 검사 방향 정규화
            int hitCount = Physics.SphereCastNonAlloc(origin, probeRadius, normalizedDirection, hitBuffer, maximumDistance, collisionLayers, QueryTriggerInteraction.Ignore); // 피벗에서 카메라 방향 구체 충돌 검사
            bool foundValidHit = false; // 유효 충돌 발견 여부 초기화

            for (int index = 0; index < hitCount; index++) // 충돌 결과 순회
            { // 충돌 결과 순회 범위
                RaycastHit hit = hitBuffer[index]; // 현재 충돌 결과 조회

                if (ShouldIgnoreCollider(hit.collider)) // 플레이어 자체 충돌체 제외 여부 확인
                { // 제외 충돌체 범위
                    continue; // 현재 충돌 결과 생략
                } // 제외 충돌체 범위 종료

                if (hit.distance >= hitDistance) // 기존 충돌보다 먼 결과 확인
                { // 먼 충돌 결과 범위
                    continue; // 더 먼 충돌 결과 생략
                } // 먼 충돌 결과 범위 종료

                hitDistance = Mathf.Max(0f, hit.distance); // 가장 가까운 충돌 거리 저장
                foundValidHit = true; // 유효 충돌 발견 상태 저장
            } // 충돌 결과 순회 범위 종료

            return foundValidHit; // 가장 가까운 충돌 발견 여부 반환
        } // 가장 가까운 충돌 탐지 범위 종료

        private bool ShouldIgnoreCollider(Collider hitCollider) // 플레이어 자체 충돌체 제외 여부 판정
        { // 충돌체 제외 판정 범위
            if (hitCollider == null) // 빈 충돌체 확인
            { // 빈 충돌체 범위
                return true; // 빈 충돌체 제외 반환
            } // 빈 충돌체 범위 종료

            if (ignoredRoot == null) // 제외 루트 미지정 확인
            { // 제외 루트 없음 범위
                return false; // 모든 실제 충돌체 사용 반환
            } // 제외 루트 없음 범위 종료

            return hitCollider.transform == ignoredRoot || hitCollider.transform.IsChildOf(ignoredRoot); // 플레이어 루트와 자식 충돌체 제외 결과 반환
        } // 충돌체 제외 판정 범위 종료
    } // 카메라 충돌 탐지 기능 범위 종료
} // 카메라 충돌 탐지 범위 종료
