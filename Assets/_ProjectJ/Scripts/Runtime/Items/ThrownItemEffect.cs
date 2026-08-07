using System.Collections.Generic; // 폭발 대상 중복 방지 목록 기능 참조
using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Player; // 외부 힘 수신 기능 참조
using UnityEngine; // Unity 투척 이동과 물리 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 투척물당 효과 한 개만 허용
    public sealed class ThrownItemEffect : MonoBehaviour // 폭탄과 연막탄 투척 효과 선언
    { // 투척 아이템 효과 묶음
        private ItemEffectType effectType; // 투척 아이템 효과 종류 저장
        private Transform ownerRoot; // 투척 사용자 루트 저장
        private Vector3 velocity; // 현재 투척 이동 속도 저장
        private float gravity; // 투척물 중력 가속도 저장
        private float fuseRemaining; // 작동까지 남은 시간 저장
        private float effectDuration; // 생성 효과 유지 시간 저장
        private float effectRadius; // 폭발 또는 연막 반경 저장
        private float force; // 폭발 밀치기 힘 저장
        private float collisionRadius; // 투척물 충돌 반지름 저장
        private Color visualColor; // 후속 효과 대표 색상 저장
        private LayerMask collisionLayers; // 투척 충돌과 효과 대상 Layer 저장
        private bool stopped; // 지면 또는 장애물 충돌 정지 여부 저장

        public void Configure(ItemEffectType newEffectType, Transform newOwnerRoot, Vector3 origin, Vector3 direction, float throwSpeed, float upwardSpeed, float newGravity, float fuseTime, float newEffectDuration, float newEffectRadius, float newForce, float newCollisionRadius, Color newVisualColor, LayerMask newCollisionLayers) // 투척 이동과 후속 효과 구성
        { // 투척물 구성 처리
            effectType = newEffectType; // 폭탄 또는 연막탄 효과 종류 저장
            ownerRoot = newOwnerRoot; // 투척 사용자 루트 저장
            transform.position = origin; // 투척 시작 위치 적용
            Vector3 safeDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized; // 안전한 투척 방향 보정
            velocity = safeDirection * Mathf.Max(0f, throwSpeed) + Vector3.up * Mathf.Max(0f, upwardSpeed); // 전방과 위쪽 초기 속도 계산
            gravity = Mathf.Min(0f, newGravity); // 아래쪽 방향 중력 보정
            fuseRemaining = Mathf.Max(0.05f, fuseTime); // 최소 작동 대기 시간 보정
            effectDuration = Mathf.Max(0.1f, newEffectDuration); // 후속 효과 최소 유지 시간 보정
            effectRadius = Mathf.Max(0.1f, newEffectRadius); // 폭발 또는 연막 최소 반경 보정
            force = Mathf.Max(0f, newForce); // 음수가 없는 폭발 힘 보정
            collisionRadius = Mathf.Max(0.05f, newCollisionRadius); // 최소 투척 충돌 반지름 보정
            visualColor = newVisualColor; // 아이템 대표 색상 저장
            collisionLayers = newCollisionLayers; // 충돌과 효과 대상 Layer 저장
            CreateVisual(); // 투척물 임시 표시 생성
        } // 투척물 구성 처리 종료

        private void Update() // 투척 이동과 작동 시간 갱신
        { // 투척물 프레임 처리
            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산

            if (!stopped) // 아직 충돌하지 않은 투척물 여부 확인
            { // 투척 이동 처리
                MoveProjectile(deltaTime); // 중력과 충돌을 반영한 투척 이동 실행
            } // 투척 이동 처리 종료

            fuseRemaining = Mathf.Max(0f, fuseRemaining - deltaTime); // 남은 작동 시간 감소

            if (fuseRemaining <= 0f) // 폭발 또는 연막 생성 시간 도달 여부 확인
            { // 투척 아이템 작동 처리
                Detonate(); // 현재 위치에서 후속 효과 실행
            } // 투척 아이템 작동 처리 종료
        } // 투척물 프레임 처리 종료

        private void MoveProjectile(float deltaTime) // 중력과 구체 판정을 반영한 투척 이동
        { // 투척 이동 처리
            velocity += Vector3.up * gravity * deltaTime; // 현재 속도에 중력 가속도 적용
            Vector3 displacement = velocity * deltaTime; // 현재 프레임 이동 벡터 계산
            float distance = displacement.magnitude; // 현재 프레임 이동 거리 계산

            if (distance <= 0.0001f) // 이동 거리 없음 여부 확인
            { // 이동 생략 처리
                return; // 투척 이동 종료
            } // 이동 생략 처리 종료

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, collisionRadius, displacement.normalized, distance, collisionLayers, QueryTriggerInteraction.Ignore); // 현재 이동 구간 모든 충돌 수집
            RaycastHit closestHit = default; // 가장 가까운 충돌 기본값 설정
            float closestDistance = float.PositiveInfinity; // 가장 가까운 충돌 거리 초기화

            for (int index = 0; index < hits.Length; index++) // 모든 충돌 결과 순회
            { // 현재 충돌 결과 확인
                RaycastHit hit = hits[index]; // 현재 충돌 조회

                if (hit.collider == null || ownerRoot != null && hit.collider.transform.IsChildOf(ownerRoot)) // 누락 충돌과 사용자 자신 여부 확인
                { // 무시할 충돌 처리
                    continue; // 현재 충돌 제외
                } // 무시할 충돌 처리 종료

                if (hit.distance < closestDistance) // 더 가까운 충돌 여부 확인
                { // 가장 가까운 충돌 갱신
                    closestDistance = hit.distance; // 새 가장 가까운 거리 저장
                    closestHit = hit; // 새 가장 가까운 충돌 저장
                } // 가장 가까운 충돌 갱신 종료
            } // 현재 충돌 결과 확인 종료

            if (closestDistance < float.PositiveInfinity) // 유효 충돌 존재 여부 확인
            { // 투척물 충돌 정지 처리
                transform.position = closestHit.point + closestHit.normal * collisionRadius; // 충돌 표면 바깥쪽 위치 적용
                velocity = Vector3.zero; // 충돌 뒤 이동 속도 제거
                stopped = true; // 투척 이동 정지 상태 적용
                return; // 남은 이동 생략
            } // 투척물 충돌 정지 처리 종료

            transform.position += displacement; // 충돌 없는 이동 위치 적용
        } // 투척 이동 처리 종료

        private void Detonate() // 투척 아이템 종류별 후속 효과 실행
        { // 투척 아이템 작동 처리
            if (effectType == ItemEffectType.Bomb) // 폭탄 효과 여부 확인
            { // 폭탄 범위 밀치기 처리
                ApplyRadialPush(); // 현재 위치 방사형 밀치기 적용
                Destroy(gameObject); // 작동 완료 폭탄 제거
                return; // 연막 생성 생략
            } // 폭탄 범위 밀치기 처리 종료

            if (effectType == ItemEffectType.SmokeGrenade) // 연막탄 효과 여부 확인
            { // 연막 구역 생성 처리
                GameObject cloudObject = new GameObject("SmokeCloud"); // 연막 구역 루트 생성
                cloudObject.transform.position = transform.position; // 투척물 작동 위치에 연막 배치
                SmokeCloudEffect cloudEffect = cloudObject.AddComponent<SmokeCloudEffect>(); // 연막 Trigger와 화면 방해 기능 추가
                cloudEffect.Configure(effectRadius, effectDuration, visualColor); // 반경 5m와 6초 연막 데이터 연결
                Destroy(gameObject); // 작동 완료 연막탄 투척물 제거
            } // 연막 구역 생성 처리 종료
        } // 투척 아이템 작동 처리 종료

        private void ApplyRadialPush() // 폭탄 반경 안 대상 방사형 밀치기 적용
        { // 폭탄 범위 밀치기 처리
            Collider[] overlaps = Physics.OverlapSphere(transform.position, effectRadius, collisionLayers, QueryTriggerInteraction.Ignore); // 폭탄 반경 안 Collider 수집
            HashSet<int> affectedIds = new HashSet<int>(); // 같은 대상 중복 밀치기 방지 목록 생성

            for (int index = 0; index < overlaps.Length; index++) // 폭발 범위 Collider 순회
            { // 현재 폭발 대상 처리
                ExternalForceReceiver receiver = overlaps[index] == null ? null : overlaps[index].GetComponentInParent<ExternalForceReceiver>(); // 현재 외부 힘 대상 조회

                if (receiver == null || receiver.transform == ownerRoot || !receiver.CanReceivePush || affectedIds.Contains(receiver.GetInstanceID())) // 사용자와 수신 불가와 중복 대상 확인
                { // 무효 폭발 대상 처리
                    continue; // 현재 대상 제외
                } // 무효 폭발 대상 처리 종료

                Vector3 direction = receiver.ForceReceiverTransform.position - transform.position; // 폭탄 중심에서 대상 방향 계산
                direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized + Vector3.up * 0.25f; // 수평과 약한 위쪽 폭발 방향 혼합
                receiver.TryReceiveExternalForce(direction.normalized, force); // 데이터 기반 폭탄 밀치기 적용
                affectedIds.Add(receiver.GetInstanceID()); // 현재 대상 적용 완료 등록
            } // 현재 폭발 대상 처리 종료
        } // 폭탄 범위 밀치기 처리 종료

        private void CreateVisual() // 투척물 임시 구체 표시 생성
        { // 투척물 표시 생성 처리
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 투척물 임시 구체 생성
            visualObject.name = "Visual"; // 투척물 표시 이름 지정
            visualObject.transform.SetParent(transform, false); // 투척물 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.zero; // 투척물 중심에 표시 배치
            visualObject.transform.localScale = Vector3.one * collisionRadius * 2f; // 충돌 반지름과 같은 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 표시 Collider 조회

            if (visualCollider != null) // 임시 Collider 존재 여부 확인
            { // 임시 Collider 제거 처리
                Destroy(visualCollider); // SphereCast와 중복되는 Collider 제거
            } // 임시 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 투척물 표시 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 투척물 색상 적용 처리
                visualRenderer.material.color = visualColor; // 아이템 대표 색상 적용
            } // 투척물 색상 적용 처리 종료
        } // 투척물 표시 생성 처리 종료
    } // 투척 아이템 효과 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
