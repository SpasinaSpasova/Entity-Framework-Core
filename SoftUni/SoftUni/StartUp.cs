using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();

            // Console.WriteLine(GetEmployeesFullInformation(context));

            // Console.WriteLine(GetEmployeesWithSalaryOver50000(context));

            // Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context));

            // Console.WriteLine(AddNewAddressToEmployee(context));

            // Console.WriteLine(GetEmployeesInPeriod(context));

            // Console.WriteLine(GetAddressesByTown(context));

            // Console.WriteLine(GetEmployee147(context));

            // Console.WriteLine(GetDepartmentsWithMoreThan5Employees(context));

            // Console.WriteLine(GetLatestProjects(context));

            // Console.WriteLine(IncreaseSalaries(context));

            // Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(context));

            // Console.WriteLine(DeleteProjectById(context));

            // Console.WriteLine(RemoveTown(context));

        }
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.MiddleName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.EmployeeId)
                .ToList();

            foreach (var e in employees)
            {
                if (e.MiddleName == null)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} {e.JobTitle} {e.Salary:F2}");
                }
                else
                {

                    sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}");
                }
            }

            return sb.ToString().TrimEnd();
        }
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                          .Where(e => e.Salary > 50000)
                          .Select(e => new
                          {
                              e.FirstName,
                              e.Salary
                          })
                          .OrderBy(e => e.FirstName)
                          .ToList();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} - {e.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                           .Where(e => e.Department.Name == "Research and Development")
                           .Select(e => new
                           {
                               e.FirstName,
                               e.LastName,
                               DepartmentName = e.Department.Name,
                               e.Salary
                           })
                           .OrderBy(e => e.Salary)
                           .ThenByDescending(e => e.FirstName)
                           .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - ${e.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context.Add(newAddress);
            context.SaveChanges();

            Employee employeeNakov = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");

            employeeNakov.Address = newAddress;

            context.SaveChanges();

            var fullInfo = context.Employees
                          .OrderByDescending(e => e.AddressId)
                          .Select(e => new
                          {
                              e.Address.AddressText
                          })
                          .Take(10)
                          .ToList();
            foreach (var e in fullInfo)
            {
                sb.AppendLine(e.AddressText);
            }
            return sb.ToString().TrimEnd();

        }
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employeeProj = context.Employees
                                .Include(x => x.EmployeesProjects)
                                .ThenInclude(x => x.Project)
                                .Where(e => e.EmployeesProjects.Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                                .Select(e => new
                                {
                                    EmplFName = e.FirstName,
                                    EmplLName = e.LastName,
                                    ManFName = e.Manager.FirstName,
                                    ManLName = e.Manager.LastName,
                                    Projects = e.EmployeesProjects.Select(ep => new
                                    {
                                        ProjName = ep.Project.Name,
                                        ProjStart = ep.Project.StartDate,
                                        ProjEnd = ep.Project.EndDate
                                    })
                                })
                                .Take(10)
                                .ToList();

            foreach (var e in employeeProj)
            {
                sb.AppendLine($"{e.EmplFName} {e.EmplLName} - Manager: {e.ManFName} {e.ManLName}");
                foreach (var p in e.Projects)
                {


                    var end = p.ProjEnd.HasValue
                        ? p.ProjEnd.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToString()
                        : "not finished";

                    sb.AppendLine($"--{p.ProjName} - {p.ProjStart.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - {end}");

                }
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetAddressesByTown(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var addresses = context.Addresses
                .Select(x => new
                {
                    count = x.Employees.Count,
                    town = x.Town.Name,
                    address = x.AddressText
                })
                .OrderByDescending(x => x.count)
                .ThenBy(x => x.town)
                .ThenBy(x => x.address)
                .Take(10)
                .ToList();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.address}, {a.town} - {a.count} employees");
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetEmployee147(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Include(e => e.EmployeesProjects)
                .ThenInclude(p => p.Project)
                .Where(x => x.EmployeeId == 147)
                .Select(x => new
                {
                    EFN = x.FirstName,
                    ELN = x.LastName,
                    EJT = x.JobTitle,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        PName = p.Project.Name
                    })

                })
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.EFN} {e.ELN} - {e.EJT}");

                foreach (var p in e.Projects.OrderBy(p => p.PName))
                {
                    sb.AppendLine($"{p.PName}");
                }
            }

            return sb.ToString().TrimEnd();
        }
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var info = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(e => e.Employees.Count)
                .ThenBy(e => e.Name)
                .Select(d => new
                {

                    DepartmentName = d.Name,
                    ManFname = d.Manager.FirstName,
                    ManLname = d.Manager.LastName,

                    EmpInfo = d.Employees.Select(e => new
                    {
                        EmpFName = e.FirstName,
                        EmpLName = e.LastName,
                        EmpJT = e.JobTitle
                    })

                })
                .ToList();

            foreach (var d in info)
            {
                sb.AppendLine($"{d.DepartmentName} - {d.ManFname}  {d.ManLname}");


                foreach (var e in d.EmpInfo.OrderBy(x => x.EmpFName)
                                           .ThenBy(x => x.EmpLName))
                {
                    sb.AppendLine($"{e.EmpFName} {e.EmpLName} - {e.EmpJT}");
                }
            }

            return sb.ToString().TrimEnd();
        }
        public static string GetLatestProjects(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.StartDate
                })
                .ToList();

            foreach (var p in projects.OrderBy(p => p.Name))
            {
                sb.AppendLine($"{p.Name}");
                sb.AppendLine($"{p.Description}");
                sb.AppendLine($"{ p.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
            }
            return sb.ToString().TrimEnd();
        }
        public static string IncreaseSalaries(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var emp = context.Employees
                .Where(e => e.Department.Name == "Engineering"
                       || e.Department.Name == "Tool Design"
                       || e.Department.Name == "Marketing"
                       || e.Department.Name == "Information Services")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in emp)
            {
                e.Salary *= 1.12m;
            }

            context.SaveChanges();

            foreach (var e in emp)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var emp = context.Employees
                .ToList();

            var res = emp.Where(e => e.FirstName.StartsWith("Sa", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in res)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var proj = context.Projects
                .FirstOrDefault(p => p.ProjectId == 2);

            var emp = context.EmployeesProjects
                .Where(p => p.ProjectId == 2)
                .ToList();

            foreach (var e in emp)
            {
                context.EmployeesProjects.Remove(e);
            }
            context.SaveChanges();

            context.Projects.Remove(proj);

            context.SaveChanges();

            var projects = context.Projects.Take(10).ToList();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var town = context.Towns.FirstOrDefault(t => t.Name == "Seattle");

            var addresses = context.Addresses
                .Where(a => a.Town.Name == "Seattle")
                .ToList();

            var emp = context.Employees.Where(a => a.Address.Town.Name == "Seattle").ToList();

            foreach (var e in emp)
            {
                e.AddressId = null;
            }
            context.SaveChanges();

            foreach (var a in addresses)
            {
                context.Addresses.Remove(a);
            }
            context.SaveChanges();

            context.Towns.Remove(town);
            context.SaveChanges();

            sb.AppendLine($"{addresses.Count} addresses in Seattle were deleted");

            return sb.ToString().TrimEnd();
        }
    }
}

