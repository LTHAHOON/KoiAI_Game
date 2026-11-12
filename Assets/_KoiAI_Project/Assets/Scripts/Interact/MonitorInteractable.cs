using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.Player;

    public struct MonitorInteractContext
    {
        public int Password {get; set;}
    }

    public class MonitorInteractable : BaseInteractable<MonitorInteractContext>, IPlayerInteractable
    {
        public override void Interact(MonitorInteractContext dataEvent)
        {
            base.Interact(dataEvent);
        }

        public void Interact(PlayerInteractor playerInteractor)
        {
        }
    }
}
