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

namespace CalloriesBot
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

        //public async Task AddMeal()
        //{
        //    string sql = " Insert into meals(name,tg_id) Values('test','-1');"
        //}
    }
}
