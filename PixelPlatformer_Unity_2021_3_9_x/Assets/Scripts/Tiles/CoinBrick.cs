using UnityEngine;
using DG.Tweening;

public class CoinBrick : BreakingBrick
{
    public int CoinsAmount = 1;
    public bool BreakAfterLastCoin = false;

    public SpriteRenderer coinSprite;
    public SpriteRenderer scoreSprite;
    public Sprite EmptySprite;

    [HideInInspector]
    public static CoinBrick SingleCoinBrick; //<-- this trick ensures that only single brick is hit at time

    private Vector2 initCoinSpritePosition;
    private Vector2 initScoreSpritePosition;
    private Sprite initBrickSprite;
    private int initCoinsAmount;

    protected override void Start()
    {
        base.Start();
        
        coinSprite.gameObject.SetActive(false);
        scoreSprite.gameObject.SetActive(false);
                
        initCoinSpritePosition = coinSprite.transform.position;
        initScoreSpritePosition = scoreSprite.transform.position;
        initBrickSprite = spriteRenderer.sprite;
        initCoinsAmount = CoinsAmount;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Does nothing if emptied of coins
        if (CoinsAmount == 0)
            return;

        base.OnCollisionEnter2D(collision);

        if (playerCollidesFromBelow != null)
        {
            SingleCoinBrick = this;

            SingleCoinBrick.CollectCoin();

            playerCollidesFromBelow.ChangeCoinsAndScore(1, 100);
            if (CoinsAmount <= 0)
            {
                if (BreakAfterLastCoin)
                {
                    SingleCoinBrick.Break(playerCollidesFromBelow);
                }
                else
                    spriteRenderer.sprite = EmptySprite;
            }
        }   
    }

    public virtual void CollectCoin()
    {
        GameObject coinInstance = Instantiate(coinSprite.gameObject);
        coinInstance.transform.position = initCoinSpritePosition;
        coinInstance.SetActive(true);

        GameObject scoreInstance = Instantiate(scoreSprite.gameObject);
        scoreInstance.transform.position = initScoreSpritePosition;
        scoreInstance.SetActive(true);

        coinInstance.transform.DOMoveY(coinSprite.transform.position.y + .48f, .5f).OnComplete(() => Destroy(coinInstance));
        scoreInstance.transform.DOMoveY(scoreSprite.transform.position.y + .24f, .5f).OnComplete(() => Destroy(scoreInstance));

        //Add coins and score
        CoinsAmount--;
    }

    public override void Reset()
    {
        base.Reset();

        //Reset sprite to brick
        spriteRenderer.sprite = initBrickSprite;
        //Reset coins
        CoinsAmount = initCoinsAmount;
    }
}
