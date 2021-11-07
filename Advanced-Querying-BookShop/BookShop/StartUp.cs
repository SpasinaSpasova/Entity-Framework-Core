namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Globalization;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            // string input = Console.ReadLine();
            // Console.WriteLine(GetBooksByAgeRestriction(db,input));

            // Console.WriteLine(GetGoldenBooks(db));

            // Console.WriteLine(GetBooksByPrice(db));

            // int year = int.Parse(Console.ReadLine()); 
            // Console.WriteLine(GetBooksNotReleasedIn(db,year));

            // string input = Console.ReadLine();
            // Console.WriteLine(GetBooksByCategory(db, input));

            // string input = Console.ReadLine();
            // Console.WriteLine(GetBooksReleasedBefore(db, input));

            // string input = Console.ReadLine();
            // Console.WriteLine(GetAuthorNamesEndingIn(db,input));

            // string input = Console.ReadLine();
            // Console.WriteLine(GetBookTitlesContaining(db,input));

            // string input = Console.ReadLine();
            // Console.WriteLine(GetBooksByAuthor(db,input));

            // int length = int.Parse(Console.ReadLine());
            // Console.WriteLine(CountBooks(db,length));

            // Console.WriteLine(CountCopiesByAuthor(db));

            // Console.WriteLine(GetTotalProfitByCategory(db));

            // Console.WriteLine(GetMostRecentBooks(db));

            // IncreasePrices(db);

            Console.WriteLine(RemoveBooks(db));
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            StringBuilder sb = new StringBuilder();
            AgeRestriction ageRestriction = Enum.Parse<AgeRestriction>(command, true);
            var bookInfo = context.Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

            foreach (var title in bookInfo)
            {
                sb.AppendLine(title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var bookInfo = context.Books
                .Where(b => b.EditionType == EditionType.Gold
                           && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            foreach (var title in bookInfo)
            {
                sb.AppendLine(title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var bookInfo = context.Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    Title = b.Title,
                    Price = b.Price
                })
                .OrderByDescending(b => b.Price)
                .ToArray();

            foreach (var book in bookInfo)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            StringBuilder sb = new StringBuilder();

            var bookInfo = context.Books
                          .Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.Year != year)
                          .OrderBy(b => b.BookId)
                          .Select(b => b.Title)
                          .ToArray();

            foreach (var title in bookInfo)
            {
                sb.AppendLine(title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var categories = input
                .ToLower()
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var books = context.BooksCategories
                  .Select(b => new
                  {
                      Title = b.Book.Title,
                      Category = b.Category.Name
                  })
                  .ToArray();

            List<string> result = new List<string>();

            foreach (var b in books)
            {
                foreach (var c in categories)
                {
                    if (b.Category.ToLower().Contains(c))
                    {
                        result.Add(b.Title.ToString());
                    }
                }
            }

            foreach (var title in result.OrderBy(b => b))
            {
                sb.AppendLine(title);
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            StringBuilder sb = new StringBuilder();

            DateTime dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);



            var book = context.Books
                .OrderByDescending(b => b.ReleaseDate)
                .Where(b => b.ReleaseDate < dateTime)
                .Select(b => new
                {
                    Title = b.Title,
                    EditionType = b.EditionType.ToString(),
                    Price = b.Price
                })
                .ToArray();

            foreach (var info in book)
            {
                sb.AppendLine($"{info.Title} - {info.EditionType} - ${info.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var names = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName
                })
                .OrderBy(a => a.FullName)
                .ToArray();

            foreach (var name in names)
            {
                sb.AppendLine(name.FullName);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var info = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToArray();

            foreach (var b in info)
            {
                sb.AppendLine(b);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            StringBuilder sb = new StringBuilder();

            var booksAuthors = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    Title = b.Title,
                    Author = b.Author.FirstName + " " + b.Author.LastName
                })
                .ToArray();

            foreach (var info in booksAuthors)
            {
                sb.AppendLine($"{info.Title} ({info.Author})");
            }

            return sb.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {


            var books = context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .ToArray();


            return (books.Length);
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var copies = context.Authors
                .Select(a => new
                {
                    countCopies = a.Books.Sum(b => b.Copies),
                    Author = a.FirstName + " " + a.LastName
                })
                .OrderByDescending(a => a.countCopies)
                .ToArray();

            foreach (var info in copies)
            {
                sb.AppendLine($"{info.Author} - {info.countCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var info = context.Categories
                .Select(a => new
                {
                    Category = a.Name,
                    Profit = a.CategoryBooks.Sum(s => s.Book.Price * s.Book.Copies)
                })
                .OrderByDescending(p => p.Profit)
                .ThenBy(n => n.Category)
                .ToArray();

            foreach (var item in info)
            {
                sb.AppendLine($"{item.Category} ${item.Profit:F2}");
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var info = context.Categories
                .Select(a => new
                {
                    Category = a.Name,
                    Books = a.CategoryBooks.Select(b => new
                    {
                        BookName = b.Book.Title,
                        Date = b.Book.ReleaseDate
                    })
                    .OrderByDescending(a => a.Date)
                    .Take(3)
                    .ToArray()
                })
                .OrderBy(a => a.Category)
                .ToArray();

            foreach (var category in info)
            {
                sb.AppendLine($"--{category.Category}");

                foreach (var item in category.Books)
                {
                    sb.AppendLine($"{item.BookName} ({item.Date.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var booksToIncrease = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToArray();

            foreach (var book in booksToIncrease)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var bookToDelete = context.Books
                .Where(b => b.Copies < 4200)
                .ToArray();
            var bc=context.BooksCategories
                 .Where(b => b.Book.Copies < 4200)
                .ToArray();

            context.BooksCategories.RemoveRange(bc);

            context.Books.RemoveRange(bookToDelete);
            context.SaveChanges();

            return bookToDelete.Length;

        }
    }
}
