using UnityEngine;

public interface IItem: IObject, IChild, IAction
{
    void SetUp(Vector3 pos, Transform parent, ItemRefsSO itemRefsSO, int itemVisual, int sortingOrder);
}
