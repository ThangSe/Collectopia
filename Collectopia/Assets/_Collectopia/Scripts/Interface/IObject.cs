using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObject
{
    void Activate();
    void Deactivate();
    bool GetActivateState();
    Vector3 GetWorldSpacePosition();
    void SettingSprite(Sprite newSprite);
}