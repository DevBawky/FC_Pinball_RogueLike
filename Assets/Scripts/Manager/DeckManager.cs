using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("Deck Settings (전체 보유 덱)")]
    public List<BallData> currentDeck = new List<BallData>(); // 플레이어가 게임 중 모은 전체 공
    
    // 전투 중 내부적으로 순환하는 리스트
    private List<BallData> drawPile = new List<BallData>();      // 뽑기 더미
    private List<BallData> discardPile = new List<BallData>();   // 버린 더미

    [Header("Round Settings (이번 라운드 탄창)")]
    public int ballsPerRound = 5; // 한 라운드에 주어지는 공의 개수
    public List<BallData> roundMagazine = new List<BallData>(); // 현재 장전된 5개의 공

    [Header("Events (UI 연동용)")]
    public UnityEvent<List<BallData>> onMagazineLoaded; // 라운드 시작 시 5개 공이 장전되었을 때 (UI 리스트 갱신)
    public UnityEvent<int> onBallFired;                 // 공을 하나 쏴서 남은 탄창 수가 변했을 때
    public UnityEvent onMagazineEmpty;                  // 5개를 다 쏴서 탄창이 비었을 때 (턴 종료/결산 트리거)

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        onMagazineLoaded.AddListener((magazine) => {
            MagazineUI ui = FindAnyObjectByType<MagazineUI>();
            if (ui != null) ui.UpdateMagazineDisplay(magazine);
        });

        InitializeRun();
        
        // ★ 핵심 수정 1: 게임 시작 직후 장전하지 않습니다.
        // 이제 GameManager가 스테이지 선택 후 Battle 페이즈에 진입할 때 알아서 장전을 지시합니다.
    }

    // 1. 게임(런) 진입 시 한 번 호출하여 전체 덱을 뽑기 더미로 복사하고 섞습니다.
    public void InitializeRun()
    {
        drawPile.Clear();
        discardPile.Clear();
        drawPile.AddRange(currentDeck);
        ShuffleList(drawPile);
    }

    // 2. 매 라운드(스테이지) 시작 시 호출하여 5개의 공을 장전합니다.
    public void StartRound()
    {
        // ★ 핵심 수정 2: 기존에 장전되어 있던 공이 있다면, 허공에 버리지 않고 '버린 더미'로 안전하게 회수합니다.
        if (roundMagazine.Count > 0)
        {
            discardPile.AddRange(roundMagazine);
        }
        
        roundMagazine.Clear();

        for (int i = 0; i < ballsPerRound; i++)
        {
            // 뽑기 더미가 비었다면 버린 더미를 다시 섞어옵니다.
            if (drawPile.Count == 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleList(drawPile);
            }

            // 그래도 뽑기 더미에 공이 있다면 탄창에 추가
            if (drawPile.Count > 0)
            {
                roundMagazine.Add(drawPile[0]);
                drawPile.RemoveAt(0);
            }
        }

        // UI에 장전된 5개의 공 데이터를 전달하여 화면에 표시합니다.
        onMagazineLoaded.Invoke(roundMagazine);
    }

    // 3. 플레이어가 발사 버튼/슬링샷을 당겼을 때 호출할 함수
    public BallData FireNextBall()
    {
        // 탄창이 비어있다면 발사 불가
        if (roundMagazine.Count == 0) return null;

        // 탄창의 맨 앞(0번 인덱스) 공을 꺼냅니다.
        BallData ballToFire = roundMagazine[0];
        roundMagazine.RemoveAt(0);
        
        // 쏜 공은 버린 더미로 보냅니다.
        discardPile.Add(ballToFire);

        // UI 갱신을 위해 남은 공의 개수를 전달합니다.
        onBallFired.Invoke(roundMagazine.Count);

        // 방금 쏜 공이 마지막 공이었다면 탄창 고갈 이벤트 발생
        if (roundMagazine.Count == 0)
        {
            onMagazineEmpty.Invoke();
        }

        return ballToFire;
    }

    // 새로운 공을 획득했을 때 (상점 등)
    public void AddBallToDeck(BallData newBall)
    {
        currentDeck.Add(newBall);
        discardPile.Add(newBall); // 획득한 공은 보통 버린 더미로 들어가 다음 사이클에 나옵니다.
    }

    // 리스트 무작위 셔플
    private void ShuffleList(List<BallData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            BallData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}