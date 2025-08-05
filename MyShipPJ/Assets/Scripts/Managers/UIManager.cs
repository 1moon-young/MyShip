using System;
using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Character")]
    public Character curCharacter;
    List<Character> characters;
    public Transform characterContainer;
    public GameObject characterSpeechBubble;
    public TextMeshProUGUI characterScriptTxt;

    /**  Top bar */
    [Header("top bar")]
    public TextMeshProUGUI coinTxt;
    public Slider fullenssSlider;
    public Image fullnessFill;
    public Slider energySlider;
    public Image energyFill;
    public Slider favorSlider;
    public TextMeshProUGUI characterLevelTxt;
    public GameObject coinShopPanel, settingsPanel;

    /** 캐릭터 리스트 */
    [Header("Character list")]
    public GameObject CharacterListPanel;
    public Transform CharacterListScrollView;
    public GameObject characterListContent;
    public GameObject CharacterCardPrefab;

    /** 0. 대기실 */
    [Header("watting room")]
    public List<Game> games;
    public GameObject gameItemPrefab;
    public GameObject gamesPanel;
    public GameObject gameContent;
    public GameObject howToGamePanel;

    /** 1. 개인실 */
    [Header("private room")]
    public Sprite[] lightImgs;   // 0-Off, 1-on
    public Image lightImg;
    public GameObject lightOffPanel;

    public List<Wallpaper> wallpapers;
    public GameObject wallpaperPrefab;
    public GameObject wallPaperPanel;
    public List<GameObject> wallpaperScrollViews;
    public List<GameObject> wallPaperContents;
    public TMP_Dropdown wallpaperDropdown;
    public Transform WallpaperContainer;

    /** 2. 식당 */
    [Header("refrigerator")]
    public List<Food> foods;
    public GameObject refrigeratorPanel;
    public GameObject refrigeratorContent;
    public GameObject foodItemPrefab;
    public GameObject foodShopBtn;

    [Header("food shop")]
    public GameObject foodShopPanel;
    public GameObject foodShopContent;
    public GameObject foodShopItemPrefab;

    [Header("selected food")]
    public GameObject selectedFood;
    public TextMeshProUGUI selectedFoodTxt;

    [Header("others")]
    public GameObject TriumphsPanel;
    public GameObject Quests;
    public GameObject potionPanel;


    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        InitData();

        LoadCharacterList();
        SetFavorUI();
        SetFullnessUI();
        SetEnergyUI();
        SetCoinUI();

        InitRefrigerator();
        InitFoodShop();
        InitWallPaperList();
        InitWallpaper();
        InitGameList();
        
        StartCoroutine(CharacterSpeech());

        if (PlayerPrefs.GetInt("CurRoom", 0) == (int)RoomNum.PRIVATE)
        {
            lightImg.sprite = lightImgs[PlayerPrefs.GetInt("LightOn", 0)];
            lightOffPanel.SetActive(PlayerPrefs.GetInt("LightOn", 0) == 0);
        }

        gamesPanel.SetActive(GameManager.instance.returnFromGame);
        GameManager.instance.returnFromGame = false;

    }

    public void InitData()
    {
        curCharacter = GameManager.instance.curCharacter;
        characters = DataManager.instance.characterSotred;
        foods = DataManager.instance.userData.foods;
        wallpapers = DataManager.instance.userData.wallpapers;
        games = DataManager.instance.userData.games;
    }

    /** [1-4] 공용 */
    /** 1. 팝업 열기 */
    public void OpenPopUP(GameObject popupPanel)
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        popupPanel.SetActive(true);
    }

    /** 2. 팝업 닫기 */
    public void ClosePopUp(GameObject popupPanel)
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        popupPanel.SetActive(false);
    }

    /** 3. Slider fill 색깔 */
    public void SetSliderFillColor(Slider slider, Image fill)
    {
        if (slider.value < 30)
            fill.color = Color.red;
        else if (slider.value < 65)
            fill.color = Color.yellow;
        else if (slider.value < 100)
            fill.color = Color.green;
        else
            fill.color = new Color(64 / 255f, 149 / 255f, 255 / 255f);
    }

    /** 4. 캐릭터 체인지 */
    public void CharacterChangeUI(int index)
    {
        UpdateSpotlight();
        curCharacter = characters[index];
        GameManager.instance.curCharacter = curCharacter;
        PlayerPrefs.SetInt("CurCharacter", index);
        UpdateSpotlight();

        // top bar 변경
        SetFavorUI();
        SetFullnessUI();
        SetEnergyUI();

        // 캐릭터 변경
        characterContainer.GetComponent<MainChaContainer>().ChangeCharacter();

        // 캐릭터 리스트 팝업 닫기
        CharacterListPanel.SetActive(false);
    }




    /** 1. 캐릭터 말풍선 생성 */
    IEnumerator CharacterSpeech()
    {
        while (true)
        {
            yield return new WaitForSeconds(20);
            string[] scripts = curCharacter.script;
            characterScriptTxt.text = scripts[UnityEngine.Random.Range(0, curCharacter.level)];
            characterSpeechBubble.SetActive(true);

            yield return new WaitForSeconds(7);
            characterSpeechBubble.SetActive(false);
        }
    }



    /*************** [2-6] top bar 세팅 **********************/
    /** 2. 호감도 UI 업데이트 */
    public void SetFavorUI()
    {
        characterLevelTxt.text = curCharacter.level.ToString();
        favorSlider.maxValue = DataManager.instance.favorMax[curCharacter.level];
        favorSlider.value = curCharacter.favor;
        UpdateCurCharacter();
    }

    /** 3. 호감도 업 */
    public void CharacterFavorUp(int favor)
    {
        if (curCharacter.level == 8)
            return;

        int fixFavor = curCharacter.favor + favor;
        if (fixFavor >= DataManager.instance.favorMax[curCharacter.level])
        {
            curCharacter.favor = fixFavor - DataManager.instance.favorMax[curCharacter.level];
            curCharacter.level += 1;
        }
        else
        {
            curCharacter.favor = fixFavor;
        }
        DataManager.instance.saveData();
        SetFavorUI();
    }

    /** 4. 포만도 UI 업데이트 */
    public void SetFullnessUI()
    {
        fullenssSlider.value = curCharacter.fullness;
        SetSliderFillColor(fullenssSlider, fullnessFill);
        UpdateCurCharacter();
    }

    /** 5. 포만도 업 */
    public void CharacterFullnessUp(int fullness)
    {
        curCharacter.fullness = Mathf.Min(curCharacter.fullness + fullness, 100);
        DataManager.instance.saveData();
        SetFullnessUI();
    }

    /** 6. 에너지 UI 업데이트 */
    public void SetEnergyUI()
    {
        energySlider.value = curCharacter.energy;
        SetSliderFillColor(energySlider, energyFill);
    }

    /** 7. 에너지 업 */
    public void CharacterEnergyUp(int energy)
    {
        curCharacter.energy = Mathf.Min(curCharacter.energy + energy, 100);
        DataManager.instance.saveData();
        SetEnergyUI();
    }

    /** 8. coin 세팅 */
    public void SetCoinUI()
    {
        int coin = DataManager.instance.userData.coin;
        string txt = coin.ToString();
        if (coin >= 1000)
        {
            txt = coin / 1000 + "K";
        }
        coinTxt.text = txt;
    }
    
    /** 10. coinShop 액션 */
    public void BuyCoin(int value){

    }



    /*************** [1-4] 캐릭터 리스트 UI 세팅 *************************/
    /** 1. 캐릭터 리스트 UI 로드 */
    public void LoadCharacterList()
    {
        // 캐릭터 카드 추가
        for (int i = 0; i < characters.Count; i++)
        {
            GameObject characterCard = Instantiate(CharacterCardPrefab, transform.position, Quaternion.identity);
            CharacterCard cardScript = characterCard.GetComponent<CharacterCard>();
            characterCard.name = characters[i].name;
            cardScript.SetUI(characters[i]);

            characterCard.transform.SetParent(characterListContent.transform, false);
            characterCard.SetActive(true);
        }
    }

    /** 2. 캐릭터 리스트 버튼 액션 */
    public void CharacterListAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);

        CharacterListScroll scrollScript = CharacterListScrollView.GetComponent<CharacterListScroll>();
        CharacterListPanel.SetActive(true);
        scrollScript.targetPos = PlayerPrefs.GetInt("CurCharacter", 0);
        scrollScript.isDrag = false;

        scrollScript.contentTransform.GetChild(scrollScript.targetPos).GetChild(1).GetComponent<Scrollbar>().value = 1;
    }

    /** 3. 캐릭터 리스트의 에너지만 갱신 */
    public void UpdateEnergyCharacterList()
    {  // 아이템을 재생성 하지 않고 내용만 업데이트
        for (int i = 0; i < characters.Count; i++)
        {
            Transform card = characterListContent.transform.GetChild(i);
            CharacterCard cardScript = card.GetComponent<CharacterCard>();
            cardScript.UpdateEnergy();
        }
    }

    /** 4. 캐릭터 리스트 중 현재 캐릭터만 갱신 */
    public void UpdateCurCharacter()
    {
        int index = PlayerPrefs.GetInt("CurCharacter", 0);
        Transform card = characterListContent.transform.GetChild(index);
        CharacterCard cardScript = card.GetComponent<CharacterCard>();
        cardScript.SetUI(characters[index]);
    }

    /** 5. 현재 캐릭터의 spotlight만 갱신 */
    public void UpdateSpotlight()
    {
        int index = PlayerPrefs.GetInt("CurCharacter", 0);
        Transform card = characterListContent.transform.GetChild(index);
        CharacterCard cardScript = card.GetComponent<CharacterCard>();
        cardScript.ChangeSpotlight();
    }





    /*********************** [1-3] 0. 대기실 **************************/
    /** 1. 게임 리스트 초기화 */
    public void InitGameList()
    {
        foreach (Game game in games)
        {
            GameObject gameItem = Instantiate(gameItemPrefab, transform.position, quaternion.identity);
            GameItem gameItemScript = gameItem.GetComponent<GameItem>();
            gameItem.name = game.name;
            gameItemScript.SetUI(game);

            gameItem.transform.SetParent(gameContent.transform, false);
            gameItem.SetActive(true);
        }
    }

    /** 2. 게임 씬 로드 */
    /** GameItem에서 사용 */
    public void SelectGame(Game game)
    {
        howToGamePanel.GetComponent<HowToGame>().SetUI(game);
        howToGamePanel.SetActive(true);
    }






    /*********************** [1-7] 1. 개인실 **************************/
    /** 1. 전등 버튼 액션 */
    public void LightAction()
    {
        int prelight = PlayerPrefs.GetInt("LightOn", 0);
        PlayerPrefs.SetInt("LightOn", prelight ^ 1);
        lightImg.sprite = lightImgs[prelight ^ 1];
        lightOffPanel.SetActive(prelight == 1);
    }

    /** 2. 벽지 리스트 버튼 액션 */
    public void WallPaperAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);

        RectTransform rt = wallPaperContents[PlayerPrefs.GetInt("WallpaperRoomNum", 0)].transform.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);

        wallPaperPanel.SetActive(true);
    }

    /** 3. 벽지 리스트 초기화 */
    public void InitWallPaperList()
    {
        foreach (Wallpaper wallpaper in wallpapers)
        {
            GameObject wallpaperItem = Instantiate(wallpaperPrefab, transform.position, Quaternion.identity);
            WallpaperItem wallpaperScript = wallpaperItem.GetComponent<WallpaperItem>();

            wallpaperItem.name = wallpaper.name;
            wallpaperScript.SetUI(wallpaper);

            wallpaperItem.transform.SetParent(wallPaperContents[(int)wallpaper.roomNum].transform, false);
        }

        wallpaperScrollViews[PlayerPrefs.GetInt("WallpaperRoomNum", 0)].SetActive(true);
        wallpaperDropdown.value = PlayerPrefs.GetInt("WallpaperRoomNum", 0);
    }

    /** 4. 벽지 룸 변경 */
    public void ChangeDropdownInWallpaper(int roomNUm)
    {
        wallpaperScrollViews[PlayerPrefs.GetInt("WallpaperRoomNum", 0)].SetActive(false);

        RectTransform rt = wallPaperContents[PlayerPrefs.GetInt("WallpaperRoomNum", 0)].transform.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);

        wallpaperScrollViews[roomNUm].SetActive(true);
        PlayerPrefs.SetInt("WallpaperRoomNum", roomNUm);
    }

    /** 5. 벽지 변경 */
    public void ChangeWallpaper(Wallpaper wallpaper)
    {
        int roomNum = (int)wallpaper.roomNum;
        Transform wallpaperContent = wallPaperContents[roomNum].transform;
        for (int i = 0; i < wallpaperContent.childCount; i++)
            wallpaperContent.GetChild(i).GetChild(0).GetChild(1).gameObject.SetActive(false); // checkedImg

        WallpaperContainer.GetChild(roomNum).GetComponent<SpriteRenderer>().sprite
            = Resources.Load<Sprite>("Sprites/Items/Wallpapers/" + roomNum + "_" + wallpaper.name);
    }

    /** 6. 벽지 초기화 */
    public void InitWallpaper()
    {
        Sprite[] paperSprites = new Sprite[WallpaperContainer.childCount];

        for (int i = 0; i < WallpaperContainer.childCount; i++)
        {
            int paper = PlayerPrefs.GetInt(i + "_wallpaper", 0);

            foreach (Wallpaper wallpaper in wallpapers)
            {
                if (wallpaper.id == paper)
                {
                    paperSprites[i] = Resources.Load<Sprite>("Sprites/Items/Wallpapers/" + i + "_" + wallpaper.name);
                    break;
                }
            }
        }
        for (int i = 0; i < WallpaperContainer.childCount; i++)
        {
            WallpaperContainer.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = paperSprites[i];
        }
    }

    /** 7. 옷장 버튼 액션 */
    public void ClothesAction()
    {
        print("ClothesAction");
    }






    /********************* [1-8] 2. 식당 ******************************/
    // 1. 냉장고 버튼 액션
    public void RefrigeratorAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        refrigeratorPanel.SetActive(true);
    }

    /** 2. 냉장고 초기화  */
    void InitRefrigerator()
    {
        foreach (Food food in foods)
        {
            GameObject foodItem = Instantiate(foodItemPrefab, transform.position, Quaternion.identity);
            FoodItem foodItemScript = foodItem.GetComponent<FoodItem>();

            foodItemScript.SetUI(food);
            foodItem.name = food.name;

            foodItem.transform.SetParent(refrigeratorContent.transform, false);
        }

        foodShopBtn.transform.SetAsLastSibling();
    }

    /** 3. 냉장고 업데이트 */
    void UpdateRefirgerator(Food food)
    {
        GameObject foodItem = refrigeratorContent.transform.GetChild(food.index).gameObject;
        FoodItem foodItemScript = foodItem.GetComponent<FoodItem>();
        foodItemScript.SetCount();
    }

    /** 4. food shop 버튼 액션 */
    public void FoodShopAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        refrigeratorPanel.SetActive(false);
        foodShopPanel.SetActive(true);
    }

    /** 5. food shop 초기화 */
    void InitFoodShop()
    {
        // 아이템 추가
        foreach (Food food in foods)
        {
            GameObject foodShopItem = Instantiate(foodShopItemPrefab, transform.position, Quaternion.identity);
            FoodShopItem foodShopItemScript = foodShopItem.GetComponent<FoodShopItem>();

            foodShopItem.name = food.name;
            foodShopItemScript.SetUI(food);

            foodShopItem.transform.SetParent(foodShopContent.transform, false);
        }
    }

    void UpdateFoodShop(Food food)
    {
        GameObject foodShopItem = foodShopContent.transform.GetChild(food.index).gameObject;
        FoodShopItem foodShopItemScript = foodShopItem.GetComponent<FoodShopItem>();
        foodShopItemScript.SetCount();
        foodShopItemScript.SetVisibleBuy();
    }

    /** 5. food shop (+냉장고) 닫기 버튼 액션 */
    public void CloseFoodShopAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        foodShopPanel.SetActive(false);
    }

    /** 6. 냉장고로 돌아가기 버튼 액션 */
    public void BackToRefrigeratorAction()
    {
        CloseFoodShopAction();
        RefrigeratorAction();
    }

    /** 7. food 내용 변경시 데이터 갱신 */
    public void FoodChange(Food food)
    {
        DataManager.instance.saveData();

        UpdateRefirgerator(food);
        UpdateFoodShop(food);
    }

    /** 8. food 선택 -> SelectedFood에 세팅 */
    /** FoodItems.SelectFoodAction()에서 사용. */
    public void SelectFoodAction(Food food)
    {
        selectedFood.GetComponent<SelectedFood>().SetUI(food);
        PlayerPrefs.SetInt("SelectedFood", food.index);
        refrigeratorPanel.SetActive(false);
    }

    public void potionAction()
    {
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);
        potionPanel.SetActive(true);
    }


}
