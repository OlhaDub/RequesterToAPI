using Npgsql;
using System.Text.Json;

namespace Requester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            do
            {
                Console.WriteLine("1-check name\t2-show DB\t0-end");
                Console.Write("Choice: ");
                string choice = Console.ReadLine().Trim();
                switch (choice)
                {
                    case "1":
                        Console.Write("Name: ");
                        string name = Console.ReadLine().Trim();
                        Person person = JsonSerializer.Deserialize<Person>(GetData(name));
                        Console.WriteLine($"Result\t {person}");
                        AddToDB(person);
                        break;
                    case "2":
                        ShowDB();
                        break;
                    default: return;
                }
                Console.WriteLine(new string('-', 25)+Environment.NewLine);
            } while (true);

        }
        static string GetData(string name) 
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync("https://api.agify.io/?name=" + name).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;

                //Console.WriteLine(responseBody);
                return responseBody;
            }
        }

        public class Person
        {
            public string name { get; set; }
            public int age { get; set; }

            public override string ToString()
            {
                return $"Name: {name}, Age: {age}";
            }
        }
        static void AddToDB(Person person)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=256914589;Database=agify";
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "INSERT INTO ageByName (name, age) VALUES (@name, @age)";

                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("name", person.name);
                    command.Parameters.AddWithValue("age", person.age);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        static void ShowDB()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=256914589;Database=agify";
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM agebyname", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Name\t\tAge");
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["name"],-15}\t{reader["age"],3}");
                        }
                    }
                }
            }
        }
    }
}