using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
public class MenuController : MonoBehaviour
{
    public Button playButton;
    public GameObject levelSelectionPanel;
    public GameObject MainMenu;

    public TextMeshProUGUI LevelNumber;
    public Button[] levelButtons;
    public GameObject[] lockIcons;
    private int selectedLevel = 1; 
    private int unlockedLevels;


    public List<Sprite> LeveSprite;
    public GameObject SaveGameButton;

    
    private void Start()
    {
        // Load saved progress
        if(PlayerPrefs.GetInt("gamesaved",0)==1){
            SaveGameButton.SetActive(true);
        }
        PlayerPrefs.SetInt("LoadSavedGame", 0);


        if(!PlayerPrefs.HasKey("UnlockedLevels")){
                PlayerPrefs.SetInt("UnlockedLevels", 1);
        }
        unlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 1);
        
        if(!PlayerPrefs.HasKey("SelectedLevel")){

         PlayerPrefs.SetInt("SelectedLevel",unlockedLevels);
        }
        SelectLevel(PlayerPrefs.GetInt("SelectedLevel"));
   
        //LevelNumber.text="Level NUMBER"+unlockedLevels;
        UpdateLevelSelectionUI();
        
        playButton.onClick.AddListener(LoadSelectedLevel);
    }

    private void UpdateLevelSelectionUI()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            levelButtons[i].interactable = levelNumber <= unlockedLevels;
            lockIcons[i].SetActive(levelNumber > unlockedLevels);
            levelButtons[i].onClick.AddListener(() => SelectLevel(levelNumber));
        }
    }

    public void OnleveselectionButtonClick(){
        levelSelectionPanel.SetActive(true);
        MainMenu.SetActive(false);

    }

     public void OnCloseLevelSelectionWindow(){
        levelSelectionPanel.SetActive(false);
        MainMenu.SetActive(true);

    }

    public void SelectLevel(int level)
    {
        selectedLevel = level;
        PlayerPrefs.SetInt("SelectedLevel",level);
        
        LevelNumber.text="Level NUMBER"+selectedLevel;
       for(int i=0;i<levelButtons.Length;i++){
            if(selectedLevel==i+1){
                levelButtons[i].gameObject.GetComponent<Image>().sprite=LeveSprite[1];
            }
            else{
                levelButtons[i].gameObject.GetComponent<Image>().sprite=LeveSprite[0];
            }
       }
    }

    public void LoadSelectedLevel()
    {
        // if(GameApp.Instance!=null){
        //     Destroy(GameApp.Instance.transform.gameObject);
        // }
        SceneManager.LoadScene("Scene_JigsawGame");
    }

    public void UnlockNextLevel()
    {
        if (unlockedLevels < levelButtons.Length)
        {
                unlockedLevels++;
            PlayerPrefs.SetInt("UnlockedLevels", unlockedLevels);
            PlayerPrefs.Save();
            UpdateLevelSelectionUI();
        }
    }

    public void LoadSavedGame(){
        
        PlayerPrefs.SetInt("LoadSavedGame", 1);
        LoadSelectedLevel();
         
    }
}
