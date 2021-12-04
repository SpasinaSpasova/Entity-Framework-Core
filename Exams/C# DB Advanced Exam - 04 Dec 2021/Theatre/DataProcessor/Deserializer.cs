namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";

        //XML
        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Plays");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayImportDto[]), xmlRoot);


            StringReader sr = new StringReader(xmlString);

            PlayImportDto[] dtos = (PlayImportDto[])xmlSerializer.Deserialize(sr);

            foreach (var pl in dtos)
            {
                if (!IsValid(pl))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var playDuration = TimeSpan.TryParseExact(pl.Duration, "c", CultureInfo.InvariantCulture, out TimeSpan validTimeSpan);


                if (validTimeSpan.Hours < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var validGenre = Enum.TryParse<Genre>(pl.Genre, out Genre valid);

                if (!validGenre)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                Play play = new Play()
                {
                    Title = pl.Title,
                    Duration = validTimeSpan,
                    Rating = pl.Rating,
                    Genre = valid,
                    Description = pl.Description,
                    Screenwriter = pl.Screenwriter
                };

                context.Plays.Add(play);
                context.SaveChanges();

                sb.AppendLine(String.Format(SuccessfulImportPlay, play.Title, play.Genre.ToString(), play.Rating));
            }

            return sb.ToString().TrimEnd();
        }

        //XML
        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Casts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CastImportDto[]), xmlRoot);


            StringReader sr = new StringReader(xmlString);

            CastImportDto[] dtos = (CastImportDto[])xmlSerializer.Deserialize(sr);

            foreach (var ca in dtos)
            {
                if (!IsValid(ca))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Cast cast = new Cast()
                {
                    FullName = ca.FullName,
                    IsMainCharacter = ca.IsMainCharacter,
                    PhoneNumber = ca.PhoneNumber,
                    PlayId = ca.PlayId
                };

                context.Casts.Add(cast);
                context.SaveChanges();
                sb.AppendLine(String.Format(SuccessfulImportActor, cast.FullName, cast.IsMainCharacter ? "main" : "lesser"));
            }

            return sb.ToString().TrimEnd();
        }

        //JSON
        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            TheatreImportDto[] dtos = JsonConvert.DeserializeObject<TheatreImportDto[]>(jsonString);


            foreach (var th in dtos)
            {
                if (!IsValid(th) || th.NumberOfHalls < 1 || th.NumberOfHalls > 10)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Theatre theatre = new Theatre()
                {
                    Name = th.Name,
                    NumberOfHalls = th.NumberOfHalls,
                    Director = th.Director
                };

                var playIds = context.Plays.Select(i => i.Id).ToList();


                //bool flagIsPlayNull = false;

                //List<Ticket> tickets = new List<Ticket>();

                foreach (var tick in th.Tickets)
                {
                    if (!IsValid(tick) || tick.RowNumber < 1 || tick.RowNumber > 10 || tick.Price < 1.00m || tick.Price > 100.00m)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    // var play = context.Plays.Find(tick.PlayId);

                    //if (playIds.Contains(tick.PlayId) == false)
                    //{
                    //    sb.AppendLine(ErrorMessage);
                    //    flagIsPlayNull = true;
                    //    break;
                    //}

                    Ticket ticket = new Ticket()
                    {
                        Price = tick.Price,
                        RowNumber = tick.RowNumber,
                        PlayId = tick.PlayId,
                        TheatreId = theatre.Id,
                        Theatre = theatre
                    };

                    theatre.Tickets.Add(ticket);
                    //tickets.Add(ticket);
                }
                // if (flagIsPlayNull == false)
                //{
                //  foreach (var ti in tickets)
                //  {
                //theatre.Tickets.Add(ticket);

                //  }

                context.Theatres.Add(theatre);
                sb.AppendLine(String.Format(SuccessfulImportTheatre, theatre.Name, theatre.Tickets.Count));
                //}

            }
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
