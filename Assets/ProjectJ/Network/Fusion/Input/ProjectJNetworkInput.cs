using Fusion; // Fusion 네트워크 입력 사용
using UnityEngine; // Vector2와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkButton // 네트워크 버튼 종류
    {
        Jump = 0,
        Sprint = 1,
        Crouch = 2,
        Push = 3,
        ItemSlotLeft = 4,
        ItemSlotRight = 5,
        ItemUse = 6,
        ItemUseHeld = 7
    }

    public struct ProjectJNetworkInput :
        INetworkInput
    {
        public Vector2 Move;
        public Vector3 AimDirection; // 로컬 카메라 조준 방향
        public NetworkButtons Buttons;
    }
}
