using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Sprite[] cardSprites;

    private List<int> cardIDList  = new List<int>();
    private List<Card> cardList = new List<Card>();

    void Start()
    {
        InitializeBoard();
    }

    public void InitializeBoard()
    {

        cardIDList.Clear(); 
        cardList.Clear();   
        GenerateCardID();
        ShuffleCardID();
        InitBoard();
    }


    void GenerateCardID()
    {
        for(int i=0; i<cardSprites.Length;  i++) 
        {
            cardIDList.Add(i);
            cardIDList.Add(i);
        }
    }

    void ShuffleCardID()
    {
        int cardCount = cardIDList.Count;
        for(int i=0; i< cardCount; i++)
        {
            int randomIndex = Random.Range(i, cardCount);
            int temp = cardIDList[randomIndex];
            cardIDList[randomIndex] = cardIDList[i];
            cardIDList[i] = temp;

        }
    }
    /*
        public void ShuffleAndReassign()
        {
            ShuffleCardID(); // cardIDList ¼¯±â

            for (int i = 0; i < cardList.Count; i++) 
            {
                cardList[i].cardID = cardIDList[i];
                cardList[i].SetMatched(); 
                cardList[i].FlipCard();  
            }
        }
    */

    void InitBoard()
    {
        float spaceY = 1.8f;
        float spaceX = 1.3f;

        int rowCount = 4;
        int colCount = 4;

        int cardIndex = 0;

        for(int row =0; row<rowCount; row++)
        {
            for(int col=0; col<colCount; col++)
            {
                float posX = (col - (colCount / 2)) * spaceX + (spaceX / 2);
                float posY = (row-(int)(rowCount/1.5)) * spaceY;
                Vector3 pos = new Vector3(posX, posY, 0f);
                GameObject cardObject = Instantiate(cardPrefab, pos, Quaternion.identity);
                Card card = cardObject.GetComponent<Card>();
                int cardID = cardIDList[cardIndex++];
                card.SetCardID(cardID);
                card.SetAnimalSprite(cardSprites[cardID]);
                cardList.Add(card);

            }
        }
    }

    public List<Card> GetCards()
    {
        return cardList;
    }
}
