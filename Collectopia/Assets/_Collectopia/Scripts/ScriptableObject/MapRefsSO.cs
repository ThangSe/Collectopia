using UnityEngine;

[CreateAssetMenu(fileName = "Map Settings SO", menuName = "ScriptableObjects/MapRefsSO")]
public class MapRefsSO : ScriptableObject
{
    [System.Serializable]
    public struct WallInfo
    {
        public enum SpecialWall
        {
            Normal,
            Fake,
            Finish
        }
        public enum WallVisual
        {
            BeeHouse = 0,
            DogHouse = 1,
            Doll = 2,
            Farm = 3,
            House = 4,
            Storage = 5,
            Store = 6,
            StrawPile = 7,
            WindMill = 8,
            WoodPanel = 9,
            TreeA = 10,
            TreeB = 11,
            TreeC = 12,
            TreeD = 13,
            TreeE = 14,
            TreeF = 15,
            Cow = 16,
            Dog = 17,
            Duck = 18,
            Horse = 19,
            Pig = 20,
            Rabit = 21,
            Sheep = 22,
            SheepA = 23,
            SheepB = 24,
        }
        public WallVisual wallVisual;
        public SpecialWall specialWall;
        public WallRefsSO wallRefsSO;
        public int sortingOrder;
        public bool isFlip;
        public float posX, posY;
    }

    [System.Serializable]
    public struct ItemInfo
    {
        public enum ItemVisual
        {
            Coin = 0,
            Diamond = 1,
            Gift = 2,
            SpeedBoost = 3
        }
        public ItemVisual itemVisual;
        public ItemRefsSO itemRefsSO;
        public int sortingOrder;
        public float posX, posY;
    }

    [System.Serializable]
    public struct ObstacleInfo
    {
        public enum ObstacleVisual
        {
            BeeHouse = 0,
            Dirt = 1,
            Bee = 2
        }
        public enum ObstacleType
        {
            Static = 0,
            Dynamic = 1
        }
        public ObstacleVisual obstacleVisual;
        public ObstacleType obstacleType;
        public int sortingOrder;
        public float posX, posY;
    }
    public WallInfo[] wallsInfo;
    public ItemInfo[] itemInfo;
    public ObstacleInfo[] obstacleInfo;
}