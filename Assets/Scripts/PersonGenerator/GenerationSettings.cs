using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonGenerator
{
    public class GenerationSettings
    {
        public int NumNames;
        public List<string> Origins;
        public List<string> Sex;
        public FilterType CountryFilterType;
        public FilterType SexFilterType;

        public GenerationSettings(int numNames, List<string> origins, List<string> sex, FilterType countryFilterType, FilterType sexFilterType)
        {
            NumNames = numNames;
            Origins = origins;
            Sex = sex;
            CountryFilterType = countryFilterType;
            SexFilterType = sexFilterType;
        }
    }

    public enum FilterType
    {
        Specific,
        Any
    }

}
