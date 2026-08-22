using Fusion; // Fusion 네트워크 입력 사용
using UnityEngine; // Vector2 사용

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkButton // 네트워크 버튼 종류
    {
        Jump = 0, // 점프 입력
        Sprint = 1, // 달리기 입력
        Crouch = 2, // 앉기 입력
        Push = 3, // 밀치기 입력
        ItemSlotLeft = 4, // Q 첫 번째 아이템 슬롯 선택
        ItemSlotRight = 5, // E 두 번째 아이템 슬롯 선택
        ItemUse = 6, // 우클릭 아이템 사용 시작
        ItemUseHeld = 7 // 우클릭 아이템 사용 유지
    }

    public struct ProjectJNetworkInput : // Fusion 입력 데이터
        INetworkInput
    {
        public Vector2 Move; // 이동 방향 입력
        public NetworkButtons Buttons; // 버튼 입력 모음
    }
}
