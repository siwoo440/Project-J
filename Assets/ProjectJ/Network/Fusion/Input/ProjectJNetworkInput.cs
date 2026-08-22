using Fusion;
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkButton
    {
        Jump = 0,
        Sprint = 1,
        Crouch = 2
    }

    public struct ProjectJNetworkInput :
        INetworkInput
    {
        public Vector2 Move;
        public NetworkButtons Buttons;
    }
}
