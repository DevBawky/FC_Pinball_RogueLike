using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject ballPrefab; 
    public float spawnDelay = 0.2f; 

    private bool isSpawning = false; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSpawning)
        {
            // 탄창이 비어있으면 쏘지 않도록 방어 (DeckManager 확인)
            if (DeckManager.Instance != null && DeckManager.Instance.roundMagazine.Count == 0)
            {
                Debug.Log("장전된 공이 없습니다!");
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; 

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFireStarted();
            }

            StartCoroutine(SpawnBallsRoutine(mousePos));
        }
    }

    private IEnumerator SpawnBallsRoutine(Vector3 spawnPosition)
    {
        isSpawning = true;

        while (true)
        {
            BallData nextBall = DeckManager.Instance.FireNextBall();
            
            if (nextBall == null)
            {
                break; // 장전된 공을 다 쐈으면 루프 탈출
            }

            GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.up; 

            BallController controller = newBall.GetComponent<BallController>();
            if (controller != null)
            {
                controller.InitializeBall(nextBall, randomDir);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFireFinished();
        }
        
        isSpawning = false;
    }
}