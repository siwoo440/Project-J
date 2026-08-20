# 6일차 개발일지 - 공통 데이터 ID 및 ScriptableObject 기반 구축

## 오늘의 목표

Project J에서 앞으로 사용할 게임 데이터의 공통 기반을 만든다.

아이템, 장애물, 맵 모듈 등 여러 데이터가
각자 다른 형식을 사용하지 않도록
공통 ID 규칙과 ScriptableObject 기본 구조를 먼저 구축한다.

이번 일차에서는 실제 아이템 데이터나 장애물 데이터를 만들지 않고,
향후 여러 데이터 타입이 공통으로 사용할 기반만 구현한다.

## 구현 내용

### 1. 공통 데이터 ID 생성 기능 구현

`GameDataId`를 추가했다.

주요 역할:

- 새로운 고유 ID 생성
- ID 형식 유효성 검사

ID는 GUID 기반 32자리 문자열 형식을 사용한다.

예시:

```text
97e53a1480a94592ad42499832b39d87
```

이를 통해 각 데이터 Asset이 내부적으로 구분될 수 있는
공통 식별 기준을 마련했다.

### 2. 공통 ScriptableObject 기반 구현

`GameData` ScriptableObject를 추가했다.

공통 필드는 다음과 같다.

- Id
- Display Name
- Description

이후 여러 데이터 타입이 공통 구조를 기반으로 확장될 수 있도록 했다.

예상 확장 구조:

```text
GameData
├─ ItemData
├─ ObstacleData
├─ MapModuleData
└─ 기타 게임 데이터
```

### 3. ID 자동 생성 및 복구

`GameData`의 `OnValidate()`에서
ID가 없거나 잘못된 형식일 경우 자동으로 새로운 ID를 생성하도록 구성했다.

따라서 새 데이터 Asset을 만들었을 때
별도로 ID를 직접 입력하지 않아도 자동으로 식별자가 생성된다.

또한 Inspector에서 ID를 삭제하거나 잘못된 값을 넣어도
유효한 ID로 다시 생성된다.

### 4. Display Name 정리

Inspector에서 입력된 Display Name의
앞뒤 불필요한 공백을 자동으로 제거하도록 처리했다.

### 5. 테스트용 ScriptableObject Asset 검증

공통 데이터 구조가 실제 Unity Inspector에서 정상 작동하는지 확인하기 위해
임시 `Day6_TestData` Asset을 생성해 다음 내용을 검증했다.

- Game Data Asset 생성 가능
- ID 자동 생성
- Display Name 입력
- Description 입력
- ID 삭제 시 새로운 ID 자동 생성

테스트가 끝난 뒤 임시 Asset과 `.meta` 파일은 삭제해
프로젝트에 테스트용 데이터가 남지 않도록 정리했다.

## 생성된 파일

```text
Assets/ProjectJ/Runtime/Data/GameData.cs
Assets/ProjectJ/Runtime/Data/GameDataId.cs
```

## 테스트

- `Create → Project J → Data → Game Data` 메뉴 확인
- ScriptableObject Asset 생성 확인
- Id 자동 생성 확인
- Id 형식 유효성 확인
- Display Name 입력 확인
- Description 입력 확인
- Id 삭제 후 자동 재생성 확인
- 테스트용 Asset 삭제 확인
- Unity Console Error 0 확인

## 결과

Project J에서 앞으로 사용할 게임 데이터의 공통 기반을 구축했다.

이후 아이템, 장애물, 맵 모듈 등 여러 데이터 타입은
동일한 ID 규칙과 ScriptableObject 구조를 기반으로 확장할 수 있다.

또한 테스트를 위해 만든 임시 Asset은 검증 후 제거해
불필요한 개발용 파일이 프로젝트에 누적되지 않도록 정리했다.
