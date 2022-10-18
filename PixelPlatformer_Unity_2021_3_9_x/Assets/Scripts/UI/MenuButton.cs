using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UniRx;

public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    public enum ButtonType { OnePlayerNewGame, TwoPlayerNewGame, Settings }
    public ButtonType Type;
        
    public bool InitiallySelect = false;
    
    [HideInInspector]
    public Button ButtonComponent;

    [HideInInspector]
    public ReactiveProperty<bool> InternalSelected = new ReactiveProperty<bool>(false);
    public BooleanNotifier Submitted = new BooleanNotifier(false);

    private Animator anim;

    private void Awake()
    {
        ButtonComponent = GetComponent<Button>();        
        anim = GetComponent<Animator>();
    }

    public void Select()
    {        
        ButtonComponent.Select();
        anim.SetTrigger("Selected");
    }

    public void OnSelect(BaseEventData eventData)
    {        
        InternalSelected.Value = true;        
    }

    public void OnDeselect(BaseEventData eventData)
    {        
        InternalSelected.Value = false;        
    }

    public void OnSubmit(BaseEventData eventData)
    {        
        Submitted.SwitchValue();
    }
}