#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZooManagementApp
{
    // ==========================================
    // 1. МОДЕЛІ ДАНИХ (ООП + Успадкування)
    // ==========================================

    // Базовий клас
    abstract class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public Animal(int id, string name, double weight)
        {
            Id = id;
            Name = name;
            Weight = weight;
        }

        // Абстрактні методи
        public abstract double CalculateFood();
        public abstract string MakeSound();

        // Віртуальний метод для запису у CSV (Базова частина)
        // Формат: Id,Type,Name,Weight
        public virtual string ToCsv()
        {
            return $"{Id},{GetType().Name},{Name},{Weight}";
        }

        // Віртуальний метод для виводу інформації
        public virtual string GetInfo()
        {
            return $"ID: {Id} | {Name}";
        }
    }

    // Лев
    class Lion : Animal
    {
        public Lion(int id, string name, double weight) : base(id, name, weight) { }

        public override double CalculateFood() => Weight * 0.05; // 5%
        public override string MakeSound() => "Рррр-арр!";

        // У лева немає додаткових унікальних полів для CSV, тому просто викликаємо базу + пусте поле
        public override string ToCsv() => base.ToCsv() + ",";
        public override string GetInfo() => base.GetInfo() + " (Лев 🦁)";
    }

    // Слон
    class Elephant : Animal
    {
        public double TrunkLength { get; set; } // Унікальне поле

        public Elephant(int id, string name, double weight, double trunkLength) : base(id, name, weight)
        {
            TrunkLength = trunkLength;
        }

        public override double CalculateFood() => Weight * 0.10;
        public override string MakeSound() => "Ту-ру-ру!";

        // Додаємо TrunkLength у CSV
        public override string ToCsv() => base.ToCsv() + $",{TrunkLength}";
        public override string GetInfo() => base.GetInfo() + $" (Слон 🐘) Хобот: {TrunkLength}м";
    }

    // Папуга
    class Parrot : Animal
    {
        public string Color { get; set; }

        public Parrot(int id, string name, double weight, string color) : base(id, name, weight)
        {
            Color = color;
        }

        public override double CalculateFood() => 0.1;
        public override string MakeSound() => "Піастри!";

        public override string ToCsv() => base.ToCsv() + $",{Color}";
        public override string GetInfo() => base.GetInfo() + $" (Папуга 🦜) Колір: {Color}";
    }

    // Жираф
    class Giraffe : Animal
    {
        public double NeckLength { get; set; }

        public Giraffe(int id, string name, double weight, double neckLength) : base(id, name, weight)
        {
            NeckLength = neckLength;
        }

        public override double CalculateFood() => Weight * 0.08;
        public override string MakeSound() => "Хрум-хрум";

        public override string ToCsv() => base.ToCsv() + $",{NeckLength}";
        public override string GetInfo() => base.GetInfo() + $" (Жираф 🦒) Шия: {NeckLength}м";
    }

    // Пінгвін
    class Penguin : Animal
    {
        public string Rank { get; set; }

        public Penguin(int id, string name, double weight, string rank) : base(id, name, weight)
        {
            Rank = rank;
        }

        public override double CalculateFood() => Weight * 0.15;
        public override string MakeSound() => "Посміхаємось і махаємо!";

        public override string ToCsv() => base.ToCsv() + $",{Rank}";
        public override string GetInfo() => base.GetInfo() + $" (Пінгвін 🐧) Звання: {Rank}";
    }

    // Модель Користувача
    class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string ToCsv() => $"{Id},{Email},{Password}";
    }

    // ==========================================
    // 2. ФАЙЛОВА БАЗА ДАНИХ (CSV Manager)
    // ==========================================
    static class FileDatabase
    {
        private static string animalsFile = "animals.csv";
        private static string usersFile = "users.csv";

        public static void Initialize()
        {
            if (!File.Exists(animalsFile))
                File.WriteAllText(animalsFile, "Id,Type,Name,Weight,ExtraData\n", Encoding.UTF8);

            if (!File.Exists(usersFile))
            {
                File.WriteAllText(usersFile, "Id,Email,Password\n", Encoding.UTF8);
                File.AppendAllText(usersFile, "1,admin,admin\n", Encoding.UTF8);
            }
        }

        public static int GenerateId(string filePath)
        {
            if (!File.Exists(filePath)) return 1;
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) return 1;

            int maxId = 0;
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (int.TryParse(parts[0], out int id) && id > maxId) maxId = id;
            }
            return maxId + 1;
        }

        // --- Читання тварин (Factory Method Pattern) ---
        public static List<Animal> LoadAnimals()
        {
            var list = new List<Animal>();
            if (!File.Exists(animalsFile)) return list;

            var lines = File.ReadAllLines(animalsFile, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                try
                {
                    int id = int.Parse(parts[0]);
                    string type = parts[1];
                    string name = parts[2];
                    double weight = double.Parse(parts[3]);
                    string extra = parts.Length > 4 ? parts[4] : "";

                    // Створення конкретного класу залежно від Типу
                    Animal animal = null;
                    switch (type)
                    {
                        case nameof(Lion):
                            animal = new Lion(id, name, weight);
                            break;
                        case nameof(Elephant):
                            double trunk = double.TryParse(extra, out double t) ? t : 0;
                            animal = new Elephant(id, name, weight, trunk);
                            break;
                        case nameof(Parrot):
                            animal = new Parrot(id, name, weight, extra);
                            break;
                        case nameof(Giraffe):
                            double neck = double.TryParse(extra, out double n) ? n : 0;
                            animal = new Giraffe(id, name, weight, neck);
                            break;
                        case nameof(Penguin):
                            animal = new Penguin(id, name, weight, extra);
                            break;
                    }

                    if (animal != null) list.Add(animal);
                }
                catch { continue; }
            }
            return list;
        }

        // --- Запис тварини ---
        public static void AddAnimal(Animal animal)
        {
            // Оновлюємо ID
            animal.Id = GenerateId(animalsFile);
            File.AppendAllText(animalsFile, animal.ToCsv() + "\n", Encoding.UTF8);
        }

        // --- Видалення ---
        public static void DeleteAnimal(int id)
        {
            var animals = LoadAnimals();
            var toDelete = animals.FirstOrDefault(a => a.Id == id);
            if (toDelete != null)
            {
                animals.Remove(toDelete);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Id,Type,Name,Weight,ExtraData");
                foreach (var a in animals) sb.AppendLine(a.ToCsv());
                File.WriteAllText(animalsFile, sb.ToString(), Encoding.UTF8);
                Console.WriteLine("Видалено!");
            }
            else Console.WriteLine("ID не знайдено.");
        }

        // --- Авторизація ---
        public static bool Register(string email, string pass)
        {
            var lines = File.ReadAllLines(usersFile);
            foreach (var l in lines.Skip(1))
                if (l.Split(',')[1] == email) return false;

            User u = new User { Id = GenerateId(usersFile), Email = email, Password = pass };
            File.AppendAllText(usersFile, u.ToCsv() + "\n", Encoding.UTF8);
            return true;
        }

        public static bool Login(string email, string pass)
        {
            var lines = File.ReadAllLines(usersFile);
            foreach (var l in lines.Skip(1))
            {
                var p = l.Split(',');
                if (p.Length >= 3 && p[1] == email && p[2] == pass) return true;
            }
            return false;
        }
    }

    // ==========================================
    // 3. ІНТЕРФЕЙС (UI)
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Cartoon Zoo + Auth (Lab 5)";
            FileDatabase.Initialize();

            // Перевірка наявності мультяшних даних при першому запуску
            SeedInitialData();

            if (!AuthMenu()) return;

            bool isRunning = true;
            while (isRunning)
            {
                ShowMenu();
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1": ShowTable(); break;
                    case "2": AddMenu(); break;
                    case "3": ListenMenu(); break;
                    case "4": DeleteMenu(); break;
                    case "5": StatisticsMenu(); break;
                    case "0": isRunning = false; break;
                    default: Console.WriteLine("???"); break;
                }
                if (isRunning) { Console.WriteLine("\n[Enter]..."); Console.ReadLine(); }
            }
        }

        static void SeedInitialData()
        {
            // Якщо база порожня (тільки шапка), додамо стартових героїв
            if (FileDatabase.GenerateId("animals.csv") == 1)
            {
                FileDatabase.AddAnimal(new Lion(0, "Алекс", 220));
                FileDatabase.AddAnimal(new Elephant(0, "Дамбо", 200, 0.5));
                FileDatabase.AddAnimal(new Parrot(0, "Яго", 0.6, "Червоний"));
                FileDatabase.AddAnimal(new Giraffe(0, "Мелман", 1200, 2.8));
                FileDatabase.AddAnimal(new Penguin(0, "Шкіпер", 15, "Капітан"));
            }
        }

        static bool AuthMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=== ВХІД / РЕЄСТРАЦІЯ ===");
                Console.ResetColor();
                Console.WriteLine("1. Вхід (admin/admin)");
                Console.WriteLine("2. Реєстрація");
                Console.WriteLine("0. Вихід");
                Console.Write("> ");
                string c = Console.ReadLine();
                if (c == "0") return false;

                Console.Write("Email: "); string e = Console.ReadLine();
                Console.Write("Pass: "); string p = Console.ReadLine();

                if (c == "1" && FileDatabase.Login(e, p)) return true;
                if (c == "2")
                {
                    if (FileDatabase.Register(e, p)) Console.WriteLine("OK! Входьте.");
                    else Console.WriteLine("Email зайнятий.");
                }
                else Console.WriteLine("Помилка.");
                Console.ReadLine();
            }
        }

        static void ShowMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== CARTOON ZOO MANAGER ===");
            Console.ResetColor();
            Console.WriteLine("1. 📋 Таблиця (Всі тварини)");
            Console.WriteLine("2. ➕ Додати тварину");
            Console.WriteLine("3. 🔊 Звуки");
            Console.WriteLine("4. ❌ Видалити");
            Console.WriteLine("5. 📊 Статистика");
            Console.WriteLine("0. 🚪 Вихід");
            Console.Write("> ");
        }

        static void ShowTable()
        {
            var animals = FileDatabase.LoadAnimals();
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("| {0,-3} | {1,-35} | {2,10} | {3,15} |", "ID", "Інфо", "Вага", "Їжа");
            Console.WriteLine("--------------------------------------------------------------------------");
            foreach (var a in animals)
            {
                Console.WriteLine("| {0,-3} | {1,-35} | {2,10} | {3,15:F2} |",
                    a.Id, a.GetInfo(), a.Weight, a.CalculateFood());
            }
        }

        static void AddMenu()
        {
            Console.WriteLine("1-Лев, 2-Слон, 3-Папуга, 4-Жираф, 5-Пінгвін");
            string type = Console.ReadLine();
            Console.Write("Ім'я: "); string name = Console.ReadLine();
            Console.Write("Вага: "); double w = double.Parse(Console.ReadLine());

            Animal a = null;
            switch (type)
            {
                case "1": a = new Lion(0, name, w); break;
                case "2":
                    Console.Write("Хобот (м): ");
                    a = new Elephant(0, name, w, double.Parse(Console.ReadLine()));
                    break;
                case "3":
                    Console.Write("Колір: ");
                    a = new Parrot(0, name, w, Console.ReadLine());
                    break;
                case "4":
                    Console.Write("Шия (м): ");
                    a = new Giraffe(0, name, w, double.Parse(Console.ReadLine()));
                    break;
                case "5":
                    Console.Write("Звання: ");
                    a = new Penguin(0, name, w, Console.ReadLine());
                    break;
            }
            if (a != null) { FileDatabase.AddAnimal(a); Console.WriteLine("Збережено!"); }
        }

        static void ListenMenu()
        {
            foreach (var a in FileDatabase.LoadAnimals())
                Console.WriteLine($"{a.Name}: {a.MakeSound()}");
        }

        static void DeleteMenu()
        {
            ShowTable();
            Console.Write("ID > ");
            if (int.TryParse(Console.ReadLine(), out int id)) FileDatabase.DeleteAnimal(id);
        }

        static void StatisticsMenu()
        {
            var list = FileDatabase.LoadAnimals();
            if (list.Count == 0) return;
            Console.WriteLine($"Всього: {list.Count}");
            Console.WriteLine($"Загальна вага: {list.Sum(x => x.Weight)} кг");
            Console.WriteLine($"Корм на день: {list.Sum(x => x.CalculateFood()):F2} кг");
        }
    }
}