using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonGenerator
{
    public class Person
    {
        public string FirstName;
        public string LastName;
        public string Origin;
        public string Sex;

        public Person(string firstName, string lastName, string origin, string sex)
        {
            FirstName = firstName;
            LastName = lastName;
            Origin = origin;
            Sex = sex;
        }
    }
}
