using UnityEngine; // Unity Trigger와 시각 오브젝트 기능 참조
using UnityEngine.Rendering; // 연막 반투명 재질 렌더링 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 연막 구역당 효과 한 개만 허용
    [RequireComponent(typeof(SphereCollider))] // 연막 범위 Trigger 보장
    public sealed class SmokeCloudEffect : MonoBehaviour // 연막탄 시야 방해 구역 선언
    { // 연막탄 시야 방해 구역 묶음
        private float remainingDuration; // 연막 구역 남은 시간 저장

        public void Configure(float radius, float duration, Color visualColor) // 연막 반경과 시간과 표시 구성
        { // 연막 구역 구성 처리
            float safeRadius = Mathf.Max(0.1f, radius); // 최소 연막 반경 보정
            remainingDuration = Mathf.Max(0.1f, duration); // 최소 연막 유지 시간 보정
            SphereCollider trigger = GetComponent<SphereCollider>(); // 연막 Trigger Collider 조회
            trigger.isTrigger = true; // 통과 가능한 연막 범위 적용
            trigger.radius = safeRadius; // 데이터 기반 연막 반경 적용
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>(); // Trigger 접촉 감지용 Rigidbody 추가
            rigidbody.isKinematic = true; // 고정 연막 구역 적용
            rigidbody.useGravity = false; // 연막 구역 중력 비활성화
            CreateVisual(safeRadius, visualColor); // 연막 범위 임시 표시 생성
        } // 연막 구역 구성 처리 종료

        private void Update() // 연막 구역 유지 시간 갱신
        { // 연막 구역 시간 처리
            remainingDuration = Mathf.Max(0f, remainingDuration - Time.deltaTime); // 프레임 시간만큼 연막 시간 감소

            if (remainingDuration <= 0f) // 연막 유지 시간 종료 여부 확인
            { // 연막 구역 제거 처리
                Destroy(gameObject); // 만료된 연막 구역 제거
            } // 연막 구역 제거 처리 종료
        } // 연막 구역 시간 처리 종료

        private void OnTriggerStay(Collider other) // 플레이어가 연막 안에 있는 동안 화면 방해 갱신
        { // 연막 접촉 처리
            PlayerItemEffectController effectController = other == null ? null : other.GetComponentInParent<PlayerItemEffectController>(); // 접촉 플레이어 효과 관리자 조회
            effectController?.ApplySmoke(0.2f); // Trigger 갱신 사이 유지되는 연막 화면 방해 적용
        } // 연막 접촉 처리 종료

        private void CreateVisual(float radius, Color visualColor) // 연막 구역 임시 구체 표시 생성
        { // 연막 표시 생성 처리
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 연막 임시 구체 생성
            visualObject.name = "Visual"; // 연막 표시 이름 지정
            visualObject.transform.SetParent(transform, false); // 연막 루트 아래 표시 배치
            visualObject.transform.localPosition = Vector3.zero; // 연막 중심에 표시 배치
            visualObject.transform.localScale = Vector3.one * radius * 2f; // 연막 반경과 같은 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 구체 Collider 조회

            if (visualCollider != null) // 중복 Collider 존재 여부 확인
            { // 중복 Collider 제거 처리
                Destroy(visualCollider); // 루트 Trigger와 겹치는 Collider 제거
            } // 중복 Collider 제거 처리 종료

            Renderer visualRenderer = visualObject.GetComponent<Renderer>(); // 연막 표시 Renderer 조회

            if (visualRenderer != null) // Renderer 존재 여부 확인
            { // 연막 표시 색상 처리
                Color cloudColor = visualColor; // 데이터 기반 연막 색상 복사
                cloudColor.a = 0.35f; // 연막 임시 표시 투명도 적용
                Material cloudMaterial = visualRenderer.material; // 연막 표시 전용 재질 인스턴스 조회
                cloudMaterial.color = cloudColor; // 연막 표시 색상 적용
                cloudMaterial.SetFloat("_Surface", 1f); // URP 재질 표면을 투명 방식으로 전환
                cloudMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); // 원본 알파 혼합 방식 적용
                cloudMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); // 배경 알파 혼합 방식 적용
                cloudMaterial.SetFloat("_ZWrite", 0f); // 투명 연막 깊이 쓰기 비활성화
                cloudMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); // URP 투명 표면 키워드 활성화
                cloudMaterial.renderQueue = (int)RenderQueue.Transparent; // 투명 오브젝트 렌더 순서 적용
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off; // 연막 구체 그림자 비활성화
            } // 연막 표시 색상 처리 종료
        } // 연막 표시 생성 처리 종료
    } // 연막탄 시야 방해 구역 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
