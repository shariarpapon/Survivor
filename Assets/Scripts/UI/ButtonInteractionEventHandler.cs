using UnityEngine;
using UnityEngine.EventSystems;
using Survivor.Core;

public class ButtonInteractionEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private void OnDisable() 
    {
        transform.localScale = Vector3.one;

        if(GameUtility.ObjectsCurrentlyTweening.Contains(transform.GetInstanceID()))
            GameUtility.ObjectsCurrentlyTweening.Remove(transform.GetInstanceID());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonHover();
        StartCoroutine(GameUtility.TweenScaleUp(transform, 1f, 1.15f, 25f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(GameUtility.TweenScaleDown(transform, 1f, 25f));
    }
}
