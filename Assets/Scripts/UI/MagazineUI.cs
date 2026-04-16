using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagazineUI : MonoBehaviour
{
    [Header("UI Slots")]
    // 에디터에서 5개의 Image 컴포넌트를 순서대로 넣어주세요.
    public List<Image> ballIconSlots = new List<Image>();

    [Header("Settings")]
    public Color emptySlotColor = new Color(1, 1, 1, 0.2f); // 공이 나갔을 때의 투명도/색상
    public Color activeSlotColor = Color.white;

    private void Start()
    {
        // 시작 시 모든 슬롯을 비워둡니다.
        ClearAllSlots();
    }

    // 1. DeckManager의 onMagazineLoaded 이벤트에 연결할 함수
    public void UpdateMagazineDisplay(List<BallData> magazine)
    {
        ClearAllSlots();

        for (int i = 0; i < magazine.Count; i++)
        {
            if (i < ballIconSlots.Count)
            {
                // 각 슬롯에 공의 Sprite를 할당하고 활성화합니다.
                ballIconSlots[i].sprite = magazine[i].ballSprite;
                ballIconSlots[i].color = activeSlotColor;
                ballIconSlots[i].gameObject.SetActive(true);
            }
        }
    }

    // 2. 공을 하나 쏠 때마다 호출하여 맨 앞의 아이콘을 지우거나 흐리게 처리
    public void RefreshOnFire(int remainingCount)
    {
        // 발사된 공의 인덱스(남은 개수 위치)를 비활성화하거나 투명하게 만듭니다.
        // 예를 들어 5개 중 1개를 쏘면 4개가 남으므로, 4번 인덱스(기존의 마지막 공)가 아닌 
        // 리스트 구조에 따라 순차적으로 시각적 피드백을 줍니다.
        
        // 여기서는 간단하게 뒤에서부터 지우는 것이 아니라, 
        // 현재 남은 공의 개수보다 많은 인덱스의 슬롯들을 비활성화합니다.
        for (int i = 0; i < ballIconSlots.Count; i++)
        {
            if (i >= remainingCount)
            {
                ballIconSlots[i].color = emptySlotColor;
                // 또는 아예 끄고 싶다면: ballIconSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearAllSlots()
    {
        foreach (var slot in ballIconSlots)
        {
            slot.sprite = null;
            slot.color = emptySlotColor;
        }
    }
}