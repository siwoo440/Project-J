using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    [CreateAssetMenu( // Project 창에서 아이템 데이터 생성 메뉴 제공
        fileName = "Item_",
        menuName = "Project J/Items/Item Definition"
    )]
    public sealed class ItemDefinition : ScriptableObject // 아이템 공통 데이터
    {
        [SerializeField] // 인스펙터 직렬화
        private string itemId = string.Empty; // 저장과 동기화에 사용할 고유 ID

        [SerializeField] // 인스펙터 직렬화
        private string displayName = string.Empty; // 화면 표시 이름

        [SerializeField] // 인스펙터 직렬화
        private ItemCategory category = ItemCategory.Utility; // 아이템 역할

        [SerializeField] // 인스펙터 직렬화
        private ItemUseMode useMode = ItemUseMode.Instant; // 사용 방식

        [SerializeField] // 인스펙터 직렬화
        private ItemTargetType targetType = ItemTargetType.Self; // Target 방식

        [SerializeField] // 인스펙터 직렬화
        [Min(0f)] // 음수 방지
        private float duration; // 지속 시간

        [SerializeField] // 인스펙터 직렬화
        [Min(0f)] // 음수 방지
        private float cooldown; // 재사용 대기 시간

        [SerializeField] // 인스펙터 직렬화
        private bool isPlaceable; // 월드 설치 가능 여부

        [SerializeField] // 인스펙터 직렬화
        private Sprite icon; // 인벤토리 아이콘

        public string ItemId // 고유 ID 조회
        {
            get
            {
                return itemId; // ID 반환
            }
        }

        public string DisplayName // 표시 이름 조회
        {
            get
            {
                return displayName; // 이름 반환
            }
        }

        public ItemCategory Category // 역할 조회
        {
            get
            {
                return category; // 역할 반환
            }
        }

        public ItemUseMode UseMode // 사용 방식 조회
        {
            get
            {
                return useMode; // 사용 방식 반환
            }
        }

        public ItemTargetType TargetType // Target 방식 조회
        {
            get
            {
                return targetType; // Target 방식 반환
            }
        }

        public float Duration // 지속 시간 조회
        {
            get
            {
                return duration; // 지속 시간 반환
            }
        }

        public float Cooldown // 재사용 대기 시간 조회
        {
            get
            {
                return cooldown; // Cooldown 반환
            }
        }

        public bool IsPlaceable // 설치 가능 여부 조회
        {
            get
            {
                return isPlaceable; // 설치 가능 여부 반환
            }
        }

        public Sprite Icon // UI 아이콘 조회
        {
            get
            {
                return icon; // Sprite 반환
            }
        }

        public void Configure( // 데이터 Import 또는 테스트용 설정
            string newItemId,
            string newDisplayName,
            ItemCategory newCategory,
            ItemUseMode newUseMode,
            ItemTargetType newTargetType,
            float newDuration,
            float newCooldown,
            bool newIsPlaceable,
            Sprite newIcon = null
        )
        {
            itemId = newItemId ?? string.Empty; // ID 저장
            displayName = newDisplayName ?? string.Empty; // 표시 이름 저장
            category = newCategory; // 역할 저장
            useMode = newUseMode; // 사용 방식 저장
            targetType = newTargetType; // Target 방식 저장
            duration = Mathf.Max(0f, newDuration); // 지속 시간 보정 후 저장
            cooldown = Mathf.Max(0f, newCooldown); // Cooldown 보정 후 저장
            isPlaceable = newIsPlaceable; // 설치 여부 저장
            icon = newIcon; // 아이콘 저장
        }

        public bool IsDefinitionValid(out string errorMessage) // 현재 데이터 유효성 검사
        {
            ItemDefinitionValidationResult result =
                ItemDefinitionValidator.Validate(this); // 공통 Validator 호출

            errorMessage = result.Message; // 오류 메시지 반환
            return result.IsValid; // 유효성 반환
        }
    }
}
