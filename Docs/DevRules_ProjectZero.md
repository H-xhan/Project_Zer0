# Project_Zer0 – Development Rules (Coding Protocol v1.3)

## 1. 구조 관련
- PlayerController 및 각 모듈(MovementModule, RotationModule, CameraRigMouseLookTPS, AnimationModule, EfficiencyModule, TimeSystemController)
  - 이름 변경 금지
  - 역할 변경 금지
  - public API(메서드, 매개변수, 반환 타입) 변경 금지

- Core/Player/Modules/... 전체 폴더 구조 변경 금지  
- 스크립트 파일의 폴더 위치 변경 금지  
- 외부 시스템(Time/Quest/Inventory 등)의 구조 변경 금지  
- 신규 기능 추가는 기존 구조 유지한 상태로 확장 방식 사용

---

## 2. 시간 시스템 규칙
- 시간 차감 로직은 **오직 TimeSystemController**만 수행  
- 다른 스크립트에서 직접 시간 차감 호출 금지  
- TimeCost/TimeConfig 관련 수치는 ScriptableObject 또는 Data Table에서 관리  
- 코드 내 Magic Number 금지

---

## 3. Inspector & Comments 규칙
- Inspector 노출 필드는 반드시 [Tooltip] 사용  
- 불필요한 코드/주석 제거  
- 구조 이해를 위한 최소한의 주석만 허용  
- 장식용/불필요한 긴 주석 금지

---

## 4. Debug.Log 규칙
- 출력 포맷: `[SystemName] 메시지`  
  예:  
  `Debug.Log("[Movement] Start Jump");`

---

## 5. 리팩터링 규칙
- 한 번에 한 파일만 수정  
- public API는 절대 변경 금지  
- 게임 플레이 감각(속도, 점프 타이밍 등) 변경 금지  
- 필요 시 private 메서드 분리, 중복 제거, 조건문 정리만 허용  
- 테스트 씬을 통해 기능 검증 필수  
- 핵심 로직(TimeSystemController / MovementModule / EfficiencyModule 등) 구조 변경 금지

---

## 6. Git 규칙
- 리팩터링은 반드시 별도 브랜치에서 작업  
- main에 직접 push 금지  
- Pull Request 생성 후 자동/수동 리뷰 후 merge  
- 커밋 메시지 규칙:
  - `refactor:` 구조 정리
  - `fix:` 버그 수정
  - `feat:` 신규 기능 추가

---

## 7. AI 협업 규칙
- 코덱스는 구조 자체를 변경할 수 없음  
- 이 문서를 모든 수정 작업의 기준으로 반드시 준수  
- 요청 없는 파일 추가/삭제 금지  
