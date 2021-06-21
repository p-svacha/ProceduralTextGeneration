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

        public void Init(Person person)
        {
            OriginImage.sprite = Resources.Load<Sprite>("Icons/Flags/" + person.Origin.Replace(" ", "-").ToLower());
            if (person.Sex == "m") SexImage.sprite = Resources.Load<Sprite>("Icons/male");
            else SexImage.sprite = Resources.Load<Sprite>("Icons/female");
            NameText.text = person.FirstName + " " + person.LastName;
        }
    }
}
