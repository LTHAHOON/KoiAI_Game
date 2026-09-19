using System;
using KoiAI.Item;
using KoiAI.Quest;
using UnityEngine;

namespace KoiAI.Core
{
    /// <summary>
    /// 게임플레이 이벤트(킬, 수집 등등)
    /// </summary>
    public static class GameplayEvents
    {
        public static Action<QuestKillData, int> OnKilled;
        public static Action OnAreaEntered;
        public static Action<ItemBase> OnCollected;
        public static Action OnInteracted;
    }
}
