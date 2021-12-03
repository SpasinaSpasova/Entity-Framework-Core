namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        //JSON
        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var serializer = JsonConvert.DeserializeObject<MovieImportDto[]>(jsonString);


            foreach (var movieDto in serializer)
            {
                if (!IsValid(movieDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Movie movie = context.Movies.FirstOrDefault(m => m.Title == movieDto.Title);

                if (movie != null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                movie = new Movie()
                {
                    Title = movieDto.Title,
                    Genre = Enum.Parse<Genre>(movieDto.Genre),
                    Rating = movieDto.Rating,
                    Duration = movieDto.Duration,
                    Director = movieDto.Director
                };

                sb.AppendLine(string.Format(SuccessfulImportMovie, movie.Title, movie.Genre,
                    movie.Rating.ToString("f2")));

                context.Movies.Add(movie);
                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();
        }

        //JSON
        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {

            StringBuilder sb = new StringBuilder();

            var serializer = JsonConvert.DeserializeObject<HallSeatsImportDto[]>(jsonString);


            foreach (var dto in serializer)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Hall hall = new Hall()
                {
                    Name = dto.Name,
                    Is4Dx = dto.Is4Dx,
                    Is3D = dto.Is3D,
                };

                for (var i = 0; i < dto.Seats; i++)
                {
                    hall.Seats.Add(new Seat());
                }

                string projectType = "";

                if (hall.Is3D && hall.Is4Dx)
                {
                    projectType = "4Dx/3D";
                }
                else if (hall.Is3D)
                {
                    projectType = "3D";

                }
                else if (hall.Is4Dx)
                {
                    projectType = "4Dx";
                }
                else
                {
                    projectType = "Normal";
                }

                context.Halls.Add(hall);
                context.SaveChanges();

                sb.AppendLine(string.Format(SuccessfulImportHallSeat, hall.Name, projectType, hall.Seats.Count));
            }

            return sb.ToString().TrimEnd();

        }

        //XML
        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Projections");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectImportDto[]), xmlRoot);

            StringReader sr = new StringReader(xmlString);

            ProjectImportDto[] dtos = (ProjectImportDto[])xmlSerializer.Deserialize(sr);

            foreach (var dto in dtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Hall hall = context.Halls.FirstOrDefault(h => h.Id == dto.HallId);

                if (hall == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Movie movie = context.Movies.FirstOrDefault(m => m.Id == dto.MovieId);

                if (movie == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var projectionDateTimeValid = DateTime.TryParseExact(dto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validDateTime);

                if (!projectionDateTimeValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Projection projection = new Projection()
                {
                    Hall = hall,
                    Movie = movie,
                    DateTime = validDateTime,
                    HallId = dto.HallId,
                    MovieId = dto.MovieId
                };

                context.Projections.Add(projection);
                context.SaveChanges();

                sb.AppendLine(string.Format(SuccessfulImportProjection, projection.Movie.Title, projection.DateTime.ToString("MM/dd/yyyy")));

            }

            return sb.ToString().TrimEnd();
        }

        //XML
        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Customers");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomerImportDto[]), xmlRoot);

            StringReader sr = new StringReader(xmlString);

            CustomerImportDto[] dtos = (CustomerImportDto[])xmlSerializer.Deserialize(sr);

            foreach (var dto in dtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Customer customer = new Customer()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Age = dto.Age,
                    Balance = dto.Balance,
                };

                foreach (var tick in dto.Tickets)
                {
                    if (!IsValid(dto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Ticket ticket = new Ticket()
                    {
                        ProjectionId = tick.ProjectionId,
                        Price = tick.Price
                    };

                    customer.Tickets.Add(ticket);
                }

                context.Customers.Add(customer);
                context.SaveChanges();

                sb.AppendLine(string.Format(SuccessfulImportCustomerTicket, customer.FirstName,
                        customer.LastName, customer.Tickets.Count));
            }

            return sb.ToString().TrimEnd();

        }
        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}