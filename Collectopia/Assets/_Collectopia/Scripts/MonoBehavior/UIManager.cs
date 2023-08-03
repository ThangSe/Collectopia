using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _fullHeartImgRef, _emptyHeartImgRef, _loseHeartImgRef;
    [SerializeField] private Image[] _scoreImgsRef;
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Text _scoreText, _textTimer, _gameEndText, _gameEndScoreText;
    [SerializeField] private GameObject _backgroundPanel, _homePage, _tutorialPage, _basePage, _gamePage, _gameEndPage;

    [SerializeField] private GameObject _popupGO;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioGamePlay, _audioPoint, _audioLoseHeart, _audioGameOver;

    [SerializeField] private FloatReference _currentHeart, _currentScore, _currentPlayingTime;


    private void Start()
    {
        _scoreText.text = _currentScore.Value.ToString();
        MainManager.Instance.OnStateChanged += MainManager_OnStateChanged;
        MainManager.Instance.OnHeartChanged += Instance_OnHeartChanged;
        MainManager.Instance.OnSpawnPopup += MainManager_OnSpawnPopup;
        MainManager.Instance.OnIncreaseScore += MainManager_OnIncreaseScore;
        for (int i = 0; i < _hearts.Length; i++)
        {
            _hearts[i].sprite = _fullHeartImgRef.sprite;
        }
    }

    private void MainManager_OnSpawnPopup(object sender, MainManager.OnSpawnPopupEventArgs e)
    {
        AddPopup(e.newPopups, e.popupPosition, e.popupVisual, e.typePopup);
    }

    private void Update()
    {
        if (Mathf.RoundToInt(_currentPlayingTime.Value) < Mathf.CeilToInt(_currentPlayingTime.Value))
        {
            SetTimerText(_currentPlayingTime.Value);
        }
    }

    private void MainManager_OnIncreaseScore(object sender, System.EventArgs e)
    {
        _audioPoint.Play();
        _scoreText.text = _currentScore.Value.ToString();
    }
    private void Instance_OnHeartChanged(object sender, System.EventArgs e)
    {
        _audioLoseHeart.Play();
        SetUIHeart((int)_currentHeart.Value);
    }
    private void SetUIHeart(int currentHeart)
    {
        if (currentHeart >= 0)
        {
            _hearts[currentHeart].sprite = _emptyHeartImgRef.sprite;
        }
    }

    private void SetTimerText(float timer)
    {
        if (timer >= 60f)
        {
            int minute = Mathf.RoundToInt(timer / 60);
            int second = Mathf.RoundToInt(timer - 60 * minute);
            if (second < 0)
            {
                minute--;
                second += 60;
            }
            if (second < 10)
            {
                _textTimer.text = minute + ":0" + second;
            }
            else
            {
                _textTimer.text = minute + ":" + second;
            }
        }
        if (timer < 60f && timer > 0f)
        {
            if (timer < 10)
            {
                _textTimer.text = "0:0" + Mathf.RoundToInt(timer);
            }
            else
            {
                _textTimer.text = "0:" + Mathf.RoundToInt(timer);
            }

        }
        if (timer <= 0f)
        {
            _textTimer.text = "0:00";
        }
    }

    private void MainManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (MainManager.Instance.IsHomeState())
        {
            _homePage.SetActive(true);
            _gameEndPage.SetActive(false);
            _audioGameOver.volume = 0f;
        }
        if (MainManager.Instance.IsWaitingToStartState())
        {
            SetTimerText(_currentPlayingTime.Value);
            _audioGameOver.volume = 0f;
            _tutorialPage.SetActive(true);
            _homePage.SetActive(false);
            _gameEndPage.SetActive(false);
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].sprite = _fullHeartImgRef.sprite;
            }
            _scoreText.text = _currentScore.Value.ToString();
        }
        if (MainManager.Instance.IsCountdownToStartState())
        {
            _audioGamePlay.time = 0f;
            if (!_audioGamePlay.isPlaying) _audioGamePlay.Play();
            _audioGamePlay.volume = 1f;
            _backgroundPanel.SetActive(false);
            _tutorialPage.SetActive(false);
            _gamePage.SetActive(true);
        }
        if (MainManager.Instance.IsGamePlayingState())
        {

        }
        if (MainManager.Instance.IsGameOverState())
        {
            _audioGamePlay.volume = 0f;
            _audioGameOver.time = 0f;
            if (!_audioGameOver.isPlaying) _audioGameOver.Play();
            _audioGameOver.volume = 1f;
            _gamePage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gameEndPage.SetActive(true);
            _gameEndScoreText.text = _currentScore.Value.ToString();
        }
    }

    private void AddPopup(List<NewPopup> newPopups, Vector3 pos, int popupVisual, int typePopup)
    {
        bool isAdded = false;
        for (int i = 0; i < newPopups.Count; i++)
        {
            if (!newPopups[i].GetActivateState())
            {
                newPopups[i].Activate();
                newPopups[i].SetUp(pos);
                if (typePopup == 0) newPopups[i].SettingSprite(_loseHeartImgRef.sprite);
                if (typePopup == 1) newPopups[i].SettingSprite(_scoreImgsRef[popupVisual].sprite);
                isAdded = true;
                break;
            }
        }
        if (!isAdded)
        {
            newPopups.Add(new NewPopup(Instantiate(_popupGO, _gamePage.transform), pos));
            if (typePopup == 0) newPopups[newPopups.Count - 1].SettingSprite(_loseHeartImgRef.sprite);
            if (typePopup == 1) newPopups[newPopups.Count - 1].SettingSprite(_scoreImgsRef[popupVisual].sprite);
        }
    }
}
