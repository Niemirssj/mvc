using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using System;
using System.Linq;

namespace MvcMovie.Models
{
    public static class SeedData2
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MvcMovieContext(
                serviceProvider.GetRequiredService<DbContextOptions<MvcMovieContext>>()))
            {
                // Sprawdź, czy istnieją jakiekolwiek rekordy w tabelach.
                if (context.UserAccounts.Any() || context.Characters.Any() ||
                    context.Missions.Any() || context.Monsters.Any() || context.Trainings.Any())
                {
                    return; // Baza danych została już zainicjalizowana
                }

                // Dodawanie danych do tabeli UserAccount
                var users = new UserAccount[]
                {
                    new UserAccount { Login = "user1", Email = "user1@example.com", Password = "Password123" },
                    new UserAccount { Login = "user2", Email = "user2@example.com", Password = "Password456" },
                      new UserAccount { Login = "admin", Email = "admin", Password = "admin", AccountType="admin" }
                };
                context.UserAccounts.AddRange(users);
                context.SaveChanges();

                // Dodawanie danych do tabeli Character
                var characters = new Character[]
                {
                    new Character { UserId = users[0].Id, CharacterName = "Warrior", Class = "Warrior", Hp = 100, Mana = 50, Strength = 10, Intelligence = 5, Dexterity = 8, Experience = 0, Level = 1 },
                    new Character { UserId = users[1].Id, CharacterName = "Mage", Class = "Mage", Hp = 60, Mana = 120, Strength = 4, Intelligence = 15, Dexterity = 6, Experience = 0, Level = 1 }
                };
                context.Characters.AddRange(characters);
                context.SaveChanges();

                // Dodawanie danych do tabeli Monster
                var monsters = new Monster[]
                {
                    new Monster { Name = "Goblin", Hp = 30, Mana = 0, Strength = 5, Intelligence = 2, Dexterity = 4, ExperienceReward = 10 },
                    new Monster { Name = "Dragon", Hp = 300, Mana = 100, Strength = 20, Intelligence = 10, Dexterity = 8, ExperienceReward = 100 }
                };
                context.Monsters.AddRange(monsters);
                context.SaveChanges();

                // Dodawanie danych do tabeli Mission
                var missions = new Mission[]
                {
                    new Mission { MissionName = "Find the Lost Sword", Description = "Retrieve the lost sword from the dungeon.", MonsterId = monsters[0].Id },
                    new Mission { MissionName = "Defeat the Dragon", Description = "Defeat the dragon terrorizing the village.", MonsterId = monsters[1].Id }
                };
                context.Missions.AddRange(missions);
                context.SaveChanges();

                // Dodawanie danych do tabeli Training
                var trainingSessions = new Training[]
                {
                    new Training { CharacterId = characters[0].Id, CompletionDate = DateTime.Now.AddDays(1), StatName = "Strength", StatPoints = 2, IsCompleted = false },
                    new Training { CharacterId = characters[1].Id, CompletionDate = DateTime.Now.AddDays(2), StatName = "Intelligence", StatPoints = 3, IsCompleted = false }
                };
                context.Trainings.AddRange(trainingSessions);
                context.SaveChanges();
            }
        }
    }
}
