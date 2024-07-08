using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcMovie.Controllers
{
    public class GameController : Controller
    {
        private readonly MvcMovieContext _context;

        public GameController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: /Game
        public IActionResult Index()
        {
            return View();
        }


        // GET: /Game/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Game/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserAccount user)
        {
            if (ModelState.IsValid)
            {
                // Sprawdzenie, czy użytkownik o podanym loginie już istnieje
                if (_context.UserAccounts.Any(u => u.Login == user.Login))
                {
                    ModelState.AddModelError("Login", "Login już istnieje.");
                    return View(user);
                }

                // Sprawdzenie, czy użytkownik o podanym emailu już istnieje
                if (_context.UserAccounts.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email już istnieje.");
                    return View(user);
                }

                // Dodanie nowego użytkownika
                _context.UserAccounts.Add(user);
                _context.SaveChanges();

                // Przekierowanie na stronę logowania
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /Game/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Game/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserAccount model)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);

            if (user != null)
            {
                // Logowanie pomyślne, ustawienie sesji
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("UserLogin", user.Login);
                HttpContext.Session.SetString("AccountType", user.AccountType);
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Błędny login lub hasło");
            return View(model);
        }

        // GET: /Game/Logout
        public IActionResult Logout()
        {
            // Wyczyszczenie sesji
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

         public IActionResult Menu()
        {
            // Sprawdzanie, czy użytkownik jest zalogowany
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (userLogin == null)
            {
                return RedirectToAction("Login");
            }

            // Sprawdzenie, czy `CharacterId` istnieje w sesji
            if (HttpContext.Session.GetInt32("CharacterId") != null)
            {
                // Usunięcie `CharacterId` z sesji
                HttpContext.Session.Remove("CharacterId");
            }

            // Pobieranie użytkownika na podstawie loginu
            var user = _context.UserAccounts.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Pobieranie postaci przypisanych do użytkownika
            var characters = _context.Characters.Where(c => c.UserId == user.Id).ToList();
            return View(characters);
        }

        // GET: /Game/CreateCharacter
        public IActionResult CreateCharacter()
        {
            // Sprawdzanie, czy użytkownik jest zalogowany
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (userLogin == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: /Game/CreateCharacter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCharacter(Character character, IFormFile Avatar)
        {
            var userLogin = HttpContext.Session.GetString("UserLogin");
            var user = _context.UserAccounts.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                character.UserId = user.Id;

                // Przypisanie statystyk w zależności od klasy postaci
                switch (character.Class)
                {
                    case "Warrior":
                        character.Hp = 100;
                        character.Mana = 60;
                        character.Strength = 15;
                        character.Intelligence = 5;
                        character.Dexterity = 10;
                        break;
                    case "Mage":
                        character.Hp = 60;
                        character.Mana = 100;
                        character.Strength = 5;
                        character.Intelligence = 15;
                        character.Dexterity = 10;
                        break;
                    case "Rogue":
                        character.Hp = 80;
                        character.Mana = 80;
                        character.Strength = 10;
                        character.Intelligence = 5;
                        character.Dexterity = 15;
                        break;
                    default:
                        ModelState.AddModelError("Class", "Invalid class.");
                        return View(character);
                }

                // Obsługa zapisu nowego awatara
                if (Avatar != null && Avatar.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    var uniqueFileName = $"{Guid.NewGuid()}_{Avatar.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Upewnij się, że folder istnieje
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(fileStream);
                    }

                    character.Avatar = $"/uploads/{uniqueFileName}";
                }

                // Sprawdzenie, czy nazwa postaci jest unikalna
                if (_context.Characters.Any(c => c.CharacterName == character.CharacterName))
                {
                    ModelState.AddModelError("CharacterName", "Character name already exists.");
                    return View(character);
                }

                // Dodanie nowej postaci do bazy danych
                _context.Characters.Add(character);
                await _context.SaveChangesAsync();

                return RedirectToAction("Menu");
            }

            return View(character);
        }

        // GET: /Game/EditCharacter/{id}
        [HttpGet]
        public async Task<IActionResult> EditCharacter(int characterId)
        {
            var character = await _context.Characters.FindAsync(characterId);

            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }

        // POST: /Game/EditCharacter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCharacter(Character model, IFormFile Avatar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCharacter = await _context.Characters.FindAsync(model.Id);

                    if (existingCharacter == null)
                    {
                        return NotFound();
                    }

                    if (_context.Characters.Any(c => c.CharacterName == model.CharacterName && c.Id != model.Id))
                    {
                        ModelState.AddModelError("CharacterName", "Character name already exists.");
                        return View(model);
                    }

                    existingCharacter.CharacterName = model.CharacterName;

                    if (Avatar != null && Avatar.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        var uniqueFileName = $"{Guid.NewGuid()}_{Avatar.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Avatar.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(existingCharacter.Avatar))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCharacter.Avatar.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        existingCharacter.Avatar = $"/uploads/{uniqueFileName}";
                    }

                    _context.Update(existingCharacter);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Menu");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }



        private bool CharacterExists(int id)
        {
            return _context.Characters.Any(e => e.Id == id);
        }





        // GET: /Game/ChooseAction/{characterId}
        public IActionResult ChooseAction(int characterId)
        {
            // Sprawdzanie, czy użytkownik jest zalogowany
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (userLogin == null)
            {
                return RedirectToAction("Login");
            }

            // Pobieranie użytkownika na podstawie loginu
            var user = _context.UserAccounts.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Pobieranie postaci na podstawie Id
            var character = _context.Characters.FirstOrDefault(c => c.Id == characterId && c.UserId == user.Id);
            if (character == null)
            {
                return RedirectToAction("Menu");
            }

            // Sprawdzenie, czy `CharacterId` istnieje w sesji
            if (HttpContext.Session.GetInt32("CharacterId") != null)
            {
                // Jeśli istnieje, najpierw usuń stare `CharacterId`
                HttpContext.Session.Remove("CharacterId");
            }

            // Dodanie `CharacterId` do sesji
            HttpContext.Session.SetInt32("CharacterId", characterId);

            return View(character);
        }

        // GET: Game/CharacterDetails/{characterId}
        public IActionResult CharacterDetails(int characterId)
        {
            // Pobierz postać na podstawie Id
            var character = _context.Characters.FirstOrDefault(c => c.Id == characterId);
            if (character == null)
            {
                return RedirectToAction("Menu");
            }

            return View(character);
        }




        // POST: /Game/TrainCharacter/{characterId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TrainCharacter(int characterId)
        {
            // Tu możesz dodać logikę treningu postaci
            return RedirectToAction("Menu");
        }



        // GET: Game/Mission
        public async Task<IActionResult> Mission(int characterId)
        {
            // Pobierz wszystkie misje z bazy danych
            var missions = await _context.Missions.ToListAsync();

            // Przygotuj ViewBag z id postaci, aby można było je przekazać do widoku Mission.cshtml
            ViewBag.CharacterId = characterId;

            // Przekaż listę misji do widoku Mission.cshtml
            return View(missions);
        }


        // GET: Game/MissionDetails/{missionId}
        public async Task<IActionResult> MissionDetails(int missionId)
        {
            var mission = await _context.Missions
                                        .Include(m => m.Monster) // Dołączamy potwora związanego z misją
                                        .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
            {
                return RedirectToAction("Mission");
            }

            return View(mission);
        }
        // GET: Game/Send/{characterId}/{missionId}
        public async Task<IActionResult> Send(int characterId, int missionId)
        {
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
            var mission = await _context.Missions.Include(m => m.Monster).FirstOrDefaultAsync(m => m.Id == missionId);

            if (character == null || mission == null)
            {
                return RedirectToAction("ChooseAction");
            }

            var monster = mission.Monster;
            var fightLog = new List<string>();
            var characterOriginal = new Character
            {
                CharacterName = character.CharacterName,
                Hp = character.Hp,
                Mana = character.Mana,
                Level = character.Level,
                Experience = character.Experience,
                Strength = character.Strength,
                Intelligence = character.Intelligence,
                Dexterity = character.Dexterity
            };

            // Symulacja walki
            var characterHp = character.Hp;
            var monsterHp = monster.Hp;

            while (characterHp > 0 && monsterHp > 0)
            {
                // Postać atakuje
                var characterAttack = new Random().Next(1, character.Strength);
                monsterHp -= characterAttack;
                fightLog.Add($"{character.CharacterName} attacks {monster.Name}, deals {characterAttack} damage.");

                if (monsterHp <= 0)
                {
                    // Postać wygrywa
                    var experienceReward = monster.ExperienceReward;
                    character.Experience += experienceReward;
                    fightLog.Add($"Victory! You defeated {monster.Name}. Gained {experienceReward} experience points.");

                    // Sprawdzenie poziomu
                    while (character.Experience >= character.Level * 1000)
                    {
                        character.Level++;
                        character.Hp += 10;
                        character.Mana += 10;
                        character.Strength++;
                        character.Intelligence++;
                        character.Dexterity++;
                        fightLog.Add($"Level up! New level: {character.Level}. Stats increased: Max HP +10, Max Mana +10, Strength +1, Intelligence +1, Dexterity +1");
                    }

                    break;
                }

                // Potwór atakuje
                var monsterAttack = new Random().Next(1, monster.Strength);
                characterHp -= monsterAttack;
                fightLog.Add($"{monster.Name} attacks {character.CharacterName}, deals {monsterAttack} damage.");

                if (characterHp <= 0)
                {
                    // Postać przegrywa
                    fightLog.Add($"Defeat! You were defeated by {monster.Name}.");
                }
            }

            // Aktualizacja danych postaci
            character.Hp = character.Hp; // Pełne zdrowie po walce
            character.Mana = character.Mana; // Pełna mana po walce
            _context.Characters.Update(character);
            await _context.SaveChangesAsync();

            // Przekazanie danych do widoku
            ViewBag.CharacterOriginal = characterOriginal;
            ViewBag.Character = character;
            ViewBag.FightLog = fightLog;

            return View("Fight");
        }



        public async Task<IActionResult> ViewRankings(string selectedStat, string searchString)
        {
            // Pobierz wszystkie postacie z bazy danych
            var characters = from c in _context.Characters
                             select c;

            // Wyszukiwanie postaci na podstawie podanej frazy
            if (!string.IsNullOrEmpty(searchString))
            {
                characters = characters.Where(c => c.CharacterName.Contains(searchString));
            }

            // Sortowanie postaci według wybranej statystyki
            switch (selectedStat)
            {
                case "Name":
                    characters = characters.OrderBy(c => c.CharacterName);
                    break;
                case "Level":
                    characters = characters.OrderByDescending(c => c.Level);
                    break;
                case "Strength":
                    characters = characters.OrderByDescending(c => c.Strength);
                    break;
                case "Intelligence":
                    characters = characters.OrderByDescending(c => c.Intelligence);
                    break;
                case "Dexterity":
                    characters = characters.OrderByDescending(c => c.Dexterity);
                    break;
                default:
                    characters = characters.OrderBy(c => c.CharacterName);
                    break;
            }

            // Lista dostępnych statystyk do wyboru
            var statTypes = new List<string>
        {
            "Name",
            "Level",
            "Strength",
            "Intelligence",
            "Dexterity"
        };

            ViewBag.StatTypes = new SelectList(statTypes);

            // Przekazanie postaci do widoku
            return View(await characters.ToListAsync());
        }


        public IActionResult ChooseActionFromSession()
        {
            // Sprawdzanie, czy `CharacterId` jest w sesji
            var characterId = HttpContext.Session.GetInt32("CharacterId");
            if (characterId == null)
            {
                // Brak `CharacterId` w sesji, przekierowanie do Menu
                return RedirectToAction("Menu");
            }

            // Pobieranie użytkownika na podstawie loginu
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (userLogin == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.UserAccounts.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Pobieranie postaci na podstawie Id z sesji
            var character = _context.Characters.FirstOrDefault(c => c.Id == characterId && c.UserId == user.Id);
            if (character == null)
            {
                // Brak postaci o podanym Id, przekierowanie do Menu
                return RedirectToAction("Menu");
            }

            return View("ChooseAction", character); // Przekierowanie do widoku `ChooseAction`
        }

        // GET: /Game/MissionFromSession
        public async Task<IActionResult> MissionFromSession()
        {
            // Sprawdzanie, czy `CharacterId` jest w sesji
            var characterId = HttpContext.Session.GetInt32("CharacterId");
            if (characterId == null)
            {
                // Brak `CharacterId` w sesji, przekierowanie do Menu
                return RedirectToAction("Menu");
            }

            // Pobieranie postaci na podstawie Id z sesji
            var character = await _context.Characters.FindAsync(characterId);
            if (character == null)
            {
                // Brak postaci o podanym Id, przekierowanie do Menu
                return RedirectToAction("Menu");
            }

            // Pobierz wszystkie misje z bazy danych
            var missions = await _context.Missions.ToListAsync();

            // Przekazanie postaci do widoku poprzez ViewBag
            ViewBag.characterId = HttpContext.Session.GetInt32("CharacterId"); ;

            // Przekazanie listy misji do widoku
            return View("Mission", missions); // Przekierowanie do widoku `Mission`
        }




    }
}
