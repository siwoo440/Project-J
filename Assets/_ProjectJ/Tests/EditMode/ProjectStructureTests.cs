using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Core; // 런타임 어셈블리 형식 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class ProjectStructureTests // 프로젝트 구조 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void RuntimeAssemblyNameMatchesExpectedValue() // 런타임 어셈블리 이름 일치 여부 검증
        {
            Assert.AreEqual("ProjectJ.Runtime", RuntimeAssemblyMarker.AssemblyName); // 예상 어셈블리 이름과 실제 값 비교
        }

        [Test] // Unity Test Runner 테스트 지정
        public void RootNamespaceMatchesExpectedValue() // 최상위 네임스페이스 일치 여부 검증
        {
            Assert.AreEqual("ProjectJ", RuntimeAssemblyMarker.RootNamespace); // 예상 네임스페이스와 실제 값 비교
        }
    }
}
