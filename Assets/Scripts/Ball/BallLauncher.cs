using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject ballPrefab; 
    public float spawnDelay = 0.2f; 
    
    public LayerMask floorLayer; 

    private bool isSpawningRoutineActive = false; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSpawningRoutineActive)
        {
            if (DeckManager.Instance == null || DeckManager.Instance.roundMagazine.Count == 0) return;
            if (GameManager.Instance != null && GameManager.Instance.isCalculating) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D, floorLayer);

            if (hitCollider != null)
            {
                mousePos.z = 0f; 
                StartCoroutine(SpawnBallsRoutine(mousePos));
            }
            else
            {
                // 바닥이 아닌 곳(벽, 허공, UI 등)을 클릭했을 때의 처리 (디버그 로그)
                Debug.Log("바닥(Floor) 영역을 클릭해야 발사할 수 있습니다!");
            }
        }
    }

    private IEnumerator SpawnBallsRoutine(Vector3 spawnPosition)
    {
        isSpawningRoutineActive = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFireStarted();
        }

        while (true)
        {
            BallData nextBall = DeckManager.Instance.FireNextBall();
            
            if (nextBall == null)
            {
                break; 
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
        
        isSpawningRoutineActive = false;
    }
}