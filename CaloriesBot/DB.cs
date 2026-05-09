using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static CaloriesBot.DBModels;

namespace CaloriesBot
{
    public class DB
    {

        private string connectionString = "Host=localhost;Username=postgres;Password=316316;Database=db_calories";

        public async Task<List<DBModels.User>>GetUsers()
        {

            const string sql = "SELECT * FROM users";

            var users = await new NpgsqlConnection(connectionString).QueryAsync<DBModels.User>(sql);
            return users.ToList();

        }
        public async Task<DBModels.User> GetUserByTgId(long tgId)
        {

            const string sql = "SELECT * FROM users Where tg_id = @TgId";

            var user = await new NpgsqlConnection(connectionString).QueryFirstOrDefaultAsync<DBModels.User>(sql, new {TgId = tgId});
            return user;

        }
        public async Task<long> AddUser(string name, long tgId)
        {
            DBModels.User user = new DBModels.User { Name = name , TgId = tgId};
            const string sql = """
                Insert into users(name,tg_id)
                Values('@Name',@TgId)
                RETURNING id;
                """;
            var id = await new NpgsqlConnection(connectionString).ExecuteScalarAsync<long>(sql, user);
            return id;
        }
        //public async Task<long> AddMeal(string description, string title, long userId, double proteins, double fats, double carbohydrates, double calories)
        public async Task<long> AddMeal(DBModels.Food food)
        {
            //DBModels.Food user = new DBModels.Food { Description = description, Title = title, UserId = userId, Proteins = proteins, Fats = fats, Carbohydrate = carbohydrates, Calories = calories};
            if (!food.isValidForDB()) return -1;

            const string sql = """
            Insert into meals(description,title,user_id,proteins,fats,carbohydrates,calories)
            Values(@Description,@Title,
            (SELECT id FROM users WHERE tg_id = @UserId)
            ,@Proteins,@Fats,@Carbohydrate,@Calories)
            RETURNING id;
            """;
            var id = await new NpgsqlConnection(connectionString).ExecuteScalarAsync<long>(sql, food);
            return id;
        }
        public async Task<DBModels.Food[]> GetMeals(long tgId)
        {
            //1170089312
            const string sql = """
            Select * 
            from meals 
            where user_id = (Select id from users where tg_id = @TgId);
            """;
            var meals = await new NpgsqlConnection(connectionString).QueryAsync<DBModels.Food>(sql, new { TgId = tgId });
            return meals.ToArray();
        }
        public async Task<DBModels.Food[]> GetLastMeals(long tgId, int dayAgo)
        {
            var startDate = DateTime.UtcNow.AddDays(dayAgo);
            const string sql = """
                SELECT * 
                FROM meals 
                WHERE user_id = (SELECT id FROM users WHERE tg_id = @TgId)
                  AND created_at >= @StartDate;
            """;
            var meals = await new NpgsqlConnection(connectionString).QueryAsync<DBModels.Food>(sql, new { TgId = tgId ,StartDate = startDate });
            return meals.ToArray();
        }

        //public async Task AddMeal()
        //{
        //    string sql = " Insert into meals(name,tg_id) Values('test','-1');"
        //}
    }
}
