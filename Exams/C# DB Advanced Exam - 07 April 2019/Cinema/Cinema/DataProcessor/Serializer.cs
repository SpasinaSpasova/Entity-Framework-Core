namespace Cinema.DataProcessor
{
    using System;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        //JSON
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var topMovies = context.Movies.ToArray()
                .Where(m => m.Rating >= rating && m.Projections.Any(p => p.Tickets.Count >= 1))
                .OrderByDescending(m => m.Rating)
                .ThenByDescending(m => m.Projections
                        .Sum(p => p.Tickets.Sum(t => t.Price)))
                .Select(m => new
                {
                    MovieName = m.Title,
                    Rating = m.Rating.ToString("f2"),
                    TotalIncomes = m.Projections
                        .Sum(p => p.Tickets.Sum(t => t.Price))
                        .ToString("f2"),
                    Customers = m.Projections.SelectMany(c => c.Tickets).Select(t => new
                    {
                        FirstName = t.Customer.FirstName,
                        LastName = t.Customer.LastName,
                        Balance = t.Customer.Balance.ToString("f2")
                    })
                    .ToArray()
                    .OrderByDescending(c => c.Balance)
                    .ThenBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                })
                .Take(10)
                .ToArray();

            var json = JsonConvert.SerializeObject(topMovies, Formatting.Indented);

            return json;
        }

        //XML

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            //   var customers = context
            //.Customers
            //.ToList()
            //.Where(c => c.Age >= age)
            //.OrderByDescending(c => c.Tickets.Sum(t => t.Price))
            //.Select(c => new ExportCustomerDto()
            //{
            //    FirstName = c.FirstName,
            //    LastName = c.LastName,
            //    SpentMoney = c.Tickets.Sum(t => t.Price).ToString("f2"),
            //    SpentTime = TimeSpan.FromMilliseconds(c.Tickets.Sum(t => t.Projection.Movie.Duration.TotalMilliseconds))
            //        .ToString(@"hh\:mm\:ss")
            //})
            //.Take(10)
            //.ToList();

            //TODO: XML SERIALIZER AND XML EXPORT DTO
        }
    }
}