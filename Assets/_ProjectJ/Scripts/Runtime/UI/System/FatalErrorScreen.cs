using UnityEngine; // Unity 즉시 모드 UI와 화면 기능 참조

namespace ProjectJ.UI // 프로젝트 UI 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 오브젝트의 치명 오류 화면 중복 추가 방지
    public sealed class FatalErrorScreen : MonoBehaviour // 게임 시작 차단 오류 안내 화면 선언
    {
        private const float PanelWidth = 680f; // 오류 안내 패널 너비 선언
        private const float PanelHeight = 360f; // 오류 안내 패널 높이 선언
        private const float ContentPadding = 24f; // 오류 안내 패널 내부 여백 선언
        private const float ButtonHeight = 44f; // 게임 종료 버튼 높이 선언
        private bool isVisible; // 오류 안내 화면 표시 여부 저장
        private string title = "게임을 시작할 수 없습니다"; // 오류 안내 기본 제목 저장
        private string message = "필수 데이터 초기화에 실패했습니다."; // 오류 안내 기본 내용 저장
        private GUIStyle titleStyle; // 오류 제목 표시 스타일 저장
        private GUIStyle messageStyle; // 오류 내용 표시 스타일 저장
        private GUIStyle buttonStyle; // 오류 화면 버튼 스타일 저장

        public bool IsVisible => isVisible; // 오류 안내 화면 표시 여부 반환

        public void Show(string newTitle, string newMessage) // 치명 오류 제목과 내용을 화면에 표시
        {
            title = string.IsNullOrWhiteSpace(newTitle) ? title : newTitle.Trim(); // 비어 있지 않은 오류 제목 저장
            message = string.IsNullOrWhiteSpace(newMessage) ? message : newMessage.Trim(); // 비어 있지 않은 오류 내용 저장
            isVisible = true; // 오류 안내 화면 표시 상태 적용
            Cursor.lockState = CursorLockMode.None; // 오류 화면 조작용 커서 잠금 해제
            Cursor.visible = true; // 오류 화면 조작용 커서 표시
        }

        public void Hide() // 치명 오류 안내 화면 숨김
        {
            isVisible = false; // 오류 안내 화면 숨김 상태 적용
        }

        private void OnGUI() // 치명 오류 안내 화면 즉시 모드 UI 그리기
        {
            if (!isVisible) // 오류 안내 화면 표시 여부 확인
            {
                return; // 숨김 상태에서 UI 그리기 생략
            }

            EnsureStyles(); // 현재 GUI 스킨 기반 표시 스타일 준비
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none); // 전체 화면 입력 차단 배경 그리기
            float panelX = Mathf.Max(0f, (Screen.width - PanelWidth) * 0.5f); // 화면 중앙 패널 가로 위치 계산
            float panelY = Mathf.Max(0f, (Screen.height - PanelHeight) * 0.5f); // 화면 중앙 패널 세로 위치 계산
            float panelWidth = Mathf.Min(PanelWidth, Screen.width); // 현재 화면에 맞는 패널 너비 계산
            float panelHeight = Mathf.Min(PanelHeight, Screen.height); // 현재 화면에 맞는 패널 높이 계산
            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight); // 오류 안내 패널 영역 생성
            GUI.Box(panelRect, GUIContent.none); // 오류 안내 패널 배경 그리기
            Rect contentRect = new Rect(panelRect.x + ContentPadding, panelRect.y + ContentPadding, panelRect.width - ContentPadding * 2f, panelRect.height - ContentPadding * 2f); // 오류 안내 내부 영역 생성
            GUILayout.BeginArea(contentRect); // 오류 안내 내부 자동 배치 영역 시작
            GUILayout.Label(title, titleStyle); // 치명 오류 제목 표시
            GUILayout.Space(20f); // 제목과 내용 사이 여백 추가
            GUILayout.Label(message, messageStyle); // 치명 오류 상세 내용 표시
            GUILayout.FlexibleSpace(); // 버튼을 패널 아래쪽으로 배치

            if (GUILayout.Button("게임 종료", buttonStyle, GUILayout.Height(ButtonHeight))) // 게임 종료 버튼 선택 여부 확인
            {
                Application.Quit(); // 실행 중인 게임 종료 요청
            }

            GUILayout.EndArea(); // 오류 안내 내부 자동 배치 영역 종료
        }

        private void EnsureStyles() // 오류 화면 전용 GUI 스타일 최초 생성
        {
            if (titleStyle != null) // 오류 화면 스타일 생성 완료 여부 확인
            {
                return; // 기존 스타일 재사용
            }

            titleStyle = new GUIStyle(GUI.skin.label); // 기본 라벨 기반 제목 스타일 생성
            titleStyle.fontSize = 28; // 제목 글자 크기 설정
            titleStyle.fontStyle = FontStyle.Bold; // 제목 굵은 글꼴 설정
            titleStyle.alignment = TextAnchor.MiddleCenter; // 제목 중앙 정렬 설정
            titleStyle.wordWrap = true; // 긴 제목 자동 줄바꿈 설정
            messageStyle = new GUIStyle(GUI.skin.label); // 기본 라벨 기반 내용 스타일 생성
            messageStyle.fontSize = 18; // 내용 글자 크기 설정
            messageStyle.alignment = TextAnchor.UpperLeft; // 내용 왼쪽 위 정렬 설정
            messageStyle.wordWrap = true; // 긴 오류 내용 자동 줄바꿈 설정
            buttonStyle = new GUIStyle(GUI.skin.button); // 기본 버튼 기반 종료 버튼 스타일 생성
            buttonStyle.fontSize = 18; // 종료 버튼 글자 크기 설정
        }
    }
}
