# 2일차 개발일지 - 핵심 폴더 및 Assembly 구조 정리

## 오늘의 목표

프로젝트가 커질 때 스크립트와 데이터를 기능별로 관리할 수 있도록
Project J 전용 폴더 구조와 Assembly Definition 구조를 구성한다.

## 구현 내용

### 1. Project J 전용 폴더 구조 정리

Assets 하위에 ProjectJ 전용 폴더를 구성했다.

주요 구조

- Assets/ProjectJ/Runtime
- Assets/ProjectJ/Editor
- Assets/ProjectJ/Data
- Assets/ProjectJ/Tests
- Assets/ProjectJ/Scenes

기존 Scene도 ProjectJ/Scenes 경로로 정리했다.

### 2. Runtime Assembly Definition 생성

ProjectJ.Runtime Assembly Definition을 생성했다.

- Assembly Name : ProjectJ.Runtime
- Root Namespace : ProjectJ

앞으로 실제 게임에서 실행되는 플레이어, 경기, 아이템, 맵 등의
런타임 스크립트가 이 Assembly를 기준으로 구성된다.

### 3. Editor Assembly Definition 생성

ProjectJ.Editor Assembly Definition을 생성했다.

- Assembly Name : ProjectJ.Editor
- Root Namespace : ProjectJ.Editor
- Platform : Editor
- Reference : ProjectJ.Runtime

Editor 전용 코드가 Runtime 코드를 사용할 수 있지만
Runtime 코드가 Editor 코드에 의존하지 않도록 방향을 분리했다.

### 4. 기본 Unity 템플릿 파일 정리

프로젝트 생성 시 포함된 URP 안내용 Readme와
TutorialInfo 관련 기본 파일을 제거했다.

실제 Project J 개발에 필요한 파일을 중심으로
Assets 구조를 단순화했다.

### 5. Scene 위치 정리

기존 Scene들을 ProjectJ/Scenes 아래로 이동했다.

- Bootstrap
- MainMenu
- Lobby
- Game

Scene 이동 이후에도 기존 Scene 참조가 유지되도록
Unity 메타데이터와 Build Settings를 함께 갱신했다.

## 테스트

- Unity 프로젝트 정상 실행 확인
- 전체 스크립트 재컴파일 확인
- Console Error 0건 확인
- ProjectJ.Editor → ProjectJ.Runtime 참조 확인
- Assembly Definition Missing Reference 없음 확인
- Scene 이동 이후 Scene 파일 정상 인식 확인

## 결과

Project J의 Runtime 코드와 Editor 코드를 분리할 수 있는
기본 Assembly 구조를 구축했다.

앞으로 기능별 스크립트를 ProjectJ 기준 구조에 추가할 수 있으며
프로젝트 확장 시 코드 의존성을 관리할 수 있는 기반을 마련했다.

## 다음 개발

3일차에는 Bootstrap, MainMenu, Game을 중심으로
최소 Scene 전환 흐름을 구축한다.