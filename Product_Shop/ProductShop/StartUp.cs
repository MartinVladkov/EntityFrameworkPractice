using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DTOs;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        static IMapper mapper;

        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var usersJson  = File.ReadAllText("Datasets/users.json");
            var productsJson = File.ReadAllText("Datasets/products.json");
            var categoriesJson = File.ReadAllText("Datasets/categories.json");
            var categoriesProdcutsJson = File.ReadAllText("Datasets/categories-products.json");

            ImportUsers(context, usersJson);
            ImportProducts(context, productsJson);
            ImportCategories(context, categoriesJson);
            ImportCategoryProducts(context, categoriesProdcutsJson);

            //Console.WriteLine(result);

            var result = GetUsersWithProducts(context);
            Console.WriteLine(result);
        }

        private static void InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            mapper = config.CreateMapper();
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();

            IEnumerable<UserInputDto> dtoUsers = JsonConvert.DeserializeObject<IEnumerable<UserInputDto>>(inputJson);

            var users = mapper.Map<IEnumerable<User>>(dtoUsers);
            
            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();
            var dtoProducts = JsonConvert.DeserializeObject<IEnumerable<ProductInputModel>>(inputJson);

            var products = mapper.Map<IEnumerable<Product>>(dtoProducts);
            
            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();

            var dtoCategories = JsonConvert.DeserializeObject<IEnumerable<CategoryInputModel>>(inputJson)
                .Where(c => c.Name != null)
                .ToList();
            var categories = mapper.Map<IEnumerable<Category>>(dtoCategories);
            
            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();
            var dtoCategoryProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryProductInputModel>>(inputJson);

            var categoryProducts = mapper.Map<IEnumerable<CategoryProduct>>(dtoCategoryProducts);
            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count()}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = $"{p.Seller.FirstName} {p.Seller.LastName}"
                })
                .OrderBy(p => p.price)
                .ToArray();

            var result = JsonConvert.SerializeObject(products, Formatting.Indented);

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldProducts = context.Users
                .Where(p => p.ProductsSold.Any(aa => aa.BuyerId != null))
                .Select(p => new
                {
                    firstName = p.FirstName,
                    lastName = p.LastName,
                    soldProducts = p.ProductsSold.Where(pr => pr.BuyerId != null)
                    .Select(pr => new
                    {
                        name = pr.Name,
                        price = pr.Price,
                        buyerFirstName = pr.Buyer.FirstName,
                        buyerLastName = pr.Buyer.LastName
                    })
                    .ToArray()
                })
                .OrderBy(p => p.lastName)
                .ThenBy(p => p.firstName)
                .ToArray();

            var result = JsonConvert.SerializeObject(soldProducts, Formatting.Indented);
            return result;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context
                .Categories
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoryProducts.Count(),
                    averagePrice = (c.CategoryProducts.Select(p => p.Product.Price).Sum() / c.CategoryProducts.Count()).ToString("F2"),
                    totalRevenue = (c.CategoryProducts.Select(p => p.Product.Price).Sum()).ToString("F2")
                })
                .OrderByDescending(c => c.productsCount)
                .ToList();

            var result = JsonConvert.SerializeObject(categories, Formatting.Indented);
            return result;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Include(x => x.ProductsSold)
                .ToList()
                .Where(p => p.ProductsSold.Any(aa => aa.BuyerId != null))
                .Select(p => new
                {
                    firstName = p.FirstName,
                    lastName = p.LastName,
                    age = p.Age,
                    soldProducts = new
                    {
                        count = p.ProductsSold.Where(x => x.BuyerId != null).Count(),
                        products = p.ProductsSold.Where(x => x.BuyerId != null).Select(s => new
                        {
                            name = s.Name,
                            price = s.Price
                        })
                    }
                })
                .OrderByDescending(x => x.soldProducts.products.Count())
                .ToList();

            var resultObject = new
            {
                usersCount = context.Users.Where(p => p.ProductsSold.Any(aa => aa.BuyerId != null)).Count(),
                users = users
            };

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var result = JsonConvert.SerializeObject(resultObject, Formatting.Indented, serializerSettings);
            return result;  
        }
    } 
}