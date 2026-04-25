using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public event Action DeckChanged;

    [Header("Deck Settings")]
    public List<BallData> currentDeck = new List<BallData>();
    [SerializeField] private int maxDeckSize = 15;

    [Header("Round Settings")]
    public int ballsPerRound = 5;
    public List<BallData> roundMagazine = new List<BallData>();

    [Header("Events")]
    public UnityEvent<List<BallData>> onMagazineLoaded;
    public UnityEvent<int> onBallFired;
    public UnityEvent onMagazineEmpty;

    public int MaxDeckSize => maxDeckSize;
    public bool IsDeckFull => currentDeck.Count >= maxDeckSize;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        InitializeRun();
    }

    public void InitializeRun()
    {
        roundMagazine.Clear();
    }

    public void StartRound()
    {
        roundMagazine.Clear();
        LoadRandomMagazine();

        onMagazineLoaded.Invoke(roundMagazine);
    }

    public BallData FireNextBall()
    {
        if (roundMagazine.Count == 0) return null;
        BallData ballToFire = roundMagazine[0];
        roundMagazine.RemoveAt(0);

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

        int magazineCount = Mathf.Min(ballsPerRound, shuffledDeck.Count);
        for (int i = 0; i < magazineCount; i++)
        {
            roundMagazine.Add(shuffledDeck[i]);
        }
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
