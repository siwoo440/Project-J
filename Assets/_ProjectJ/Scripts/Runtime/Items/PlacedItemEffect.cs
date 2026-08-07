using System.Collections.Generic; // 접촉 대상 중복 방지 목록 기능 참조
using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Player; // 외부 힘 수신 기능 참조
using UnityEngine; // Unity Trigger와 오브젝트 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 설치 오브젝트당 효과 한 개만 허용
    [RequireComponent(typeof(BoxCollider))] // 설치 Trigger Collider 필수 지정
    public sealed class PlacedItemEffect : MonoBehaviour // 바나나 쿠션과 지뢰와 트램폴린 설치 효과 선언
    { // 설치 아이템 효과 묶음
        private readonly HashSet<int> affectedReceiverIds = new HashSet<int>(); // 이미 효과를 받은 대상 번호 저장
        private ItemEffectType effectType; // 설치 아이템 효과 종류 저장
        private Transform ownerRoot; // 설치 사용자 루트 저장
        private Vector3 forwardDirection; // 바나나 미끄러짐 방향 저장
        private float force; // 대상 외부 힘 크기 저장
        private float lifeTime; // 설치물 남은 유지 시간 저장
        private int remainingUses; // 트램폴린 남은 사용 횟수 저장

        public void Configure(ItemEffectType newEffectType, Transform newOwnerRoot, Vector3 newForwardDirection, float newForce, float newLifeTime, Vector3 halfExtents, Color visualColor, int newMaximumUses = 1) // 설치 효과와 임시 표시 구성
        { // 설치 효과 구성 처리
            effectType = newEffectType; // 설치 아이템 효과 종류 저장
            ownerRoot = newOwnerRoot; // 설치 사용자 루트 저장
            forwardDirection = Vector3.ProjectOnPlane(newForwardDirection, Vector3.up).normalized; // 수평 전방 방향 보정 후 저장
            force = Mathf.Max(0f, newForce); // 음수가 없는 힘 저장
            lifeTime = Mathf.Max(0.1f, newLifeTime); // 최소 설치 유지 시간 저장
            remainingUses = Mathf.Max(1, newMaximumUses); // 최소 한 번 이상의 사용 횟수 저장
            BoxCollider trigger = GetComponent<BoxCollider>(); // 설치 Trigger Collider 조회
            trigger.isTrigger = true; // 통과 가능한 접촉 판정 적용
            trigger.center = Vector3.up * halfExtents.y; // 지면 기준 Collider 중심 적용
            trigger.size = halfExtents * 2f; // 설치 검사와 같은 전체 크기 적용
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>(); // CharacterController 접촉 감지용 Rigidbody 추가
            rigidbody.isKinematic = true; // 물리 힘에 움직이지 않는 설치물 적용
            rigidbody.useGravity = false; // 설치물 중력 비활성화
            CreateVisual(halfExtents, visualColor); // 설치 크기와 색상 기반 임시 표시 생성
        } // 설치 효과 구성 처리 종료

        private void Update() // 설치물 유지 시간 갱신
        { // 설치물 수명 처리
            lifeTime = Mathf.Max(0f, lifeTime - Time.deltaTime); // 프레임 시간만큼 남은 수명 감소

            if (lifeTime <= 0f) // 설치물 수명 종료 여부 확인
            { // 설치물 자동 제거 처리
                Destroy(gameObject); // 만료된 설치물 제거
            } // 설치물 자동 제거 처리 종료
        } // 설치물 수명 처리 종료

        private void OnTriggerEnter(Collider other) // 대상 접촉 시 설치 효과 적용
        { // 설치 효과 접촉 처리
            if (other == null) // 누락 접촉 대상 여부 확인
            { // 누락 접촉 처리
                return; // 설치 효과 적용 생략
            } // 누락 접촉 처리 종료

            bool isOwner = ownerRoot != null && (other.transform == ownerRoot || other.transform.IsChildOf(ownerRoot)); // 현재 접촉 대상이 설치 사용자인지 확인

            if (effectType == ItemEffectType.Trampoline) // 트램폴린 효과 여부 확인
            { // 사용자 전용 트램폴린 처리
                TryUseTrampoline(other, isOwner); // 사용자와 남은 횟수 기반 도약 처리
                return; // 다른 설치 효과 처리 생략
            } // 사용자 전용 트램폴린 처리 종료

            if (isOwner) // 바나나 쿠션과 지뢰 사용자의 접촉 여부 확인
            { // 설치 사용자 제외 처리
                return; // 자기 설치 방해 효과 생략
            } // 설치 사용자 제외 처리 종료

            ExternalForceReceiver receiver = other.GetComponentInParent<ExternalForceReceiver>(); // 접촉 대상 외부 힘 수신기 조회

            if (receiver == null || !receiver.CanReceivePush) // 유효한 효과 대상 여부 확인
            { // 효과 대상 없음 처리
                return; // 설치 효과 적용 생략
            } // 효과 대상 없음 처리 종료

            int receiverId = receiver.GetInstanceID(); // 대상 중복 확인용 인스턴스 번호 조회

            if (effectType == ItemEffectType.BananaCushion && affectedReceiverIds.Contains(receiverId)) // 바나나 쿠션 중복 접촉 여부 확인
            { // 중복 바나나 접촉 처리
                return; // 같은 대상 추가 미끄러짐 생략
            } // 중복 바나나 접촉 처리 종료

            if (effectType == ItemEffectType.BananaCushion) // 바나나 쿠션 효과 여부 확인
            { // 전방 미끄러짐 적용 처리
                receiver.TryReceiveExternalForce(forwardDirection, force); // 설치 방향으로 미끄러지는 공통 밀치기 힘 적용
                affectedReceiverIds.Add(receiverId); // 현재 대상 적용 완료 등록
                return; // 지뢰 처리 생략
            } // 전방 미끄러짐 적용 처리 종료

            if (effectType == ItemEffectType.Mine) // 지뢰 효과 여부 확인
            { // 지뢰 폭발 적용 처리
                Vector3 outwardDirection = receiver.ForceReceiverTransform.position - transform.position; // 지뢰에서 대상 바깥쪽 방향 계산
                outwardDirection = Vector3.ProjectOnPlane(outwardDirection, Vector3.up).normalized + Vector3.up * 0.8f; // 바깥쪽과 위쪽 혼합 방향 계산
                receiver.TryReceiveExternalForce(outwardDirection.normalized, force); // 위쪽과 바깥쪽 공통 밀치기 힘 적용
                Destroy(gameObject); // 한 번 작동한 지뢰 제거
            } // 지뢰 폭발 적용 처리 종료
        } // 설치 효과 접촉 처리 종료

        private void TryUseTrampoline(Collider other, bool isOwner) // 사용자 전용 트램폴린 도약 적용
        { // 트램폴린 도약 처리
            if (!isOwner || remainingUses <= 0) // 설치 사용자와 남은 횟수 확인
            { // 트램폴린 사용 차단 처리
                return; // 도약 적용 생략
            } // 트램폴린 사용 차단 처리 종료

            PlayerExternalForceController receiver = other.GetComponentInParent<PlayerExternalForceController>(); // 사용자 외부 힘 관리자 조회

            if (receiver == null || !receiver.enabled) // 도약 적용 가능한 사용자 여부 확인
            { // 트램폴린 대상 없음 처리
                return; // 도약 적용 생략
            } // 트램폴린 대상 없음 처리 종료

            if (!receiver.ApplyObstacleImpulse(Vector3.up, force)) // 위쪽 도약 힘 적용 성공 여부 확인
            { // 트램폴린 힘 적용 실패 처리
                return; // 사용 횟수 유지
            } // 트램폴린 힘 적용 실패 처리 종료

            remainingUses--; // 성공한 도약 사용 횟수 차감

            if (remainingUses <= 0) // 세 번 사용 완료 여부 확인
            { // 트램폴린 소진 처리
                Destroy(gameObject); // 모든 횟수를 사용한 트램폴린 제거
            } // 트램폴린 소진 처리 종료
        } // 트램폴린 도약 처리 종료

        private void CreateVisual(Vector3 halfExtents, Color visualColor) // 설치 아이템 임시 표시 생성
        { // 설치 아이템 표시 생성 처리
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 설치물 임시 큐브 생성
            visualObject.name = "Visual"; // 설치물 표시 이름 지정
            visualObject.transform.SetParent(transform, false); // 설치물 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.up * halfExtents.y; // 지면 기준 표시 중심 적용
            visualObject.transform.localScale = halfExtents * 2f; // 설치 검사 크기와 같은 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 표시 Collider 조회

            if (visualCollider != null) // 임시 표시 Collider 존재 여부 확인
            { // 중복 Collider 제거 처리
                Destroy(visualCollider); // 루트 Trigger와 겹치는 Collider 제거
            } // 중복 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 임시 표시 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 설치 아이템 색상 적용 처리
                visualRenderer.material.color = visualColor; // 아이템 대표 색상 적용
            } // 설치 아이템 색상 적용 처리 종료
        } // 설치 아이템 표시 생성 처리 종료
    } // 설치 아이템 효과 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
