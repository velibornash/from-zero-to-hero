using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance { get; private set; }
    public static event System.Action<int> SlotBuilt;

    List<BuildSlot> slots = new List<BuildSlot>();
    int unlockedIndex = 0;
    int towersBuilt = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Reset state in case this is a duplicate or stale instance
        slots.Clear();
        unlockedIndex = 0;
        Instance = this;

        slots = Object.FindObjectsByType<BuildSlot>(FindObjectsInactive.Include)
            .OrderBy(s => s.slotIndex)
            .ToList();

        Debug.Log($"SlotManager.Start: found {slots.Count} slots. Initial unlockedIndex = {unlockedIndex}");
        for (int i = 0; i < slots.Count; i++)
        {
            Debug.Log($"  Slot {i}: {(slots[i] != null ? slots[i].name : "null")} state={slots[i].state}");
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i <= unlockedIndex)
                slots[i].Unlock();
        }
    }

    public void OnSlotBuilt(int index)
    {
        // Notify subscribers FIRST (e.g. VillageWalls builds fences) regardless of unlock state
        SlotBuilt?.Invoke(index);

        // Track tower builds — unlock mage tiles only after ALL 4 are built (any order)
        if (index >= 2 && index <= 5)
        {
            towersBuilt++;
            Debug.Log($"SlotManager: Tower {index} built ({towersBuilt}/4 so far).");
            if (towersBuilt >= 4)
            {
                Debug.Log($"SlotManager: All 4 towers built — unlocking mage tiles.");
                int mageCount = Mathf.Min(4, slots.Count - 1 - unlockedIndex);
                for (int i = 0; i < mageCount; i++)
                {
                    unlockedIndex++;
                    slots[unlockedIndex].Unlock();
                    slots[unlockedIndex].PulseUnlock();
                }
                HUDController.PushEvent("All towers stand! Ancient runes glow beneath 4 new tiles — mages are ready to be summoned.");
            }
            return;
        }

        if (index != unlockedIndex) return;
        if (unlockedIndex >= slots.Count - 1) return;

        // After the flag (index 1) unlock all four tower corners at once.
        if (index == 1)
        {
            int towerCount = 4;
            for (int i = 0; i < towerCount && unlockedIndex < slots.Count - 1; i++)
            {
                unlockedIndex++;
                slots[unlockedIndex].Unlock();
                slots[unlockedIndex].PulseUnlock();
            }
            HUDController.PushEvent("All four foundation corners are open! Build towers to defend the valley.");
            return;
        }

        // Default: unlock next slot
        unlockedIndex++;
        slots[unlockedIndex].Unlock();
        slots[unlockedIndex].PulseUnlock();
        HUDController.PushEvent($"New foundation revealed: {slots[unlockedIndex].data.slotName}");
    }
}
