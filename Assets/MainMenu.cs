using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum Panels { MainMenu = 0, Options= 1, }

public class MainMenu : MonoBehaviour
{

    private Panels activePanel = Panels.MainMenu;

    public MenuPanels _menuPanels;
    [System.Serializable]
    public class MenuPanels
    {
        public GameObject MainMenu, Options;
        
    }
    public Options options;
    [System.Serializable]
    public class Options
    {
        public Slider heightSlider, widthSlider;
        public Text widthValuetext, heightValuetext;
    }

    void Awake()
    {
        SettingCheck();

        CurrentPanel(0);
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        options.widthValuetext.text = options.widthSlider.value.ToString();
        options.heightValuetext.text = options.heightSlider.value.ToString();
    }

    public void LoadScene(int scenenumber)
    {
        SceneManager.LoadScene(scenenumber);
    }


    public void CurrentPanel(int current)
    {
        activePanel = (Panels)current;

        switch (activePanel)
        {
            case Panels.MainMenu:

                _menuPanels.MainMenu.SetActive(true);
                _menuPanels.Options.SetActive(false);
                PlayerPrefs.SetInt("GridWidth", Convert.ToInt32(options.widthSlider.value));
                PlayerPrefs.SetInt("GridHeight", Convert.ToInt32(options.heightSlider.value));


                break;

            case Panels.Options:

                _menuPanels.MainMenu.SetActive(false);
                _menuPanels.Options.SetActive(true);

                break;
        }
    }

    public void SettingCheck()
    {
        if (PlayerPrefs.HasKey("GridWidth"))
        {
            options.widthSlider.value = PlayerPrefs.GetInt("GridWidth");
        }
        else if (PlayerPrefs.GetInt("GridWidth") < 2 || PlayerPrefs.GetInt("GridWidth") > 15)
        {
            options.widthSlider.value = 8;
            PlayerPrefs.SetInt("GridWidth", 8);
        }
        else
        {
            options.widthSlider.value = 8;
            PlayerPrefs.SetInt("GridWidth", 8);
         
        }

        if (PlayerPrefs.HasKey("GridHeight"))
        {
            options.heightSlider.value = PlayerPrefs.GetInt("GridHeight");
        }

        else if (PlayerPrefs.GetInt("GridHeight") < 2 || PlayerPrefs.GetInt("GridHeight") > 13)
        {
            options.heightSlider.value = 9;
            PlayerPrefs.SetInt("GridHeight", 9);
        }

        else
        {
            options.heightSlider.value = 9;
            PlayerPrefs.SetInt("GridHeight", 9);
        }

    }

    //public void WidthChange()
    //{
    //    PlayerPrefs.SetInt("GridWidth", Convert.ToInt32(options.heightSlider.value));
    //}
    
    //public void HeightChange()
    //{
    //    PlayerPrefs.SetInt("GridHeight", Convert.ToInt32(options.heightSlider.value));
    //}


}
