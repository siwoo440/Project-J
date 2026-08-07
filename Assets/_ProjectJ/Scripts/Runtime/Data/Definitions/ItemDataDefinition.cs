using UnityEngine; // Unity 아이콘과 색상과 벡터 데이터 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{ // 프로젝트 데이터 묶음
    public enum ItemImplementationPriority // 아이템 구현 우선순위 선언
    { // 아이템 구현 우선순위 묶음
        P0, // 42일차 핵심 구현 대상
        P1, // 43일차 구현 대상
        P2 // 44일차 구현 대상
    } // 아이템 구현 우선순위 묶음 종료

    public enum ItemUseType // 아이템 공통 사용 방식 선언
    { // 아이템 공통 사용 방식 묶음
        Instant, // 누르는 즉시 발동
        Duration, // 일정 시간 효과 유지
        Projectile, // 투사체 발사
        Placement // 지면 설치
    } // 아이템 공통 사용 방식 묶음 종료

    public enum ItemEffectType // 28종 아이템 효과 종류 선언
    { // 28종 아이템 효과 종류 묶음
        SpringShoes, // 스프링 신발
        JellyShield, // 젤리 보호막
        BananaCushion, // 바나나 쿠션
        BalloonTrumpet, // 풍선 나팔
        WaterGun, // 물총
        Firework, // 폭죽
        FeatherShoes, // 깃털 신발
        Snowball, // 눈덩이
        Mine, // 지뢰
        Ball, // 풀 공
        Jetpack, // 제트팩
        Hammer, // 망치
        Bomb, // 폭탄
        PufferArmor, // 복어 갑옷
        InkOctopus, // 먹물 문어
        FishingRod, // 낚시대
        GrapplingHook, // 갈고리
        SoapBubble, // 비눗방울
        SmokeGrenade, // 연막탄
        Trampoline, // 트램폴린
        GiantBalloon, // 거대 풍선
        RewindClock, // 되감기 시계
        HomingMissile, // 유도탄
        MiniaturePotion, // 소형화 물약
        Drone, // 드론
        InvisibilityCloak, // 투명 망토
        SniperWaterGun, // 저격 물총
        Cart // 카트
    } // 28종 아이템 효과 종류 묶음 종료

    [CreateAssetMenu(fileName = "ItemData", menuName = "Project J/Data/Item")] // Project 창 아이템 데이터 생성 메뉴 등록
    public sealed class ItemDataDefinition : ProjectDataAsset // 아이템 공통 데이터 에셋 선언
    { // 아이템 공통 데이터 묶음
        [SerializeField, TextArea(2, 4)] private string description; // 아이템 간단 설명 저장
        [SerializeField] private Sprite inventoryIcon; // 인벤토리 표시 아이콘 저장
        [SerializeField] private Color pickupColor = Color.white; // 상자 구분용 대표 색상 저장
        [SerializeField, Min(0.1f)] private float pickupVisualScale = 0.75f; // 상자 내부 표시 크기 저장
        [Header("42일차 사용 규칙")] // 42일차 사용 규칙 Inspector 구분
        [SerializeField] private ItemImplementationPriority implementationPriority; // 구현 우선순위 저장
        [SerializeField] private ItemUseType useType; // 공통 사용 방식 저장
        [SerializeField] private ItemEffectType effectType; // 실제 효과 종류 저장
        [SerializeField, Min(0.01f)] private float spawnWeight = 1f; // 상자 선택 가중치 저장
        [SerializeField, Min(1)] private int maximumStackCount = 1; // 한 슬롯 최대 보유 수량 저장
        [Header("42일차 효과 수치")] // 42일차 효과 수치 Inspector 구분
        [SerializeField, Min(0f)] private float effectDuration; // 효과 유지 또는 준비 시간 저장
        [SerializeField] private float primaryValue; // 핵심 힘과 배율 수치 저장
        [SerializeField] private float secondaryValue; // 보조 각도와 간격 수치 저장
        [SerializeField, Min(0f)] private float effectRange; // 직선 효과 거리 저장
        [SerializeField, Min(0f)] private float effectRadius; // 범위 효과 반지름 저장
        [SerializeField, Min(0f)] private float cooldown; // 효과 내부 반복 간격 저장
        [SerializeField, Min(0f)] private float projectileSpeed; // 투사체 이동 속도 저장
        [SerializeField] private Vector3 placementHalfExtents = Vector3.one * 0.5f; // 설치 공간 절반 크기 저장

        public override ProjectDataCategory Category => ProjectDataCategory.Item; // 아이템 데이터 분류 반환
        public string Description => description; // 아이템 설명 반환
        public Sprite InventoryIcon => inventoryIcon; // 인벤토리 아이콘 반환
        public Color PickupColor => pickupColor; // 상자 대표 색상 반환
        public float PickupVisualScale => Mathf.Max(0.1f, pickupVisualScale); // 안전하게 보정한 표시 크기 반환
        public ItemImplementationPriority ImplementationPriority => implementationPriority; // 구현 우선순위 반환
        public ItemUseType UseType => useType; // 공통 사용 방식 반환
        public ItemEffectType EffectType => effectType; // 실제 효과 종류 반환
        public float SpawnWeight => Mathf.Max(0.01f, spawnWeight); // 안전하게 보정한 상자 가중치 반환
        public int MaximumStackCount => Mathf.Max(1, maximumStackCount); // 안전하게 보정한 최대 중첩 수 반환
        public float EffectDuration => Mathf.Max(0f, effectDuration); // 안전하게 보정한 효과 시간 반환
        public float PrimaryValue => primaryValue; // 핵심 효과 수치 반환
        public float SecondaryValue => secondaryValue; // 보조 효과 수치 반환
        public float EffectRange => Mathf.Max(0f, effectRange); // 안전하게 보정한 효과 거리 반환
        public float EffectRadius => Mathf.Max(0f, effectRadius); // 안전하게 보정한 효과 반지름 반환
        public float Cooldown => Mathf.Max(0f, cooldown); // 안전하게 보정한 반복 간격 반환
        public float ProjectileSpeed => Mathf.Max(0f, projectileSpeed); // 안전하게 보정한 투사체 속도 반환
        public Vector3 PlacementHalfExtents => new Vector3(Mathf.Max(0.01f, placementHalfExtents.x), Mathf.Max(0.01f, placementHalfExtents.y), Mathf.Max(0.01f, placementHalfExtents.z)); // 안전하게 보정한 설치 크기 반환

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureItemForEditor(string newDescription, Sprite newInventoryIcon, Color newPickupColor, float newPickupVisualScale) // 기존 Editor 도구와 테스트용 표시 데이터 설정
        { // 기존 표시 데이터 설정 처리
            description = newDescription ?? string.Empty; // 빈 값을 허용한 아이템 설명 저장
            inventoryIcon = newInventoryIcon; // 아이템 아이콘 저장
            pickupColor = newPickupColor; // 아이템 대표 색상 저장
            pickupVisualScale = Mathf.Max(0.1f, newPickupVisualScale); // 최소 표시 크기 보정 후 저장
        } // 기존 표시 데이터 설정 처리 종료

        public void ConfigureUsageForEditor(ItemImplementationPriority newPriority, ItemUseType newUseType, ItemEffectType newEffectType, float newSpawnWeight, int newMaximumStackCount, float newEffectDuration, float newPrimaryValue, float newSecondaryValue, float newEffectRange, float newEffectRadius, float newCooldown, float newProjectileSpeed, Vector3 newPlacementHalfExtents) // 42일차 사용과 효과 데이터 설정
        { // 42일차 사용과 효과 데이터 설정 처리
            implementationPriority = newPriority; // 구현 우선순위 저장
            useType = newUseType; // 공통 사용 방식 저장
            effectType = newEffectType; // 실제 효과 종류 저장
            spawnWeight = Mathf.Max(0.01f, newSpawnWeight); // 상자 가중치 보정 후 저장
            maximumStackCount = Mathf.Max(1, newMaximumStackCount); // 최대 중첩 수 보정 후 저장
            effectDuration = Mathf.Max(0f, newEffectDuration); // 효과 시간 보정 후 저장
            primaryValue = newPrimaryValue; // 핵심 효과 수치 저장
            secondaryValue = newSecondaryValue; // 보조 효과 수치 저장
            effectRange = Mathf.Max(0f, newEffectRange); // 효과 거리 보정 후 저장
            effectRadius = Mathf.Max(0f, newEffectRadius); // 효과 반지름 보정 후 저장
            cooldown = Mathf.Max(0f, newCooldown); // 반복 간격 보정 후 저장
            projectileSpeed = Mathf.Max(0f, newProjectileSpeed); // 투사체 속도 보정 후 저장
            placementHalfExtents = new Vector3(Mathf.Max(0.01f, newPlacementHalfExtents.x), Mathf.Max(0.01f, newPlacementHalfExtents.y), Mathf.Max(0.01f, newPlacementHalfExtents.z)); // 설치 크기 보정 후 저장
        } // 42일차 사용과 효과 데이터 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 아이템 공통 데이터 묶음 종료
} // 프로젝트 데이터 묶음 종료
