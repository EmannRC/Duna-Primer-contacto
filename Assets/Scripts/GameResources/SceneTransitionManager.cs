using UnityEngine;


public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Animator animator;

    private void Awake()
    {
        Instance = this;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        StartFadeIn();
    }

    public void StartFadeOut()
    {
        if (animator == null)
        {
            Debug.LogError(
                "[FadeInOnSceneLoad] No se encontró el Animator.",
                this
            );
            return;
        }

     
        animator.SetTrigger("StartFadeOut");
    }

    public void StartFadeIn()
    {
        

        animator.SetTrigger("StartFadeIn");
    }

}
