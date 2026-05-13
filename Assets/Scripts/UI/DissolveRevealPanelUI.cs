using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DissolveRevealPanelUI : MonoBehaviour
{
    private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    [SerializeField] private float revealDuration = 0.45f;
    [SerializeField] private float hideDuration = 0.35f;
    [SerializeField] private bool useUnscaledTime = false;

    private readonly Dictionary<Graphic, Material> originalMaterials = new Dictionary<Graphic, Material>();
    private readonly List<Material> dissolveMaterials = new List<Material>();
    private CanvasGroup canvasGroup;
    private Coroutine dissolveRoutine;
    private Shader dissolveShader;
    private bool isClosing;

    void OnEnable()
    {
        if (!isClosing)
        {
            PlayReveal();
        }
    }

    void OnDisable()
    {
        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
            dissolveRoutine = null;
        }

        RestoreOriginalMaterials();
    }

    void OnDestroy()
    {
        DestroyDissolveMaterials();
    }

    public static void SetActiveWithDissolve(GameObject panel, bool active)
    {
        if (panel == null)
        {
            return;
        }

        DissolveRevealPanelUI reveal = panel.GetComponent<DissolveRevealPanelUI>();
        if (reveal == null)
        {
            reveal = panel.AddComponent<DissolveRevealPanelUI>();
        }

        if (active)
        {
            panel.SetActive(true);
            reveal.PlayReveal();
            return;
        }

        if (panel.activeSelf)
        {
            reveal.PlayHide();
        }
        else
        {
            panel.SetActive(false);
        }
    }

    public void PlayReveal()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        isClosing = false;
        PrepareCanvasGroup();
        PrepareGraphics();

        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
        }

        dissolveRoutine = StartCoroutine(DissolveRoutine(1f, 0f, 0f, 1f, revealDuration, false));
    }

    public void PlayHide()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        isClosing = true;
        PrepareCanvasGroup();
        PrepareGraphics();

        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
        }

        dissolveRoutine = StartCoroutine(DissolveRoutine(0f, 1f, 1f, 0f, hideDuration, true));
    }

    private IEnumerator DissolveRoutine(float fromDissolve, float toDissolve, float fromAlpha, float toAlpha, float duration, bool deactivateOnComplete)
    {
        float elapsed = 0f;
        SetDissolveState(fromDissolve, fromAlpha);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(duration > 0f ? elapsed / duration : 1f);
            float eased = t * t * (3f - 2f * t);

            SetDissolveState(Mathf.Lerp(fromDissolve, toDissolve, eased), Mathf.Lerp(fromAlpha, toAlpha, eased));
            yield return null;
        }

        SetDissolveState(toDissolve, toAlpha);
        RestoreOriginalMaterials();
        dissolveRoutine = null;

        if (deactivateOnComplete)
        {
            isClosing = false;
            gameObject.SetActive(false);
        }
    }

    private void PrepareCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void PrepareGraphics()
    {
        RestoreOriginalMaterials();
        DestroyDissolveMaterials();

        if (dissolveShader == null)
        {
            dissolveShader = Resources.Load<Shader>("Shaders/UI_DissolveReveal");
        }

        if (dissolveShader == null)
        {
            dissolveShader = Shader.Find("UI/Dissolve Reveal");
        }

        if (dissolveShader == null)
        {
            return;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null || (!(graphic is Image) && !(graphic is RawImage)))
            {
                continue;
            }

            Material dissolveMaterial = new Material(dissolveShader);
            dissolveMaterial.name = "Runtime UI Dissolve Reveal";

            originalMaterials[graphic] = graphic.material;
            dissolveMaterials.Add(dissolveMaterial);
            graphic.material = dissolveMaterial;
        }
    }

    private void SetDissolveState(float dissolve, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }

        for (int i = 0; i < dissolveMaterials.Count; i++)
        {
            Material material = dissolveMaterials[i];
            if (material == null)
            {
                continue;
            }

            material.SetFloat(DissolveId, dissolve);
            material.SetFloat(AlphaId, alpha);
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (KeyValuePair<Graphic, Material> entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.material = entry.Value;
            }
        }

        originalMaterials.Clear();
    }

    private void DestroyDissolveMaterials()
    {
        for (int i = 0; i < dissolveMaterials.Count; i++)
        {
            Material material = dissolveMaterials[i];
            if (material != null)
            {
                Destroy(material);
            }
        }

        dissolveMaterials.Clear();
    }
}
