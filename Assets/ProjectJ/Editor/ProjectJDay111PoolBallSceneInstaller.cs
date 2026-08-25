using System.Collections.Generic; // SortKey 중복 검사
using Fusion; // NetworkObject와 NetworkObjectBaker 사용
using UnityEditor; // Editor 메뉴와 SerializedObject 사용
using UnityEditor.SceneManagement; // Day49 Scene 열기와 저장
using UnityEngine; // GameObject와 Component 사용
using UnityEngine.SceneManagement; // Scene 탐색과 배치

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay111PoolBallSceneInstaller
    {
        private const string Day49ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day49/Day49_AllSystemsTest.unity"; // Day49 테스트 Scene 경로

        private const string PoolBallDefinitionPath =
            "Assets/ProjectJ/Data/Items/Item_PoolBall.asset"; // 풀 공 Definition 경로

        private const string ReferencePickupName =
            "Pickup_9_mine_A"; // 복제 기준 Mine Pickup 이름

        private static readonly string[] PickupNames =
        {
            "Pickup_10_pool_ball_A",
            "Pickup_10_pool_ball_B",
            "Pickup_10_pool_ball_C",
            "Pickup_10_pool_ball_D",
            "Pickup_10_pool_ball_E",
            "Pickup_10_pool_ball_F"
        }; // 풀 공 Pickup 6개 이름

        private static readonly float[] PickupXPositions =
        {
            -87f,
            -83f,
            -79f,
            -75f,
            -71f,
            -67f
        }; // 풀 공 Pickup X 배치 위치

        private static NetworkObjectBaker networkObjectBaker; // Fusion Baker 재사용 참조

        private static NetworkObjectBaker Baker
        {
            get
            {
                if (networkObjectBaker == null)
                {
                    networkObjectBaker =
                        new NetworkObjectBaker(); // Fusion Baker 최초 생성
                }

                return networkObjectBaker; // 재사용 Baker 반환
            }
        }

        [MenuItem(
            "Project J/Day111/Install Pool Ball Pickups"
        )]
        private static void InstallFromMenu()
        {
            InstallPoolBallPickups(); // 풀 공 Pickup 수동 설치 실행
        }

        private static void InstallPoolBallPickups()
        {
            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    Day49ScenePath
                ); // Day49 Scene Asset 로드

            if (sceneAsset == null)
            {
                Debug.LogError(
                    "[Project J/Day111] Day49 테스트 Scene을 찾지 못했습니다. / " +
                    Day49ScenePath
                ); // Scene 누락 오류 출력

                return;
            }

            Object poolBallDefinition =
                AssetDatabase.LoadAssetAtPath<Object>(
                    PoolBallDefinitionPath
                ); // 풀 공 Definition 로드

            if (poolBallDefinition == null)
            {
                Debug.LogError(
                    "[Project J/Day111] Item_PoolBall.asset을 찾지 못했습니다. / " +
                    PoolBallDefinitionPath
                ); // Definition 누락 오류 출력

                return;
            }

            Scene scene =
                SceneManager.GetSceneByPath(
                    Day49ScenePath
                ); // 현재 열린 Day49 Scene 검색

            bool openedByInstaller =
                !scene.IsValid() ||
                !scene.isLoaded; // Installer가 Scene을 새로 열어야 하는지 확인

            if (
                !openedByInstaller &&
                scene.isDirty &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return; // 기존 Scene 저장 취소 시 설치 중단
            }

            if (openedByInstaller)
            {
                scene =
                    EditorSceneManager.OpenScene(
                        Day49ScenePath,
                        OpenSceneMode.Additive
                    ); // 현재 Scene을 유지하고 Day49 Scene 추가 열기
            }

            try
            {
                bool changed;
                bool configured =
                    TryEnsurePickups(
                        scene,
                        poolBallDefinition,
                        out changed
                    ); // 풀 공 Pickup 생성 또는 기존 상태 확인

                if (!configured)
                {
                    return; // 생성 단계 실패 시 저장 중단
                }

                if (!changed)
                {
                    Debug.Log(
                        "[Project J/Day111] Day49 풀 공 Pickup 6개가 이미 정상 구성되어 있습니다. / " +
                        Day49ScenePath
                    ); // 기존 정상 구성 로그 출력

                    return;
                }

                EditorSceneManager.MarkSceneDirty(
                    scene
                ); // Pickup 생성 변경 상태 표시

                bool sceneSaved =
                    EditorSceneManager.SaveScene(
                        scene
                    ); // Scene을 먼저 저장하여 Scene NetworkObject Bake 반영

                if (!sceneSaved)
                {
                    Debug.LogError(
                        "[Project J/Day111] Day49 Scene 저장에 실패했습니다. / " +
                        Day49ScenePath
                    ); // Scene 저장 실패 출력

                    return;
                }

                AssetDatabase.SaveAssets(); // 변경된 Asset 저장
                AssetDatabase.Refresh(); // Asset Database 갱신

                if (scene.isDirty)
                {
                    if (
                        !EditorSceneManager.SaveScene(
                            scene
                        )
                    )
                    {
                        Debug.LogError(
                            "[Project J/Day111] Fusion Bake 변경 사항의 추가 Scene 저장에 실패했습니다. / " +
                            Day49ScenePath
                        ); // Bake 후 추가 저장 실패 출력

                        return;
                    }
                }

                GameObject referencePickup =
                    FindObjectByName(
                        scene,
                        ReferencePickupName
                    ); // 저장 후 기준 Mine Pickup 다시 검색

                NetworkObject referenceNetworkObject =
                    referencePickup != null
                        ? referencePickup.GetComponent<NetworkObject>()
                        : null; // 저장 후 기준 NetworkObject 다시 확인

                if (referenceNetworkObject == null)
                {
                    Debug.LogError(
                        "[Project J/Day111] Scene 저장 후 기준 Mine Pickup의 NetworkObject를 찾지 못했습니다. / " +
                        ReferencePickupName
                    ); // 기준 NetworkObject 누락 출력

                    return;
                }

                if (
                    !AreExistingPickupsValid(
                        scene,
                        poolBallDefinition,
                        referenceNetworkObject.SortKey,
                        true
                    )
                )
                {
                    Debug.LogError(
                        "[Project J/Day111] Scene 저장 후 풀 공 Pickup의 Fusion 구성이 올바르지 않습니다. " +
                        "Console의 직전 세부 오류를 확인하십시오."
                    ); // 저장 후 최종 Fusion 검증 실패 출력

                    return;
                }

                Debug.Log(
                    "[Project J/Day111] Day49 풀 공 Pickup 6개 재구성 및 Fusion 검증 완료. / " +
                    Day49ScenePath
                ); // 최종 설치 성공 출력
            }
            finally
            {
                if (
                    openedByInstaller &&
                    scene.IsValid() &&
                    scene.isLoaded
                )
                {
                    EditorSceneManager.CloseScene(
                        scene,
                        true
                    ); // Installer가 연 Day49 Scene만 닫기
                }
            }
        }

        private static bool TryEnsurePickups(
            Scene scene,
            Object poolBallDefinition,
            out bool changed
        )
        {
            changed =
                false; // 초기 변경 없음 설정

            GameObject referencePickup =
                FindObjectByName(
                    scene,
                    ReferencePickupName
                ); // 복제 기준 Mine Pickup 검색

            if (referencePickup == null)
            {
                Debug.LogError(
                    "[Project J/Day111] 복제 기준 Network Pickup을 찾지 못했습니다. / " +
                    ReferencePickupName
                ); // 기준 Pickup 누락 출력

                return false;
            }

            NetworkObject referenceNetworkObject =
                referencePickup.GetComponent<NetworkObject>(); // 기준 NetworkObject 검색

            if (referenceNetworkObject == null)
            {
                Debug.LogError(
                    "[Project J/Day111] 기준 Mine Pickup에 NetworkObject가 없습니다. / " +
                    ReferencePickupName
                ); // 기준 NetworkObject 누락 출력

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
                ); // 기준 NetworkItemBox 누락 출력

                return false;
            }

            if (
                AreExistingPickupsValid(
                    scene,
                    poolBallDefinition,
                    referenceNetworkObject.SortKey,
                    true
                )
            )
            {
                return true; // 기존 6개가 완전히 정상인 경우 유지
            }

            RemoveExistingPoolBallPickups(
                scene
            ); // 기존 불완전 풀 공 Pickup 제거

            Vector3 referencePosition =
                referencePickup.transform.position; // 기준 Pickup 높이와 Z 보관

            Transform referenceParent =
                referencePickup.transform.parent; // 기준 Pickup 부모 보관

            for (
                int index = 0;
                index < PickupNames.Length;
                index++
            )
            {
                GameObject pickupObject =
                    Object.Instantiate(
                        referencePickup
                    ); // 완성된 Mine Pickup Scene Instance 복제

                if (pickupObject == null)
                {
                    Debug.LogError(
                        "[Project J/Day111] Mine Pickup Scene Instance 복제에 실패했습니다. / " +
                        PickupNames[index]
                    ); // 복제 실패 출력

                    return false;
                }

                pickupObject.name =
                    PickupNames[index]; // 풀 공 Pickup 이름 지정

                SceneManager.MoveGameObjectToScene(
                    pickupObject,
                    scene
                ); // 복제본을 Day49 Scene으로 이동

                if (
                    referenceParent != null &&
                    referenceParent.gameObject.scene == scene
                )
                {
                    pickupObject.transform.SetParent(
                        referenceParent,
                        true
                    ); // 기준 Pickup과 같은 Hierarchy 부모 연결
                }

                pickupObject.transform.position =
                    new Vector3(
                        PickupXPositions[index],
                        referencePosition.y,
                        referencePosition.z
                    ); // Stack 테스트용 일렬 배치

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
                    ); // Definition 연결 실패 출력

                    return false;
                }

                NetworkObject networkObject =
                    pickupObject.GetComponent<NetworkObject>(); // 복제본 NetworkObject 검색

                if (networkObject == null)
                {
                    Debug.LogError(
                        "[Project J/Day111] 복제본에 NetworkObject가 없습니다. / " +
                        pickupObject.name
                    ); // 복제본 NetworkObject 누락 출력

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
                    ); // 복제본 NetworkItemBox 누락 출력

                    return false;
                }

                Baker.Bake(
                    pickupObject
                ); // NetworkObject와 NetworkBehaviour Baked Data 준비

                EditorUtility.SetDirty(
                    networkObject
                ); // NetworkObject 변경 상태 표시

                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    networkObject
                ); // Prefab Instance NetworkObject 변경 기록

                EditorUtility.SetDirty(
                    pickupObject
                ); // Pickup GameObject 변경 상태 표시
            }

            changed =
                true; // Pickup 재생성 완료 표시

            if (
                !AreExistingPickupsValid(
                    scene,
                    poolBallDefinition,
                    referenceNetworkObject.SortKey,
                    false
                )
            )
            {
                Debug.LogError(
                    "[Project J/Day111] Scene 저장 전 풀 공 Pickup의 기본 구성이 올바르지 않습니다. " +
                    "Console의 직전 세부 오류를 확인하십시오."
                ); // 저장 전 Component와 Definition 검증 실패 출력

                return false;
            }

            return true; // Scene 저장 단계 진행 허용
        }

        private static bool AreExistingPickupsValid(
            Scene scene,
            Object poolBallDefinition,
            uint referenceSortKey,
            bool validateSortKeys
        )
        {
            HashSet<uint> sortKeys =
                new HashSet<uint>(); // 풀 공 SortKey 중복 검사 집합 생성

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
                    ); // 현재 풀 공 Pickup 검색

                if (pickupObject == null)
                {
                    return false; // 풀 공 Pickup 누락 처리
                }

                NetworkObject networkObject =
                    pickupObject.GetComponent<NetworkObject>(); // 현재 NetworkObject 검색

                if (networkObject == null)
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup에 NetworkObject가 없습니다. / " +
                        pickupObject.name
                    ); // NetworkObject 누락 경고 출력

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
                        "[Project J/Day111] 기존 풀 공 Pickup에 ProjectJNetworkItemBox가 없습니다. / " +
                        pickupObject.name
                    ); // NetworkItemBox 누락 경고 출력

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
                        "[Project J/Day111] 기존 풀 공 Pickup의 Definition이 올바르지 않습니다. / " +
                        pickupObject.name
                    ); // Definition 불일치 경고 출력

                    return false;
                }

                if (!validateSortKeys)
                {
                    continue; // Scene 저장 전에는 SortKey 검증 생략
                }

                uint sortKey =
                    networkObject.SortKey; // 현재 Fusion SortKey 확인

                if (
                    sortKey == 0u ||
                    sortKey == referenceSortKey ||
                    !sortKeys.Add(
                        sortKey
                    )
                )
                {
                    Debug.LogWarning(
                        "[Project J/Day111] 기존 풀 공 Pickup의 Fusion SortKey가 0이거나 기준 Pickup 또는 다른 풀 공과 중복됩니다. / " +
                        pickupObject.name +
                        " / SortKey=" +
                        sortKey
                    ); // 최종 SortKey 오류 출력

                    return false;
                }
            }

            return true; // 모든 풀 공 Pickup 검증 성공
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
                    ); // 기존 풀 공 Pickup 검색

                if (pickupObject == null)
                {
                    continue; // 존재하지 않는 Pickup 건너뜀
                }

                Object.DestroyImmediate(
                    pickupObject
                ); // 불완전 풀 공 Pickup 즉시 제거
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
                ); // ItemPickup Component 검색

            if (itemPickup == null)
            {
                return false; // ItemPickup 누락 처리
            }

            SerializedObject serializedComponent =
                new SerializedObject(
                    itemPickup
                ); // ItemPickup 직렬화 객체 생성

            SerializedProperty iterator =
                serializedComponent.GetIterator(); // 직렬화 속성 순회 준비

            bool enterChildren =
                true; // 첫 순회 하위 속성 포함

            while (
                iterator.NextVisible(
                    enterChildren
                )
            )
            {
                enterChildren =
                    false; // 이후 순회 현재 깊이 유지

                if (
                    iterator.propertyType !=
                    SerializedPropertyType.ObjectReference
                )
                {
                    continue; // ObjectReference 외 속성 건너뜀
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
                    continue; // Definition 속성이 아닌 경우 건너뜀
                }

                iterator.objectReferenceValue =
                    poolBallDefinition; // Item_PoolBall Definition 연결

                serializedComponent.ApplyModifiedPropertiesWithoutUndo(); // 직렬화 변경 적용

                EditorUtility.SetDirty(
                    itemPickup
                ); // ItemPickup 변경 상태 표시

                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    itemPickup
                ); // Prefab Instance Definition 변경 기록

                return true; // Definition 연결 성공
            }

            return false; // Definition 속성 미발견
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
                ); // ItemPickup Component 검색

            if (itemPickup == null)
            {
                return false; // ItemPickup 누락 처리
            }

            SerializedObject serializedComponent =
                new SerializedObject(
                    itemPickup
                ); // ItemPickup 직렬화 객체 생성

            SerializedProperty iterator =
                serializedComponent.GetIterator(); // 직렬화 속성 순회 준비

            bool enterChildren =
                true; // 첫 순회 하위 속성 포함

            while (
                iterator.NextVisible(
                    enterChildren
                )
            )
            {
                enterChildren =
                    false; // 이후 순회 현재 깊이 유지

                if (
                    iterator.propertyType !=
                    SerializedPropertyType.ObjectReference
                )
                {
                    continue; // ObjectReference 외 속성 건너뜀
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
                    continue; // Definition 속성이 아닌 경우 건너뜀
                }

                return
                    iterator.objectReferenceValue ==
                    poolBallDefinition; // 현재 Definition 일치 여부 반환
            }

            return false; // Definition 속성 미발견
        }

        private static bool HasComponentTypeName(
            GameObject target,
            string typeName
        )
        {
            Component[] components =
                target.GetComponents<Component>(); // 대상 Component 전체 수집

            for (
                int index = 0;
                index < components.Length;
                index++
            )
            {
                Component component =
                    components[index]; // 현재 Component 참조

                if (
                    component != null &&
                    component.GetType().Name ==
                    typeName
                )
                {
                    return true; // 정확한 타입 이름 일치
                }
            }

            return false; // 대상 타입 미발견
        }

        private static Component FindComponentByTypeNameContains(
            GameObject target,
            string typeNamePart
        )
        {
            Component[] components =
                target.GetComponents<Component>(); // 대상 Component 전체 수집

            for (
                int index = 0;
                index < components.Length;
                index++
            )
            {
                Component component =
                    components[index]; // 현재 Component 참조

                if (component == null)
                {
                    continue; // Missing Script Component 건너뜀
                }

                if (
                    component.GetType().Name.IndexOf(
                        typeNamePart,
                        System.StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return component; // 부분 타입 이름 일치 Component 반환
                }
            }

            return null; // 대상 Component 미발견
        }

        private static GameObject FindObjectByName(
            Scene scene,
            string objectName
        )
        {
            GameObject[] roots =
                scene.GetRootGameObjects(); // Scene 루트 GameObject 수집

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
                    ); // 현재 루트 하위 재귀 검색

                if (found != null)
                {
                    return found; // 검색된 GameObject 반환
                }
            }

            return null; // 이름 일치 GameObject 미발견
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
                return current; // 현재 GameObject 이름 일치
            }

            if (current == null)
            {
                return null; // null GameObject 처리
            }

            Transform transform =
                current.transform; // 현재 Transform 참조

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
                    ); // 자식 Hierarchy 재귀 검색

                if (found != null)
                {
                    return found; // 자식에서 검색된 GameObject 반환
                }
            }

            return null; // 현재 Hierarchy 미발견
        }
    }
}
