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
        public string Street;

        public Person(string firstName, string lastName, Country origin, string sex, string city, string street)
        {
            FirstName = firstName;
            LastName = lastName;
            Origin = origin;
            Sex = sex;
            City = city;
            Street = street;
        }
    }
}
