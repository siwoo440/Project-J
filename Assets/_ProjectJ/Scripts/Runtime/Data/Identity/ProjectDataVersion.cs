using System; // 직렬화와 값 비교 기능 참조
using UnityEngine; // Unity 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct ProjectDataVersion : IEquatable<ProjectDataVersion> // 데이터 버전 값 형식 선언
    {
        [SerializeField] private int major; // 호환성이 달라지는 주 버전 저장
        [SerializeField] private int minor; // 기능 추가용 부 버전 저장
        [SerializeField] private int patch; // 수정용 패치 버전 저장

        public int Major => major; // 주 버전 값 반환
        public int Minor => minor; // 부 버전 값 반환
        public int Patch => patch; // 패치 버전 값 반환
        public bool IsValid => major >= 1 && minor >= 0 && patch >= 0; // 데이터 버전 유효 여부 반환

        public ProjectDataVersion(int major, int minor, int patch) // 데이터 버전 값 생성
        {
            this.major = major; // 전달된 주 버전 저장
            this.minor = minor; // 전달된 부 버전 저장
            this.patch = patch; // 전달된 패치 버전 저장
        }

        public bool Equals(ProjectDataVersion other) // 다른 데이터 버전과 값 일치 여부 비교
        {
            return major == other.major // 주 버전 일치 여부 확인
                && minor == other.minor // 부 버전 일치 여부 확인
                && patch == other.patch; // 패치 버전 일치 여부 확인
        }

        public override bool Equals(object obj) // 일반 객체와 데이터 버전 일치 여부 비교
        {
            return obj is ProjectDataVersion other && Equals(other); // 형식과 값 일치 여부 반환
        }

        public override int GetHashCode() // 데이터 버전 해시 코드 생성
        {
            return HashCode.Combine(major, minor, patch); // 세 버전 값을 조합한 해시 코드 반환
        }

        public override string ToString() // 데이터 버전을 문자열로 변환
        {
            return $"{major}.{minor}.{patch}"; // 점으로 구분된 버전 문자열 반환
        }

        public static bool operator ==(ProjectDataVersion left, ProjectDataVersion right) // 두 데이터 버전의 일치 여부 비교 연산
        {
            return left.Equals(right); // 값 일치 여부 반환
        }

        public static bool operator !=(ProjectDataVersion left, ProjectDataVersion right) // 두 데이터 버전의 불일치 여부 비교 연산
        {
            return !left.Equals(right); // 값 불일치 여부 반환
        }
    }
}
