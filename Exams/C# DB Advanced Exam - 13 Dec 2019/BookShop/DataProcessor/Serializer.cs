namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        //JSON
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var mostCraziestAuthors = context.Authors
               .Select(a => new
               {
                   AuthorName = a.FirstName + ' ' + a.LastName,
                   Books = a.AuthorsBooks
                                .OrderByDescending(b => b.Book.Price)
                                .Select(b => new
                                {
                                    BookName = b.Book.Name,
                                    BookPrice = b.Book.Price.ToString("f2")
                                })
                                .ToArray()
               })
               .ToArray()
               .OrderByDescending(a => a.Books.Count())
               .ThenBy(a => a.AuthorName)
               .ToArray();

            var result = JsonConvert.SerializeObject(mostCraziestAuthors, Formatting.Indented);
            return result;
        }

        //XML
        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Books");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BookExportDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            var oldestBooks = context.Books.ToArray()
                .Where(b => b.PublishedOn <= date && (int)b.Genre == 3)
                .Select(b => new BookExportDto
                {
                    Pages = b.Pages,
                    Name = b.Name,
                    Date = b.PublishedOn.ToString("d", CultureInfo.InvariantCulture)
                })
                .OrderByDescending(b => b.Pages)
                .ThenByDescending(b => b.Date)
                .Take(10)
                .ToArray();

            xmlSerializer.Serialize(sw, oldestBooks, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}