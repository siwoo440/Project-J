using System.Collections.Generic;
using UnityEngine;

namespace ProjectJ.Map
{
    [DisallowMultipleComponent]
    public sealed class MapModule : MonoBehaviour
    {
        public const float DefaultModuleSize = 20f;
        public const float PlayerHeightReference = 2f;

        [SerializeField]
        private string moduleId = "Module";

        [SerializeField]
        [Min(0.1f)]
        private float moduleSize =
            DefaultModuleSize;

        [SerializeField]
        private MapModuleSocket[] sockets;

        public string ModuleId
        {
            get
            {
                return moduleId;
            }
        }

        public float ModuleSize
        {
            get
            {
                return moduleSize;
            }
        }

        public IReadOnlyList<MapModuleSocket> Sockets
        {
            get
            {
                return sockets;
            }
        }

        public int EntranceCount
        {
            get
            {
                return CountState(
                    MapModuleFaceState.Entrance
                );
            }
        }

        public int ExitCount
        {
            get
            {
                return CountState(
                    MapModuleFaceState.Exit
                );
            }
        }

        public void Configure(
            string newModuleId,
            float newModuleSize,
            MapModuleSocket[] newSockets
        )
        {
            moduleId =
                string.IsNullOrWhiteSpace(
                    newModuleId
                )
                    ? "Module"
                    : newModuleId;

            moduleSize =
                Mathf.Max(
                    0.1f,
                    newModuleSize
                );

            sockets = newSockets;
        }

        public bool TryGetSocket(
            MapModuleFaceDirection direction,
            out MapModuleSocket socket
        )
        {
            socket = null;

            if (sockets == null)
            {
                return false;
            }

            for (
                int i = 0;
                i < sockets.Length;
                i++
            )
            {
                MapModuleSocket candidate =
                    sockets[i];

                if (
                    candidate != null &&
                    candidate.Direction ==
                    direction
                )
                {
                    socket = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool IsDefinitionValid()
        {
            if (
                sockets == null ||
                sockets.Length != 6
            )
            {
                return false;
            }

            bool[] directionFound =
                new bool[6];

            int entranceCount = 0;
            int exitCount = 0;

            for (
                int i = 0;
                i < sockets.Length;
                i++
            )
            {
                MapModuleSocket socket =
                    sockets[i];

                if (socket == null)
                {
                    return false;
                }

                int directionIndex =
                    (int)socket.Direction;

                if (
                    directionIndex < 0 ||
                    directionIndex >=
                    directionFound.Length ||
                    directionFound[directionIndex]
                )
                {
                    return false;
                }

                directionFound[directionIndex] =
                    true;

                if (
                    socket.State ==
                    MapModuleFaceState.Entrance
                )
                {
                    entranceCount++;
                }

                if (
                    socket.State ==
                    MapModuleFaceState.Exit
                )
                {
                    exitCount++;
                }
            }

            return
                entranceCount >= 1 &&
                exitCount >= 1;
        }

        private int CountState(
            MapModuleFaceState state
        )
        {
            if (sockets == null)
            {
                return 0;
            }

            int count = 0;

            for (
                int i = 0;
                i < sockets.Length;
                i++
            )
            {
                MapModuleSocket socket =
                    sockets[i];

                if (
                    socket != null &&
                    socket.State == state
                )
                {
                    count++;
                }
            }

            return count;
        }

        public static bool CanConnect(
            MapModuleFaceDirection fromDirection,
            MapModuleFaceState fromState,
            MapModuleFaceDirection toDirection,
            MapModuleFaceState toState
        )
        {
            return
                fromState ==
                MapModuleFaceState.Exit &&
                toState ==
                MapModuleFaceState.Entrance &&
                GetOppositeDirection(
                    fromDirection
                ) ==
                toDirection;
        }

        public static bool IsFaceStateSetValid(
            IReadOnlyList<MapModuleFaceState> states
        )
        {
            if (
                states == null ||
                states.Count != 6
            )
            {
                return false;
            }

            int entranceCount = 0;
            int exitCount = 0;

            for (
                int i = 0;
                i < states.Count;
                i++
            )
            {
                if (
                    states[i] ==
                    MapModuleFaceState.Entrance
                )
                {
                    entranceCount++;
                }

                if (
                    states[i] ==
                    MapModuleFaceState.Exit
                )
                {
                    exitCount++;
                }
            }

            return
                entranceCount >= 1 &&
                exitCount >= 1;
        }

        public static MapModuleFaceDirection GetOppositeDirection(
            MapModuleFaceDirection direction
        )
        {
            switch (direction)
            {
                case MapModuleFaceDirection.North:
                    return MapModuleFaceDirection.South;

                case MapModuleFaceDirection.South:
                    return MapModuleFaceDirection.North;

                case MapModuleFaceDirection.East:
                    return MapModuleFaceDirection.West;

                case MapModuleFaceDirection.West:
                    return MapModuleFaceDirection.East;

                case MapModuleFaceDirection.Up:
                    return MapModuleFaceDirection.Down;

                case MapModuleFaceDirection.Down:
                    return MapModuleFaceDirection.Up;

                default:
                    return MapModuleFaceDirection.North;
            }
        }

        public static Vector3Int GetDirectionCellOffset(
            MapModuleFaceDirection direction
        )
        {
            switch (direction)
            {
                case MapModuleFaceDirection.North:
                    return new Vector3Int(
                        0,
                        0,
                        1
                    );

                case MapModuleFaceDirection.South:
                    return new Vector3Int(
                        0,
                        0,
                        -1
                    );

                case MapModuleFaceDirection.East:
                    return new Vector3Int(
                        1,
                        0,
                        0
                    );

                case MapModuleFaceDirection.West:
                    return new Vector3Int(
                        -1,
                        0,
                        0
                    );

                case MapModuleFaceDirection.Up:
                    return new Vector3Int(
                        0,
                        1,
                        0
                    );

                case MapModuleFaceDirection.Down:
                    return new Vector3Int(
                        0,
                        -1,
                        0
                    );

                default:
                    return Vector3Int.zero;
            }
        }

        public static Vector3 GetDirectionVector(
            MapModuleFaceDirection direction
        )
        {
            switch (direction)
            {
                case MapModuleFaceDirection.North:
                    return Vector3.forward;

                case MapModuleFaceDirection.South:
                    return Vector3.back;

                case MapModuleFaceDirection.East:
                    return Vector3.right;

                case MapModuleFaceDirection.West:
                    return Vector3.left;

                case MapModuleFaceDirection.Up:
                    return Vector3.up;

                case MapModuleFaceDirection.Down:
                    return Vector3.down;

                default:
                    return Vector3.zero;
            }
        }
    }
}
