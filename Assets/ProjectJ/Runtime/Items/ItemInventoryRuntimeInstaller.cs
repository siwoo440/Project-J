using System.Collections;
using ProjectJ.Items.Status;
using ProjectJ.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Items
{
    public sealed class ItemInventoryRuntimeInstaller :
        MonoBehaviour
    {
        private PlayerInput installedPlayer;

        private ItemInventoryCanvasView
            inventoryView;

        private ItemStatusHudView
            statusHudView;

        private ItemUseFeedbackCanvasView
            feedbackView;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void CreateInstaller()
        {
            ItemInventoryRuntimeInstaller existing =
                FindFirstObjectByType<
                    ItemInventoryRuntimeInstaller
                >();

            if (existing != null)
            {
                return;
            }

            GameObject installerObject =
                new GameObject(
                    "=== Item Inventory Runtime ==="
                );

            installerObject.AddComponent<
                ItemInventoryRuntimeInstaller
            >();

            DontDestroyOnLoad(
                installerObject
            );
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded +=
                OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(
                InstallWhenPlayerReady()
            );
        }

        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode
        )
        {
            installedPlayer = null;

            StartCoroutine(
                InstallWhenPlayerReady()
            );
        }

        private IEnumerator
            InstallWhenPlayerReady()
        {
            while (installedPlayer == null)
            {
                PlayerInput playerInput =
                    FindLocalPlayerInput();

                if (playerInput != null)
                {
                    InstallForPlayer(
                        playerInput
                    );

                    yield break;
                }

                yield return
                    new WaitForSecondsRealtime(
                        0.25f
                    );
            }
        }

        private void InstallForPlayer(
            PlayerInput playerInput
        )
        {
            installedPlayer =
                playerInput;

            PlayerItemInventory inventory =
                playerInput.GetComponent<
                    PlayerItemInventory
                >();

            if (inventory == null)
            {
                inventory =
                    playerInput.gameObject
                        .AddComponent<
                            PlayerItemInventory
                        >();
            }

            PlayerItemUseController useController =
                playerInput.GetComponent<
                    PlayerItemUseController
                >();

            if (useController == null)
            {
                useController =
                    playerInput.gameObject
                        .AddComponent<
                            PlayerItemUseController
                        >();
            }

            PlayerItemInventoryInput input =
                playerInput.GetComponent<
                    PlayerItemInventoryInput
                >();

            if (input == null)
            {
                playerInput.gameObject
                    .AddComponent<
                        PlayerItemInventoryInput
                    >();
            }

            PlayerItemStatusTracker tracker =
                playerInput.GetComponent<
                    PlayerItemStatusTracker
                >();

            if (tracker == null)
            {
                tracker =
                    playerInput.gameObject
                        .AddComponent<
                            PlayerItemStatusTracker
                        >();
            }

            if (inventoryView == null)
            {
                inventoryView =
                    ItemInventoryCanvasView
                        .Create(transform);
            }

            if (statusHudView == null)
            {
                statusHudView =
                    ItemStatusHudView
                        .Create(transform);
            }

            if (feedbackView == null)
            {
                feedbackView =
                    ItemUseFeedbackCanvasView
                        .Create(transform);
            }

            inventoryView.Bind(
                inventory
            );

            statusHudView.Bind(
                tracker
            );

            feedbackView.Bind(
                useController
            );
        }

        private static PlayerInput
            FindLocalPlayerInput()
        {
            PlayerInput[] inputs =
                FindObjectsByType<PlayerInput>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < inputs.Length;
                i++
            )
            {
                PlayerInput input =
                    inputs[i];

                if (
                    input != null &&
                    input.isActiveAndEnabled &&
                    input.actions != null
                )
                {
                    return input;
                }
            }

            return null;
        }
    }
}
