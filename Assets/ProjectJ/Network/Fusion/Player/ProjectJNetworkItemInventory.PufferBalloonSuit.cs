using System.Collections.Generic; // 대상별 재발동 제한 저장
using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 복어 풍선옷 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 복어 풍선옷 네트워크 상태
    {
        private readonly List<ProjectJNetworkExternalGameplay> pufferTargetBuffer =
            new List<ProjectJNetworkExternalGameplay>(8); // 현재 Runner 대상 재사용 목록

        private readonly Dictionary<int, TickTimer> pufferTargetCooldowns =
            new Dictionary<int, TickTimer>(8); // PlayerRef Index별 재발동 제한

        [Networked] // 복어 풍선옷 지속 시간 동기화
        private TickTimer NetworkPufferBalloonSuitTimer
        {
            get; // Networked 상태 조회
            set; // State Authority 상태 갱신
        }

        public bool IsPufferBalloonSuitActive =>
            IsTimerActive(NetworkPufferBalloonSuitTimer); // 복어 풍선옷 활성 여부 조회

        public float PufferBalloonSuitRemaining =>
            GetRemainingTime(NetworkPufferBalloonSuitTimer); // 남은 지속 시간 조회

        private void InitializePufferBalloonSuitAuthority()
        {
            NetworkPufferBalloonSuitTimer = TickTimer.None; // 최초 효과 초기화
            pufferTargetCooldowns.Clear(); // 대상별 재발동 기록 초기화
        }

        private bool UsePufferBalloonSuitAuthority()
        {
            bool runnerReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority; // 서버 권한 준비 확인

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed; // 경기 입력 허용 상태 확인

            if (!ProjectJPufferBalloonSuitPolicy.CanActivate(
                IsPufferBalloonSuitActive,
                gameplayAllowed,
                runnerReady
            ))
            {
                return false; // 중첩·권한·경기 상태 실패 시 소비 차단
            }

            pufferTargetCooldowns.Clear(); // 새 사용마다 대상 재발동 기록 초기화
            NetworkPufferBalloonSuitTimer = TickTimer.CreateFromSeconds(
                Runner,
                ProjectJPufferBalloonSuitPolicy.DurationSeconds
            ); // 서버 기준 5초 효과 시작

            return true; // 사용 성공
        }

        private void UpdatePufferBalloonSuitAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return; // State Authority 외 자동 판정 차단
            }

            if (!IsPufferBalloonSuitActive)
            {
                NetworkPufferBalloonSuitTimer = TickTimer.None; // 만료 Timer 정리
                pufferTargetCooldowns.Clear(); // 만료 시 대상 기록 정리
                return; // 비활성 상태 판정 종료
            }

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                ClearPufferBalloonSuitAuthority(); // 경기 종료·완주 상태에서 즉시 해제
                return;
            }

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                pufferTargetBuffer
            ); // 현재 Runner의 모든 Player 조회

            Vector3 origin = transform.position; // 사용자 중심 위치

            for (int index = 0; index < pufferTargetBuffer.Count; index++)
            {
                ProjectJNetworkExternalGameplay target = pufferTargetBuffer[index]; // 현재 대상

                if (
                    target == null ||
                    target.Object == null ||
                    !target.Object.IsValid
                )
                {
                    continue; // 무효 Player 제외
                }

                bool isSelf = target == externalGameplay; // 자기 자신 여부
                int targetIndex = target.Object.InputAuthority.AsIndex; // 대상 PlayerRef Index
                bool cooldownActive = IsPufferTargetCooldownActive(targetIndex); // 재발동 제한 확인

                if (!ProjectJPufferBalloonSuitPolicy.CanTriggerTarget(
                    isSelf,
                    cooldownActive
                ))
                {
                    continue; // 자신·대상 쿨타임 제외
                }

                if (!target.GameplayInputAllowed)
                {
                    continue; // 이미 완주·종료 처리된 대상 제외
                }

                Vector3 offset = target.transform.position - origin; // 사용자→대상 방향
                float distance = offset.magnitude; // 3차원 실제 거리 계산

                if (!ProjectJPufferBalloonSuitPolicy.IsInsideDetectionRadius(distance))
                {
                    continue; // 1.2m 밖 대상 제외
                }

                offset.y = 0f; // 복어 밀치기는 수평 바깥 방향만 사용

                if (offset.sqrMagnitude <= 0.0001f)
                {
                    offset = transform.forward; // 같은 위치일 때 사용자 전방 사용
                    offset.y = 0f; // 수평 방향 유지
                }

                if (offset.sqrMagnitude <= 0.0001f)
                {
                    offset = Vector3.forward; // 잘못된 전방 최종 보정
                }

                Vector3 velocityChange =
                    offset.normalized *
                    ProjectJPufferBalloonSuitPolicy.PushSpeedMetersPerSecond; // 6m/s 바깥 방향 외력

                bool applied = target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Item,
                    velocityChange
                ); // 기존 젤리 보호막·부활 보호를 포함한 공통 외력 판정

                if (!applied)
                {
                    continue; // 보호 상태 등으로 차단되면 쿨타임을 시작하지 않음
                }

                pufferTargetCooldowns[targetIndex] = TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJPufferBalloonSuitPolicy.PerTargetCooldownSeconds
                ); // 성공한 대상만 1초 재발동 제한
            }
        }

        private bool IsPufferTargetCooldownActive( // 대상별 재발동 제한 확인
            int targetIndex // PlayerRef Index
        )
        {
            if (!pufferTargetCooldowns.TryGetValue(
                targetIndex,
                out TickTimer cooldown
            ))
            {
                return false; // 기록이 없으면 즉시 발동 가능
            }

            if (cooldown.ExpiredOrNotRunning(Runner))
            {
                pufferTargetCooldowns.Remove(targetIndex); // 만료 기록 제거
                return false; // 다시 발동 허용
            }

            return true; // 아직 1초 제한 중
        }

        private void ClearPufferBalloonSuitAuthority()
        {
            NetworkPufferBalloonSuitTimer = TickTimer.None; // 효과 즉시 해제
            pufferTargetCooldowns.Clear(); // 대상별 재발동 기록 제거
        }
    }
}
