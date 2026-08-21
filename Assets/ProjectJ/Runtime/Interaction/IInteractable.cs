using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Interaction // 상호작용 시스템 네임스페이스
{
    public interface IInteractable // 공통 상호작용 대상 규칙
    {
        Transform InteractionTransform { get; } // 상호작용 기준 위치

        bool CanInteract(GameObject interactor); // 현재 상호작용 가능 여부

        void Interact(GameObject interactor); // 상호작용 실행
    }
}
