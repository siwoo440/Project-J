using ProjectJ.Player;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.Editor
{
    public static class Day26PlayerHeightSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/Player.prefab";

        private const string HeightReferenceName =
            "HeightReference_Foot";

        [MenuItem("ProjectJ/Day26/Setup Player Foot Height Reference")]
        public static void SetupPlayerFootHeightReference()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    PlayerPrefabPath
                );

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "Player.prefab을 열 수 없습니다: " +
                    PlayerPrefabPath
                );

                return;
            }

            try
            {
                CapsuleCollider capsule =
                    prefabRoot.GetComponent<
                        CapsuleCollider
                    >();

                if (capsule == null)
                {
                    Debug.LogError(
                        "Player.prefab에 CapsuleCollider가 없습니다."
                    );

                    return;
                }

                Transform heightReference =
                    FindOrCreateHeightReference(
                        prefabRoot.transform
                    );

                heightReference.localPosition =
                    PlayerHeightTracker
                        .CalculateCapsuleFootLocalPosition(
                            capsule.center,
                            capsule.height,
                            capsule.direction
                        );

                heightReference.localRotation =
                    Quaternion.identity;

                heightReference.localScale =
                    Vector3.one;

                PlayerHeightTracker tracker =
                    prefabRoot.GetComponent<
                        PlayerHeightTracker
                    >();

                if (tracker == null)
                {
                    tracker =
                        prefabRoot.AddComponent<
                            PlayerHeightTracker
                        >();
                }

                tracker.Configure(
                    heightReference
                );

                EditorUtility.SetDirty(
                    tracker
                );

                EditorUtility.SetDirty(
                    heightReference
                );

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    PlayerPrefabPath
                );

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject =
                    AssetDatabase.LoadAssetAtPath<
                        GameObject
                    >(
                        PlayerPrefabPath
                    );

                Debug.Log(
                    "Day26 Player 발 기준점과 높이 추적기 설정 완료. " +
                    "HeightReference_Foot Local Position = " +
                    heightReference.localPosition
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                );
            }
        }

        private static Transform FindOrCreateHeightReference(
            Transform playerRoot
        )
        {
            Transform existing =
                playerRoot.Find(
                    HeightReferenceName
                );

            if (existing != null)
            {
                return existing;
            }

            GameObject referenceObject =
                new GameObject(
                    HeightReferenceName
                );

            referenceObject.transform.SetParent(
                playerRoot,
                false
            );

            return referenceObject.transform;
        }
    }
}
