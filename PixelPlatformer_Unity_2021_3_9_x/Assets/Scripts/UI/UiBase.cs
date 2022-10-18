using UnityEngine;

public class UiBase : MonoBehaviour
{
    public virtual void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
