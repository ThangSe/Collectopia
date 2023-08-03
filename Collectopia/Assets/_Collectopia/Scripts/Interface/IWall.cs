using UnityEngine;

public interface IWall : IObject, IChild, IAction
{
    void SetUp(Vector3 position, WallRefsSO wallRefsSO, Transform parent, int sortingOrder, bool isFlip);
}
