using UnityEngine;

namespace ProjectJ.Map
{
    [DisallowMultipleComponent]
    public sealed class MapModuleSocket : MonoBehaviour
    {
        [SerializeField]
        private MapModuleFaceDirection direction;

        [SerializeField]
        private MapModuleFaceState state;

        public MapModuleFaceDirection Direction
        {
            get
            {
                return direction;
            }
        }

        public MapModuleFaceState State
        {
            get
            {
                return state;
            }
        }

        public bool IsOpen
        {
            get
            {
                return state !=
                    MapModuleFaceState.Closed;
            }
        }

        public void Configure(
            MapModuleFaceDirection newDirection,
            MapModuleFaceState newState
        )
        {
            direction = newDirection;
            state = newState;
        }
    }
}
