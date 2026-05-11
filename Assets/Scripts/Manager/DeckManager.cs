using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public const int CylinderCapacity = 5;
    public event Action DeckChanged;

    [Header("Deck Settings")]
    public List<BallData> currentDeck = new List<BallData>();
    [SerializeField] private int maxDeckSize = 15;

    [Header("Round Settings")]
    [SerializeField, Min(1)] private int ballsPerRound = CylinderCapacity;
    public List<BallData> roundMagazine = new List<BallData>();
    public BallData CurrentLoadedBall { get; private set; }

    [Header("Events")]
    public UnityEvent<List<BallData>> onMagazineLoaded = new UnityEvent<List<BallData>>();
    public UnityEvent<int> onBallFired = new UnityEvent<int>();
    public UnityEvent onMagazineEmpty = new UnityEvent();

    public int MaxDeckSize => maxDeckSize;
    public bool IsDeckFull => currentDeck.Count >= maxDeckSize;
    public int BallsPerRound => Mathf.Min(ballsPerRound, CylinderCapacity);
    public int RemainingMagazineCount => roundMagazine.Count;

    void Awake()
    {
        if (Instance == null) Instance = this;
        ballsPerRound = BallsPerRound;
    }

    private void OnValidate()
    {
        ballsPerRound = Mathf.Clamp(ballsPerRound, 1, CylinderCapacity);
    }

    void Start()
    {
        InitializeRun();
    }

    public void InitializeRun()
    {
        roundMagazine.Clear();
        CurrentLoadedBall = null;
    }

    public void StartRound()
    {
        roundMagazine.Clear();
        LoadRandomMagazine();
        RefreshCurrentLoadedBall();

        onMagazineLoaded.Invoke(roundMagazine);
    }

    public BallData FireNextBall()
    {
        if (roundMagazine.Count == 0) return null;

        BallData ballToFire = roundMagazine[0];
        roundMagazine.RemoveAt(0);
        RefreshCurrentLoadedBall();

        onBallFired.Invoke(roundMagazine.Count);

        if (roundMagazine.Count == 0)
        {
            onMagazineEmpty.Invoke();
        }

        return ballToFire;
    }

    public bool AddBallToDeck(BallData newBall)
    {
        if (newBall == null || IsDeckFull)
        {
            return false;
        }

        currentDeck.Add(newBall);
        InitializeRun();
        DeckChanged?.Invoke();
        return true;
    }

    public bool RemoveBallFromDeck(BallData ballToRemove)
    {
        if (ballToRemove == null || currentDeck.Count <= 1)
        {
            return false;
        }

        bool removed = currentDeck.Remove(ballToRemove);
        if (!removed)
        {
            return false;
        }

        InitializeRun();
        DeckChanged?.Invoke();
        return true;
    }

    private void LoadRandomMagazine()
    {
        if (currentDeck.Count == 0)
        {
            return;
        }

        List<BallData> shuffledDeck = new List<BallData>(currentDeck);
        ShuffleList(shuffledDeck);

        int magazineCount = Mathf.Min(BallsPerRound, shuffledDeck.Count);
        for (int i = 0; i < magazineCount; i++)
        {
            roundMagazine.Add(shuffledDeck[i]);
        }
    }

    private void RefreshCurrentLoadedBall()
    {
        CurrentLoadedBall = roundMagazine.Count > 0 ? roundMagazine[0] : null;
    }

    private void ShuffleList(List<BallData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            BallData temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
