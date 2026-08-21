using System.Collections.Generic; // 중복 대상 방지용 HashSet 사용
using ProjectJ.Push; // 기존 플레이어 밀치기 및 외력 시스템 사용
using UnityEngine; // Unity 물리 기능 사용

namespace ProjectJ.Items.Effects
{
    public sealed class BalloonHornEffect :
        IItemUseEffect
    {
        private const float Range = 6f; // 풍선 나팔 효과 거리
        private const float HalfAngle = 55f; // 전방 좌우 판정 각도
        private const float PushMultiplier = 2.5f; // 기본 캐릭터 밀치기의 2.5배

        public ItemUseResult TryUse(
            ItemUseContext context
        )
        {
            if (context.User == null)
            {
                return ItemUseResult.Fail(
                    ItemUseStatus.InvalidItem,
                    "풍선 나팔 사용자를 찾을 수 없습니다."
                );
            }

            Transform userTransform =
                context.User.transform;

            PlayerPushController pushController =
                context.User.GetComponent<
                    PlayerPushController
                >(); // 현재 캐릭터의 실제 밀치기 설정 조회

            float basePushForce =
                pushController != null
                    ? pushController.HorizontalVelocityChange
                    : 12f; // Controller 누락 시 현재 기본값 12 사용

            float hornPushForce =
                basePushForce *
                PushMultiplier; // 풍선 나팔은 현재 밀치기의 2.5배

            Vector3 origin =
                userTransform.position +
                Vector3.up * 1f;

            Collider[] overlaps =
                Physics.OverlapSphere(
                    origin,
                    Range,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            HashSet<
                PlayerExternalForceReceiver
            > processed =
                new HashSet<
                    PlayerExternalForceReceiver
                >();

            for (
                int i = 0;
                i < overlaps.Length;
                i++
            )
            {
                PlayerExternalForceReceiver receiver =
                    overlaps[i]
                        .GetComponentInParent<
                            PlayerExternalForceReceiver
                        >();

                if (
                    receiver == null ||
                    receiver.gameObject ==
                        context.User ||
                    !processed.Add(receiver)
                )
                {
                    continue;
                }

                Vector3 toTarget =
                    receiver.transform.position -
                    userTransform.position;

                toTarget.y = 0f;

                if (
                    toTarget.sqrMagnitude <
                    0.01f
                )
                {
                    continue;
                }

                float angle =
                    Vector3.Angle(
                        userTransform.forward,
                        toTarget
                    );

                if (angle > HalfAngle)
                {
                    continue;
                }

                Vector3 direction =
                    toTarget.normalized;

                receiver.TryApplyVelocityChange(
                    ExternalForceSource.Item,
                    direction *
                    hornPushForce
                );
            }

            return ItemUseResult.Success();
        }
    }
}
