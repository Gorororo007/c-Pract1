using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Абстрактный класс для представления пользователя
public abstract class User
{
    public string Name { get; protected set; }

    public User(string name)
    {
        Name = name;
    }

    // Абстрактный метод для отображения меню действий
    public abstract void ShowMenu(Library library);

    public override string ToString()
    {
        return $"Name: {Name}";
    }
}

// Класс представляющий библиотекаря
public class Librarian : User
{
    public Librarian(string name) : base(name) { }

    public override void ShowMenu(Library library)
    {
        while (true)
        {
            Console.WriteLine("\nLibrarian Menu:");
            Console.WriteLine("1. Add new book");
            Console.WriteLine("2. Remove book");
            Console.WriteLine("3. Register new user");
            Console.WriteLine("4. View all users");
            Console.WriteLine("5. View all books");
            Console.WriteLine("6. Exit");

            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    library.AddBook();
                    break;
                case "2":
                    library.RemoveBook();
                    break;
                case "3":
                    library.RegisterUser();
                    break;
                case "4":
                    library.ViewAllUsers();
                    break;
                case "5":
                    library.ViewAllBooks();
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}

// Класс представляющий пользователя
public class RegularUser : User
{
    private List<string> borrowedBooks = new List<string>();

    public RegularUser(string name) : base(name) { }

    //  для загрузки данных о взятых книгах из файла
    public void LoadBorrowedBooks(string filename)
    {
        if (File.Exists(filename))
        {
            try
            {
                borrowedBooks = File.ReadAllLines(filename).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowed books: {ex.Message}");
            }
        }
    }

    // для сохранения данных о взятых книгах в файл
    public void SaveBorrowedBooks(string filename)
    {
        try
        {
            File.WriteAllLines(filename, borrowedBooks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving borrowed books: {ex.Message}");
        }
    }

    public override void ShowMenu(Library library)
    {
        while (true)
        {
            Console.WriteLine("\nUser Menu:");
            Console.WriteLine("1. View available books");
            Console.WriteLine("2. Borrow a book");
            Console.WriteLine("3. Return a book");
            Console.WriteLine("4. View my borrowed books");
            Console.WriteLine("5. Exit");

            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    library.ViewAvailableBooks();
                    break;
                case "2":
                    BorrowBook(library);
                    break;
                case "3":
                    ReturnBook(library);
                    break;
                case "4":
                    ViewBorrowedBooks();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    public void BorrowBook(Library library)
    {
        Console.Write("Enter the title of the book you want to borrow: ");
        string title = Console.ReadLine();

        Book book = library.FindBook(title);

        if (book == null)
        {
            Console.WriteLine("Book not found.");
            return;
        }

        if (book.IsBorrowed)
        {
            Console.WriteLine("This book is currently unavailable.");
            return;
        }

        book.IsBorrowed = true;
        borrowedBooks.Add(title); // Добавляем книгу в список взятых пользователем
        library.SaveBooksToFile(); // Сохраняем изменения в файле книг
        SaveBorrowedBooks(Name + "_borrowed.txt"); // Сохраняем изменения в файле взятых книг
        Console.WriteLine($"You have borrowed '{title}'.");
    }


    public void ReturnBook(Library library)
    {
        Console.Write("Enter the title of the book you want to return: ");
        string title = Console.ReadLine();

        Book book = library.FindBook(title);

        if (book == null)
        {
            Console.WriteLine("Book not found.");
            return;
        }

        if (!book.IsBorrowed)
        {
            Console.WriteLine("This book is not currently borrowed.");
            return;
        }

        book.IsBorrowed = false;
        borrowedBooks.Remove(title);
        library.SaveBooksToFile(); // Сохраняем изменения в файле книг
        SaveBorrowedBooks(Name + "_borrowed.txt"); // Сохраняем изменения в файле взятых книг
        Console.WriteLine($"You have returned '{title}'.");
    }

    public void ViewBorrowedBooks()
    {
        if (borrowedBooks.Count == 0)
        {
            Console.WriteLine("You have not borrowed any books yet.");
        }
        else
        {
            Console.WriteLine("Borrowed Books:");
            foreach (var book in borrowedBooks)
            {
                Console.WriteLine($"- {book}");
            }
        }
    }
}

// представляю книгу
public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public bool IsBorrowed { get; set; }

    public Book(string title, string author, bool isBorrowed = false)
    {
        Title = title;
        Author = author;
        IsBorrowed = isBorrowed;
    }

    public override string ToString()
    {
        return $"Title: {Title}, Author: {Author}, Status: {(IsBorrowed ? "Borrowed" : "Available")}";
    }
}

// представляющий библиотеку
public class Library
{
    private List<Book> books = new List<Book>();
    private List<User> users = new List<User>();
    private const string BooksFileName = "books.txt";
    private const string UsersFileName = "users.txt";

    public Library()
    {
        LoadData();
    }

    // загрузка данных о книгах и пользователях из файлов
    private void LoadData()
    {
        LoadBooksFromFile();
        LoadUsersFromFile();
    }


    private void LoadBooksFromFile()
    {
        if (File.Exists(BooksFileName))
        {
            try
            {
                string[] lines = File.ReadAllLines(BooksFileName);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        string title = parts[0];
                        string author = parts[1];
                        bool isBorrowed = bool.Parse(parts[2]);
                        books.Add(new Book(title, author, isBorrowed));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
            }
        }
    }

    private void LoadUsersFromFile()
    {
        if (File.Exists(UsersFileName))
        {
            try
            {
                string[] lines = File.ReadAllLines(UsersFileName);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        string role = parts[1];

                        User user = null;

                        if (role == "Librarian")
                        {
                            user = new Librarian(name);
                        }
                        else if (role == "RegularUser")
                        {
                            user = new RegularUser(name);
                            ((RegularUser)user).LoadBorrowedBooks(name + "_borrowed.txt"); // загружаем книги для пользователя
                        }

                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
    }

    // сохранение данных о книгах и пользователях в файлы
    public void SaveData()
    {
        SaveBooksToFile();
        SaveUsersToFile();
    }

    public void SaveBooksToFile()
    {
        try
        {
            List<string> lines = new List<string>();
            foreach (var book in books)
            {
                lines.Add($"{book.Title}|{book.Author}|{book.IsBorrowed}");
            }
            File.WriteAllLines(BooksFileName, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving books: {ex.Message}");
        }
    }

    public void SaveUsersToFile()
    {
        try
        {
            List<string> lines = new List<string>();
            foreach (var user in users)
            {
                string role = user is Librarian ? "Librarian" : "RegularUser";
                lines.Add($"{user.Name}|{role}");
            }
            File.WriteAllLines(UsersFileName, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving users: {ex.Message}");
        }
    }



    // для работы с книгами
    public void AddBook()
    {
        Console.Write("Enter book title: ");
        string title = Console.ReadLine();
        Console.Write("Enter book author: ");
        string author = Console.ReadLine();

        books.Add(new Book(title, author));
        SaveBooksToFile();
        Console.WriteLine("Book added successfully.");
    }

    public void RemoveBook()
    {
        Console.Write("Enter the title of the book you want to remove: ");
        string title = Console.ReadLine();

        Book bookToRemove = books.FirstOrDefault(b => b.Title == title);

        if (bookToRemove != null)
        {
            books.Remove(bookToRemove);
            SaveBooksToFile();
            Console.WriteLine("Book removed successfully.");
        }
        else
        {
            Console.WriteLine("Book not found.");
        }
    }

    public Book FindBook(string title)
    {
        return books.FirstOrDefault(b => b.Title == title);
    }


    public void ViewAllBooks()
    {
        if (books.Count == 0)
        {
            Console.WriteLine("No books in the library.");
        }
        else
        {
            Console.WriteLine("All Books:");
            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }
    }

    public void ViewAvailableBooks()
    {
        if (books.Count == 0)
        {
            Console.WriteLine("No books in the library.");
        }
        else
        {
            Console.WriteLine("Available Books:");
            foreach (var book in books)
            {
                if (!book.IsBorrowed)
                {
                    Console.WriteLine(book);
                }
            }
        }
    }


    // для работы с пользователями
    public void RegisterUser()
    {
        Console.Write("Enter user name: ");
        string name = Console.ReadLine();

        Console.WriteLine("Choose user role (1 - Librarian, 2 - Regular User):");
        string roleChoice = Console.ReadLine();

        User newUser = null;
        if (roleChoice == "1")
        {
            newUser = new Librarian(name);
        }
        else if (roleChoice == "2")
        {
            newUser = new RegularUser(name);
        }
        else
        {
            Console.WriteLine("Invalid role choice.");
            return;
        }

        users.Add(newUser);
        SaveUsersToFile();
        Console.WriteLine("User registered successfully.");
    }

    public void ViewAllUsers()
    {
        if (users.Count == 0)
        {
            Console.WriteLine("No users registered.");
        }
        else
        {
            Console.WriteLine("All Users:");
            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }
    }

    // для аутентификации пользователя
    public User AuthenticateUser()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();

        User user = users.FirstOrDefault(u => u.Name == name);

        if (user != null)
        {
            return user;
        }
        else
        {
            Console.WriteLine("User not found.  Please register or try again.");
            return null;
        }
    }
}


public class Program
{
    public static void Main(string[] args)

    {
        Console.WriteLine($"Каталог приложения: {AppDomain.CurrentDomain.BaseDirectory}");


        Library library = new Library();

        while (true)
        {
            Console.WriteLine("\nWelcome to the Library!");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");

            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    User currentUser = library.AuthenticateUser();
                    if (currentUser != null)
                    {
                        currentUser.ShowMenu(library);
                    }
                    break;
                case "2":
                    library.RegisterUser();
                    break;
                case "3":
                    library.SaveData(); // сохрарнение 
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}
