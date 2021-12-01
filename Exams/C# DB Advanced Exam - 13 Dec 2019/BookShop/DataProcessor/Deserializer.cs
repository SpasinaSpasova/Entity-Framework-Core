namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using BookShop.DataProcessor.ImportResults;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string StringFormat { get; private set; }

        //XML
        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Books");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BookImportDto[]), xmlRoot);


            StringReader sr = new StringReader(xmlString);

            BookImportDto[] dtos = (BookImportDto[])xmlSerializer.Deserialize(sr);

            List<Book> validBooks = new List<Book>();

            foreach (var book in dtos)
            {
                if (!IsValid(book))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool checkDate = DateTime.TryParseExact(book.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime valid);

                if (!checkDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Book currentBook = new Book()
                {
                    Name = book.Name,
                    Genre = (Genre)book.Genre,
                    Price = book.Price,
                    Pages = book.Pages,
                    PublishedOn = valid
                };

                validBooks.Add(currentBook);
                sb.AppendLine(String.Format(SuccessfullyImportedBook, currentBook.Name, currentBook.Price));
            }

            context.Books.AddRange(validBooks);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        //JSON
        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            AuthorImportDto[] dtos = JsonConvert.DeserializeObject<AuthorImportDto[]>(jsonString);


            foreach (var author in dtos)
            {
                if (!IsValid(author))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var emails = context.Authors.Select(x => x.Email).ToList();

                if (emails.Contains(author.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Author validAuthor = new Author()
                {
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    Email = author.Email,
                    Phone = author.Phone
                };


                bool flagAfterContinue = false;

                foreach (var book in author.Books)
                {
                    var bookIds = context.Books.Select(x => x.Id).ToList();

                    if (book.Id == null)
                    {
                        continue;
                    }

                    if (!bookIds.Contains((int)book.Id))
                    {
                        continue;
                    }

                    flagAfterContinue = true;

                    AuthorBook ab = new AuthorBook()
                    {
                        BookId = (int)book.Id
                    };

                    validAuthor.AuthorsBooks.Add(ab);
                }

                if (flagAfterContinue)
                {
                    sb.AppendLine(string.Format(SuccessfullyImportedAuthor, validAuthor.FirstName + " " + validAuthor.LastName, validAuthor.AuthorsBooks.Count));

                    context.Authors.Add(validAuthor);
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }

                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();

        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}