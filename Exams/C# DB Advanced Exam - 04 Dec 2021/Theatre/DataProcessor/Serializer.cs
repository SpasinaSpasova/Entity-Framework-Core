namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ExportDto;

    public class Serializer
    {
        //JSON
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var theatres = context.Theatres.ToArray()
                .Where(t => t.NumberOfHalls >= numbersOfHalls && t.Tickets.Count >= 20)
                .Select(t => new
                {
                    Name = t.Name,
                    Halls = t.NumberOfHalls,
                    TotalIncome = t.Tickets.Where(x => x.RowNumber >= 1 && x.RowNumber <= 5).Sum(t => t.Price),
                    Tickets = t.Tickets.Where(x => x.RowNumber >= 1 && x.RowNumber <= 5)
                    .OrderByDescending(t => t.Price)
                    .Select(t => new
                    {
                        Price = t.Price,
                        RowNumber = t.RowNumber
                    })
                    .ToArray()
                })
                .OrderByDescending(t => t.Halls)
                .ThenBy(t => t.Name)
                .ToArray();

            var result = JsonConvert.SerializeObject(theatres, Formatting.Indented);
            return result;
        }

        public static string ExportPlays(TheatreContext context, double rating)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Plays");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayExportDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            var players = context
                .Plays
                .ToArray()
                .Where(r => r.Rating <= rating)
                .OrderBy(p => p.Title)
                .ThenByDescending(p => p.Genre)
                .Select(p => new PlayExportDto
                {
                    Title = p.Title,
                    Duration = p.Duration.ToString("c", CultureInfo.InvariantCulture),
                    Rating = p.Rating == 0 ? "Premier" : p.Rating.ToString(),
                    Genre = p.Genre.ToString(),
                    Actors = p.Casts
                               .Where(a => a.IsMainCharacter == true)
                               .OrderByDescending(a => a.FullName)
                               .Select(p => new ActorExportDto
                               {
                                   FullName = p.FullName,
                                   MainCharacter = $"Plays main character in '{p.Play.Title}'."
                               })
                               .ToArray()

                })
                //.OrderBy(p => p.Title)
                //.ThenByDescending(p => p.Genre)
                .ToArray();


            xmlSerializer.Serialize(sw, players, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}

