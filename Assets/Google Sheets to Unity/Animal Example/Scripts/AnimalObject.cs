using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalObject : MonoBehaviour
{
    public Animal animal;


    public Text nameTxt, healthTxt, attackTxt, defenceTxt;

	// Use this for initialization
	void Start ()
    {
        BuildAnimalInfo();	
	}

    public void BuildAnimalInfo()
    {
        nameTxt.text = animal.name;
        healthTxt.text = "Health" + animal.health;
        attackTxt.text = "Attack" + animal.attack;
        defenceTxt.text = "Defence" + animal.defence;
    }
}
