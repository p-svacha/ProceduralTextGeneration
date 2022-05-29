using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonGenerator
{
    public class GenerationSettings
    {
        public int NumNames;
        public List<Country> Origins;
        public List<string> Sex;
        public OriginType OriginFilterType;
        public SexType SexFilterType;

        public int MaxLengthFirstName;
        public int MaxLengthLastName;
        public int MaxLengthStreet;
        public int MaxLengthCity;

        public GenerationSettings(int numNames, List<Country> origins, List<string> sex, OriginType countryFilterType, SexType sexFilterType, int maxLengthFirstName, int maxLengthLastName, int maxLengthStreet, int maxLengthCity)
        {
            NumNames = numNames;
            Origins = origins;
            Sex = sex;
            OriginFilterType = countryFilterType;
            SexFilterType = sexFilterType;

            MaxLengthFirstName = maxLengthFirstName;
            MaxLengthLastName = maxLengthLastName;
            MaxLengthStreet = maxLengthStreet;
            MaxLengthCity = maxLengthCity;
        }
    }

    public enum OriginType
    {
        Mixed,
        Specific,
        Unspecified
    }

    public enum SexType
    {
        Mixed,
        Male,
        Female,
        Unspecified
    }

}
