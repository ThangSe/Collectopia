using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings SO", menuName = "ScriptableObjects/GameSettingRefsSO")]

public class GameSettingRefsSO : ScriptableObject
{
    public LayerMask unwalkableLayerMask;
    public LayerMask newPlaceLayerMask;
    public FloatVariableSO
        currentScore,
        turnPlayLeft, turnPlayMax,
        currentHeart, maxHeart,
        countdownToStartTime, countdownToStartTimeMax,
        playingTime, playingTimeMax;
}
