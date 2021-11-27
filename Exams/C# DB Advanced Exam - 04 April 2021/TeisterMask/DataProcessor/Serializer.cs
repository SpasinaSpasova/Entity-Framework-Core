namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        //XML 
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Projects");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectOutputDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            var projectWithTheirTasks = context.Projects.ToArray()
                .Where(p => p.Tasks.Count >= 1)
                .Select(p => new ProjectOutputDto
                {
                    ProjectName = p.Name,
                    TasksCount = p.Tasks.Count(),
                    HasEndDate = p.DueDate.HasValue ? "Yes" : "No",
                    Tasks = p.Tasks.Select(t => new TaskOutputDto
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString()
                    })
                    .OrderBy(t=>t.Name)
                    .ToArray()
                })
                .OrderByDescending(p=>p.TasksCount)
                .ThenBy(p=>p.ProjectName)
                .ToArray();

            xmlSerializer.Serialize(sw, projectWithTheirTasks, namespaces);

            return sb.ToString().TrimEnd();
        }

        //JSON
        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var mostBusiestEmployees = context.Employees.ToArray()
                .Where(e => e.EmployeesTasks.Any(t => t.Task.OpenDate >= date))
                .Select(e => new
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks
                    .Where(t => t.Task.OpenDate >= date)
                    .OrderByDescending(t => t.Task.DueDate)
                    .ThenBy(t => t.Task.Name)
                    .Select(t => new
                    {
                        TaskName = t.Task.Name,
                        OpenDate = t.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = t.Task.DueDate.ToString("d",
                        CultureInfo.InvariantCulture),
                        LabelType = t.Task.LabelType.ToString(),
                        ExecutionType = t.Task.ExecutionType.ToString()
                    })
                    .ToArray()
                })
                .OrderByDescending(e => e.Tasks.Length)
                .ThenBy(e => e.Username)
                .Take(10)
                .ToArray();

            string result = JsonConvert.SerializeObject(mostBusiestEmployees, Formatting.Indented);

            return result;

        }
    }
}