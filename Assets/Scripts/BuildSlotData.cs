using UnityEngine;

public enum UpgradeEffect
{
    None,
    MageTower
}

[CreateAssetMenu(fileName = "NewSlot", menuName = "From Zero to Hero/Build Slot")]
public class BuildSlotData : ScriptableObject
{
    public string slotName = "Slot";
    public int cost = 50;
    public GameObject buildingPrefab;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public bool unlocksNext = true;
    public UpgradeEffect upgradeEffect = UpgradeEffect.None;
    [TextArea]
    public string completedMessage = "Building complete!";
}
