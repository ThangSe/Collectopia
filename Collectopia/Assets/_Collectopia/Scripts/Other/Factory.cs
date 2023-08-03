using UnityEngine;

public static class Factory
{
    public static IWall CreateWall(GameObject gameObject, Vector3 pos, WallRefsSO wallRefsSO, int sortingOrder, bool isFlip)
    {
        return new NewWall(gameObject, pos, wallRefsSO, sortingOrder, isFlip);
    }

    public static IMap CreateMap(GameObject gameObject, Vector3 pos, MapRefsSO mapRefsSO)
    {
        return new NewMap(gameObject, pos, mapRefsSO);
    }
    public static IItem CreateItem(GameObject gameObject, Vector3 pos, ItemRefsSO itemRefsSO, int itemVisual, int sortingOrder)
    {
        return new NewItem(gameObject, pos, itemRefsSO, itemVisual, sortingOrder);
    }
}
