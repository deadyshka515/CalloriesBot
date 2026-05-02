using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CaloriesBot
{
    static internal class Parsing
    {
        public static bool ParseFood(string text, out DBModels.Food food)
        {
            food = new DBModels.Food();
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                if (line.StartsWith("Продукт:"))
                    food.Title = line.Substring(8).Trim();
                if (line.StartsWith("Белки:"))
                    food.Proteins = ExtractDouble(line);
                else if (line.StartsWith("Жиры:"))
                    food.Fats = ExtractDouble(line);
                else if (line.StartsWith("Углеводы:"))
                    food.Carbohydrate = ExtractDouble(line);
                else if (line.StartsWith("Калорийность:"))
                    food.Calories = ExtractDouble(line);
                else if (line.StartsWith("Продукт:"))
                    food.Description = line.Substring(9).Trim();
            }
            return food.isValid();

        }
        //private static int ExtractInt(string line)
        //{
        //    return int.TryParse(string.Join("", line.Where(char.IsDigit)), out int value) ? value : 0;
        //}

        private static double ExtractDouble(string line)
        {
            var match = Regex.Match(line, @"-?[\d]+(?:[.,][\d]+)?");
            if (!match.Success) return 0;

            string normalized = match.Value.Replace(',', '.');
            return double.TryParse(normalized, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out double result) ? result : 0;
        }
    }
}