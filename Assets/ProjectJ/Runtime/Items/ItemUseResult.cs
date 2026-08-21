namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public enum ItemUseStatus // 공통 아이템 사용 결과 상태
    {
        Success, // 정상 사용 완료
        EmptySlot, // 선택 슬롯이 비어 있음
        InvalidItem, // ItemDefinition이 잘못됨
        NoEffectHandler, // 아직 해당 아이템 효과가 등록되지 않음
        InvalidTarget, // 필요한 Target이 없음
        InvalidPosition, // 설치 또는 사용 위치가 잘못됨
        Blocked, // 현재 상태에서 사용이 차단됨
        Cooldown, // 재사용 대기 중
        EffectFailed, // Effect 내부 실행 실패
        InventoryChanged // Effect 실행 중 선택 슬롯 내용이 변경됨
    }

    public readonly struct ItemUseResult // 공통 아이템 사용 결과
    {
        public ItemUseStatus Status { get; } // 결과 상태

        public string Message { get; } // 개발 및 UI 확장용 메시지

        public bool IsSuccess // 성공 여부 조회
        {
            get
            {
                return Status == ItemUseStatus.Success; // Success 상태만 성공
            }
        }

        public ItemUseResult(ItemUseStatus status, string message) // 결과 생성
        {
            Status = status; // 상태 저장
            Message = message ?? string.Empty; // 메시지 저장
        }

        public static ItemUseResult Success() // 성공 결과 생성
        {
            return new ItemUseResult(
                ItemUseStatus.Success,
                string.Empty
            ); // 성공 반환
        }

        public static ItemUseResult Fail( // 실패 결과 생성
            ItemUseStatus status,
            string message = ""
        )
        {
            return new ItemUseResult(status, message); // 실패 결과 반환
        }
    }
}
