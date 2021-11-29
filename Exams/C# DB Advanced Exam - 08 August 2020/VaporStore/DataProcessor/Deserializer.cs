namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
    {
        //JSON
        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            GameInputDto[] gameDtos = JsonConvert.DeserializeObject<GameInputDto[]>(jsonString);


            foreach (var dto in gameDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                if (!dto.Tags.Any())
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var developer = context.Developers.FirstOrDefault(x => dto.Developer == x.Name);

                if (developer == null)
                {
                    developer = new Developer()
                    {
                        Name = dto.Developer
                    };

                }

                var genre = context.Genres.FirstOrDefault(x => x.Name == dto.Genre);

                if (genre == null)
                {
                    genre = new Genre()
                    {
                        Name = dto.Genre
                    };

                }

                Game game = new Game()
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    ReleaseDate = DateTime.ParseExact(dto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Developer = developer,
                    Genre = genre
                };


                foreach (var tag in dto.Tags)
                {
                    var currTag = context.Tags.Where(x => x.Name == tag).FirstOrDefault();

                    if (currTag == null)
                    {
                        currTag = new Tag()
                        {
                            Name = tag
                        };
                    }

                    game.GameTags.Add(new GameTag()
                    {
                        Tag = currTag
                    });
                }

                sb.AppendLine($"Added {dto.Name} ({dto.Genre}) with {dto.Tags.Length} tags");

                context.Games.Add(game);
                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();
        }

        //JSON
        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            UserInputDto[] userDtos = JsonConvert.DeserializeObject<UserInputDto[]>(jsonString);

            foreach (var dto in userDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                if (!dto.Cards.Any())
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                User user = new User()
                {
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Email = dto.Email,
                    Age = dto.Age
                };

                foreach (var cardDto in dto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    Card card = new Card()
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.CVC,
                        Type = Enum.Parse<CardType>(cardDto.Type)
                    };

                    user.Cards.Add(card);
                }

                sb.AppendLine($"Imported {dto.Username} with {dto.Cards.Length} cards");

                context.Users.Add(user);
                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();
        }

        //XML
        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Purchases");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PurchaseImportDto[]), xmlRoot);


            using StringReader sr = new StringReader(xmlString);

            PurchaseImportDto[] dtos = (PurchaseImportDto[])xmlSerializer.Deserialize(sr);

            foreach (var p in dtos)
            {
                if (!IsValid(p))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Purchase purchase = new Purchase()
                {
                    Type = Enum.Parse<PurchaseType>(p.Type),
                    ProductKey = p.Key,
                    Date = DateTime.ParseExact(p.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    Card = context.Cards.FirstOrDefault(c => c.Number == p.Card) ?? new Card() { Number = p.Card },
                    Game = context.Games.FirstOrDefault(g => g.Name == p.Title) ??
                    new Game() { Name = p.Title }
                };

                context.Purchases.Add(purchase);
                context.SaveChanges();
                sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
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