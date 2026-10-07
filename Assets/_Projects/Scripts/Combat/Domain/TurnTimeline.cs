using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedition33.Combat
{
    public class TurnTimeline
    {
        private class TimelineEntry
        {
            public CombatActorStats Actor { get; }
            public float CurrentActionGauge { get; set; }

            public TimelineEntry(CombatActorStats actor)
            {
                Actor = actor;
                CurrentActionGauge = 0f;
            }
        }

        private readonly List<TimelineEntry> _entries = new();
        private const float ActionThreshold = 100f;

        public event Action OnTimelineUpdated;

        public void RegisterActor(CombatActorStats actor)
        {
            if (_entries.Exists(e => e.Actor == actor))
                return;

            _entries.Add(new TimelineEntry(actor));
            OnTimelineUpdated?.Invoke();
        }

        public void UnregisterActor(CombatActorStats actor)
        {
            _entries.RemoveAll(e => e.Actor == actor);
            OnTimelineUpdated?.Invoke();
        }

        public CombatActorStats GetNextTurnActor()
        {
            RemoveDefeatedActors();

            if (_entries.Count == 0)
                return null;

            // Advance gauges until at least one actor reaches ActionThreshold
            while (true)
            {
                TimelineEntry readyEntry = null;
                float highestGauge = -1f;

                foreach (TimelineEntry entry in _entries)
                {
                    if (entry.CurrentActionGauge >= ActionThreshold && entry.CurrentActionGauge > highestGauge)
                    {
                        highestGauge = entry.CurrentActionGauge;
                        readyEntry = entry;
                    }
                }

                if (readyEntry != null)
                {
                    // Reset gauge upon taking turn
                    readyEntry.CurrentActionGauge -= ActionThreshold;
                    OnTimelineUpdated?.Invoke();
                    return readyEntry.Actor;
                }

                // Advance all active actors by their Agility speed
                foreach (TimelineEntry entry in _entries)
                {
                    entry.CurrentActionGauge += Mathf.Max(1, entry.Actor.Agility);
                }
            }
        }

        public List<CombatActorStats> PreviewUpcomingTurns(int count)
        {
            var previewList = new List<CombatActorStats>();
            if (_entries.Count == 0)
                return previewList;

            // Clone gauge states for simulation
            var simulatedGauges = new List<(CombatActorStats actor, float gauge)>();
            foreach (TimelineEntry entry in _entries)
            {
                if (!entry.Actor.IsDefeated)
                {
                    simulatedGauges.Add((entry.Actor, entry.CurrentActionGauge));
                }
            }

            if (simulatedGauges.Count == 0)
                return previewList;

            while (previewList.Count < count)
            {
                int readyIndex = -1;
                float highest = -1f;

                for (int i = 0; i < simulatedGauges.Count; i++)
                {
                    if (simulatedGauges[i].gauge >= ActionThreshold && simulatedGauges[i].gauge > highest)
                    {
                        highest = simulatedGauges[i].gauge;
                        readyIndex = i;
                    }
                }

                if (readyIndex != -1)
                {
                    var ready = simulatedGauges[readyIndex];
                    previewList.Add(ready.actor);
                    simulatedGauges[readyIndex] = (ready.actor, ready.gauge - ActionThreshold);
                }
                else
                {
                    for (int i = 0; i < simulatedGauges.Count; i++)
                    {
                        var entry = simulatedGauges[i];
                        simulatedGauges[i] = (entry.actor, entry.gauge + Mathf.Max(1, entry.actor.Agility));
                    }
                }
            }

            return previewList;
        }

        private void RemoveDefeatedActors()
        {
            _entries.RemoveAll(e => e.Actor.IsDefeated);
        }
    }
}
