using UnityEngine;

public class NewWall : IWall
{
    private GameObject _object;
    private GameObject _player;

    public NewWall(GameObject newObject, Vector3 pos, WallRefsSO wallRefsSO, int sortingOrder, bool isFlip)
    {
        _object = newObject;
        _object.transform.localPosition = pos;
        _player = GameObject.Find("Player");
        _object.transform.GetChild(1).transform.localScale = new Vector3(wallRefsSO.defaultSize * wallRefsSO.multiplerX, wallRefsSO.defaultSize * wallRefsSO.multiplerY, 1);
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        if (isFlip)
        {
            _object.transform.GetChild(0).transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _object.transform.GetChild(0).transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SetUp(Vector3 position, WallRefsSO wallRefsSO, Transform parent, int sortingOrder, bool isFlip)
    {
        _object.transform.parent = parent;
        _object.transform.localPosition = position;
        _object.transform.GetChild(1).transform.localScale = new Vector3(wallRefsSO.defaultSize * wallRefsSO.multiplerX, wallRefsSO.defaultSize * wallRefsSO.multiplerY, 1);
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        if (isFlip)
        {
            _object.transform.GetChild(0).transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _object.transform.GetChild(0).transform.localScale = new Vector3(1, 1, 1);
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

    public bool GetActivateStateGlobal()
    {
        return _object.transform.parent.gameObject.activeSelf;
    }

    public void SettingSprite(Sprite newSprite)
    {
        _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newSprite;
        _object.transform.GetChild(0).transform.localPosition = new Vector3(0, _object.transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.y / 2 - _object.GetComponentInChildren<BoxCollider2D>().bounds.size.y * _object.transform.GetChild(1).transform.localScale.y / 2, 0);
    }

    public Vector3 GetWorldSpacePosition()
    {
        return _object.transform.position;
    }

    public void Action()
    {
        if(_player.transform.position.y > _object.transform.position.y)
        {
            if (_object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName != "AbovePlayer") _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "AbovePlayer";
        }
        if (_player.transform.position.y < _object.transform.position.y)
        {
            if (_object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName != "BehindPlayer") _object.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "BehindPlayer";
        }
    }

    public bool GetActivateState()
    {
        return _object.activeSelf;
    }
}
