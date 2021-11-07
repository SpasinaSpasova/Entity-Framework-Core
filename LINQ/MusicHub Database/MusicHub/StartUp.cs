namespace MusicHub
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;


    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //Test your solutions here
            // Console.WriteLine(ExportAlbumsInfo(context, 9));

           // Console.WriteLine(ExportSongsAboveDuration(context, 4));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var info = context.Albums.ToArray()
                      .Where(a => a.ProducerId == producerId)
                      .OrderByDescending(a => a.Price)
                      .Select(a => new
                      {
                          AlbumName = a.Name,
                          ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                          ProducerName = a.Producer.Name,
                          TotalPrice = a.Price.ToString("f2"),
                          Songs = a.Songs.ToArray().Select(s => new
                          {
                              SongName = s.Name,
                              Price = s.Price.ToString("f2"),
                              Writer = s.Writer.Name
                          })
                          .OrderByDescending(s => s.SongName)
                          .ThenBy(s => s.Writer)
                          .ToArray()
                      })
                      .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var album in info)
            {
                sb.AppendLine($"-AlbumName: {album.AlbumName}")
                    .AppendLine($"-ReleaseDate: {album.ReleaseDate}")
                    .AppendLine($"-ProducerName: {album.ProducerName}")
                    .AppendLine("-Songs:");

                int i = 1;

                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{i++}")
                        .AppendLine($"---SongName: {song.SongName}")
                        .AppendLine($"---Price: {song.Price}")
                        .AppendLine($"---Writer: {song.Writer}");
                }


                sb.AppendLine($"-AlbumPrice: {album.TotalPrice}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var info = context.Songs
                .ToArray()
                .Where(s => s.Duration.TotalSeconds >duration)
                .Select(s => new
                {
                    SongName = s.Name,
                    Performer = s.SongPerformers.ToArray().
                    Select(p => $"{p.Performer.FirstName} {p.Performer.LastName}")
                    .FirstOrDefault(),
                    Writer = s.Writer.Name,
                    Producer = s.Album.Producer.Name,
                    Duration = s.Duration.ToString("c")

                })
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.Writer)
                .ThenBy(s => s.Performer)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            int i = 1;
            foreach (var song in info)
            {
                sb.AppendLine($"-Song #{i++}")
                   .AppendLine($"---SongName: {song.SongName}")
                   .AppendLine($"---Writer: {song.Writer}")
                   .AppendLine($"---Performer: {song.Performer}")
                   .AppendLine($"---AlbumProducer: {song.Producer}")
                   .AppendLine($"---Duration: {song.Duration}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
