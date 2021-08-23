using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Elements")]
    public List<GameObject> MenuOptions;

    [Header("Colors")]
    public Color ActiveOptionColor;
    public Color InactiveOptionColor;

    private int SelectedOptionId;

    void Start()
    {
        SetSelectedOption(0);

    }

    void Update()
    {

    }

    public void SetSelectedOption(int optionId)
    {
        SelectedOptionId = optionId;
        for(int i = 0; i < MenuOptions.Count; i++)
        {
            MenuOptions[i].GetComponent<Image>().color = i == SelectedOptionId ? ActiveOptionColor : InactiveOptionColor;
        }
    }

    public void SelectOption(int optionId)
    {
        string sceneName = "";
        if (optionId == 0) sceneName = "WordGenerator";
        else if (optionId == 1) sceneName = "PeopleGenerator";
        else throw new System.Exception("Menu Option is not mapped to a scene");

        SceneManager.LoadScene(sceneName: sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
