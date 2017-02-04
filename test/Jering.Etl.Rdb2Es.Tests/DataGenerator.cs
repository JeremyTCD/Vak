using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jering.Etl.Rdb2Es.Tests
{
    public class DataGenerator
    {
        private static string _dataPath = $"{Environment.GetEnvironmentVariable("TMP")}\\VakDummyData.txt";
        private static Random _random = new Random();

        public static string GetData()
        {
            try { 
                return File.ReadAllText(_dataPath);
            }
            catch
            {
                return GenerateData();
            }
        }

        private static string GenerateData()
        {
            List<VakUnit> vakUnits = new List<VakUnit>();
            int numAdjectives = _adjectives.Length;
            int numNouns = _nouns.Length;

            for (int i = 0; i < numAdjectives; i++)
            {
                for(int j = 0; j < numNouns; j++)
                {
                    vakUnits.Add(new VakUnit
                    {
                        VakUnitId = i * numAdjectives + j + 1,
                        Name = _adjectives[i] + " " + _nouns[j],
                        RowVersion = 1,
                        Tags = new string[] { _adjectives[_random.Next(0, numAdjectives/2)],
                            _adjectives[_random.Next(numAdjectives/2 + 1, numAdjectives - 1)]}
                    });
                }
            }

            // Generate VakUnit list
            string output = JsonConvert.SerializeObject(vakUnits);
            File.WriteAllText(_dataPath, output);

            return output;
        }

        private static readonly string[] _adjectives = new string[] {
            "beautiful",
            "adorable",
            "clean",
            "elegant",
            "fancy",
            "magnificent",
            "plain",
            "quaint",
            "sparkling",
            "modern",
            "old-fashioned",
            "fun",
            "sharp",
            "professional",
            "honest",
            "sleek",
            "classy"};

        private static readonly string[] _nouns = new string[] {
            "button",
            "decoration",
            "loader",
            "menu",
            "icon",
            "cow",
            "monster",
            "headphones",
            "furniture",
            "toys",
            "clock",
            "computer",
            "time machine",
            "lamp",
            "tree",
            "flower",
            "bee"};
    }
}
