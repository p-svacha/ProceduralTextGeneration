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

        public GenerationSettings(int numNames, List<Country> origins, List<string> sex, OriginType countryFilterType, SexType sexFilterType)
        {
            NumNames = numNames;
            Origins = origins;
            Sex = sex;
            OriginFilterType = countryFilterType;
            SexFilterType = sexFilterType;
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
