using NUnit.Framework;
using ProjectJ.Map;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class MapModuleTests
    {
        [Test]
        public void Definition_RequiresAtLeastOneEntranceAndExit()
        {
            MapModuleFaceState[] states =
            {
                MapModuleFaceState.Exit,
                MapModuleFaceState.Entrance,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed
            };

            Assert.That(
                MapModule.IsFaceStateSetValid(
                    states
                ),
                Is.True
            );
        }

        [Test]
        public void Definition_DropDoesNotReplaceExit()
        {
            MapModuleFaceState[] states =
            {
                MapModuleFaceState.Drop,
                MapModuleFaceState.Entrance,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed
            };

            Assert.That(
                MapModule.IsFaceStateSetValid(
                    states
                ),
                Is.False
            );
        }

        [Test]
        public void Definition_RejectsMissingEntrance()
        {
            MapModuleFaceState[] states =
            {
                MapModuleFaceState.Exit,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed,
                MapModuleFaceState.Closed
            };

            Assert.That(
                MapModule.IsFaceStateSetValid(
                    states
                ),
                Is.False
            );
        }

        [Test]
        public void NorthExit_ConnectsToSouthEntrance()
        {
            bool canConnect =
                MapModule.CanConnect(
                    MapModuleFaceDirection.North,
                    MapModuleFaceState.Exit,
                    MapModuleFaceDirection.South,
                    MapModuleFaceState.Entrance
                );

            Assert.That(
                canConnect,
                Is.True
            );
        }

        [Test]
        public void UpExit_ConnectsToDownEntrance()
        {
            bool canConnect =
                MapModule.CanConnect(
                    MapModuleFaceDirection.Up,
                    MapModuleFaceState.Exit,
                    MapModuleFaceDirection.Down,
                    MapModuleFaceState.Entrance
                );

            Assert.That(
                canConnect,
                Is.True
            );
        }

        [Test]
        public void Exit_DoesNotConnectToExit()
        {
            bool canConnect =
                MapModule.CanConnect(
                    MapModuleFaceDirection.North,
                    MapModuleFaceState.Exit,
                    MapModuleFaceDirection.South,
                    MapModuleFaceState.Exit
                );

            Assert.That(
                canConnect,
                Is.False
            );
        }

        [Test]
        public void Drop_IsNotNormalProgressConnection()
        {
            bool canConnect =
                MapModule.CanConnect(
                    MapModuleFaceDirection.North,
                    MapModuleFaceState.Drop,
                    MapModuleFaceDirection.South,
                    MapModuleFaceState.Entrance
                );

            Assert.That(
                canConnect,
                Is.False
            );
        }

        [Test]
        public void OppositeDirection_IsCorrectForAllAxes()
        {
            Assert.That(
                MapModule.GetOppositeDirection(
                    MapModuleFaceDirection.North
                ),
                Is.EqualTo(
                    MapModuleFaceDirection.South
                )
            );

            Assert.That(
                MapModule.GetOppositeDirection(
                    MapModuleFaceDirection.East
                ),
                Is.EqualTo(
                    MapModuleFaceDirection.West
                )
            );

            Assert.That(
                MapModule.GetOppositeDirection(
                    MapModuleFaceDirection.Up
                ),
                Is.EqualTo(
                    MapModuleFaceDirection.Down
                )
            );
        }

        [Test]
        public void UpDirection_MovesOneGridCellUp()
        {
            Vector3Int offset =
                MapModule.GetDirectionCellOffset(
                    MapModuleFaceDirection.Up
                );

            Assert.That(
                offset,
                Is.EqualTo(
                    new Vector3Int(
                        0,
                        1,
                        0
                    )
                )
            );
        }

        [Test]
        public void ModuleSize_UsesOneToOneToOneCubeStandard()
        {
            Assert.That(
                MapModule.DefaultModuleSize,
                Is.EqualTo(20f)
            );

            Assert.That(
                MapModule.PlayerHeightReference,
                Is.EqualTo(2f)
            );
        }
    }
}
