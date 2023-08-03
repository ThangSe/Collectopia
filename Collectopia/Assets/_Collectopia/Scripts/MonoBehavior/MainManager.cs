using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    public event EventHandler<OnSpawnPopupEventArgs> OnSpawnPopup;
    public class OnSpawnPopupEventArgs: EventArgs
    {
        public List<NewPopup> newPopups;
        public int popupVisual;
        public Vector3 popupPosition;
        public int typePopup;
    }
    public event EventHandler OnStateChanged;
    public event EventHandler OnIncreaseScore;
    public event EventHandler OnHeartChanged;
    [SerializeField] private GameObject _wallGO, _mapGO, _itemGO, _obstacleGO, _poupGO;
    [SerializeField] private Transform _player, _world, _mainCamera, _cameraFollow;
    [SerializeField] private Button _tutorialButton, _playButton, _replayButton;
    [SerializeField] private MapRefsSO[] _mapRefsSO;
    [SerializeField] private GameSettingRefsSO _gameSettingsRefsSO;

    private float _moveX, _moveY;
    [SerializeField] private float _speed;
    [SerializeField] private float _speedMultipler;
    private Vector3 _mouseWorldPosition;
    private bool _canMove;
    private Vector3 _moveDir;
    private Vector3 _lastMoveDir;
    private CircleCollider2D _circleCollider2D;
    private bool _isFirstTime = true;
    private List<IWall> _wallList;
    private List<IItem> _itemList;
    private List<IMap> _mapList;
    private List<NewObstacle> _obstacleList;
    private List<NewPopup> _popupList;
    private GameObject _currentPlaceGO;
    private float _threshold = 0.05f;
    private float _thresholdSquared;
    [SerializeField] SpriteRenderer _playerVisual;

    [SerializeField] private Image[] _wallImgs;
    [SerializeField] private Image[] _itemImgs;
    [SerializeField] private Image[] _obstacleImgs;
    [SerializeField] private Image[] _playerImgs;
    [SerializeField] private Image[] _groundImgs;

    private enum State
    {
        Home,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }
    private State _state;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
        _wallList = new List<IWall>();
        _mapList = new List<IMap>();
        _itemList = new List<IItem>();
        _obstacleList = new List<NewObstacle>();
        _popupList = new List<NewPopup>();
        _circleCollider2D = _player.GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        _thresholdSquared = _threshold * _threshold;
        AddMap(Vector3.zero, _mapRefsSO[0]);
        SetUpDefault();
        MapChanged();
        _isFirstTime = false;
        _tutorialButton.onClick.AddListener(() =>
        {
            if (_gameSettingsRefsSO.turnPlayLeft.value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _replayButton.onClick.AddListener(() =>
        {
            SetUpDefault();
            if (_gameSettingsRefsSO.turnPlayLeft.value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _state = State.Home;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _playButton.onClick.AddListener(() =>
        {
            _state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _currentPlaceGO = _mapList[0].GetMapGO();
        NewItem.OnItemEaten += NewItem_OnItemEaten;
        NewObstacle.OnHitPlayer += NewObstacle_OnHitPlayer;
    }

    private void NewObstacle_OnHitPlayer(object sender, EventArgs e)
    {
        StartCoroutine(CameraShake());
        _gameSettingsRefsSO.currentHeart.value--;
        OnHeartChanged?.Invoke(this, EventArgs.Empty);
        OnSpawnPopup?.Invoke(this, new OnSpawnPopupEventArgs
        {
            newPopups = _popupList,
            popupPosition = PlayerPositionToScreenPosition(),
            popupVisual = 0,
            typePopup = 0
        });
    }

    private void NewItem_OnItemEaten(object sender, NewItem.OnItemEatenEventArgs e)
    {
        for (int i = 0; i < _mapList.Count; i++)
        {
            if((_currentPlaceGO.transform.position - _mapList[i].GetWorldSpacePosition()).sqrMagnitude < _thresholdSquared)
            {
                _mapList[i].RemoveItemFromMap(e.localPos);
                break;
            }
        }
        if(e.itemType == (int)ItemRefsSO.ItemType.Speed)
        {
            StartCoroutine(GetSpeedEffect(e.timeEffect));
        }
        if(e.itemType == (int)ItemRefsSO.ItemType.Score)
        {
            IncScore(e.score);
            OnIncreaseScore?.Invoke(this, EventArgs.Empty);
            OnSpawnPopup?.Invoke(this, new OnSpawnPopupEventArgs
            {
                newPopups = _popupList,
                popupPosition = PlayerPositionToScreenPosition(),
                popupVisual = e.itemVisual,
                typePopup = 1
            });
        }
    }

    private void SetUpDefault()
    {
        _playerVisual.sprite = _playerImgs[0].sprite;
        _gameSettingsRefsSO.countdownToStartTime.value = _gameSettingsRefsSO.countdownToStartTimeMax.value;
        _currentPlaceGO = _mapList[0].GetMapGO();
        _mainCamera.transform.position = new Vector3(0, 0, -10);
        _player.transform.position = Vector3.zero;
        _gameSettingsRefsSO.playingTime.value = _gameSettingsRefsSO.playingTimeMax.value;
        _gameSettingsRefsSO.turnPlayLeft.value = _gameSettingsRefsSO.turnPlayMax.value;
        _gameSettingsRefsSO.currentHeart.value = _gameSettingsRefsSO.maxHeart.value;
        _gameSettingsRefsSO.currentScore.value = 0;
        if (!_isFirstTime)
        {
            StartCoroutine(DelayReset());
            for(int i = 0; i < _popupList.Count; i++)
            {
                _popupList[i].Deactivate();
            }
        }
    }

    IEnumerator DelayReset()
    {
        for (int i = 0; i < _mapList.Count ; i++)
        {
            _mapList[i].ResetStuffInMap();
            if (_mapList[i].GetActivateState()) _mapList[i].Deactivate();
        }
        yield return null;
        for (int i = 0; i < 9; i++)
        {
            _mapList[i].Activate();
            LoadStuffToMap(_mapList[i]);
        }
    }

    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _gameSettingsRefsSO.countdownToStartTime.value -= Time.deltaTime;
                if (_gameSettingsRefsSO.countdownToStartTime.value < 0f)
                {
                    _state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                Movement();
                LocateNewPlace();
                UpdateAction();
                ChangePlayerSprite();
                _gameSettingsRefsSO.playingTime.value -= Time.deltaTime;
                if (_gameSettingsRefsSO.playingTime.value < 0f)
                {
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                if (_gameSettingsRefsSO.currentHeart.value <= 0)
                {
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }

    }

    private Vector3 PlayerPositionToScreenPosition()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_player.transform.GetChild(1).transform.position);
        return screenPosition;
    }

    private void LocateNewPlace()
    {
        if(Vector3.Distance(_currentPlaceGO.transform.position, _player.transform.position) > 15f)
        {
            for(int i = 0; i < _mapList.Count; i++)
            {
                if (Vector3.Distance(_mapList[i].GetWorldSpacePosition(), _player.transform.position) <= 15f)
                {
                    if(_currentPlaceGO.GetInstanceID() != _mapList[i].GetMapGO().GetInstanceID())
                    {
                        _currentPlaceGO = _mapList[i].GetMapGO();
                        MapChanged();
                        break;
                    }
                }
            }
        }
    }

    private void Movement()
    { 
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            _mouseWorldPosition = GetMouseWorldPosition();
            if (Vector3.Distance(_mouseWorldPosition, _player.position) < .5f)
            {
                return;
            }
            _moveDir = (_mouseWorldPosition - _player.position).normalized;
        }
        else
        {
            _moveX = 0;
            _moveY = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _moveY = 1f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _moveY = -1f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _moveX = 1f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _moveX = -1f;
            }
            _moveDir = new Vector3(_moveX, _moveY).normalized;
        }
        _canMove = !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, _moveDir, 0.05f, _gameSettingsRefsSO.unwalkableLayerMask);
        if (!_canMove)
        {
            Vector3 moveDirX = new Vector3(_moveDir.x, 0f, 0f).normalized;
            _canMove = _moveDir.x != 0 && !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, moveDirX, 0.05f, _gameSettingsRefsSO.unwalkableLayerMask);
            if (_canMove)
            {
                _moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirY = new Vector3(0, _moveDir.y, 0f).normalized;
                _canMove = _moveDir.y != 0 && !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, moveDirY, 0.05f, _gameSettingsRefsSO.unwalkableLayerMask);
                if (_canMove)
                {
                    _moveDir = moveDirY;
                }
            }
        }
        if (_canMove)
        {
            _player.position += _moveDir * _speed * _speedMultipler * Time.deltaTime;
        }
        if (_moveDir.x != 0 && _moveDir.y != 0)
        {
            _lastMoveDir = _moveDir;
        }
        Vector3 velocity = (_cameraFollow.position - _mainCamera.transform.position) * 3;
        _mainCamera.transform.position = Vector3.SmoothDamp(_mainCamera.transform.position, _cameraFollow.position, ref velocity, 1f, Time.deltaTime);
    }

    private int _walkBackSprite = 3;
    private int _walkFontSprite = 5;
    private int _walkSideSprite = 7;
    private float _delayChangeMax = .1f;
    private float _delayChange;
    private void ChangePlayerSprite()
    {
        if ((_moveDir.x > .2f || _moveDir.x < -.2f) && (_moveDir.y < 0 || _moveDir.y == 0))
        {
            if(_moveDir.x > 0)
            {
                _player.transform.localScale = new Vector3(1, 1, 1);
            } else
            {
                _player.transform.localScale = new Vector3(-1, 1, 1);
            }
            _delayChange += Time.deltaTime;
            if(_delayChange > _delayChangeMax)
            {
                _delayChange -= _delayChangeMax;
                _playerVisual.sprite = _playerImgs[_walkSideSprite].sprite;
                _walkSideSprite++;
            }
            if (_walkSideSprite > 8) _walkSideSprite = 7;
        } else if (_moveDir.y < 0 && _moveDir.x > -.2f && _moveDir.x < .2f)
        {
            _delayChange += Time.deltaTime;
            if (_delayChange > _delayChangeMax)
            {
                _delayChange -= _delayChangeMax;
                _playerVisual.sprite = _playerImgs[_walkFontSprite].sprite;
                _walkFontSprite++;
            }
            if (_walkFontSprite > 6) _walkFontSprite = 5;
        } else if(_moveDir.y > 0)
        {
            _delayChange += Time.deltaTime;
            if (_delayChange > _delayChangeMax)
            {
                _delayChange -= _delayChangeMax;
                _playerVisual.sprite = _playerImgs[_walkBackSprite].sprite;
                _walkBackSprite++;
            }
            if (_walkBackSprite > 4) _walkBackSprite = 3;
        } else
        {
            if (_lastMoveDir.x != 0 && _lastMoveDir.y < .2f && _lastMoveDir.y > -.2f)
            {
                if (_lastMoveDir.x > 0) _player.transform.localScale = new Vector3(1, 1, 1);
                else _player.transform.localScale = new Vector3(-1, 1, 1);
                if(_playerVisual.sprite != _playerImgs[1].sprite) _playerVisual.sprite = _playerImgs[1].sprite;
            }
            else if(_lastMoveDir.y > 0)
            {
                if (_playerVisual.sprite != _playerImgs[2].sprite) _playerVisual.sprite = _playerImgs[2].sprite;
            }
            else if(_lastMoveDir.y < 0)
            {
                if (_playerVisual.sprite != _playerImgs[0].sprite) _playerVisual.sprite = _playerImgs[0].sprite;
            }
        }
    }

    private void IncScore(int scoreAmount)
    {
        _gameSettingsRefsSO.currentScore.value += scoreAmount;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    IEnumerator GetSpeedEffect(float time)
    {
        _speedMultipler = 1.5f;
        yield return new WaitForSeconds(time);
        _speedMultipler = 1f;
    }

    private IEnumerator CameraShake()
    {
        Vector3 originalPos = _mainCamera.transform.position;
        float duration = .15f;
        float elapsed = 0f;
        float shakeAmount = .1f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(originalPos.x - shakeAmount, originalPos.x + shakeAmount);
            float y = UnityEngine.Random.Range(originalPos.y - shakeAmount, originalPos.y + shakeAmount);
            _mainCamera.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
    public bool IsHomeState()
    {
        return _state == State.Home;
    }
    public bool IsWaitingToStartState()
    {
        return _state == State.WaitingToStart;
    }

    public bool IsCountdownToStartState()
    {
        return _state == State.CountdownToStart;
    }

    public bool IsGamePlayingState()
    {
        return _state == State.GamePlaying;
    }

    public bool IsGameOverState()
    {
        return _state == State.GameOver;
    }

    private void UpdateAction()
    {
        for(int i = 0; i < _wallList.Count; i++)
        {
            if (_wallList[i].GetActivateStateGlobal() && _wallList[i].GetActivateState() && Vector3.Distance(_player.transform.position, _wallList[i].GetWorldSpacePosition()) < 15f)
            {
                _wallList[i].Action();
            }
        }
        for(int i = 0; i <_itemList.Count; i++)
        {
            if (_itemList[i].GetActivateState() && _itemList[i].GetActivateState() && Vector3.Distance(_player.position, _itemList[i].GetWorldSpacePosition()) < 15f)
            {
                _itemList[i].Action();
            }
        }
        for(int i = 0; i < _obstacleList.Count; i++)
        {
            if (_obstacleList[i].GetActivateState() && _obstacleList[i].GetActivateState() && Vector3.Distance(_player.position, _obstacleList[i].GetWorldSpacePosition()) < 30f)
            {
                _obstacleList[i].Action();
            }
        }
        for(int i = 0; i < _popupList.Count; i++)
        {
            if (_popupList[i].GetActivateState())
            {
                _popupList[i].Action();
            }
        }
    }

    private void AddObstacle(Vector3 pos, Transform parent, int obstacleVisual, int obstacleType, int sortingOrder)
    {
        bool isAdded = false;
        for(int i = 0; i < _obstacleList.Count; i++)
        {
            if (_obstacleList[i].GetActivateStateGlobal() == false)
            {
                _obstacleList[i].Activate();
                _obstacleList[i].SetUp(pos, parent, obstacleType, sortingOrder);
                _obstacleList[i].SettingSprite(_obstacleImgs[obstacleVisual].sprite);
                isAdded = true;
                break;
            }
        }
        if(!isAdded)
        {
            _obstacleList.Add(new NewObstacle(Instantiate(_obstacleGO, parent), pos, obstacleType, sortingOrder));
            _obstacleList[_obstacleList.Count - 1].SettingSprite(_obstacleImgs[obstacleVisual].sprite);
        }
    }

    private void AddWall(Vector3 pos, WallRefsSO wallRefsSO, Transform parent, int wallVisual, bool isFlip, int sortingOrder)
    {
        bool isAdded = false;
        for(int i = 0; i < _wallList.Count; i++)
        {
            if (_wallList[i].GetActivateStateGlobal() == false)
            {
                _wallList[i].Activate();
                _wallList[i].SetUp(pos, wallRefsSO, parent, sortingOrder, isFlip);
                _wallList[i].SettingSprite(_wallImgs[wallVisual].sprite);
                isAdded = true;
                break;
            }
        }
        if(!isAdded) {
            _wallList.Add(Factory.CreateWall(GameObject.Instantiate(_wallGO, parent), pos, wallRefsSO, sortingOrder, isFlip));
            _wallList[_wallList.Count - 1].SettingSprite(_wallImgs[wallVisual].sprite);
        }
    }

    private void AddItem(Vector3 pos, ItemRefsSO itemRefsSO, Transform parent, int itemVisual, int sortingOrder)
    {
        bool isAdded = false;
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (_itemList[i].GetActivateStateGlobal() == false)
            {
                _itemList[i].Activate();
                _itemList[i].SetUp(pos, parent, itemRefsSO, itemVisual, sortingOrder);
                _itemList[i].SettingSprite(_itemImgs[itemVisual].sprite);
                isAdded = true;
                break;
            }
        }
        if (!isAdded)
        {
            _itemList.Add(Factory.CreateItem(GameObject.Instantiate(_itemGO, parent), pos, itemRefsSO, itemVisual, sortingOrder));
            _itemList[_itemList.Count - 1].SettingSprite(_itemImgs[itemVisual].sprite);
        }
    }

    private void AddMap(Vector3 pos, MapRefsSO mapRefsSO)
    {
        GameObject newMap = Instantiate(_mapGO, _world);
        newMap.GetComponentInChildren<SpriteRenderer>().sprite = _groundImgs[UnityEngine.Random.Range(0, _groundImgs.Length)].sprite;
        _mapList.Add(Factory.CreateMap(newMap, pos, mapRefsSO));
        for (int i = 0; i < mapRefsSO.wallsInfo.Length; i++)
        {
            if (mapRefsSO.wallsInfo[i].sortingOrder == 0)
            {
                int sortingOrder = 5000;
                for (int j = 0; j < mapRefsSO.wallsInfo.Length; j++)
                {
                    if (mapRefsSO.wallsInfo[i].posY - (mapRefsSO.wallsInfo[i].wallRefsSO.multiplerY) / 2 > mapRefsSO.wallsInfo[j].posY - (mapRefsSO.wallsInfo[j].wallRefsSO.multiplerY / 2))
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.itemInfo.Length; j++)
                {
                    if (mapRefsSO.wallsInfo[i].posY - (mapRefsSO.wallsInfo[i].wallRefsSO.multiplerY) / 2 > mapRefsSO.itemInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.obstacleInfo.Length; j++)
                {
                    if (mapRefsSO.wallsInfo[i].posY - (mapRefsSO.wallsInfo[i].wallRefsSO.multiplerY) / 2 > mapRefsSO.obstacleInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                mapRefsSO.wallsInfo[i].sortingOrder = sortingOrder;
            }
            AddWall(new Vector3(mapRefsSO.wallsInfo[i].posX, mapRefsSO.wallsInfo[i].posY), mapRefsSO.wallsInfo[i].wallRefsSO, newMap.transform, (int)mapRefsSO.wallsInfo[i].wallVisual, mapRefsSO.wallsInfo[i].isFlip, mapRefsSO.wallsInfo[i].sortingOrder);
        }
        
        for (int i = 0; i < mapRefsSO.itemInfo.Length; i++)
        {
            if (mapRefsSO.itemInfo[i].sortingOrder == 0)
            {
                int sortingOrder = 5000;
                for (int j = 0; j < mapRefsSO.wallsInfo.Length; j++)
                {
                    if (mapRefsSO.itemInfo[i].posY > mapRefsSO.wallsInfo[j].posY - (mapRefsSO.wallsInfo[j].wallRefsSO.multiplerY / 2))
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.itemInfo.Length; j++)
                {
                    if (mapRefsSO.itemInfo[i].posY > mapRefsSO.itemInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.obstacleInfo.Length; j++)
                {
                    if (mapRefsSO.itemInfo[i].posY > mapRefsSO.obstacleInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                mapRefsSO.itemInfo[i].sortingOrder = sortingOrder;
            }  
            AddItem(new Vector3(mapRefsSO.itemInfo[i].posX, mapRefsSO.itemInfo[i].posY), mapRefsSO.itemInfo[i].itemRefsSO, newMap.transform, (int)mapRefsSO.itemInfo[i].itemVisual, mapRefsSO.itemInfo[i].sortingOrder);
        }

        for (int i = 0; i < mapRefsSO.obstacleInfo.Length; i++)
        {
            if (mapRefsSO.obstacleInfo[i].sortingOrder == 0)
            {
                int sortingOrder = 5000;
                for (int j = 0; j < mapRefsSO.wallsInfo.Length; j++)
                {
                    if (mapRefsSO.obstacleInfo[i].posY > mapRefsSO.wallsInfo[j].posY - (mapRefsSO.wallsInfo[j].wallRefsSO.multiplerY / 2))
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.itemInfo.Length; j++)
                {
                    if (mapRefsSO.obstacleInfo[i].posY > mapRefsSO.itemInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                for (int j = 0; j < mapRefsSO.obstacleInfo.Length; j++)
                {
                    if (mapRefsSO.obstacleInfo[i].posY > mapRefsSO.obstacleInfo[j].posY)
                    {
                        sortingOrder--;
                    }
                }
                mapRefsSO.obstacleInfo[i].sortingOrder = sortingOrder;
            }
            AddObstacle(new Vector3(mapRefsSO.obstacleInfo[i].posX, mapRefsSO.obstacleInfo[i].posY), newMap.transform, (int)mapRefsSO.obstacleInfo[i].obstacleVisual, (int)mapRefsSO.obstacleInfo[i].obstacleType, mapRefsSO.obstacleInfo[i].sortingOrder);
        }
    }

    private void MapChanged()
    {
        StartCoroutine(DelayMapChanged());
    }

    IEnumerator DelayMapChanged()
    {
        for (int i = 0; i < _mapList.Count; i++)
        {
            if (Vector3.Distance(_mapList[i].GetWorldSpacePosition(), _player.position) > 50f)
            {
                _mapList[i].Deactivate();
            }
        }
        yield return null;
        for (int i = 0; i < 8; i++)
        {
            Transform neighbourMap = _currentPlaceGO.transform.GetChild(i);
            bool detectPlace = false;
            foreach (var map in _mapList)
            {
                if (map.GetWorldSpacePosition() == neighbourMap.position)
                {
                    detectPlace = true;
                    break;
                }
            }
            if (detectPlace)
            {
                for(int j = 0; j < _mapList.Count; j++)
                {
                    if (_mapList[j].GetWorldSpacePosition() == neighbourMap.position && !_mapList[j].GetActivateState())
                    {
                        _mapList[j].Activate();
                        LoadStuffToMap(_mapList[j]);
                        break;
                    }
                }       
            }
            if (!detectPlace)
            {
                AddMap(neighbourMap.position, _mapRefsSO[UnityEngine.Random.Range(1, _mapRefsSO.Length)]);
            }
        }
    }

    private void LoadStuffToMap(IMap map)
    {
        GameObject mapGO = map.GetMapGO();
        foreach(var wall in map.GetMapRefsSO().wallsInfo) 
        {
            bool hasWall = false;
            foreach(Transform child in mapGO.transform)
            {
                if(child.CompareTag("Wall"))
                {
                    if(child.position.x == wall.posX + map.GetWorldSpacePosition().x && child.position.y == wall.posY + map.GetWorldSpacePosition().y && child.gameObject.activeSelf)
                    {
                        hasWall = true;
                        break;
                    }
                }
            }
        if(!hasWall)
            {
                AddWall(new Vector3(wall.posX, wall.posY), wall.wallRefsSO, mapGO.transform, (int)wall.wallVisual, wall.isFlip, wall.sortingOrder);
            }
        }
        foreach(var item in map.GetItemExistedInfo())
        {
            bool hasItem = false;
            foreach (Transform child in mapGO.transform)
            {
                if (child.CompareTag("Item"))
                {
                    if (child.position.x == item.posX + map.GetWorldSpacePosition().x && child.position.y == item.posY + map.GetWorldSpacePosition().y && child.gameObject.activeSelf)
                    {
                        hasItem = true;
                        break;
                    }
                }
            }
            if(!hasItem)
            {
                AddItem(new Vector3(item.posX, item.posY), item.itemRefsSO, mapGO.transform, (int)item.itemVisual, item.sortingOrder);
            }
        }
        foreach(var obstacle in map.GetMapRefsSO().obstacleInfo)
        {
            bool hasObstacle = false;
            foreach (Transform child in mapGO.transform)
            {
                if (child.CompareTag("Obstacle"))
                {
                    if (child.position.x == obstacle.posX + map.GetWorldSpacePosition().x && child.position.y == obstacle.posY + map.GetWorldSpacePosition().y && child.gameObject.activeSelf)
                    {
                        hasObstacle = true;
                        break;
                    }
                }
            }
            if(!hasObstacle)
            {
                AddObstacle(new Vector3(obstacle.posX, obstacle.posY), mapGO.transform, (int)obstacle.obstacleVisual, (int)obstacle.obstacleType, obstacle.sortingOrder);
            }
        }
    }
}
