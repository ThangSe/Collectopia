using UnityEngine;
using UnityEngine.UI;

public class NewPopup : IObject, IAction
{
    GameObject _object;
    private float _speed = 300f;
    private float _disappearTimer;
    private float _disappeartTimerMax = 1f;

    public NewPopup(GameObject newObject, Vector3 position) {
        _object = newObject;
        _object.transform.position = position;
        _disappearTimer = _disappeartTimerMax;
    }

    public void Action()
    {
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0)
        {
            Deactivate();
        }
        else
        {
            _object.transform.position += Vector3.up * _speed * Time.deltaTime;
        }
    }

    public void Activate()
    {
        _object.SetActive(true);
    }

    public void SetUp(Vector3 position)
    {
        _disappearTimer = _disappeartTimerMax;
        _object.transform.position = position;
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

    public void SettingSprite(Sprite sprite)
    {
        _object.GetComponent<Image>().sprite = sprite;
    }

    public Vector3 GetWorldSpacePosition()
    {
        throw new System.NotImplementedException();
    }
}
