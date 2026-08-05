using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 체크포인트 진행 규칙 범위
    public static class CheckpointProgressRules // 체크포인트 순서와 진행률 계산 도구 선언
    { // 체크포인트 진행 규칙 기능 범위
        public static int ClampCheckpointCount(int checkpointCount) // 전체 체크포인트 개수 안전 보정
        { // 체크포인트 개수 보정 범위
            return Mathf.Max(1, checkpointCount); // 최소 한 개의 체크포인트 보장
        } // 체크포인트 개수 보정 범위 종료

        public static int ClampCheckpointIndex(int checkpointIndex, int checkpointCount) // 현재 체크포인트 번호 안전 보정
        { // 체크포인트 번호 보정 범위
            int validCheckpointCount = ClampCheckpointCount(checkpointCount); // 유효한 전체 체크포인트 개수 계산
            return Mathf.Clamp(checkpointIndex, 0, validCheckpointCount); // 시작 지점부터 마지막 체크포인트까지 번호 제한
        } // 체크포인트 번호 보정 범위 종료

        public static bool IsCheckpointIndexInRange(int checkpointIndex, int checkpointCount) // 체크포인트 번호 유효 범위 판정
        { // 체크포인트 번호 범위 판정 범위
            int validCheckpointCount = ClampCheckpointCount(checkpointCount); // 유효한 전체 체크포인트 개수 계산
            return checkpointIndex >= 1 && checkpointIndex <= validCheckpointCount; // 첫 번째부터 마지막 체크포인트까지 포함 여부 반환
        } // 체크포인트 번호 범위 판정 범위 종료

        public static bool CanActivateCheckpoint(int currentCheckpointIndex, int candidateCheckpointIndex, int checkpointCount) // 더 높은 체크포인트 활성화 가능 여부 판정
        { // 체크포인트 활성화 판정 범위
            if (!IsCheckpointIndexInRange(candidateCheckpointIndex, checkpointCount)) // 후보 체크포인트 범위 확인
            { // 잘못된 후보 체크포인트 범위
                return false; // 범위 밖 체크포인트 활성화 차단
            } // 잘못된 후보 체크포인트 범위 종료

            int validCurrentIndex = ClampCheckpointIndex(currentCheckpointIndex, checkpointCount); // 현재 체크포인트 번호 보정
            return candidateCheckpointIndex > validCurrentIndex; // 현재보다 높은 체크포인트만 활성화 허용
        } // 체크포인트 활성화 판정 범위 종료

        public static float CalculateProgress01(int currentCheckpointIndex, int checkpointCount) // 체크포인트 진행 비율 계산
        { // 체크포인트 진행 비율 계산 범위
            int validCheckpointCount = ClampCheckpointCount(checkpointCount); // 유효한 전체 체크포인트 개수 계산
            int validCurrentIndex = ClampCheckpointIndex(currentCheckpointIndex, validCheckpointCount); // 현재 체크포인트 번호 보정
            return (float)validCurrentIndex / validCheckpointCount; // 0부터 1 사이 체크포인트 진행 비율 반환
        } // 체크포인트 진행 비율 계산 범위 종료
    } // 체크포인트 진행 규칙 기능 범위 종료
} // 체크포인트 진행 규칙 범위 종료
