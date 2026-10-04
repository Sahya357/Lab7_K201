using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ApartmentLab7
{
    // Власний тип виключення
    public class InvalidApartmentException : Exception
    {
        public InvalidApartmentException(string message) : base(message)
        {
        }
    }

    // ===== Клас Квартира (5 полів) =====
    // А) IComparable: порівняння квартир за ціною в CompareTo()
    public class Apartment : IComparable
    {
        private string address;
        private double price;
        private double area;
        private int rooms;
        private int floor;

        public Apartment(string address, double price, double area,
                         int rooms, int floor)
        {
            Address = address;
            Price = price;
            Area = area;
            Rooms = rooms;
            Floor = floor;
        }

        public string Address
        {
            get { return address; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidApartmentException("Адреса не може бути порожньою.");
                address = value;
            }
        }

        public double Price
        {
            get { return price; }
            set
            {
                if (value <= 0)
                    throw new InvalidApartmentException("Ціна повинна бути більшою за 0.");
                price = value;
            }
        }

        public double Area
        {
            get { return area; }
            set
            {
                if (value <= 0)
                    throw new InvalidApartmentException("Площа повинна бути більшою за 0.");
                area = value;
            }
        }

        public int Rooms
        {
            get { return rooms; }
            set
            {
                if (value <= 0)
                    throw new InvalidApartmentException("Кількість кімнат повинна бути більшою за 0.");
                rooms = value;
            }
        }

        public int Floor
        {
            get { return floor; }
            set
            {
                if (value <= 0)
                    throw new InvalidApartmentException("Поверх повинен бути більшим за 0.");
                floor = value;
            }
        }

        // А) Порівняння за ціною
        public int CompareTo(object obj)
        {
            Apartment other = obj as Apartment;

            if (other == null)
                throw new ArgumentException(
                    "Порівнюваний об'єкт не належить до класу Apartment");

            return Price.CompareTo(other.Price);
        }

        public static bool operator <(Apartment a1, Apartment a2)
        {
            return a1.CompareTo(a2) < 0;
        }

        public static bool operator >(Apartment a1, Apartment a2)
        {
            return a1.CompareTo(a2) > 0;
        }

        public static bool operator <=(Apartment a1, Apartment a2)
        {
            return a1.CompareTo(a2) <= 0;
        }

        public static bool operator >=(Apartment a1, Apartment a2)
        {
            return a1.CompareTo(a2) >= 0;
        }

        public override string ToString()
        {
            return string.Format("{0,-24} {1,12:F2} {2,9:F1} {3,8} {4,8}",
                Address, Price, Area, Rooms, Floor);
        }

        public static string Header()
        {
            return string.Format("{0,-24} {1,12} {2,9} {3,8} {4,8}",
                "Адреса", "Ціна, $", "Площа", "Кімнат", "Поверх")
                + Environment.NewLine + new string('-', 65);
        }

        // ===== Б) Інтерфейс IComparer: допоміжні класи =====

        // За ціною (при рівних цінах - за кількістю кімнат)
        public class SortByPrice : IComparer
        {
            int IComparer.Compare(object ob1, object ob2)
            {
                Apartment a1 = (Apartment)ob1;
                Apartment a2 = (Apartment)ob2;

                if (a1.Price > a2.Price) return 1;
                if (a1.Price < a2.Price) return -1;
                return a1.Rooms.CompareTo(a2.Rooms);
            }
        }

        // За кількістю кімнат (при рівній кількості - за ціною)
        public class SortByRooms : IComparer
        {
            int IComparer.Compare(object ob1, object ob2)
            {
                Apartment a1 = (Apartment)ob1;
                Apartment a2 = (Apartment)ob2;

                if (a1.Rooms > a2.Rooms) return 1;
                if (a1.Rooms < a2.Rooms) return -1;
                return a1.Price.CompareTo(a2.Price);
            }
        }

        // За ціною, а при рівних цінах - за площею (для пункту В)
        public class SortByPriceThenArea : IComparer
        {
            int IComparer.Compare(object ob1, object ob2)
            {
                Apartment a1 = (Apartment)ob1;
                Apartment a2 = (Apartment)ob2;

                if (a1.Price > a2.Price) return 1;
                if (a1.Price < a2.Price) return -1;
                return a1.Area.CompareTo(a2.Area);
            }
        }
    }

    // ===== В) Інтерфейс IEnumerable =====
    public class Apartments : IEnumerable
    {
        private Apartment[] container;

        public Apartments(Apartment[] container)
        {
            if (container == null)
                throw new InvalidApartmentException("Масив квартир не заданий.");
            this.container = container;
        }

        public int Count
        {
            get { return container.Length; }
        }

        public Apartment[] ToArray()
        {
            return container;
        }

        // Додавання квартири (масив збільшується на 1)
        public void Add(Apartment apartment)
        {
            Apartment[] newArray = new Apartment[container.Length + 1];
            Array.Copy(container, newArray, container.Length);
            newArray[newArray.Length - 1] = apartment;
            container = newArray;
        }

        // Видалення квартири за індексом (масив зменшується на 1)
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= container.Length)
                throw new InvalidApartmentException("Такої квартири немає.");

            Apartment[] newArray = new Apartment[container.Length - 1];
            Array.Copy(container, 0, newArray, 0, index);
            Array.Copy(container, index + 1, newArray, index,
                       container.Length - index - 1);
            container = newArray;
        }

        // Запис усіх квартир у файл (по одній на рядок, поля через ';')
        public void SaveToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                foreach (Apartment a in container)
                {
                    writer.WriteLine(string.Join(";",
                        a.Address,
                        a.Price.ToString(CultureInfo.InvariantCulture),
                        a.Area.ToString(CultureInfo.InvariantCulture),
                        a.Rooms,
                        a.Floor));
                }
            }
        }

        // Читання квартир з файлу. Некоректні рядки пропускаються.
        public static Apartments LoadFromFile(string fileName)
        {
            List<Apartment> list = new List<Apartment>();

            foreach (string line in File.ReadAllLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] p = line.Split(';');
                if (p.Length != 5)
                    continue;

                try
                {
                    list.Add(new Apartment(
                        p[0],
                        double.Parse(p[1], CultureInfo.InvariantCulture),
                        double.Parse(p[2], CultureInfo.InvariantCulture),
                        int.Parse(p[3]),
                        int.Parse(p[4])));
                }
                catch (FormatException) { }
                catch (OverflowException) { }
                catch (InvalidApartmentException) { }
            }

            return new Apartments(list.ToArray());
        }

        // Реалізація IEnumerable - ітератор (yield)
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < container.Length; i++)
                yield return container[i];
        }

        // Ітератор: квартири, впорядковані за ціною, потім за площею
        public IEnumerable OrderedByPriceThenArea()
        {
            Apartment[] copy = (Apartment[])container.Clone();
            Array.Sort(copy, new Apartment.SortByPriceThenArea());

            foreach (Apartment a in copy)
                yield return a;
        }
    }

    class Program
    {
        const string FileName = "apartments.txt";

        // Якщо файлу немає - створюємо його з початковими даними
        static Apartments LoadData()
        {
            if (!File.Exists(FileName))
                CreateData().SaveToFile(FileName);

            return Apartments.LoadFromFile(FileName);
        }

        static void Print(IEnumerable list)
        {
            Console.WriteLine(Apartment.Header());

            bool any = false;
            foreach (Apartment a in list)
            {
                Console.WriteLine(a);
                any = true;
            }

            if (!any)
                Console.WriteLine("База даних порожня.");
        }

        // Таблиця з номерами записів
        static void PrintNumbered(Apartments apartments)
        {
            Console.WriteLine("{0,-4}{1}", "№", Apartment.Header());

            int i = 1;
            foreach (Apartment a in apartments)
            {
                Console.WriteLine("{0,-4}{1}", i, a);
                i++;
            }
        }

        static void DeleteApartment(Apartments apartments)
        {
            try
            {
                if (apartments.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("База даних порожня.");
                    Console.ResetColor();
                    return;
                }

                PrintNumbered(apartments);
                Console.WriteLine();

                Console.Write("Введіть номер квартири для видалення: ");
                int number = Convert.ToInt32(Console.ReadLine());
                int index = number - 1;

                if (index < 0 || index >= apartments.Count)
                    throw new InvalidApartmentException("Такої квартири немає.");

                Apartment selected = apartments.ToArray()[index];

                Console.WriteLine();
                Console.WriteLine("Буде видалено: " + selected.Address +
                                  " (ціна " + selected.Price + ")");
                Console.Write("Ви дійсно хочете видалити запис? (так/ні): ");
                string answer = Console.ReadLine();

                if (answer != null && answer.Trim().ToLower() == "так")
                {
                    apartments.RemoveAt(index);
                    apartments.SaveToFile(FileName);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Запис видалено.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("Видалення скасовано.");
                }
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: номер повинен бути числом.");
                Console.ResetColor();
            }
            catch (OverflowException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: занадто велике число.");
                Console.ResetColor();
            }
            catch (InvalidApartmentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: " + ex.Message);
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка роботи з файлом: " + ex.Message);
                Console.ResetColor();
            }
        }

        static Apartments CreateData()
        {
            return new Apartments(new Apartment[]
            {
                new Apartment("вул. Головна, 12",     45000,  52.5, 2, 3),
                new Apartment("вул. Руська, 7",       68000,  78.0, 3, 5),
                new Apartment("пр. Незалежності, 3",  32000,  34.0, 1, 2),
                new Apartment("вул. Шевченка, 21",    45000,  48.0, 2, 1),
                new Apartment("вул. Франка, 5",       91000, 105.0, 4, 7),
                new Apartment("вул. Садова, 14",      54000,  61.5, 2, 4),
                new Apartment("вул. Лесі Українки, 9",68000,  72.0, 3, 2),
                new Apartment("вул. Бережана, 30",    29500,  31.0, 1, 6),
                new Apartment("вул. Героїв, 18",      77000,  89.0, 3, 8),
                new Apartment("вул. Хотинська, 2",   120000, 130.0, 5, 9),
                new Apartment("вул. Комарова, 40",    38000,  42.0, 1, 4)
            });
        }

        static void AddApartment(Apartments apartments)
        {
            try
            {
                Console.Write("Адреса: ");
                string address = Console.ReadLine();

                Console.Write("Ціна: ");
                double price = Convert.ToDouble(Console.ReadLine());

                Console.Write("Площа: ");
                double area = Convert.ToDouble(Console.ReadLine());

                Console.Write("Кількість кімнат: ");
                int rooms = Convert.ToInt32(Console.ReadLine());

                Console.Write("Поверх: ");
                int floor = Convert.ToInt32(Console.ReadLine());

                apartments.Add(new Apartment(address, price, area, rooms, floor));
                apartments.SaveToFile(FileName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Квартиру додано.");
                Console.ResetColor();
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: введено дані неправильного формату.");
                Console.ResetColor();
            }
            catch (OverflowException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: занадто велике число.");
                Console.ResetColor();
            }
            catch (InvalidApartmentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка: " + ex.Message);
                Console.ResetColor();
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка роботи з файлом: " + ex.Message);
                Console.ResetColor();
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Apartments apartments = LoadData();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==========================================");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("            БАЗА КВАРТИР");
                Console.ResetColor();
                Console.WriteLine("==========================================");
                Console.WriteLine("1 - Вивести всі квартири");
                Console.WriteLine("2 - Сортування за ціною (IComparable)");
                Console.WriteLine("3 - Сортування за ціною (IComparer)");
                Console.WriteLine("4 - Сортування за кількістю кімнат (IComparer)");
                Console.WriteLine("5 - Список за ціною, потім за площею (IEnumerable)");
                Console.WriteLine("6 - Порівняння двох квартир (<, >)");
                Console.WriteLine("7 - Додати квартиру");
                Console.WriteLine("8 - Видалити квартиру");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Enter - Вихід");
                Console.ResetColor();
                Console.Write("Виберіть дію: ");

                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();

                if (key.Key == ConsoleKey.Enter)
                    break;

                try
                {
                    switch (key.KeyChar)
                    {
                        case '1':
                            Print(apartments);
                            break;

                        case '2':
                            Array.Sort(apartments.ToArray());
                            apartments.SaveToFile(FileName);
                            Print(apartments);
                            break;

                        case '3':
                            Array.Sort(apartments.ToArray(),
                                       new Apartment.SortByPrice());
                            apartments.SaveToFile(FileName);
                            Print(apartments);
                            break;

                        case '4':
                            Array.Sort(apartments.ToArray(),
                                       new Apartment.SortByRooms());
                            apartments.SaveToFile(FileName);
                            Print(apartments);
                            break;

                        case '5':
                            Print(apartments.OrderedByPriceThenArea());
                            break;

                        case '6':
                            Apartment[] arr = apartments.ToArray();
                            Console.WriteLine("Квартира 1: " + arr[0].Address +
                                              " (" + arr[0].Price + ")");
                            Console.WriteLine("Квартира 2: " + arr[1].Address +
                                              " (" + arr[1].Price + ")");
                            Console.WriteLine("Квартира 1 > Квартира 2 : " + (arr[0] > arr[1]));
                            Console.WriteLine("Квартира 1 < Квартира 2 : " + (arr[0] < arr[1]));
                            break;

                        case '7':
                            AddApartment(apartments);
                            break;

                        case '8':
                            DeleteApartment(apartments);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Помилка: " + ex.Message);
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Натисніть будь-яку клавішу...");
                Console.ResetColor();
                Console.ReadKey(true);
            }
        }
    }
}