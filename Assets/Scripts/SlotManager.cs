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
        slots = Object.FindObjectsByType<BuildSlot>(FindObjectsInactive.Include)
            .OrderBy(s => s.slotIndex)
            .ToList();

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
            int mageCount = Mathf.Min(4, slots.Count - 1 - unlockedIndex);
            for (int i = 0; i < mageCount; i++)
            {
                unlockedIndex++;
                slots[unlockedIndex].Unlock();
                slots[unlockedIndex].PulseUnlock();
            }
            HUDController.PushEvent("Mage tiles unlocked! Upgrade your defenses with magic.");
            return;
        }
        // Towers (2-5) do NOT auto-unlock the next slot
        if (index >= 2 && index <= 5) return;

        // Default: unlock next slot
        unlockedIndex++;
        slots[unlockedIndex].Unlock();
        slots[unlockedIndex].PulseUnlock();
        HUDController.PushEvent($"{slots[unlockedIndex].data.slotName} slot unlocked!");
    }
}
