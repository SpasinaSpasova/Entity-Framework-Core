namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        //JSON
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisonersByCells = context.Prisoners
                .ToArray()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.FullName,
                    CellNumber = p.Cell.CellNumber,
                    Officers = p.PrisonerOfficers.Select(po => new
                    {
                        OfficerName = po.Officer.FullName,
                        Department = po.Officer.Department.Name
                    })
                    .OrderBy(o => o.OfficerName)
                    .ToArray(),
                    TotalOfficerSalary = Math.Round(p.PrisonerOfficers.Sum(po => po.Officer.Salary), 2)
                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToArray();

            var result = JsonConvert.SerializeObject(prisonersByCells, Formatting.Indented);
            return result;

            //return "";
        }

        //XML
        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Prisoners");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrisonerExportDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            var prisonersNamesSepareted = prisonersNames.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray();

            var prisonersInbox = context.Prisoners.ToArray()
                .Where(p => prisonersNamesSepareted.Contains(p.FullName))
                .Select(p => new PrisonerExportDto()
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EncryptedMessages = p.Mails.Select(m => new MessageExportDto()
                    {
                        Description = string.Join(",", m.Description.Reverse())
                    })
                    .ToArray()

                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToArray();

            xmlSerializer.Serialize(sw, prisonersInbox, namespaces);

            return sb.ToString().TrimEnd();

        }
    }
}