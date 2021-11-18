using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO.Input;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext context = new CarDealerContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //string suppliers = File.ReadAllText("../../../Datasets/suppliers.json");
            //Console.WriteLine(ImportSuppliers(context, suppliers));

            //string parts = File.ReadAllText("../../../Datasets/parts.json");
            //Console.WriteLine(ImportParts(context, parts));

            //string cars = File.ReadAllText("../../../Datasets/cars.json");
            //Console.WriteLine(ImportCars(context, cars));

            //string customers = File.ReadAllText("../../../Datasets/customers.json");
            //Console.WriteLine(ImportCustomers(context,customers));

            //string sales = File.ReadAllText("../../../Datasets/sales.json");
            //Console.WriteLine(ImportSales(context,sales));

            //Console.WriteLine(GetOrderedCustomers(context));

            //Console.WriteLine(GetCarsFromMakeToyota(context));

            //Console.WriteLine(GetLocalSuppliers(context));

            //Console.WriteLine(GetCarsWithTheirListOfParts(context));

            //Console.WriteLine(GetTotalSalesByCustomer(context));

            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            IEnumerable<SuppliersInputDto> suppliers = JsonConvert.DeserializeObject<IEnumerable<SuppliersInputDto>>(inputJson);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedSuppliers = mapper.Map<IEnumerable<Supplier>>(suppliers);

            context.Suppliers.AddRange(mappedSuppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}.";


        }
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var suppliers = context.Suppliers.Select(x => x.Id).ToList();

            IEnumerable<PartsInputDto> parts = JsonConvert.DeserializeObject<IEnumerable<PartsInputDto>>(inputJson);

            parts = parts.Where(p => suppliers.Any(s => s == p.SupplierId)).ToList();

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedParts = mapper.Map<IEnumerable<Part>>(parts);

            context.Parts.AddRange(mappedParts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}.";

        }
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            List<CarsInputDto> carsInput = JsonConvert.DeserializeObject<List<CarsInputDto>>(inputJson);

            List<Car> allCars = new List<Car>();
            foreach (var carJson in carsInput)
            {
                Car car = new Car()
                {
                    Make = carJson.Make,
                    Model = carJson.Model,
                    TravelledDistance = carJson.TravelledDistance
                };
                foreach (var partId in carJson.PartsId.Distinct())
                {
                    car.PartCars.Add(new PartCar()
                    {
                        Car = car,
                        PartId = partId
                    });
                }
                allCars.Add(car);
            }

            context.Cars.AddRange(allCars);
            context.SaveChanges();
            return $"Successfully imported {allCars.Count()}.";


        }
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var customers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);
            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var sales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                })
                .ToList();


            string result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToList();

            string result = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return result;
        }
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count()
                })
                .ToList();

            string result = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return result;
        }
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsParst = context.Cars
                .Select(c => new
                {
                    car = new
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TravelledDistance = c.TravelledDistance
                    },
                    parts = c.PartCars.Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = $"{p.Part.Price:f2}"
                    })
                    .ToList()
                })
                .ToList();

            string result = JsonConvert.SerializeObject(carsParst, Formatting.Indented);

            return result;
        }
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any(x => x.CustomerId == c.Id))
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count,
                    spentMoney = c.Sales.Sum(s => s.Car.PartCars.Sum(p => p.Part.Price))
                })
                .OrderByDescending(c => c.spentMoney)
                .ThenByDescending(c => c.boughtCars)
                .ToList();

            string result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context
                .Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("f2"),
                    price = (s.Car.Sales.Sum(pc => pc.Car.PartCars.Sum(p => p.Part.Price))).ToString("f2"),
                    priceWithDiscount =
                    (s.Car.Sales.Sum(pc => pc.Car.PartCars.Sum(p => p.Part.Price)) - s.Car.Sales.Sum(pc => pc.Car.PartCars.Sum(p => p.Part.Price)) * (s.Discount / 100)).ToString("f2")
                })
                .ToList();

            string result = JsonConvert.SerializeObject(sales, Formatting.Indented);

            return result;
        }
    }
}