using System;
using R3;
using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.Quest;

    public class StairsController : BaseInteractable<QuestData>
    {
        [SerializeField]
        private int _stageIndex = 0;

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
                _disposableInteract.Dispose();
            });
            QuestEvents.OnQuestCleared = Interact;
        }
    }
}
