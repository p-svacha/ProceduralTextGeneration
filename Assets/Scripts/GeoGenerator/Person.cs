using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonGenerator
{
    public class Person
    {
        public string FirstName;
        public string LastName;
        public Country Origin;
        public string Sex;
        public string City;

        public Person(string firstName, string lastName, Country origin, string sex, string city)
        {
            FirstName = firstName;
            LastName = lastName;
            Origin = origin;
            Sex = sex;
            City = city;
        }
    }
}
