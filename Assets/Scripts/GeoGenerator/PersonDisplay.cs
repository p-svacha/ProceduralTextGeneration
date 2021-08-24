using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PersonGenerator
{
    public class PersonDisplay : MonoBehaviour
    {
        public Image SexImage;
        public Image OriginImage;
        public Text NameText;
        public Text CityText;

        public void Init(Person person)
        {
            if (person.Origin == null)
            {
                OriginImage.sprite = Resources.Load<Sprite>("Icons/Flags/united-nations");
                CityText.text = "";
            }
            else
            {
                OriginImage.sprite = Resources.Load<Sprite>("Icons/Flags/" + person.Origin.Name.Replace(" ", "-").ToLower());
                CityText.text = "from " + person.City + ", " + person.Origin.Name;
            }
            if (person.Sex == "m") SexImage.sprite = Resources.Load<Sprite>("Icons/male");
            else if (person.Sex == "f") SexImage.sprite = Resources.Load<Sprite>("Icons/female");
            else SexImage.sprite = Resources.Load<Sprite>("Icons/unspecified");
            NameText.text = person.FirstName + " " + person.LastName;

            Canvas.ForceUpdateCanvases();
            GetComponent<HorizontalLayoutGroup>().enabled = false;
            GetComponent<HorizontalLayoutGroup>().enabled = true;
        }
    }
}
