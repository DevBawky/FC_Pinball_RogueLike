using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject ballPrefab;
    public float spawnDelay = 0.2f;
    [SerializeField] private int initialBallPoolSize = 12;

    public LayerMask floorLayer;

    private bool isSpawningRoutineActive = false;

    void Start()
    {
        GameObjectPoolManager.Prewarm(ballPrefab, initialBallPoolSize);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSpawningRoutineActive)
        {
            if (GameManager.Instance != null && GameManager.Instance.currentPhase != GameManager.GamePhase.Battle) return;

            if (DeckManager.Instance == null || DeckManager.Instance.CurrentLoadedBall == null) return;
            if (GameManager.Instance != null && GameManager.Instance.isCalculating) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D, floorLayer);

            if (hitCollider != null)
            {
                if (GameManager.Instance != null && !GameManager.Instance.TryConsumeLife()) return;

                mousePos.z = 0f;
                StartCoroutine(SpawnBallsRoutine(mousePos));
            }
            else
            {
                Debug.Log("바닥(Floor) 영역을 클릭해야 발사할 수 있습니다!");
            }
        }
    }

    private IEnumerator SpawnBallsRoutine(Vector3 spawnPosition)
    {
        isSpawningRoutineActive = true;

        try
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFireStarted();
            }

            while (true)
            {
                if (DeckManager.Instance == null)
                {
                    Debug.LogWarning("DeckManager is missing while firing balls.");
                    break;
                }

                BallData nextBall = DeckManager.Instance.FireNextBall();

                if (nextBall == null)
                {
                    break;
                }

                if (ballPrefab == null)
                {
                    Debug.LogError("Ball prefab is not assigned.");
                    break;
                }

                GameObject newBall = GameObjectPoolManager.Spawn(ballPrefab, spawnPosition, Quaternion.identity);
                if (newBall == null)
                {
                    Debug.LogError("Failed to spawn a pooled ball.");
                    break;
                }

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                if (randomDir == Vector2.zero) randomDir = Vector2.up;

                BallController controller = newBall.GetComponent<BallController>();
                if (controller != null)
                {
                    controller.InitializeBall(nextBall, randomDir);
                }

                yield return new WaitForSeconds(spawnDelay);
            }
        }
        finally
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFireFinished();
            }

            isSpawningRoutineActive = false;
        }
    }
}
