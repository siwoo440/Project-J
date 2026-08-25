using System.Collections.Generic; // SortKey 중복 검사
using Fusion; // NetworkObject와 NetworkObjectBaker 사용
using UnityEditor; // Editor 자동 실행과 SerializedObject 사용
using UnityEditor.SceneManagement; // Day49 Scene 열기와 저장
using UnityEngine; // GameObject와 Component 사용
using UnityEngine.SceneManagement; // Scene 탐색과 배치

namespace ProjectJ.EditorTools
{
    [InitializeOnLoad] // Unity Script Reload 후 Day49 풀 공 테스트 배치 자동 확인
    public static class ProjectJDay111PoolBallSceneInstaller
    {
        private const string Day49ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day49/Day49_AllSystemsTest.unity";

        private const string PoolBallDefinitionPath =
            "Assets/ProjectJ/Data/Items/Item_PoolBall.asset";

        private const string ReferencePickupName =
            "Pickup_9_mine_A";

        private const string SessionConfiguredKey =
            "ProjectJ.Day111.PoolBallSceneInstaller.Configured";

        private static readonly string[] PickupNames =
        {
            "Pickup_10_pool_ball_A",
            "Pickup_10_pool_ball_B",
            "Pickup_10_pool_ball_C",
            "Pickup_10_pool_ball_D",
            "Pickup_10_pool_ball_E",
            "Pickup_10_pool_ball_F"
        };

        private static readonly float[] PickupXPositions =
        {
            -87f,
            -83f,
            -79f,
            -75f,
            -71f,
            -67f
        };

        private static NetworkObjectBaker networkObjectBaker;

        private static NetworkObjectBaker Baker
        {
            get
            {
                if (networkObjectBaker == null)
                {
                    networkObjectBaker =
                        new NetworkObjectBaker(); // Fusion 정식 Baker를 재사용
                }

                return networkObjectBaker;
            }
        }

        static ProjectJDay111PoolBallSceneInstaller()
        {
            EditorApplication.delayCall +=
                TryInstallAutomatically; // Domain Reload 종료 후 자동 적용
        }

        [MenuItem(
            "Project J/Day111/Install Pool Ball Pickups"
        )]
        private static void InstallFromMenu()
        {
            SessionState.SetBool(
                SessionConfiguredKey,
                false
            );

            InstallPoolBallPickups(
                true
            ); // 메뉴 실행 시 현재 Scene 상태를 다시 검사
        }

        private static void TryInstallAutomatically()
        {
            if (
                SessionState.GetBool(
                    SessionConfiguredKey,
                    false
                )
            )
            {
                return; // 현재 Editor Session에서 이미 정상 확인됨
            }

            if (
                EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating
            )
            {
                EditorApplication.delayCall +=
                    TryInstallAutomatically; // Editor 안정화 후 다시 시도

                return;
            }

            InstallPoolBallPickups(
                false
            ); // Day49 Scene의 풀 공 Pickup 상태를 검사하고 복구
        }

        private static void InstallPoolBallPickups(
            bool requestedFromMenu
        )
        {
            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    Day49ScenePath
                );

            if (sceneAsset == null)
            {
                Debug.LogError(
                    "[Project J/Day111] Day49 테스트 Scene을 찾지 못했습니다. / " +
                    Day49ScenePath
                );

                return;
            }

            Object poolBallDefinition =
                AssetDatabase.LoadAssetAtPath<Object>(
                    PoolBallDefinitionPath
                );

            if (poolBallDefinition == null)
            {
                Debug.LogError(
                    "[Project J/Day111] Item_PoolBall.asset을 찾지 못했습니다. / " +
                    PoolBallDefinitionPath
                );

                return;
            }

            Scene scene =
                SceneManager.GetSceneByPath(
                    Day49ScenePath
                );

            bool openedByInstaller =
                !scene.IsValid() ||
                !scene.isLoaded;

            if (
                !openedByInstaller &&
                scene.isDirty &&
                !requestedFromMenu
            )
            {
                Debug.LogWarning(
                    "[Project J/Day111] Day49 Scene에 저장되지 않은 변경 사항이 있어 자동 수정을 건너뜁니다. " +
                    "Scene을 저장한 뒤 Project J/Day111/Install Pool Ball Pickups 메뉴를 실행하십시오."
                );

                return;
            }

            if (
                !openedByInstaller &&
                scene.isDirty &&
                requestedFromMenu &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return; // 기존 Scene 저장을 취소한 경우 중단
            }

            if (openedByInstaller)
            {
                scene =
                    EditorSceneManager.OpenScene(
                        Day49ScenePath,
                        OpenSceneMode.Additive
                    ); // 현재 작업 Scene을 유지한 채 Day49 Scene만 열기
            }

            bool changed;
            bool configured =
                TryEnsurePickups(
                    scene,
                    poolBallDefinition,
                    out changed
                );

            if (configured)
            {
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(
                        scene
                    );

                    EditorSceneManager.SaveScene(
                        scene
                    ); // 실제 Day49 Scene 파일에 저장

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log(
                        "[Project J/Day111] Day49 풀 공 Pickup 6개 재구성 및 Fusion Bake 완료. / " +
                        Day49ScenePath
                    );
                }
                else if (requestedFromMenu)
                {
                    Debug.Log(
                        "[Project J/Day111] Day49 풀 공 Pickup 6개가 이미 정상 구성되어 있습니다. / " +
                        Day49ScenePath
                    );
                }

                SessionState.SetBool(
                    SessionConfiguredKey,
                    true
                );
            }

            if (
                openedByInstaller &&
                scene.IsValid() &&
                scene.isLoaded
            )
            {
                EditorSceneManager.CloseScene(
                    scene,
                    true
                ); // 자동으로 연 Scene만 닫기
            }
        }

        private static bool TryEnsurePickups(
            Scene scene,
            Object poolBallDefinition,
            out bool changed
        )
        {
            changed = false;

            GameObject referencePickup =
                FindObjectByName(
                    scene,
                    ReferencePickupName
                );

            if (referencePickup == null)
            {
                Debug.LogError(
                    "[Project J/Day111] 복제 기준 Network Pickup을 찾지 못했습니다. / " +
                    ReferencePickupName
                );

                return false;
            }

            NetworkObject referenceNetworkObject =
                referencePickup.GetComponent<NetworkObject>();

            if (referenceNetworkObject == null)
            {
                Debug.LogError(
                    "[Project J/Day111] 기준 Mine Pickup에 NetworkObject가 없습니다. / " +
                    ReferencePickupName
                );

                return false;
            }

            if (
                !HasComponentTypeName(
                    referencePickup,
                    "ProjectJNetworkItemBox"
                )
            )
            {
                Debug.LogError(
                    "[Project J/Day111] 기준 Mine Pickup에 ProjectJNetworkItemBox가 없습니다. / " +
                    ReferencePickupName
                );

                return false;
            }

            if (
                AreExistingPickupsValid(
                    scene,
                    poolBallDefinition,
                    referenceNetworkObject.SortKey
                )
            )
            {
                return true; // 정상 6개가 있으면 Scene을 변경하지 않음
            }

            RemoveExistingPoolBallPickups(
                scene
            ); // 이전 실패 실행으로 남은 불완전한 Pickup 제거

            Vector3 referencePosition =
                referencePickup.transform.position; // 기존 Pickup 높이와 Z 사용

            Transform referenceParent =
                referencePickup.transform.parent;

            for (
                int index = 0;
                index < PickupNames.Length;
                index++
            )
            {
                GameObject pickupObject =
                    Object.Instantiate(
                        referencePickup
                    ); // 완성된 Mine Scene Instance의 Component/Override 전체 복제

                if (pickupObject == null)
                {
                    Debug.LogError(
                        "[Project J/Day111] Mine Pickup Scene Instance 복제에 실패했습니다. / " +
                        PickupNames[index]
                    );

                    return false;
                }

                pickupObject.name =
                    PickupNames[index];

                SceneManager.MoveGameObjectToScene(
                    pickupObject,
                    scene
                );

                if (
                    referenceParent != null &&
                    referenceParent.gameObject.scene == scene
                )
                {
                    pickupObject.transform.SetParent(
                        referenceParent,
                        true
                    ); // 기준 Pickup과 같은 Hierarchy 부모 유지
                }

                pickupObject.transform.position =
                    new Vector3(
                        PickupXPositions[index],
                        referencePosition.y,
                        referencePosition.z
                    ); // 최대 5 Stack과 6번째 거부 테스트용 일렬 배치

                if (
                    !TryAssignPoolBallDefinition(
                        pickupObject,
                        poolBallDefinition
                    )
                )
                {
                    Debug.LogError(
                        "[Project J/Day111] ItemPickup Definition 연결에 실패했습니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                NetworkObject networkObject =
                    pickupObject.GetComponent<NetworkObject>();

                if (networkObject == null)
                {
                    Debug.LogError(
                        "[Project J/Day111] 복제본에 NetworkObject가 없습니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                if (
                    !HasComponentTypeName(
                        pickupObject,
                        "ProjectJNetworkItemBox"
                    )
                )
                {
                    Debug.LogError(
                        "[Project J/Day111] 복제본에 ProjectJNetworkItemBox가 없습니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                Baker.Bake(
                    pickupObject
                ); // 수동 SortKey 대입 대신 Fusion이 Scene NetworkObject Baked Data를 다시 생성

                EditorUtility.SetDirty(
                    networkObject
                );

                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    networkObject
                );

                EditorUtility.SetDirty(
                    pickupObject
                );
            }

            changed = true;

            if (
                !AreExistingPickupsValid(
                    scene,
                    poolBallDefinition,
                    referenceNetworkObject.SortKey
                )
            )
            {
                Debug.LogError(
                    "[Project J/Day111] Fusion Bake 후 NetworkObject 구성이 유효하지 않습니다. " +
                    "Console의 직전 세부 오류를 확인하십시오."
                );

                return false;
            }

            return true;
        }

        private static bool AreExistingPickupsValid(
            Scene scene,
            Object poolBallDefinition,
            uint referenceSortKey
        )
        {
            HashSet<uint> sortKeys =
                new HashSet<uint>();

            for (
                int index = 0;
                index < PickupNames.Length;
                index++
            )
            {
                GameObject pickupObject =
                    FindObjectByName(
                        scene,
                        PickupNames[index]
                    );

                if (pickupObject == null)
                {
                    return false;
                }

                NetworkObject networkObject =
                    pickupObject.GetComponent<NetworkObject>();

                if (networkObject == null)
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup에 NetworkObject가 없어 재생성합니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                if (
                    !HasComponentTypeName(
                        pickupObject,
                        "ProjectJNetworkItemBox"
                    )
                )
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup에 ProjectJNetworkItemBox가 없어 재생성합니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                if (
                    !HasPoolBallDefinition(
                        pickupObject,
                        poolBallDefinition
                    )
                )
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup의 Definition이 올바르지 않아 재생성합니다. / " +
                        pickupObject.name
                    );

                    return false;
                }

                uint sortKey =
                    networkObject.SortKey;

                if (
                    sortKey == referenceSortKey ||
                    !sortKeys.Add(
                        sortKey
                    )
                )
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup의 Fusion SortKey가 기준 Pickup 또는 다른 풀 공과 중복되어 재생성합니다. / " +
                        pickupObject.name +
                        " / SortKey=" +
                        sortKey
                    );

                    return false;
                }
            }

            return true;
        }

        private static void RemoveExistingPoolBallPickups(
            Scene scene
        )
        {
            for (
                int index = 0;
                index < PickupNames.Length;
                index++
            )
            {
                GameObject pickupObject =
                    FindObjectByName(
                        scene,
                        PickupNames[index]
                    );

                if (pickupObject == null)
                {
                    continue;
                }

                Object.DestroyImmediate(
                    pickupObject
                ); // 실패 실행에서 남은 잘못된 복제본 제거
            }
        }

        private static bool TryAssignPoolBallDefinition(
            GameObject pickupObject,
            Object poolBallDefinition
        )
        {
            Component itemPickup =
                FindComponentByTypeNameContains(
                    pickupObject,
                    "ItemPickup"
                );

            if (itemPickup == null)
            {
                return false;
            }

            SerializedObject serializedComponent =
                new SerializedObject(
                    itemPickup
                );

            SerializedProperty iterator =
                serializedComponent.GetIterator();

            bool enterChildren = true;

            while (
                iterator.NextVisible(
                    enterChildren
                )
            )
            {
                enterChildren = false;

                if (
                    iterator.propertyType !=
                    SerializedPropertyType.ObjectReference
                )
                {
                    continue;
                }

                if (
                    iterator.name.IndexOf(
                        "definition",
                        System.StringComparison.OrdinalIgnoreCase
                    ) < 0 &&
                    iterator.displayName.IndexOf(
                        "definition",
                        System.StringComparison.OrdinalIgnoreCase
                    ) < 0
                )
                {
                    continue;
                }

                iterator.objectReferenceValue =
                    poolBallDefinition; // Definition을 Item_PoolBall.asset으로 교체

                serializedComponent.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(
                    itemPickup
                );

                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    itemPickup
                );

                return true;
            }

            return false;
        }

        private static bool HasPoolBallDefinition(
            GameObject pickupObject,
            Object poolBallDefinition
        )
        {
            Component itemPickup =
                FindComponentByTypeNameContains(
                    pickupObject,
                    "ItemPickup"
                );

            if (itemPickup == null)
            {
                return false;
            }

            SerializedObject serializedComponent =
                new SerializedObject(
                    itemPickup
                );

            SerializedProperty iterator =
                serializedComponent.GetIterator();

            bool enterChildren = true;

            while (
                iterator.NextVisible(
                    enterChildren
                )
            )
            {
                enterChildren = false;

                if (
                    iterator.propertyType !=
                    SerializedPropertyType.ObjectReference
                )
                {
                    continue;
                }

                if (
                    iterator.name.IndexOf(
                        "definition",
                        System.StringComparison.OrdinalIgnoreCase
                    ) < 0 &&
                    iterator.displayName.IndexOf(
                        "definition",
                        System.StringComparison.OrdinalIgnoreCase
                    ) < 0
                )
                {
                    continue;
                }

                return
                    iterator.objectReferenceValue ==
                    poolBallDefinition;
            }

            return false;
        }

        private static bool HasComponentTypeName(
            GameObject target,
            string typeName
        )
        {
            Component[] components =
                target.GetComponents<Component>();

            for (
                int index = 0;
                index < components.Length;
                index++
            )
            {
                Component component =
                    components[index];

                if (
                    component != null &&
                    component.GetType().Name ==
                    typeName
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static Component FindComponentByTypeNameContains(
            GameObject target,
            string typeNamePart
        )
        {
            Component[] components =
                target.GetComponents<Component>();

            for (
                int index = 0;
                index < components.Length;
                index++
            )
            {
                Component component =
                    components[index];

                if (component == null)
                {
                    continue;
                }

                if (
                    component.GetType().Name.IndexOf(
                        typeNamePart,
                        System.StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindObjectByName(
            Scene scene,
            string objectName
        )
        {
            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                GameObject found =
                    FindObjectInHierarchy(
                        roots[index],
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindObjectInHierarchy(
            GameObject current,
            string objectName
        )
        {
            if (
                current != null &&
                current.name ==
                objectName
            )
            {
                return current;
            }

            if (current == null)
            {
                return null;
            }

            Transform transform =
                current.transform;

            for (
                int index = 0;
                index < transform.childCount;
                index++
            )
            {
                GameObject found =
                    FindObjectInHierarchy(
                        transform.GetChild(
                            index
                        ).gameObject,
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
