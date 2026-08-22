using System; // ReadOnlySpan 사용
using System.Collections.Generic; // Dictionary와 List 사용
using Fusion; // Fusion 네트워크 입력 사용
using Fusion.Sockets; // Fusion 소켓 콜백 사용
using UnityEngine; // MonoBehaviour 사용
using UnityEngine.InputSystem; // 새 Input System 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 입력 공급자 중복 방지
    public sealed class ProjectJFusionInputProvider : // Fusion 입력 공급자
        MonoBehaviour,
        INetworkRunnerCallbacks
    {
        private ProjectJNetworkInput cachedInput; // 현재 프레임 입력 저장
        private ProjectJNetworkInput lastSubmittedInput; // 마지막 전송 입력 저장

        private bool pendingJump; // 점프 단발 입력 보존
        private bool pendingPush; // 밀치기 단발 입력 보존
        private bool pendingItemSlotLeft; // Q 단발 입력 보존
        private bool pendingItemSlotRight; // E 단발 입력 보존
        private bool pendingItemUse; // 우클릭 사용 시작 단발 입력 보존

        public Vector2 LastSubmittedMove => // 마지막 이동 입력 조회
            lastSubmittedInput.Move;

        public bool LastSubmittedJump => // 마지막 점프 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Jump
            );

        public bool LastSubmittedSprint => // 마지막 달리기 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Sprint
            );

        public bool LastSubmittedCrouch => // 마지막 앉기 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Crouch
            );

        public bool LastSubmittedPush => // 마지막 밀치기 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.Push
            );

        public bool LastSubmittedItemSlotLeft => // 마지막 Q 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.ItemSlotLeft
            );

        public bool LastSubmittedItemSlotRight => // 마지막 E 입력 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.ItemSlotRight
            );

        public bool LastSubmittedItemUse => // 마지막 아이템 사용 시작 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.ItemUse
            );

        public bool LastSubmittedItemUseHeld => // 마지막 아이템 사용 유지 조회
            lastSubmittedInput.Buttons.IsSet(
                ProjectJNetworkButton.ItemUseHeld
            );

        public string LastSubmittedTick // 마지막 입력 Tick 문자열
        {
            get;
            private set;
        } =
            "-"; // 초기 Tick 표시

        public int SubmitCount // 입력 전송 횟수
        {
            get;
            private set;
        }

        private void Update() // 로컬 장치 입력 수집
        {
            Keyboard keyboard = // 현재 키보드 조회
                Keyboard.current;

            Mouse mouse = // 현재 마우스 조회
                Mouse.current;

            Vector2 move = // 이동 입력 초기화
                Vector2.zero;

            if (keyboard != null) // 키보드 연결 확인
            {
                if (keyboard.wKey.isPressed) // 전진 키 확인
                {
                    move.y += 1f; // 전진 입력 추가
                }

                if (keyboard.sKey.isPressed) // 후진 키 확인
                {
                    move.y -= 1f; // 후진 입력 추가
                }

                if (keyboard.aKey.isPressed) // 좌측 키 확인
                {
                    move.x -= 1f; // 좌측 입력 추가
                }

                if (keyboard.dKey.isPressed) // 우측 키 확인
                {
                    move.x += 1f; // 우측 입력 추가
                }

                if (keyboard.spaceKey.wasPressedThisFrame) // 점프 누름 확인
                {
                    pendingJump = true; // 다음 Fusion Tick까지 점프 보존
                }

                if (keyboard.gKey.wasPressedThisFrame) // 개발 테스트 밀치기 키 확인
                {
                    pendingPush = true; // 다음 Fusion Tick까지 밀치기 보존
                }

                if (keyboard.qKey.wasPressedThisFrame) // 첫 슬롯 선택 확인
                {
                    pendingItemSlotLeft = true; // 다음 Fusion Tick까지 Q 보존
                }

                if (keyboard.eKey.wasPressedThisFrame) // 두 번째 슬롯 선택 확인
                {
                    pendingItemSlotRight = true; // 다음 Fusion Tick까지 E 보존
                }

                cachedInput.Buttons.Set( // 달리기 버튼 저장
                    ProjectJNetworkButton.Sprint,
                    keyboard.leftShiftKey.isPressed ||
                    keyboard.rightShiftKey.isPressed
                );

                cachedInput.Buttons.Set( // 앉기 버튼 저장
                    ProjectJNetworkButton.Crouch,
                    keyboard.leftCtrlKey.isPressed ||
                    keyboard.rightCtrlKey.isPressed
                );
            }
            else // 키보드 미연결 처리
            {
                cachedInput.Buttons.Set( // 달리기 해제
                    ProjectJNetworkButton.Sprint,
                    false
                );

                cachedInput.Buttons.Set( // 앉기 해제
                    ProjectJNetworkButton.Crouch,
                    false
                );
            }

            if (mouse != null) // 마우스 연결 확인
            {
                if (mouse.leftButton.wasPressedThisFrame) // 좌클릭 밀치기 확인
                {
                    pendingPush = true; // 다음 Fusion Tick까지 밀치기 보존
                }

                if (mouse.rightButton.wasPressedThisFrame) // 우클릭 사용 시작 확인
                {
                    pendingItemUse = true; // 다음 Fusion Tick까지 사용 시작 보존
                }

                cachedInput.Buttons.Set( // 우클릭 유지 상태 저장
                    ProjectJNetworkButton.ItemUseHeld,
                    mouse.rightButton.isPressed
                );
            }
            else
            {
                cachedInput.Buttons.Set( // 마우스 없음 시 사용 유지 해제
                    ProjectJNetworkButton.ItemUseHeld,
                    false
                );
            }

            if (move.sqrMagnitude > 1f) // 대각선 입력 크기 제한
            {
                move.Normalize(); // 이동 입력 정규화
            }

            cachedInput.Move = move; // 이동 입력 저장
        }

        public void OnInput( // Fusion Tick 입력 제출
            NetworkRunner runner,
            NetworkInput input
        )
        {
            ProjectJNetworkInput networkInput = // 현재 입력 복사
                cachedInput;

            networkInput.Buttons.Set( // 점프 단발 입력 삽입
                ProjectJNetworkButton.Jump,
                pendingJump
            );

            networkInput.Buttons.Set( // 밀치기 단발 입력 삽입
                ProjectJNetworkButton.Push,
                pendingPush
            );

            networkInput.Buttons.Set( // Q 슬롯 선택 입력 삽입
                ProjectJNetworkButton.ItemSlotLeft,
                pendingItemSlotLeft
            );

            networkInput.Buttons.Set( // E 슬롯 선택 입력 삽입
                ProjectJNetworkButton.ItemSlotRight,
                pendingItemSlotRight
            );

            networkInput.Buttons.Set( // 우클릭 사용 시작 입력 삽입
                ProjectJNetworkButton.ItemUse,
                pendingItemUse
            );

            input.Set( // Fusion 입력 제출
                networkInput
            );

            lastSubmittedInput = networkInput; // 마지막 전송 입력 기록

            LastSubmittedTick = // 마지막 Tick 기록
                runner.InputTick.ToString();

            SubmitCount++; // 전송 횟수 증가

            pendingJump = false; // 점프 단발 입력 초기화
            pendingPush = false; // 밀치기 단발 입력 초기화
            pendingItemSlotLeft = false; // Q 단발 입력 초기화
            pendingItemSlotRight = false; // E 단발 입력 초기화
            pendingItemUse = false; // 우클릭 사용 시작 단발 입력 초기화
        }

        public void OnObjectExitAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnObjectEnterAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnPlayerJoined(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnPlayerLeft(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnShutdown(
            NetworkRunner runner,
            ShutdownReason shutdownReason
        )
        {
        }

        public void OnDisconnectedFromServer(
            NetworkRunner runner,
            NetDisconnectReason reason
        )
        {
        }

        public void OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token
        )
        {
        }

        public void OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason
        )
        {
        }

#pragma warning disable CS0618
        public void OnUserSimulationMessage(
            NetworkRunner runner,
            SimulationMessagePtr message
        )
        {
        }
#pragma warning restore CS0618

        public void OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ReadOnlySpan<byte> data
        )
        {
        }

        public void OnReliableDataProgress(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            float progress
        )
        {
        }

        public void OnInputMissing(
            NetworkRunner runner,
            PlayerRef player,
            NetworkInput input
        )
        {
        }

        public void OnConnectedToServer(
            NetworkRunner runner
        )
        {
        }

        public void OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList
        )
        {
        }

        public void OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data
        )
        {
        }

        public void OnHostMigration(
            NetworkRunner runner,
            HostMigrationToken hostMigrationToken
        )
        {
        }

        public void OnSceneLoadDone(
            NetworkRunner runner
        )
        {
        }

        public void OnSceneLoadStart(
            NetworkRunner runner
        )
        {
        }
    }
}
