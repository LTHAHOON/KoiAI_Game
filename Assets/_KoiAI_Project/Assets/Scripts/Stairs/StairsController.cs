using System;
using R3;
using UnityEngine;

namespace KoiAI.Interact
{
    using DG.Tweening;
    using KoiAI.Quest;

    public class StairsController : BaseInteractable<QuestData>
    {
        [SerializeField]
        private int _stageIndex = 0;
        [SerializeField]
        private float _openDuration = 5f;
        [SerializeField]
        private Ease _openEaseType = Ease.Linear;

        private IDisposable _disposableInteract;
        
        private void Awake()
        {
            QuestEvents.OnQuestAccpeted = SubscribeOpen;
        }

        public void SubscribeOpen(QuestData questData)
        {
            if(questData.QuestIndex != _stageIndex)
            {
                return;
            }
            _disposableInteract = OnInteract.Subscribe(questData =>
            {
                //열기
                Open();
                _disposableInteract.Dispose();
            });
            QuestEvents.OnQuestAccpeted -= SubscribeOpen;
            QuestEvents.OnQuestCleared = Interact;
        }
        
        public void Open()
        {
            transform.DOLocalRotate(Vector3.zero, _openDuration).SetEase(_openEaseType);
        }
    }
}
