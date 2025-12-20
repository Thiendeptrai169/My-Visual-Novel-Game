using UnityEngine;
using UnityEngine.EventSystems;

public class ChoiceButtonAnimator : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AnimationManager.instance != null)
        { 
            AnimationManager.instance.OnChoiceHover(transform);
        }

        //Play hover sound
        // AudioManager.instance?.PlaySFX("UI_Hover");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (AnimationManager.instance != null)
        {
            AnimationManager.instance.OnChoiceExit(transform);
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Click animation handled in LevelView.CreateChoiceButton

        // Play click sound
        // AudioManager.instance?.PlaySFX("UI_Click");

    }
}
