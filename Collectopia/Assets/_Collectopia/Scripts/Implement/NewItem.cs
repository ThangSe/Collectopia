using System;
using UnityEngine;

public class NewItem : IItem
{
    public static event EventHandler<OnItemEatenEventArgs> OnItemEaten;

    public class OnItemEatenEventArgs : EventArgs
    {
        public int score;
        public int itemType;
        public int itemVisual;
        public float timeEffect;
        public Vector3 localPos;
    }
    private LayerMask _playerLayerMask = 256;
    private GameObject _object;
    private int _score;
    private GameObject _player;
    private Color _itemColor = Color.yellow;
    private float _speed = 3f;
    private int _itemType;
    private int _itemVisual;
    private float _timeEffect;
    public NewItem(GameObject newObject, Vector3 pos, ItemRefsSO itemRefsSO, int itemVisual, int sortingOrder)
    {
        _object = newObject;
        _player = GameObject.Find("Player");
        _object.transform.localPosition = pos;
        _score = itemRefsSO.score;
        _itemType = (int)itemRefsSO.itemType;
        _itemVisual = itemVisual;
        _timeEffect = itemRefsSO.timeEffect;
        _object.transform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
    }

    public void Action()
    {
        if (_player.transform.position.y > _object.transform.position.y)
        {
            if (_object.transform.GetComponent<SpriteRenderer>().sortingLayerName != "AbovePlayer") _object.transform.GetComponent<SpriteRenderer>().sortingLayerName = "AbovePlayer";
        }
        if (_player.transform.position.y < _object.transform.position.y)
        {
            if (_object.transform.GetComponent<SpriteRenderer>().sortingLayerName != "BehindPlayer") _object.transform.GetComponent<SpriteRenderer>().sortingLayerName = "BehindPlayer";
        }
        _itemColor.a = Mathf.Abs(Mathf.Sin(Time.time) * _speed);
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().material.color = _itemColor;
        if (Physics2D.CircleCast(_object.transform.position, .8f / 2, Vector2.zero, .05f, _playerLayerMask) && Vector3.Distance(_player.transform.position, _object.transform.position) < 3f)
        {
            OnItemEaten?.Invoke(this, new OnItemEatenEventArgs
            {
                score = _score,
                itemType = _itemType,
                itemVisual = _itemVisual,
                timeEffect = _timeEffect,
                localPos = _object.transform.localPosition,
            });
            Deactivate();
        }
    }

    public void Activate()
    {
        _object.SetActive(true);
    }

    public void SetUp(Vector3 pos, Transform parent, ItemRefsSO itemRefsSO, int itemVisual, int sortingOrder)
    {
        _object.transform.parent = parent;
        _object.transform.localPosition = pos;
        _itemVisual = itemVisual;
        _itemType = (int)itemRefsSO.itemType;
        _score = itemRefsSO.score;
        _timeEffect = itemRefsSO.timeEffect;
        _object.transform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
    }

    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetActivateState()
    {
        return _object.activeSelf;
    }

    public void SettingSprite(Sprite newSprite)
    {
        _object.GetComponent<SpriteRenderer>().sprite = newSprite;
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newSprite;
        _object.GetComponentInChildren<SpriteMask>().sprite = newSprite;
    }

    public Vector3 GetWorldSpacePosition()
    {
        return _object.transform.position;
    }

    public bool GetActivateStateGlobal()
    {
        return _object.transform.parent.gameObject.activeSelf;
    }
}
