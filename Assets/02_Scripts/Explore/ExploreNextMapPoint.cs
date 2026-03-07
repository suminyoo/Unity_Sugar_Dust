using UnityEngine;

public class ExploreNextMapPoint : MonoBehaviour, IInteractable
{
    public ExploreManager exploreManager;
    public SoundData walkSound;

    public string GetInteractPrompt() => LocalizationHelper.L("PROMPT_EXPLORE_DEEPER");

    void Start()
    {
        exploreManager = FindAnyObjectByType<ExploreManager>();
    }

    public void OnInteract()
    {
        CommonConfirmPopup.Instance.OpenPopup(
            LocalizationHelper.L("CONFIRM_MOVE_NEXT_AREA"), 
            () =>
            {
                if (exploreManager != null)
                {
                    if (walkSound.clip != null) SoundManager.Instance.PlaySFX2D(walkSound);

                    exploreManager.AttemptMoveToNextStage();
                }
            }
        );
    }
}
