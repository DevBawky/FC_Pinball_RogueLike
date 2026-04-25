using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagazineUI : MonoBehaviour
{
    [Header("UI Slots")]
    public List<Image> ballIconSlots = new List<Image>();

    [Header("Settings")]
    public Color emptySlotColor = new Color(1f, 1f, 1f, 0.2f);
    public Color activeSlotColor = Color.white;
    [SerializeField] private Sprite emptyChamberSprite;

    [Header("Cylinder Layout")]
    [SerializeField] private float cylinderRadius = 24f;
    [SerializeField] private float startAngle = 90f;

    [Header("Fire Animation")]
    [SerializeField] private float rotationDuration = 0.18f;
    [SerializeField] private bool rotateClockwise = true;

    private bool isSubscribed;
    private float cylinderRotation;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        DisableLayoutGroup();
        PrepareSlots();
        ArrangeCylinder();
        ClearAllSlots();
    }

    private void OnEnable()
    {
        SubscribeDeckEvents();
        SyncWithCurrentMagazine();
    }

    private void Start()
    {
        SubscribeDeckEvents();
        SyncWithCurrentMagazine();
    }

    private void OnDisable()
    {
        UnsubscribeDeckEvents();
    }

    private void OnValidate()
    {
        PrepareSlots();
        ArrangeCylinder();
    }

    public void UpdateMagazineDisplay(List<BallData> magazine)
    {
        cylinderRotation = 0f;
        ApplyMagazineDisplay(magazine);
    }

    public void RefreshOnFire(int remainingCount)
    {
        List<BallData> nextMagazine = DeckManager.Instance != null ? DeckManager.Instance.roundMagazine : null;

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        rotationCoroutine = StartCoroutine(RotateOnFireRoutine(nextMagazine, remainingCount));
    }

    private IEnumerator RotateOnFireRoutine(List<BallData> nextMagazine, int remainingCount)
    {
        int slotCount = ballIconSlots.Count;
        if (slotCount <= 0)
        {
            yield break;
        }

        float direction = rotateClockwise ? -1f : 1f;
        float fromRotation = cylinderRotation;
        float toRotation = cylinderRotation + (360f / slotCount * direction);
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = rotationDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / rotationDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            cylinderRotation = Mathf.Lerp(fromRotation, toRotation, eased);
            ArrangeCylinder();
            yield return null;
        }

        cylinderRotation = toRotation;
        ArrangeCylinder();

        if (nextMagazine != null)
        {
            ApplyMagazineDisplay(nextMagazine);
        }
        else
        {
            ApplyFallbackRemainingDisplay(remainingCount);
        }

        rotationCoroutine = null;
    }

    private void ClearAllSlots()
    {
        foreach (Image slot in ballIconSlots)
        {
            if (slot == null) continue;

            SetEmptySlot(slot);
        }
    }

    private void ApplyMagazineDisplay(List<BallData> magazine)
    {
        ArrangeCylinder();
        ClearAllSlots();

        if (magazine == null) return;

        int displayCount = Mathf.Min(magazine.Count, ballIconSlots.Count);
        for (int i = 0; i < displayCount; i++)
        {
            Image slot = ballIconSlots[i];
            BallData ballData = magazine[i];
            if (slot == null || ballData == null) continue;

            slot.sprite = ballData.ballSprite;
            slot.color = activeSlotColor;
            slot.preserveAspect = true;
            slot.gameObject.SetActive(true);
        }
    }

    private void ApplyFallbackRemainingDisplay(int remainingCount)
    {
        for (int i = 0; i < ballIconSlots.Count; i++)
        {
            Image slot = ballIconSlots[i];
            if (slot == null) continue;

            if (i >= remainingCount)
            {
                SetEmptySlot(slot);
            }
            else
            {
                slot.color = activeSlotColor;
            }
        }
    }

    private void SetEmptySlot(Image slot)
    {
        slot.sprite = emptyChamberSprite;
        slot.color = emptySlotColor;
        slot.preserveAspect = true;
        slot.gameObject.SetActive(true);
    }

    private void OnMagazineEmpty()
    {
        if (rotationCoroutine == null)
        {
            ClearAllSlots();
        }
    }

    private void SubscribeDeckEvents()
    {
        if (isSubscribed || DeckManager.Instance == null) return;

        DeckManager.Instance.onMagazineLoaded.AddListener(UpdateMagazineDisplay);
        DeckManager.Instance.onBallFired.AddListener(RefreshOnFire);
        DeckManager.Instance.onMagazineEmpty.AddListener(OnMagazineEmpty);
        isSubscribed = true;
    }

    private void UnsubscribeDeckEvents()
    {
        if (!isSubscribed || DeckManager.Instance == null) return;

        DeckManager.Instance.onMagazineLoaded.RemoveListener(UpdateMagazineDisplay);
        DeckManager.Instance.onBallFired.RemoveListener(RefreshOnFire);
        DeckManager.Instance.onMagazineEmpty.RemoveListener(OnMagazineEmpty);
        isSubscribed = false;
    }

    private void SyncWithCurrentMagazine()
    {
        if (DeckManager.Instance != null && DeckManager.Instance.roundMagazine.Count > 0)
        {
            UpdateMagazineDisplay(DeckManager.Instance.roundMagazine);
        }
        else
        {
            ClearAllSlots();
        }
    }

    private void PrepareSlots()
    {
        for (int i = 0; i < ballIconSlots.Count; i++)
        {
            Image slot = ballIconSlots[i];
            if (slot == null) continue;

            RectTransform rect = slot.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private void ArrangeCylinder()
    {
        int slotCount = ballIconSlots.Count;
        if (slotCount <= 0) return;

        for (int i = 0; i < slotCount; i++)
        {
            Image slot = ballIconSlots[i];
            if (slot == null) continue;

            RectTransform rect = slot.rectTransform;
            if (slotCount == 1)
            {
                rect.anchoredPosition = Vector2.zero;
                continue;
            }

            float angle = startAngle + cylinderRotation - (360f / slotCount * i);
            float radian = angle * Mathf.Deg2Rad;
            rect.anchoredPosition = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * cylinderRadius;
        }
    }

    private void DisableLayoutGroup()
    {
        LayoutGroup layoutGroup = GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }
    }
}
