using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesBot
{
    static public class DBModels
    {
        public class User
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public long TgId{ get; set; }
        }
        public class Food
        {
            public long Id { get; set; }
            public DateTimeOffset Date { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty ;
            public long UserId { get; set; }
            public double Proteins { get; set; }
            public double Fats { get; set; }
            public double Carbohydrate { get; set; }
            public double Calories { get; set; }
            public bool isValidForDB()
            {
                if (UserId == 0) return false;
                return isValid();
            }
            public bool isValid()
            {
                if (string.Empty.Equals(Description)) return false;
                if (string.Empty.Equals(Title)) return false;
                //if (UserId == 0) return false;
                if (Proteins < 0) return false;
                if (Fats  < 0) return false;
                if (Carbohydrate < 0) return false;
                if (Calories < 0) return false;
                return true;
            }
            public override string ToString()
            {
                return $"Дата:{Date}\n" +
                    $"{Title}\n" +
                    $"{Description}\n" +
                    $"Калорийность: {Calories}\n" +
                    $"Белки:{Proteins}" +
                    $"Жиры:{Fats}" +
                    $"Углеводы:{Carbohydrate}";
            }

        }
    }
}
