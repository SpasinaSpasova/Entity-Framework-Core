using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Dtos.Input;
using ProductShop.Dtos.Output;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var usersJsonAsString = File.ReadAllText("../../../Datasets/users.json");
            //Console.WriteLine(ImportUsers(context, usersJsonAsString));

            //var productsJsonAsString = File.ReadAllText("../../../Datasets/products.json");
            //Console.WriteLine(ImportProducts(context, productsJsonAsString));

            //var categoriesJsonAsString = File.ReadAllText("../../../Datasets/categories.json");
            //Console.WriteLine(ImportCategories(context, categoriesJsonAsString));

            //var categoryProductsJsonAsString = File.ReadAllText("../../../Datasets/categories-products.json");
            //Console.WriteLine(ImportCategoryProducts(context, categoryProductsJsonAsString));

            //Console.WriteLine(GetProductsInRange(context));

            //Console.WriteLine(GetSoldProducts(context));

            //Console.WriteLine(GetCategoriesByProductsCount(context));

            Console.WriteLine(GetUsersWithProducts(context));
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            IEnumerable<UserInputDto> users = JsonConvert.DeserializeObject<IEnumerable<UserInputDto>>(inputJson);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedUsers = mapper.Map<IEnumerable<User>>(users);

            context.Users.AddRange(mappedUsers);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<ProductInputDto> products = JsonConvert.DeserializeObject<IEnumerable<ProductInputDto>>(inputJson);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedProducts = mapper.Map<IEnumerable<Product>>(products);

            context.Products.AddRange(mappedProducts);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryInputDto> categories = JsonConvert.DeserializeObject<IEnumerable<CategoryInputDto>>(inputJson)
                .Where(c => c.Name != null);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedCategories = mapper.Map<IEnumerable<Category>>(categories);

            context.Categories.AddRange(mappedCategories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryProductsInputDto> categoryProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryProductsInputDto>>(inputJson);

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            IMapper mapper = new Mapper(mapperConfiguration);

            var mappedCategoryProducts = mapper.Map<IEnumerable<CategoryProduct>>(categoryProducts);

            context.CategoryProducts.AddRange(mappedCategoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count()}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var result = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                .Select(x => new ProductOutputDto
                {
                    Name = x.Name,
                    Price = x.Price,
                    Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
                })
                .ToList();


            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver
            };

            string productAsJson = JsonConvert.SerializeObject(result, jsonSettings);

            return productAsJson;
        }
        public static string GetSoldProducts(ProductShopContext context)
        {
            var result = context
               .Users
               .Include(p => p.ProductsSold)
               .Where(x => x.ProductsSold.Any(y => y.Buyer != null))
               .OrderBy(x => x.LastName)
               .ThenBy(x => x.FirstName)
               .Select(x => new UserSoldProductsOutputDto
               {
                   FirstName = x.FirstName,
                   LastName = x.LastName,
                   SoldProducts = x.ProductsSold
                   .Select(p => new SoldProdcutOutputDto
                   {
                       Name = p.Name,
                       Price = p.Price,
                       BuyerFirstName = p.Buyer.FirstName,
                       BuyerLastName = p.Buyer.LastName
                   })
                   .ToList()
               })
               .ToList();

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver
            };

            string productAsJson = JsonConvert.SerializeObject(result, jsonSettings);

            return productAsJson;

        }
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var result = context
             .Categories
             .OrderByDescending(x => x.CategoryProducts.Count)
             .Select(x => new CategoryOutputDto
             {
                 Category = x.Name,
                 ProductsCount = x.CategoryProducts.Count,
                 AveragePrice = $"{(x.CategoryProducts.Sum(cp => cp.Product.Price) / x.CategoryProducts.Count):F2}",
                 TotalRevenue = $"{x.CategoryProducts.Sum(cp => cp.Product.Price):F2}"
             })
             .ToList();

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver
            };

            string categoriesAsJson = JsonConvert.SerializeObject(result, jsonSettings);

            return categoriesAsJson;

        }
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                 .Include(x => x.ProductsSold)
                 .ToList()
                 .Where(x => x.ProductsSold.Any(b => b.Buyer != null))
                 .Select(x => new UserProductsOutputDto
                 {
                     FirstName = x.FirstName,
                     LastName = x.LastName,
                     Age = x.Age,
                     SoldProducts = new ProductsOutputDto
                     {
                         Count = x.ProductsSold
                         .Where(p => p.Buyer != null)
                         .Count(),

                         Products = x.ProductsSold
                         .Where(p => p.Buyer != null)
                         .Select(p => new ProductOutputDto
                         {
                             Name = p.Name,
                             Price = p.Price,
                         })
                         .ToList()
                     }
                 })
                 .OrderByDescending(x => x.SoldProducts.Count)
                 .ToList();

            var result = new UsersWithSoldProductsOutputDto
            {
                UsersCount = users.Count(),
                Users = users
            };
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver,
                NullValueHandling =  NullValueHandling.Ignore
            };

            string userProductsAsJson = JsonConvert.SerializeObject(result, jsonSettings);

            return userProductsAsJson;
        }

    }
}