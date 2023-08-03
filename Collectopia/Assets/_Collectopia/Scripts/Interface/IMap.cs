using System.Collections.Generic;
using UnityEngine;

public interface IMap: IObject
{
    MapRefsSO GetMapRefsSO();
    GameObject GetMapGO();
    void ResetStuffInMap();
    void RemoveItemFromMap(Vector3 itemPosition);
    List<MapRefsSO.ItemInfo> GetItemExistedInfo();
}
