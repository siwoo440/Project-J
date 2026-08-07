using System.Collections; // 재생성 대기 코루틴 기능 참조
using ProjectJ.Data; // 아이템 공통 데이터 형식 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using UnityEngine; // Unity 오브젝트 생성 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 생성 지점 컴포넌트 중복 방지
    public sealed class ItemChestSpawnPoint : MonoBehaviour // 단일 상자 생성과 재생성 지점 선언
    { // 단일 상자 생성 지점 묶음
        private ItemDataDefinition[] itemPool; // 지점에서 선택 가능한 아이템 목록 저장
        private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        private float respawnDelay; // 획득 후 재생성 대기 시간 저장
        private int maximumRespawnCount; // 지점 최대 재생성 횟수 저장
        private int randomSeed; // 지점 아이템 선택 난수 시드 저장
        private int completedRespawnCount; // 완료된 추가 재생성 횟수 저장
        private ItemChestPickup activeChest; // 현재 지점 상자 참조 저장
        private Coroutine respawnCoroutine; // 진행 중 재생성 코루틴 저장

        public int CompletedRespawnCount => completedRespawnCount; // 완료된 재생성 횟수 반환
        public bool HasActiveChest => activeChest != null && !activeChest.IsCollected; // 획득 가능한 현재 상자 존재 여부 반환

        public void ConfigureRuntime(ItemDataDefinition[] newItemPool, PrototypeMatchController newMatchController, float newRespawnDelay, int newMaximumRespawnCount, int newRandomSeed) // 런타임 상자 지점 규칙 설정과 최초 생성
        { // 런타임 상자 지점 설정 처리
            itemPool = newItemPool; // 아이템 후보 목록 저장
            matchController = newMatchController; // 경기 관리자 저장
            respawnDelay = Mathf.Max(0f, newRespawnDelay); // 재생성 대기 시간 보정 후 저장
            maximumRespawnCount = Mathf.Max(0, newMaximumRespawnCount); // 최대 재생성 횟수 보정 후 저장
            randomSeed = newRandomSeed; // 지점 전용 난수 시드 저장
            completedRespawnCount = 0; // 재생성 완료 횟수 초기화
            SpawnChest(0); // 최초 상자 즉시 생성
        } // 런타임 상자 지점 설정 처리 종료

        private void OnDisable() // 지점 비활성화 시 대기 작업 정리
        { // 지점 비활성화 정리 처리
            if (respawnCoroutine != null) // 진행 중 코루틴 존재 여부 확인
            { // 재생성 코루틴 중지 처리
                StopCoroutine(respawnCoroutine); // 비활성 지점 재생성 대기 중지
                respawnCoroutine = null; // 재생성 코루틴 참조 초기화
            } // 재생성 코루틴 중지 처리 종료
        } // 지점 비활성화 정리 처리 종료

        private void SpawnChest(int spawnSequence) // 지정 생성 순서의 상자 생성
        { // 단일 상자 생성 처리
            if (itemPool == null || itemPool.Length == 0) // 아이템 후보 누락 여부 확인
            { // 상자 생성 불가 처리
                return; // 상자 생성 중단
            } // 상자 생성 불가 처리 종료

            ItemDataDefinition selectedItem = SelectItem(spawnSequence); // 지점 시드 기반 지급 아이템 선택

            if (selectedItem == null) // 선택 아이템 누락 여부 확인
            { // 상자 생성 불가 처리
                return; // 상자 생성 중단
            } // 상자 생성 불가 처리 종료

            GameObject chestObject = new GameObject($"ItemChest_{spawnSequence + 1:00}_{selectedItem.DataId}"); // 생성 순서와 아이템 ID 기반 상자 루트 생성
            chestObject.transform.SetParent(transform, false); // 생성 지점 아래 상자 배치
            chestObject.transform.localPosition = Vector3.zero; // 지면 기준 로컬 위치 초기화
            chestObject.transform.localRotation = Quaternion.identity; // 지면 기준 로컬 회전 초기화
            BoxCollider pickupTrigger = chestObject.AddComponent<BoxCollider>(); // 플레이어 접촉 감지 Collider 추가
            pickupTrigger.isTrigger = true; // 통과 가능한 Trigger 설정
            pickupTrigger.center = Vector3.up * 0.6f; // 지면 위 접촉 중심 적용
            pickupTrigger.size = new Vector3(1.2f, 1.2f, 1.2f); // 상자 접촉 범위 적용
            Rigidbody rigidbody = chestObject.AddComponent<Rigidbody>(); // CharacterController 접촉용 Rigidbody 추가
            rigidbody.isKinematic = true; // 물리 힘에 움직이지 않는 상자 설정
            rigidbody.useGravity = false; // 상자 중력 비활성화
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // 고정 상자 기본 충돌 검사 적용
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 임시 상자 표시 큐브 생성
            visualObject.name = "Visual"; // 상자 표시 오브젝트 이름 지정
            visualObject.transform.SetParent(chestObject.transform, false); // 상자 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.up * 0.6f; // 지면 위 표시 중심 적용
            visualObject.transform.localScale = Vector3.one * selectedItem.PickupVisualScale; // 아이템별 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 표시 Collider 조회

            if (visualCollider != null) // 임시 표시 Collider 존재 여부 확인
            { // 중복 Collider 제거 처리
                Destroy(visualCollider); // 루트 Trigger와 겹치는 표시 Collider 제거
            } // 중복 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 표시 큐브 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 아이템 대표 색상 적용 처리
                visualRenderer.material.color = selectedItem.PickupColor; // 아이템 데이터 대표 색상 적용
            } // 아이템 대표 색상 적용 처리 종료

            activeChest = chestObject.AddComponent<ItemChestPickup>(); // 상자 획득 기능 추가
            activeChest.ConfigureRuntime(selectedItem, pickupTrigger, visualObject, true, true); // 지급 데이터와 상자 참조 연결
            activeChest.Collected += HandleChestCollected; // 상자 획득 완료 이벤트 연결
        } // 단일 상자 생성 처리 종료

        private ItemDataDefinition SelectItem(int spawnSequence) // 지점 시드와 생성 순서 기반 가중치 아이템 선택
        { // 가중치 아이템 선택 처리
            System.Random random = new System.Random(randomSeed + spawnSequence * 7919); // 생성 순서별 결정적 난수 생성
            return ItemSelectionRules.SelectByNormalizedValue(itemPool, (float)random.NextDouble()); // P0과 P1과 P2 가중치 기반 아이템 반환
        } // 가중치 아이템 선택 처리 종료

        private void HandleChestCollected(ItemChestPickup collectedChest, ItemDataDefinition unusedItemData, int unusedSlotIndex) // 현재 상자 획득 완료 후 재생성 예약
        { // 상자 획득 완료 처리
            if (collectedChest != null) // 획득 상자 참조 존재 여부 확인
            { // 획득 상자 이벤트 정리 처리
                collectedChest.Collected -= HandleChestCollected; // 획득 완료 이벤트 연결 해제
            } // 획득 상자 이벤트 정리 처리 종료

            activeChest = null; // 현재 활성 상자 참조 초기화

            if (!ItemChestSpawnRules.CanRespawn(completedRespawnCount, maximumRespawnCount)) // 남은 재생성 횟수 확인
            { // 재생성 종료 처리
                return; // 추가 상자 생성 생략
            } // 재생성 종료 처리 종료

            respawnCoroutine = StartCoroutine(RespawnAfterDelay(collectedChest)); // 획득 상자 정리와 재생성 대기 시작
        } // 상자 획득 완료 처리 종료

        private IEnumerator RespawnAfterDelay(ItemChestPickup collectedChest) // 획득 상자 제거 후 규칙 기반 재생성 대기
        { // 상자 재생성 대기 처리
            if (collectedChest != null) // 획득 상자 참조 존재 여부 확인
            { // 획득 상자 제거 처리
                Destroy(collectedChest.gameObject); // 비활성 획득 상자 오브젝트 제거
            } // 획득 상자 제거 처리 종료

            yield return new WaitForSeconds(respawnDelay); // 설정된 재생성 대기 시간 적용

            if (matchController != null && matchController.IsMatchFinished) // 경기 종료 상태 여부 확인
            { // 경기 종료 재생성 차단 처리
                respawnCoroutine = null; // 재생성 코루틴 참조 초기화
                yield break; // 경기 종료 뒤 상자 재생성 중단
            } // 경기 종료 재생성 차단 처리 종료

            completedRespawnCount++; // 완료된 추가 재생성 횟수 증가
            SpawnChest(completedRespawnCount); // 다음 순서 상자 생성
            respawnCoroutine = null; // 재생성 코루틴 참조 초기화
        } // 상자 재생성 대기 처리 종료
    } // 단일 상자 생성 지점 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
