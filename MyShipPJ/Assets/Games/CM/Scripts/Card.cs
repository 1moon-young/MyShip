using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer cardRenderer;

    [SerializeField]
    private Sprite animalSprites;

    [SerializeField]
    private Sprite backSprite;
    private bool isFlipped = false;
    private bool isFlipping = false;
    private bool isMatched = false;
    public int cardID;

    public void SetCardID(int id)
    {
        cardID = id;
    }

    public void SetMatched()
    {
        isMatched = true;
    }
    public void SetAnimalSprite(Sprite sprite)
    {
        animalSprites = sprite;
    }

    public void FlipCard()
    {

        isFlipping = true;


        //¾Ö´Ï
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(0f, originalScale.y, originalScale.z);

        transform.DOScale(targetScale, 0.2f).OnComplete(() =>
        {
            isFlipped = !isFlipped;

            if (isFlipped)
            {
                cardRenderer.sprite = animalSprites;
            }
            else
            {
                cardRenderer.sprite = backSprite;
            }
            transform.DOScale(originalScale, 0.2f).OnComplete(() => {
                isFlipping=false;
            });
        });

       
    }

    void OnMouseDown()
    {
        if (isMatched || isFlipping|| isFlipped) return;

        GameManagerTwo.instance.CardClicked(this);
    }
}
