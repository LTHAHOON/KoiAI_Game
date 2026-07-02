using R3;
using UnityEngine;

namespace KoiAI.Player
{
    public abstract class BaseInteractable<T> : MonoBehaviour
    {
        private readonly Subject<T> _interactSubject = new();
        public Observable<T> OnInteract => _interactSubject;

        public virtual void Interact(T dataEvent)
        {
            _interactSubject.OnNext(dataEvent);   
        }
    }
}
