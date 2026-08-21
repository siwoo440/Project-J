using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Interaction // 상호작용 시스템 네임스페이스
{
    public abstract class InteractableBehaviour : MonoBehaviour, IInteractable // 상호작용 대상 공통 기반
    {
        public virtual Transform InteractionTransform // 기본 상호작용 위치
        {
            get
            {
                return transform; // 현재 오브젝트 위치 반환
            }
        }

        public abstract bool CanInteract(GameObject interactor); // 상호작용 가능 판정

        public abstract void Interact(GameObject interactor); // 상호작용 실행
    }
}
