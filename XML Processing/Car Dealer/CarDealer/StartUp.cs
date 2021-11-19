using CarDealer.Data;
using CarDealer.Dtos.Input;
using CarDealer.Dtos.Output;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //string suppliers = File.ReadAllText("../../../Datasets/suppliers.xml");
            //Console.WriteLine(ImportSuppliers(context,suppliers));

            //string parts= File.ReadAllText("../../../Datasets/parts.xml");
            //Console.WriteLine(ImportParts(context,parts));

            //string cars= File.ReadAllText("../../../Datasets/cars.xml");
            //Console.WriteLine(ImportCars(context,cars));

            //string customers= File.ReadAllText("../../../Datasets/customers.xml");
            //Console.WriteLine(ImportCustomers(context,customers));

            //string sales= File.ReadAllText("../../../Datasets/sales.xml");
            //Console.WriteLine(ImportSales(context, sales));

            //Console.WriteLine(GetCarsWithDistance(context));

            //Console.WriteLine(GetCarsFromMakeBmw(context));

            //Console.WriteLine(GetLocalSuppliers(context));

            //Console.WriteLine(GetCarsWithTheirListOfParts(context));

            //Console.WriteLine(GetTotalSalesByCustomer(context));

            //Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Suppliers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SupplierInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            SupplierInputDto[] dtos = (SupplierInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Supplier> suppliers = new HashSet<Supplier>();

            foreach (SupplierInputDto dto in dtos)
            {
                Supplier supplier = new Supplier()
                {
                    Name = dto.Name,
                    IsImporter = dto.IsImporter
                };
                suppliers.Add(supplier);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Parts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PartInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            PartInputDto[] dtos = (PartInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Part> parts = new HashSet<Part>();

            foreach (PartInputDto dto in dtos)
            {
                var suppliers = context.Suppliers.Select(s => s.Id).ToList();

                if (suppliers.Contains(dto.supplierId))
                {
                    Part part = new Part()
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        Quantity = dto.Quantity,
                        SupplierId = dto.supplierId
                    };
                    parts.Add(part);
                }
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Cars");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCarDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            ImportCarDto[] carDtos = (ImportCarDto[])
               xmlSerializer.Deserialize(sr);

            ICollection<Car> cars = new HashSet<Car>();

            foreach (ImportCarDto carDto in carDtos)
            {
                Car c = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance
                };

                ICollection<PartCar> currentCarParts = new HashSet<PartCar>();
                foreach (int partId in carDto.Parts.Select(p => p.Id).Distinct())
                {
                    Part part = context
                        .Parts
                        .Find(partId);

                    if (part == null)
                    {
                        continue;
                    }

                    PartCar partCar = new PartCar()
                    {
                        Car = c,
                        Part = part
                    };
                    currentCarParts.Add(partCar);
                }

                c.PartCars = currentCarParts;
                cars.Add(c);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Customers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomerInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            CustomerInputDto[] dtos = (CustomerInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Customer> customers = new HashSet<Customer>();

            foreach (CustomerInputDto dto in dtos)
            {
                Customer customer = new Customer()
                {
                    Name = dto.Name,
                    BirthDate = dto.BirthDate,
                    IsYoungDriver = dto.IsYoungDriver
                };
                customers.Add(customer);

            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Sales");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SaleInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            SaleInputDto[] dtos = (SaleInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Sale> sales = new HashSet<Sale>();

            var cars = context.Cars.Select(c => c.Id).ToList();

            foreach (SaleInputDto dto in dtos)
            {
                if (cars.Contains(dto.CarId))
                {
                    Sale sale = new Sale()
                    {
                        CarId = dto.CarId,
                        CustomerId = dto.CustomerId,
                        Discount = dto.Discount
                    };
                    sales.Add(sale);
                }

            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("cars");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<CarOutputDto> cars = context.Cars
                .Where(c => c.TravelledDistance > 2000000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new CarOutputDto
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToList();

            xmlSerializer.Serialize(sw, cars, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("cars");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarBMWOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<CarBMWOutputDto> cars = context.Cars
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new CarBMWOutputDto
                {
                    Id = c.Id,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToList();

            xmlSerializer.Serialize(sw, cars, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("suppliers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SupplierOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<SupplierOutputDto> suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new SupplierOutputDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToList();

            xmlSerializer.Serialize(sw, suppliers, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("cars");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarsOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<CarsOutputDto> carsAndParts = context.Cars
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new CarsOutputDto
                {

                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    Parts = c.PartCars.Select(p => new PartOutputDto
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToList()
                })
                .ToList();

            xmlSerializer.Serialize(sw, carsAndParts, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("customers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CustomersOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<CustomersOutputDto> customers =  context.Customers
             .Where(c => c.Sales.Any(x => x.CustomerId == c.Id))
             .Select(c => new CustomersOutputDto()
             {
                 FullName = c.Name,
                 BoughtCars = c.Sales.Count,
                 SpentMoney = c.Sales
                               .SelectMany(s => s.Car.PartCars.Select(pc => pc.Part.Price)).Sum()
             })
             .OrderByDescending(c => c.SpentMoney)
             .ToList();

            xmlSerializer.Serialize(sw, customers, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("customers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ExportSalesWithDiscountDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<ExportSalesWithDiscountDto> dtos = context
                .Sales
                .Select(s => new ExportSalesWithDiscountDto()
                {
                    Car = new ExportSalesCarDto()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TravelledDistance.ToString()
                    },
                    Discount = s.Discount.ToString(CultureInfo.InvariantCulture),
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(pc => pc.Part.Price).ToString(CultureInfo.InvariantCulture),
                    PriceWithDiscount = (s.Car.PartCars.Sum(pc => pc.Part.Price) -
                                         s.Car.PartCars.Sum(pc => pc.Part.Price) * s.Discount / 100).ToString(CultureInfo.InvariantCulture)
                })
                .ToList();

            xmlSerializer.Serialize(sw, dtos, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}