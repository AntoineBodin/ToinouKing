using UnityEngine;
using System.Collections;

public class UIPanelAnimationManager : MonoBehaviour
{
    private Animator[] animators;
    public string animationEnter = "Apparition";
    public string animationExit = "Disparition";
    public float waitTime;
    public bool IsStartingPanel = false;

    void Awake()
    {
        // Trouve tous les Animator dans les enfants du Panel
        animators = GetComponentsInChildren<Animator>();
    }

    private void Start()
    {
        if (IsStartingPanel)
        {
            ShowPanel();
        }
    }

    private IEnumerator ShowPanelAfter3seconds()
    {
        yield return new WaitForSeconds(3);
        ShowPanel();
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        PlayAnimations(animationEnter);
    }

    public void HidePanel()
    {
        StartCoroutine(PlayExitAnimations());
    }

    public void ForceHidePanel()
    {
        gameObject.SetActive(false);
    }

    private void PlayAnimations(string animationName)
    {
        foreach (Animator anim in animators)
        {
            if (anim != null)
            {
                if (anim.HasState(0, Animator.StringToHash(animationName)))
                {
                    anim.Play(animationName, 0, 0);
                }
            }
        }
    }

    private IEnumerator PlayExitAnimations()
    {
        PlayAnimations(animationExit);
        yield return new WaitForSeconds(waitTime);
        gameObject.SetActive(false);
    }
}
