using UnityEngine; // Unity 아이콘과 색상 데이터 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{ // 프로젝트 데이터 묶음
    [CreateAssetMenu(fileName = "ItemData", menuName = "Project J/Data/Item")] // Project 창 아이템 데이터 생성 메뉴 등록
    public sealed class ItemDataDefinition : ProjectDataAsset // 아이템 공통 데이터 에셋 선언
    { // 아이템 공통 데이터 묶음
        [SerializeField, TextArea(2, 4)] private string description; // 아이템 간단 설명 저장
        [SerializeField] private Sprite inventoryIcon; // 인벤토리 표시 아이콘 저장
        [SerializeField] private Color pickupColor = Color.white; // 상자 구분용 대표 색상 저장
        [SerializeField, Min(0.1f)] private float pickupVisualScale = 0.75f; // 상자 내부 표시 크기 저장

        public override ProjectDataCategory Category => ProjectDataCategory.Item; // 아이템 데이터 분류 반환
        public string Description => description; // 아이템 설명 반환
        public Sprite InventoryIcon => inventoryIcon; // 인벤토리 아이콘 반환
        public Color PickupColor => pickupColor; // 상자 대표 색상 반환
        public float PickupVisualScale => Mathf.Max(0.1f, pickupVisualScale); // 안전하게 보정한 표시 크기 반환

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureItemForEditor(string newDescription, Sprite newInventoryIcon, Color newPickupColor, float newPickupVisualScale) // Editor 도구와 테스트용 아이템 공통 데이터 설정
        { // 아이템 공통 데이터 설정 처리
            description = newDescription ?? string.Empty; // 빈 값을 허용한 아이템 설명 저장
            inventoryIcon = newInventoryIcon; // 아이템 아이콘 저장
            pickupColor = newPickupColor; // 아이템 대표 색상 저장
            pickupVisualScale = Mathf.Max(0.1f, newPickupVisualScale); // 최소 표시 크기 보정 후 저장
        } // 아이템 공통 데이터 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 아이템 공통 데이터 묶음 종료
} // 프로젝트 데이터 묶음 종료
