using System.Collections.Generic;
using UnityEngine;

public class NewMap : IMap
{
    private GameObject _object;
    MapRefsSO _mapRefsSO;
    public Vector3 _position;
    private List<MapRefsSO.ItemInfo> _itemExistedInfos;
    public NewMap(GameObject newObject, Vector3 pos, MapRefsSO mapRefsSO)
    {
        _itemExistedInfos = new List<MapRefsSO.ItemInfo>();
        _object = newObject;
        _object.transform.position = pos;
        _object.SetActive(true);
        _mapRefsSO = mapRefsSO;
        _position = pos;
        ResetStuffInMap();
    }
    public void Activate()
    {
        _object.SetActive(true);
    }

    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetActivateState()
    {
        return _object.activeSelf;
    }

    public MapRefsSO GetMapRefsSO()
    {
        return _mapRefsSO;
    }

    public Vector3 GetWorldSpacePosition()
    {
        return _position;
    }

    public GameObject GetMapGO()
    {
        return _object;
    }

    public void SettingSprite(Sprite newSprite)
    {
        _object.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
    public List<MapRefsSO.ItemInfo> GetItemExistedInfo()
    {
        return _itemExistedInfos;
    }

    public void ResetStuffInMap()
    {
        _itemExistedInfos.Clear();
        _itemExistedInfos.AddRange(_mapRefsSO.itemInfo);
    }
    public void RemoveItemFromMap(Vector3 itemPosition)
    {
        for (int i = 0; i < _itemExistedInfos.Count; i++)
        {
            if (_itemExistedInfos[i].posX == itemPosition.x && _itemExistedInfos[i].posY == itemPosition.y)
            {
                _itemExistedInfos.RemoveAt(i);
                break;
            }
        }
    }
}
