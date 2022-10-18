public class Coin : CollectableBase
{
    protected override void Collect(PlayerCharacter collector)
    {
        base.Collect(collector);
        collector.ChangeCoinsAndScore(1, 100); //And one coin and 100 score
    }
}
