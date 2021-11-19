using Microsoft.EntityFrameworkCore;
using ProductShop.Data;
using ProductShop.Dtos.Input;
using ProductShop.Dtos.Output;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            ProductShopContext context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //string users = File.ReadAllText("../../../Datasets/users.xml");
            //Console.WriteLine(ImportUsers(context, users));

            //string products = File.ReadAllText("../../../Datasets/products.xml");
            //Console.WriteLine(ImportProducts(context, products));

            //string categories = File.ReadAllText("../../../Datasets/categories.xml");
            //Console.WriteLine(ImportCategories(context, categories));

            //string categoryProducts = File.ReadAllText("../../../Datasets/categories-products.xml");
            //Console.WriteLine(ImportCategoryProducts(context, categoryProducts));

            //Console.WriteLine(GetProductsInRange(context));

            //Console.WriteLine(GetSoldProducts(context));

            //Console.WriteLine(GetCategoriesByProductsCount(context));

            Console.WriteLine(GetUsersWithProducts(context));
        }
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Users");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            UserInputDto[] dtos = (UserInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<User> allUsers = new HashSet<User>();

            foreach (UserInputDto userDto in dtos)
            {
                User user = new User()
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Age = userDto.Age
                };
                allUsers.Add(user);
            }

            context.Users.AddRange(allUsers);
            context.SaveChanges();

            return $"Successfully imported {allUsers.Count}";
        }
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Products");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            ProductInputDto[] dtos = (ProductInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Product> allProducts = new HashSet<Product>();

            foreach (ProductInputDto productDto in dtos)
            {
                Product product = new Product()
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    SellerId = productDto.SellerId,
                };

                if (productDto.BuyerId != 0)
                {
                    product.BuyerId = productDto.BuyerId;
                }

                allProducts.Add(product);
            }

            context.Products.AddRange(allProducts);
            context.SaveChanges();

            return $"Successfully imported {allProducts.Count}";
        }
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Categories");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            CategoryInputDto[] dtos = (CategoryInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<Category> allCategories = new HashSet<Category>();

            foreach (CategoryInputDto categoryDto in dtos)
            {
                if (categoryDto.Name == null)
                {
                    continue;
                }
                Category category = new Category()
                {
                    Name = categoryDto.Name
                };
                allCategories.Add(category);
            }

            context.Categories.AddRange(allCategories);
            context.SaveChanges();

            return $"Successfully imported {allCategories.Count}";
        }
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("CategoryProducts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryProductInputDto[]), xmlRoot);


            using StringReader sr = new StringReader(inputXml);

            CategoryProductInputDto[] dtos = (CategoryProductInputDto[])xmlSerializer.Deserialize(sr);

            ICollection<CategoryProduct> categoryProducts = new HashSet<CategoryProduct>();

            var categories = context.Categories.Select(c => c.Id).ToList();
            var products = context.Products.Select(p => p.Id).ToList();

            foreach (CategoryProductInputDto categoryProductDto in dtos)
            {
                if (categories.Contains(categoryProductDto.CategoryId)
                    && products.Contains(categoryProductDto.ProductId))
                {

                    CategoryProduct cp = new CategoryProduct()
                    {
                        CategoryId = categoryProductDto.CategoryId,
                        ProductId = categoryProductDto.ProductId
                    };
                    categoryProducts.Add(cp);
                }
            }

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count}";
        }
        public static string GetProductsInRange(ProductShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Products");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductsInRangeOutputDto[]), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            ProductsInRangeOutputDto[] productsInRanges = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Take(10)
                .Select(p => new ProductsInRangeOutputDto
                {
                    Name = p.Name,
                    Price = p.Price,
                    Buyer = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .ToArray();

            xmlSerializer.Serialize(sw, productsInRanges, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetSoldProducts(ProductShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Users");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UsersOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<UsersOutputDto> usersWithProducts = context
               .Users
               .Include(p => p.ProductsSold)
               .Where(x => x.ProductsSold.Any(y => y.Buyer != null))
               .OrderBy(x => x.LastName)
               .ThenBy(x => x.FirstName)
               .Take(5)
               .Select(x => new UsersOutputDto
               {
                   FirstName = x.FirstName,
                   LastName = x.LastName,
                   SoldProducts = x.ProductsSold
                   .Select(p => new ProductsOutputDto
                   {
                       Name = p.Name,
                       Price = p.Price,
                   })
                   .ToList()
               })
               .ToList();

            xmlSerializer.Serialize(sw, usersWithProducts, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Categories");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CategoriesOutputDto>), xmlRoot);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            List<CategoriesOutputDto> categories = context
             .Categories
             .Select(x => new CategoriesOutputDto
             {
                 Name = x.Name,
                 Count = x.CategoryProducts.Count,
                 AveragePrice = x.CategoryProducts.Average(x => x.Product.Price),
                 TotalRevenue = x.CategoryProducts.Sum(x => x.Product.Price)
             })
             .OrderByDescending(x => x.Count)
             .ThenBy(x => x.TotalRevenue)
             .ToList();

            xmlSerializer.Serialize(sw, categories, namespaces);

            return sb.ToString().TrimEnd();
        }
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = new UserRootDTO()
            {
                Count = context.Users.Count(u => u.FirstName != null),

                Users = context.Users
                    .Where(u => u.ProductsSold.Count > 0)
                    .OrderByDescending(u => u.ProductsSold.Count)
                    .Select(u => new UserExportDTO
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Age = u.Age,
                        SoldProducts = new ProductSoldRootDTO
                        {
                            Count = u.ProductsSold.Count(p => p.Buyer != null),
                            Products = u.ProductsSold.Where(p => p.Buyer != null)
                                .Select(p => new ProductSoldDTO
                                {
                                    Name = p.Name,
                                    Price = p.Price
                                })
                                .OrderByDescending(p => p.Price)
                                .ToList()
                        }
                    })
                    .Take(10)
                    .ToList()
            };


            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(UserRootDTO), new XmlRootAttribute("Users"));
            XmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().Trim();

        }
    }
}