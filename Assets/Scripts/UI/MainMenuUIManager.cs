using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] GameObject _mainmenuPanel;
    [SerializeField] GameObject _EnemySelectPanel;
    [SerializeField] GameObject _MainGamePanel;

    [Header("Button")]
    [SerializeField] Button _gameStartButton;
    

    void Start()
    {
        _gameStartButton.onClick.AddListener(onStartButtonPressed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onStartButtonPressed()
    {
        _mainmenuPanel.SetActive(false);
        _EnemySelectPanel.SetActive(true);
    }
}
