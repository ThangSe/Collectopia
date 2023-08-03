using UnityEngine;

[CreateAssetMenu(fileName = "Item Settings SO", menuName = "ScriptableObjects/ItemRefsSO")]
public class ItemRefsSO : ScriptableObject
{
    public enum ItemType
    {
        None = 0,
        Score = 1,
        Speed = 2
    }
    public ItemType itemType;
    public float timeEffect;
    public int score;
}
