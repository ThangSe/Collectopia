using System;
using UnityEngine;

public class NewObstacle: IObject, IChild, IAction
{
    public static event EventHandler OnHitPlayer;
    private GameObject _object;
    private GameObject _player;
    private LayerMask _playerLayerMask = 256;
    private Color _itemColor = Color.red;
    Vector3 _startPosition;
    private float _speed = 3f;
    private float _moveSpeed, _moveWidth, _moveHeight, _randomDir;
    private enum ObstacleType
    {
        Static = 0,
        Dynamic = 1
    }
    private ObstacleType _obstacleType;

    public NewObstacle(GameObject newObject, Vector3 pos, int obstacleType, int sortingOrder)
    {
        _object = newObject;
        _player = GameObject.Find("Player");
        _object.transform.localPosition = pos;
        _startPosition = _object.transform.position;
        _object.transform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        if ((int)ObstacleType.Dynamic == obstacleType)
        {
            _obstacleType = ObstacleType.Dynamic;
            _moveSpeed = UnityEngine.Random.Range(3f, 7f);
            _moveWidth = UnityEngine.Random.Range(2f, 5f);
            _moveHeight = UnityEngine.Random.Range(2f, 5f);
            _randomDir = UnityEngine.Random.Range(0f, 1f);
        }
        if((int)ObstacleType.Static == obstacleType)
        {
            _obstacleType = ObstacleType.Static;
        }
    }

    public void SetUp(Vector3 pos, Transform parent, int obstacleType, int sortingOrder)
    {
        _object.transform.parent = parent;
        _object.transform.localPosition = pos;
        _startPosition = _object.transform.position;
        _object.transform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        if ((int)ObstacleType.Dynamic == obstacleType)
        {
            _obstacleType = ObstacleType.Dynamic;
            _moveSpeed = UnityEngine.Random.Range(3f, 7f);
            _moveWidth = UnityEngine.Random.Range(2f, 5f);
            _moveHeight = UnityEngine.Random.Range(2f, 5f);
            _randomDir = UnityEngine.Random.Range(0f, 1f);
        }
        if ((int)ObstacleType.Static == obstacleType)
        {
            _obstacleType = ObstacleType.Static;
        }
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

    public void Action()
    {
        if (_player.transform.position.y > _object.transform.position.y)
        {
            if (_object.GetComponent<SpriteRenderer>().sortingLayerName != "AbovePlayer") _object.GetComponent<SpriteRenderer>().sortingLayerName = "AbovePlayer";
        }
        if (_player.transform.position.y < _object.transform.position.y)
        {
            if (_object.GetComponent<SpriteRenderer>().sortingLayerName != "BehindPlayer") _object.GetComponent<SpriteRenderer>().sortingLayerName = "BehindPlayer";
        }
        _itemColor.a = Mathf.Abs(Mathf.Sin(Time.time) * _speed);
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().material.color = _itemColor;
        switch (_obstacleType)
        {
            case ObstacleType.Static:
                if(Vector3.Distance(_object.transform.position, _player.transform.position) < 5f)
                {
                    DetectPlayer();
                }
                break;
            case ObstacleType.Dynamic:
                if (_randomDir > .5f)
                {
                    _object.transform.position = _startPosition + new Vector3(Mathf.Sin(Time.time * _moveSpeed) * _moveWidth, Mathf.Cos(Time.time * _moveSpeed) * _moveHeight);
                }
                else
                {
                    _object.transform.position = _startPosition + new Vector3(Mathf.Cos(Time.time * _moveSpeed) * _moveWidth, Mathf.Sin(Time.time * _moveSpeed) * _moveHeight);
                }
                DetectPlayer();
                break;
        }
    }

    private void DetectPlayer()
    {
        if (Physics2D.CircleCast(_object.transform.position, .8f, Vector2.zero, .05f, _playerLayerMask))
        {
            OnHitPlayer?.Invoke(this, EventArgs.Empty);
            Deactivate();
        }
    }

    public bool GetActivateStateGlobal()
    {
        return _object.transform.parent.gameObject.activeSelf;
    }
}
