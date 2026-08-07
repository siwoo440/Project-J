using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Player; // 외부 힘 수신 기능 참조
using UnityEngine; // Unity 투사체 이동과 물리 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 투사체당 효과 한 개만 허용
    public sealed class ItemProjectileEffect : MonoBehaviour // 직선 아이템 투사체 효과 선언
    { // 아이템 투사체 효과 묶음
        [SerializeField] private LayerMask collisionLayers = ~0; // 투사체 충돌 검사 Layer 저장

        private ItemEffectType effectType; // 투사체 아이템 효과 종류 저장
        private Transform ownerRoot; // 투사체 사용자 루트 저장
        private Vector3 direction; // 투사체 진행 방향 저장
        private float speed; // 투사체 초당 이동 거리 저장
        private float primaryValue; // 밀치기와 상태 핵심 수치 저장
        private float statusDuration; // 상태 이상 유지 시간 저장
        private float remainingLifeTime; // 투사체 남은 수명 저장
        private float radius; // 투사체 충돌 반지름 저장

        public void Configure(ItemEffectType newEffectType, Transform newOwnerRoot, Vector3 newDirection, float newSpeed, float newPrimaryValue, float newStatusDuration, float newLifeTime, float newRadius, Color visualColor, LayerMask newCollisionLayers) // 투사체 이동과 효과와 표시 구성
        { // 투사체 구성 처리
            effectType = newEffectType; // 투사체 아이템 효과 종류 저장
            ownerRoot = newOwnerRoot; // 투사체 사용자 루트 저장
            direction = newDirection.sqrMagnitude <= 0.0001f ? Vector3.forward : newDirection.normalized; // 안전한 진행 방향 저장
            speed = Mathf.Max(0f, newSpeed); // 음수가 없는 투사체 속도 저장
            primaryValue = newPrimaryValue; // 효과 핵심 수치 저장
            statusDuration = Mathf.Max(0f, newStatusDuration); // 상태 이상 시간 저장
            remainingLifeTime = Mathf.Max(0.1f, newLifeTime); // 최소 투사체 수명 저장
            radius = Mathf.Max(0.05f, newRadius); // 최소 충돌 반지름 저장
            collisionLayers = newCollisionLayers; // 충돌 검사 Layer 저장
            CreateVisual(visualColor); // 아이템 대표 색상 기반 임시 표시 생성
        } // 투사체 구성 처리 종료

        private void Update() // 투사체 이동과 충돌 갱신
        { // 투사체 프레임 처리
            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            float travelDistance = speed * deltaTime; // 현재 프레임 이동 거리 계산

            if (TryFindHit(travelDistance, out RaycastHit hit)) // 현재 이동 구간 충돌 여부 확인
            { // 투사체 충돌 처리
                ApplyHit(hit.collider); // 충돌 대상에 아이템 효과 적용
                transform.position = hit.point; // 투사체를 충돌 지점으로 이동
                Destroy(gameObject); // 사용 완료 투사체 제거
                return; // 남은 이동과 수명 처리 생략
            } // 투사체 충돌 처리 종료

            transform.position += direction * travelDistance; // 충돌 없는 현재 프레임 직선 이동 적용
            remainingLifeTime = Mathf.Max(0f, remainingLifeTime - deltaTime); // 남은 투사체 수명 감소

            if (remainingLifeTime <= 0f) // 투사체 수명 종료 여부 확인
            { // 투사체 자동 제거 처리
                Destroy(gameObject); // 만료된 투사체 제거
            } // 투사체 자동 제거 처리 종료
        } // 투사체 프레임 처리 종료

        private bool TryFindHit(float travelDistance, out RaycastHit validHit) // 사용자 자신을 제외한 가장 가까운 충돌 검색
        { // 투사체 충돌 검색 처리
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, direction, travelDistance, collisionLayers, QueryTriggerInteraction.Ignore); // 현재 이동 구간 모든 구체 충돌 수집
            float closestDistance = float.PositiveInfinity; // 가장 가까운 충돌 거리 초기화
            validHit = default; // 충돌 없음 기본 결과 설정

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++) // 모든 충돌 결과 순회
            { // 현재 충돌 결과 검사 처리
                RaycastHit hit = hits[hitIndex]; // 현재 충돌 결과 조회

                if (hit.collider == null || ownerRoot != null && hit.collider.transform.IsChildOf(ownerRoot)) // 누락 충돌과 사용자 자신 여부 확인
                { // 무시할 충돌 처리
                    continue; // 현재 충돌 생략
                } // 무시할 충돌 처리 종료

                if (hit.distance < closestDistance) // 현재 결과가 더 가까운지 확인
                { // 가장 가까운 충돌 갱신 처리
                    closestDistance = hit.distance; // 새 가장 가까운 거리 저장
                    validHit = hit; // 새 가장 가까운 충돌 저장
                } // 가장 가까운 충돌 갱신 처리 종료
            } // 현재 충돌 결과 검사 처리 종료

            return closestDistance < float.PositiveInfinity; // 유효 충돌 존재 여부 반환
        } // 투사체 충돌 검색 처리 종료

        private void ApplyHit(Collider hitCollider) // 투사체 종류별 적중 효과 적용
        { // 투사체 적중 효과 처리
            if (hitCollider == null) // 충돌 대상 누락 여부 확인
            { // 적중 효과 불가 처리
                return; // 적중 효과 적용 생략
            } // 적중 효과 불가 처리 종료

            PlayerItemEffectController targetEffectController = hitCollider.GetComponentInParent<PlayerItemEffectController>(); // 대상 플레이어 효과 관리자 조회

            if (effectType == ItemEffectType.Snowball) // 눈덩이 투사체 여부 확인
            { // 눈덩이 감속 처리
                targetEffectController?.ApplySlow(statusDuration, primaryValue); // 데이터 기반 감속 시간과 배율 적용
                return; // 다른 투사체 효과 처리 생략
            } // 눈덩이 감속 처리 종료

            if (effectType == ItemEffectType.InkOctopus) // 먹물 문어 투사체 여부 확인
            { // 먹물 화면 방해 처리
                targetEffectController?.ApplyInk(statusDuration, primaryValue); // 화면 중앙 먹물 가림 효과 적용
                return; // 다른 투사체 효과 처리 생략
            } // 먹물 화면 방해 처리 종료

            if (effectType == ItemEffectType.SoapBubble) // 비눗방울 투사체 여부 확인
            { // 비눗방울 조작 제한 처리
                targetEffectController?.ApplySoapBubble(Mathf.RoundToInt(primaryValue)); // A와 D 교대 탈출 횟수 적용
                return; // 다른 투사체 효과 처리 생략
            } // 비눗방울 조작 제한 처리 종료

            if (effectType == ItemEffectType.Ball) // 풀 공 투사체 여부 확인
            { // 풀 공 약한 밀치기 처리
                ExternalForceReceiver receiver = hitCollider.GetComponentInParent<ExternalForceReceiver>(); // 충돌 대상 외부 힘 수신기 조회

                if (receiver != null && receiver.CanReceivePush) // 유효한 밀치기 대상 여부 확인
                { // 풀 공 밀치기 적용 처리
                    receiver.TryReceiveExternalForce(direction, Mathf.Max(0f, primaryValue)); // 진행 방향 약한 공통 밀치기 힘 적용
                } // 풀 공 밀치기 적용 처리 종료
            } // 풀 공 약한 밀치기 처리 종료
        } // 투사체 적중 효과 처리 종료

        private void CreateVisual(Color visualColor) // 투사체 임시 구체 표시 생성
        { // 투사체 표시 생성 처리
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 투사체 임시 구체 생성
            visualObject.name = "Visual"; // 투사체 표시 이름 지정
            visualObject.transform.SetParent(transform, false); // 투사체 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.zero; // 투사체 중심에 표시 배치
            visualObject.transform.localScale = Vector3.one * radius * 2f; // 충돌 반지름과 같은 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 구체 Collider 조회

            if (visualCollider != null) // 임시 구체 Collider 존재 여부 확인
            { // 불필요 Collider 제거 처리
                Destroy(visualCollider); // SphereCast와 중복되는 Collider 제거
            } // 불필요 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 임시 표시 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 투사체 색상 적용 처리
                visualRenderer.material.color = visualColor; // 아이템 대표 색상 적용
            } // 투사체 색상 적용 처리 종료
        } // 투사체 표시 생성 처리 종료
    } // 아이템 투사체 효과 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
