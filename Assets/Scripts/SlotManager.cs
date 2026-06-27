using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance { get; private set; }
    public static event System.Action<int> SlotBuilt;

    List<BuildSlot> slots = new List<BuildSlot>();
    int unlockedIndex = 0;

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

        // Hard guard: towers (2-5) NEVER unlock anything
        if (index >= 2 && index <= 5)
        {
            Debug.Log($"SlotManager: Tower {index} built. NOT unlocking next slot (wait for all 4 towers).");
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
            HUDController.PushEvent("All four tower corners are open!");
            return;
        }
        // After the 4th tower (index 5) is built, unlock all 4 mage tiles at once
        if (index == 5)
        {
            Debug.Log($"SlotManager: Tower 5 built, unlocking mage tiles. Current unlockedIndex={unlockedIndex}");
            int mageCount = Mathf.Min(4, slots.Count - 1 - unlockedIndex);
            Debug.Log($"SlotManager: Will unlock {mageCount} mage tiles");
            for (int i = 0; i < mageCount; i++)
            {
                unlockedIndex++;
                slots[unlockedIndex].Unlock();
                slots[unlockedIndex].PulseUnlock();
            }
            HUDController.PushEvent("Mage tiles unlocked! Upgrade your defenses with magic.");
            return;
        }

        // Default: unlock next slot
        unlockedIndex++;
        slots[unlockedIndex].Unlock();
        slots[unlockedIndex].PulseUnlock();
        HUDController.PushEvent($"{slots[unlockedIndex].data.slotName} slot unlocked!");
    }
}
