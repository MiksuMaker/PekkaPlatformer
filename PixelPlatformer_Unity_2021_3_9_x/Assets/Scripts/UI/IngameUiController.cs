using UnityEngine.UI;

public class IngameUiController : UiBase
{
    public Text TextCoins;
    public Text TextTime;

    public void UpdateTextCoins(int currentCoins)
    {        
        TextCoins.text = "x" + currentCoins.ToString("00"); 
    }

    public void UpdateTextTime(int deltaTime)
    {
        
    }
}
