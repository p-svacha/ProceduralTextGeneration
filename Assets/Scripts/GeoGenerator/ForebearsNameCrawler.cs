using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace PersonGenerator
{
    /// <summary>
    /// The point of this script is that it crawls through all countries and checks the most common forenames and surnames for that country and saves them into a file
    /// The target webistes are
    /// https://forebears.io/xyz/forenames
    /// https://forebears.io/xyz/surnames
    /// whereas xyz is the country
    /// </summary>
    public class ForebearsNameCrawler : MonoBehaviour
    {
        private List<string> Regions = new List<string>();

        private Dictionary<string, List<string>> RegionForenames = new Dictionary<string, List<string>>();

        private Dictionary<string, string> NameSex = new Dictionary<string, string>();
        private Dictionary<string, List<string>> ForenamesToCheck = new Dictionary<string, List<string>>();

        void Start()
        {
            Crawl();
        }

        private void Crawl()
        {
            ReadRegionList();

            // Get and save surnames
            foreach(string region in Regions)
            {
                List<string> surNames = CrawlNames(region, NameType.Surname, 5000);
                SaveSurnames(surNames, "Assets/Resources/Geographical/surnames", region + "_surnames");

                Thread.Sleep(Random.Range(300, 500));
            }

            // Get forenames
            foreach(string region in Regions)
            {
                RegionForenames.Add(region, CrawlNames(region, NameType.Forename, 10000));

                Thread.Sleep(Random.Range(300, 500));
            }

            // Check forenames without sex
            foreach(KeyValuePair<string, List<string>> kvp in ForenamesToCheck)
            {
                string region = kvp.Key;
                List<string> names = kvp.Value;
                foreach(string name in names)
                {
                    string sex = "";
                    NameSex.TryGetValue(name, out sex);
                    //Debug.Log("Checking sex for " + name + ": " + sex);
                    if(sex != null)
                    {
                        if (!RegionForenames.ContainsKey(region)) RegionForenames.Add(region, new List<string>() { name + "," + sex });
                        else RegionForenames[region].Add(name + "," + sex);
                    }
                }
            }
            
            // Save forenames
            foreach(string region in Regions)
            {
                Debug.Log("Saving " + RegionForenames[region].Count + " forenames for " + region);
                SaveForenames(RegionForenames[region], "Assets/Resources/Geographical/forenames", region + "_forenames");
            }
        }

        private void ReadRegionList()
        {
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader("Assets/Resources/Geographical/regionlist.txt");
            while ((line = file.ReadLine()) != null)
            {
                Regions.Add(line.ToLower().Replace(" ", "-").Replace(",", ""));
            }

            file.Close();
        }

        private List<string> CrawlNames(string regionString, NameType nameType, int maxAmount)
        {
            List<string> names = new List<string>();

            
            string nameString = nameType == NameType.Forename ? "forenames" : "surnames";

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string url = "https://forebears.io/" + regionString + "/" + nameString;
                //var htmlData = client.DownloadData("https://forebears.io/" + regionString + "/" + nameString);
                string htmlCode = Encoding.UTF8.GetString(client.DownloadData(url));
                //Debug.Log(htmlCode);
                int bodyStart = htmlCode.IndexOf("<tbody>");
                int bodyEnd = htmlCode.IndexOf("</tbody");
                if(bodyStart < 0)
                {
                    Debug.Log("No " + nameType.ToString() + " found for " + regionString);
                    return names;
                }
                string websiteBody = htmlCode.Substring(bodyStart, bodyEnd - bodyStart);

                string[] nameTds = websiteBody.Split(new string[] { "<tr>" }, System.StringSplitOptions.None);
                int counter = -1;
                foreach (string nameTd in nameTds)
                {
                    counter++;
                    if (counter > 0)
                    {
                        string name = "";
                        if (nameType == NameType.Forename)
                        {
                            string sex = "";
                            bool checkSexLater = false;
                            string[] split = nameTd.Split(new string[] { "class=\"" }, System.StringSplitOptions.None);
                            if (split.Length == 2) // No data for sex => check later
                            {
                                name = split[1];
                                checkSexLater = true;
                            }
                            else if (split.Length == 3) // Sex is clear 100%
                            {
                                name = split[2];
                                sex = split[1][0].ToString();
                            }
                            else if (split.Length == 4) // Sex is mixed, take that with more %
                            {
                                name = split[3];
                                string mPerc = split[1];
                                int mStartIndex = mPerc.IndexOf("width:") + 6;
                                int mEndIndex = mPerc.IndexOf("px");
                                mPerc = mPerc.Substring(mStartIndex, mEndIndex - mStartIndex);
                                int m = int.Parse(mPerc);
                                if (m > 50) sex = "m";
                                else sex = "f";
                            }
                            else Debug.LogWarning("Weird formatting in " + nameTd);

                            int nameEndIndex = name.IndexOf("</a>");
                            name = name.Substring(0, name.Length - (name.Length - nameEndIndex));
                            int nameStartIndex = name.LastIndexOf(">") + 1;
                            name = name.Substring(nameStartIndex, name.Length - nameStartIndex);

                            name = FixNameEncoding(name);

                            if (checkSexLater)
                            {
                                if (ForenamesToCheck.ContainsKey(regionString)) ForenamesToCheck[regionString].Add(name);
                                else ForenamesToCheck.Add(regionString, new List<string>() { name });
                                continue;
                            }
                            else
                            {
                                if (sex == "m" && !NameSex.ContainsKey(name)) NameSex.Add(name, "m");
                                if (sex == "f" && !NameSex.ContainsKey(name)) NameSex.Add(name, "f");
                            }
                            name += ("," + sex);
                        }
                        else
                        {
                            name = nameTd;
                            int nameEndIndex = name.IndexOf("</a>");
                            
                            name = name.Substring(0, name.Length - (name.Length - nameEndIndex));
                            int nameStartIndex = name.LastIndexOf(">") + 1;
                            name = name.Substring(nameStartIndex, name.Length - nameStartIndex);
                            name = FixNameEncoding(name);
                        }

                        

                        names.Add(name);
                    }
                    if (counter == maxAmount) break;
                }
            }

            if (nameType == NameType.Forename)
            {
                int numCheckLater = 0;
                if (ForenamesToCheck.ContainsKey(regionString)) numCheckLater = ForenamesToCheck[regionString].Count;
                Debug.Log("Returning " + names.Count + " " + nameType.ToString() + " for " + regionString + " (" + numCheckLater + " are missing sex could be added later)");
            }
            else
            {
                Debug.Log("Returning " + names.Count + " " + nameType.ToString() + " for " + regionString);
            }

            return names;
        }

        private void SaveForenames(List<string> names, string path, string fileName)
        {
            string fullPathM = path + "/" + fileName + "_male.txt";
            string fullPathF = path + "/" + fileName + "_female.txt";

            TextWriter twm = new StreamWriter(fullPathM);
            TextWriter twf = new StreamWriter(fullPathF);
            foreach(string s in names)
            {
                char sex = s[s.Length - 1];
                string name = s.Substring(0, s.Length - 2);
                if (sex == 'm')
                {
                    twm.WriteLine(name);
                }
                else if (sex == 'f')
                {
                    twf.WriteLine(name);
                }
                else Debug.LogWarning("Sex not found for name " + s);
            }
            twm.Close();
            twf.Close();
        }

        private void SaveSurnames(List<string> names, string path, string fileName)
        {
            string fullPath = path + "/" + fileName + ".txt";

            TextWriter tw = new StreamWriter(fullPath);

            foreach (string s in names) tw.WriteLine(s);

            tw.Close();
        }

        private string FixNameEncoding(string name)
        {
            name = name.Replace("&#xC0;", "À");
            name = name.Replace("&#xC1;", "Á");
            name = name.Replace("&#xC2;", "Â");
            name = name.Replace("&#xC3;", "Ã");
            name = name.Replace("&#xC4;", "Ä");
            name = name.Replace("&#xC5;", "Å");
            name = name.Replace("&#xC6;", "Æ");
            name = name.Replace("&#xC7;", "Ç");
            name = name.Replace("&#xC8;", "È");
            name = name.Replace("&#xC9;", "É");
            name = name.Replace("&#xCA;", "Ê");
            name = name.Replace("&#xCB;", "Ë");
            name = name.Replace("&#xCC;", "Ì");
            name = name.Replace("&#xCD;", "Í");
            name = name.Replace("&#xCE;", "Î");
            name = name.Replace("&#xCF;", "Ï");

            name = name.Replace("&#xD0;", "Ð");
            name = name.Replace("&#xD1;", "Ñ");
            name = name.Replace("&#xD2;", "Ò");
            name = name.Replace("&#xD3;", "Ó");
            name = name.Replace("&#xD4;", "Ô");
            name = name.Replace("&#xD5;", "Õ");
            name = name.Replace("&#xD6;", "Ö");
            name = name.Replace("&#xD8;", "Ø");
            name = name.Replace("&#xD9;", "Ù");
            name = name.Replace("&#xDA;", "Ú");
            name = name.Replace("&#xDB;", "Û");
            name = name.Replace("&#xDC;", "Ü");
            name = name.Replace("&#xDD;", "Ý");
            name = name.Replace("&#xDE;", "Þ");
            name = name.Replace("&#xDF;", "ß");

            name = name.Replace("&#xE0;", "à");
            name = name.Replace("&#xE1;", "á");
            name = name.Replace("&#xE2;", "â");
            name = name.Replace("&#xE3;", "ã");
            name = name.Replace("&#xE4;", "ä");
            name = name.Replace("&#xE5;", "å");
            name = name.Replace("&#xE6;", "æ");
            name = name.Replace("&#xE7;", "ç");
            name = name.Replace("&#xE8;", "è");
            name = name.Replace("&#xE9;", "é");
            name = name.Replace("&#xEA;", "ê");
            name = name.Replace("&#xEB;", "ë");
            name = name.Replace("&#xEC;", "ì");
            name = name.Replace("&#xED;", "í");
            name = name.Replace("&#xEE;", "î");
            name = name.Replace("&#xEF;", "ï");

            name = name.Replace("&#xF0;", "ð");
            name = name.Replace("&#xF1;", "ñ");
            name = name.Replace("&#xF2;", "ò");
            name = name.Replace("&#xF3;", "ó");
            name = name.Replace("&#xF4;", "ô");
            name = name.Replace("&#xF5;", "õ");
            name = name.Replace("&#xF6;", "ö");
            name = name.Replace("&#xF8;", "ø");
            name = name.Replace("&#xF9;", "ù");
            name = name.Replace("&#xFA;", "ú");
            name = name.Replace("&#xFB;", "û");
            name = name.Replace("&#xFC;", "ü");
            name = name.Replace("&#xFD;", "ý");
            name = name.Replace("&#xFE;", "þ");
            name = name.Replace("&#xFF;", "ÿ");

            name = name.Replace("&#xDC;", "Ü");

            name = name.Replace("&#x100;", "Ā");
            name = name.Replace("&#x101;", "ā");
            name = name.Replace("&#x102;", "Ă");
            name = name.Replace("&#x103;", "ă");
            name = name.Replace("&#x104;", "Ą");
            name = name.Replace("&#x105;", "ą");
            name = name.Replace("&#x106;", "Ć");
            name = name.Replace("&#x107;", "ć");
            name = name.Replace("&#x10C;", "Č");
            name = name.Replace("&#x10D;", "č");
            name = name.Replace("&#x10E;", "Ď");
            name = name.Replace("&#x10F;", "ď");

            name = name.Replace("&#x11E;", "Ğ");
            name = name.Replace("&#x11F;", "ğ");
            name = name.Replace("&#x110;", "Đ");
            name = name.Replace("&#x111;", "đ");
            name = name.Replace("&#x116;", "Ė");
            name = name.Replace("&#x117;", "ė");
            name = name.Replace("&#x118;", "Ę");
            name = name.Replace("&#x119;", "ę");
            name = name.Replace("&#x11A;", "Ě");
            name = name.Replace("&#x11B;", "ě");

            name = name.Replace("&#x12A;", "Ī");
            name = name.Replace("&#x12B;", "ī");

            name = name.Replace("&#x130;", "İ");
            name = name.Replace("&#x131;", "ı");
            name = name.Replace("&#x138;", "ĸ");
            name = name.Replace("&#x139;", "Ĺ");
            name = name.Replace("&#x13A;", "ĺ");
            name = name.Replace("&#x13B;", "Ļ");
            name = name.Replace("&#x13C;", "ļ");
            name = name.Replace("&#x13D;", "Ľ");
            name = name.Replace("&#x13E;", "ľ");
            name = name.Replace("&#x13F;", "Ŀ");

            name = name.Replace("&#x141;", "Ł");
            name = name.Replace("&#x142;", "ł");
            name = name.Replace("&#x143;", "Ń");
            name = name.Replace("&#x144;", "ń");
            name = name.Replace("&#x147;", "Ň");
            name = name.Replace("&#x148;", "ň");

            name = name.Replace("&#x150;", "Ő");
            name = name.Replace("&#x151;", "ő");
            name = name.Replace("&#x152;", "Œ");
            name = name.Replace("&#x153;", "œ");
            name = name.Replace("&#x154;", "Ŕ");
            name = name.Replace("&#x155;", "ŕ");
            name = name.Replace("&#x158;", "Ř");
            name = name.Replace("&#x159;", "ř");
            name = name.Replace("&#x15A;", "Ś");
            name = name.Replace("&#x15B;", "ś");
            name = name.Replace("&#x15E;", "Ş");
            name = name.Replace("&#x15F;", "ş");

            name = name.Replace("&#x160;", "Š");
            name = name.Replace("&#x161;", "š");
            name = name.Replace("&#x162;", "Ţ");
            name = name.Replace("&#x163;", "ţ");
            name = name.Replace("&#x164;", "Ť");
            name = name.Replace("&#x165;", "ť");
            name = name.Replace("&#x16A;", "Ū");
            name = name.Replace("&#x16B;", "ū");
            name = name.Replace("&#x16E;", "Ů");
            name = name.Replace("&#x16F;", "ů");

            name = name.Replace("&#x170;", "Ű");
            name = name.Replace("&#x171;", "ű");
            name = name.Replace("&#x179;", "Ź");
            name = name.Replace("&#x17A;", "ź");
            name = name.Replace("&#x17B;", "Ż");
            name = name.Replace("&#x17C;", "ż");
            name = name.Replace("&#x17D;", "Ž");
            name = name.Replace("&#x17E;", "ž");


            name = name.Replace("&#x191;", "Ƒ");
            name = name.Replace("&#x192;", "ƒ");

            name = name.Replace("&#x218;", "Ș");
            name = name.Replace("&#x219;", "ș");
            name = name.Replace("&#x21A;", "Ț");
            name = name.Replace("&#x21B;", "ț");

            if (name.Contains("&")) Debug.LogWarning("Encoding problem with name " + name);

            return name;
        }
    }

    public enum NameType
    {
        Forename,
        Surname
    }
}
