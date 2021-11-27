namespace TeisterMask.DataProcessor
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
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Projects");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectImportDto[]), xmlRoot);


            using StringReader sr = new StringReader(xmlString);

            ProjectImportDto[] dtos = (ProjectImportDto[])xmlSerializer.Deserialize(sr);

            HashSet<Project> validProject = new HashSet<Project>();

            foreach (var projectDto in dtos)
            {
                if (!IsValid(projectDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isOpenProjectDateValid = DateTime.TryParseExact(projectDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime projectOpenDate);

                if (!isOpenProjectDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime? pDueDate = null;
                if (!String.IsNullOrWhiteSpace(projectDto.DueDate))
                {
                    bool isDueProjectDateValid = DateTime.TryParseExact(projectDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime projectDueDate);

                    if (!isDueProjectDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    pDueDate = projectDueDate;
                }

                Project project = new Project()
                {
                    Name = projectDto.Name,
                    OpenDate = projectOpenDate,
                    DueDate = pDueDate,
                };

                HashSet<Task> currTasks = new HashSet<Task>();

                foreach (var taskDto in projectDto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isOpenTaskDateValid = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime taskOpenDate);

                    if (!isOpenTaskDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isDueTaskDateValid = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime taskDueDate);

                    if (!isDueTaskDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (taskOpenDate < projectOpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (project.DueDate.HasValue && taskDueDate > project.DueDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Task task = new Task()
                    {
                        Name = taskDto.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = (ExecutionType)taskDto.ExecutionType,
                        LabelType = (LabelType)taskDto.LabelType
                    };

                    currTasks.Add(task);
                }
                project.Tasks = currTasks;

                validProject.Add(project);

                sb.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));
            }
            context.Projects.AddRange(validProject);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            EmployeeImportDto[] employeeDtos = JsonConvert.DeserializeObject<EmployeeImportDto[]>(jsonString);

            HashSet<Employee> validEmployees = new HashSet<Employee>();

            foreach (EmployeeImportDto employeeDto in employeeDtos)
            {
                if (!IsValid(employeeDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Employee employee = new Employee()
                {
                    Username = employeeDto.Username,
                    Email = employeeDto.Email,
                    Phone = employeeDto.Phone
                };

                HashSet<EmployeeTask> validTask = new HashSet<EmployeeTask>();

                foreach (int taskId in employeeDto.Tasks.Distinct())
                {
                    var tasksIds = context.Tasks.Select(t => t.Id).ToList();

                    if (!tasksIds.Contains(taskId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    EmployeeTask employeeTask = new EmployeeTask()
                    {
                        Employee = employee,
                        TaskId = taskId
                    };
                    validTask.Add(employeeTask);
                }

                employee.EmployeesTasks = validTask;

                validEmployees.Add(employee);

                sb.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, validTask.Count));
            }

            context.Employees.AddRange(validEmployees);
            context.SaveChanges();

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