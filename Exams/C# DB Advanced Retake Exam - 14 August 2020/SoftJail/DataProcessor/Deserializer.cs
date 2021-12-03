namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        //JSON
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            DepartmentImportDto[] dtos = JsonConvert.DeserializeObject<DepartmentImportDto[]>(jsonString);

            foreach (var dep in dtos)
            {
                if (!IsValid(dep))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Department department = new Department()
                {
                    Name = dep.Name
                };

                foreach (var cell in dep.Cells)
                {
                    if (!IsValid(dep))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }
                    if (cell.CellNumber <= 0 || cell.CellNumber > 1000)
                    {
                        break;
                    }
                    Cell currCell = new Cell()
                    {
                        CellNumber = cell.CellNumber,
                        HasWindow = cell.HasWindow
                    };

                    department.Cells.Add(currCell);
                }

                if (department.Cells.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                context.Departments.Add(department);
                context.SaveChanges();
                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            return sb.ToString().TrimEnd();
        }

        //JSON
        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            PrisonerImportDto[] dtos = JsonConvert.DeserializeObject<PrisonerImportDto[]>(jsonString);

            List<Prisoner> validPrisoners = new List<Prisoner>();

            foreach (var pris in dtos)
            {
                if (!IsValid(pris))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                DateTime? releaseDate = null;

                var releaseDateNotNull = DateTime.TryParseExact(pris.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validReleaseDate);

                if (releaseDateNotNull)
                {
                    releaseDate = validReleaseDate;
                }

                var incarcerationDate = DateTime.TryParseExact(pris.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validIncarcerationDate);

                if (incarcerationDate == null)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Prisoner prisoner = new Prisoner()
                {
                    FullName = pris.FullName,
                    Nickname = pris.Nickname,
                    Age = pris.Age,
                    IncarcerationDate = validIncarcerationDate,
                    ReleaseDate = releaseDate
                };

                bool hasInvalid = false;

                List<Mail> validMails = new List<Mail>();

                foreach (var em in pris.Mails)
                {
                    if (!IsValid(em))
                    {
                        sb.AppendLine("Invalid Data");
                        hasInvalid = true;
                        continue;
                    }

                    Mail mail = new Mail()
                    {
                        Description = em.Description,
                        Sender = em.Sender,
                        Address = em.Address
                    };

                    validMails.Add(mail);
                }

                if (!hasInvalid)
                {
                    foreach (var email in validMails)
                    {
                        prisoner.Mails.Add(email);
                    }
                }
                else
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                validPrisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }
            context.Prisoners.AddRange(validPrisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        //XML
        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Officers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OfficerInputDto[]), xmlRoot);


            StringReader sr = new StringReader(xmlString);

            OfficerInputDto[] dtos = (OfficerInputDto[])xmlSerializer.Deserialize(sr);

            List<Officer> validOfficers = new List<Officer>();

            foreach (var off in dtos)
            {
                if (!IsValid(off))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var validPosition = Enum.TryParse<Position>(off.Position, true, out Position validEnumPosition);

                if (!validPosition)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var validWeapon = Enum.TryParse<Weapon>(off.Weapon, true, out Weapon validEnumWeapon);

                if (!validWeapon)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Officer officer = context.Officers.FirstOrDefault(o => o.FullName == off.Name);

                if (officer == null)
                {
                    officer = new Officer()
                    {
                        FullName = off.Name,
                        Salary = off.Money,
                        Position = validEnumPosition,
                        Weapon = validEnumWeapon,
                        DepartmentId = off.DepartmentId
                    };

                }


                foreach (var pris in off.Prisoners)
                {
                    if (!IsValid(pris))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    officer.OfficerPrisoners.Add(new OfficerPrisoner() { Officer=officer,PrisonerId = pris.Id });
                }

                validOfficers.Add(officer);

                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }
            context.Officers.AddRange(validOfficers);
            context.SaveChanges();

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