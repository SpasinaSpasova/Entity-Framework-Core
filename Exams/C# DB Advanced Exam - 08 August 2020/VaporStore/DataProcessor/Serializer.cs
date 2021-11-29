namespace VaporStore.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        //JSON
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var gamesByGenres = context.Genres.ToArray()
                .Where(x => genreNames.Contains(x.Name))
                .Select(genre => new
                {
                    Id = genre.Id,
                    Genre = genre.Name,
                    Games = genre.Games.Select(game => new
                    {
                        Id = game.Id,
                        Title = game.Name,
                        Developer = game.Developer.Name,
                        Tags = string.Join(", ", game.GameTags.
                                                Select(
                                                t => t.Tag.Name
                                                )),
                        Players = game.Purchases.Count
                    })
                    .Where(g => g.Players > 0)
                    .OrderByDescending(g => g.Players)
                    .ThenBy(g => g.Id),

                    TotalPlayers = genre.Games.Sum(g => g.Purchases.Count()),

                })
                .OrderByDescending(g => g.TotalPlayers)
                .ThenBy(g => g.Id)
                .ToArray();

            return JsonConvert.SerializeObject(gamesByGenres, Formatting.Indented);

        }

        //XML
        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Users");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserOutputDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);


            var userPurchasesByType = context.Users.ToList()
                .Where(x => x.Cards.Any(c => c.Purchases.Any(p => p.Type.ToString() == storeType)))
                .Select(x => new UserOutputDto
                {
                    Username = x.Username,
                    TotalSpent = x.Cards.Sum(
                        c => c.Purchases.Where(p => p.Type.ToString() == storeType)
                              .Sum(p => p.Game.Price)),
                    Purchases = x.Cards.SelectMany(c => c.Purchases)
                        .Where(p => p.Type.ToString() == storeType)
                        .Select(p => new PurchaseOutputDto
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new GameOutputDto
                            {
                                Title = p.Game.Name,
                                Price = p.Game.Price,
                                Genre = p.Game.Genre.Name,
                            }
                        })
                        .OrderBy(x => x.Date)
                        .ToArray()
                })
                .OrderByDescending(x => x.TotalSpent).ThenBy(x => x.Username).ToArray();

            xmlSerializer.Serialize(sw, userPurchasesByType, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}