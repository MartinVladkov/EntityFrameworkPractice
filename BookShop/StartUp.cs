namespace BookShop
{
	using BookShop.Models.Enums;
	using Data;
    using Initializer;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class StartUp
    {
        public static void Main()
        {
            using var dbContext = new BookShopContext();
            DbInitializer.ResetDatabase(dbContext);

            //string ageRestricitonString = Console.ReadLine();
            string result = CountCopiesByAuthor(dbContext);

			Console.WriteLine(result);
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
		{
            StringBuilder sb = new StringBuilder();

            AgeRestriction ageRestriction = Enum.Parse<AgeRestriction>(command, true);

            var books = context
                .Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

			foreach (string title in books)
			{
                sb.AppendLine(title);
			}

            return sb.ToString().TrimEnd();
		}

        public static string GetGoldenBooks(BookShopContext context)
		{
            StringBuilder sb = new StringBuilder();

            var goldenBooks = context
                .Books
                .Where(b => b.EditionType == EditionType.Gold)
                .Where(b => b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title);
                

			foreach (string book in goldenBooks)
			{
                sb.AppendLine(book);
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
		{
            StringBuilder sb = new StringBuilder();

            var priceBooks = context
                .Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .OrderByDescending(b => b.Price);

            foreach (var book in priceBooks)
            {
                sb.AppendLine($"{book.Title} - ${book.Price}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
		{
            StringBuilder sb = new StringBuilder();

            var yearBooks = context
               .Books
               .Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.Year != year)
               .Select(b => new
               {
                   b.BookId,
                   b.Title
               })
               .OrderBy(b => b.BookId);

			foreach (var book in yearBooks)
			{
                sb.AppendLine(book.Title);
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
		{
            StringBuilder sb = new StringBuilder();

            List<string> categories = input
                .ToLower()
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

			foreach (var category in categories)
			{
                var books = context
                    .Books
                    //.ToArray()
                    .Where(b => b.BookCategories.SelectMany(c => c.Category.Name).ToString() == category)
                    .Select(b => new { b.Title })
                    .OrderBy(b => b.Title);
                    

                foreach (var book in books)
                {
                    sb.AppendLine(book.Title);
                }
            }

            return sb.ToString().TrimEnd();
            
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
		{
            StringBuilder sb = new StringBuilder();

            var books = context
                .Books
                .ToList()
                .Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.CompareTo(DateTime.ParseExact(date,"dd-MM-yyyy",null)) < 0)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                });
                

			foreach (var book in books)
			{
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
		{
            StringBuilder sb = new StringBuilder();

            var authors = context
                .Authors
                .Where(b => b.FirstName.EndsWith(input))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName) 
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName
                })
                .ToList();

			foreach (var author in authors)
			{
                sb.AppendLine($"{author.FirstName} {author.LastName}");
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
		{
            StringBuilder sb = new StringBuilder();

            input = input.ToLower();

            var books = context
                .Books
                .Where(b => b.Title.ToLower().Contains(input))
                .OrderBy(b => b.Title)
                .Select(b => new { b.Title })
                .ToList();

			foreach (var book in books)
			{
                sb.AppendLine($"{book.Title}");
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
		{
            StringBuilder sb = new StringBuilder();

            input = input.ToLower();

            var books = context
                .Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    AuthorFirstName = b.Author.FirstName,
                    AuthorLastName = b.Author.LastName
                })
                .ToList();

			foreach (var book in books)
			{
                sb.AppendLine($"{book.Title} ({book.AuthorFirstName} {book.AuthorLastName})");
			}

            return sb.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
		{
            var books = context
                .Books
                .ToList()
                .Where(b => b.Title.Count() > lengthCheck)
                .ToList();

            return books.Count();
        }

        public static string CountCopiesByAuthor(BookShopContext context)
		{
            StringBuilder sb = new StringBuilder();

            var totalCopies = context
                .Authors
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    copiesCount = a.Books.Sum(b => b.Copies),
                })
                .OrderByDescending(a => a.copiesCount);

			foreach (var author in totalCopies)
			{
                sb.AppendLine($"{author.FirstName} {author.LastName} - {author.copiesCount}");
			}

            return sb.ToString().TrimEnd();
        }
		
		  public static string GetTotalProfitByCategory(BookShopContext context)
		{
            StringBuilder sb = new StringBuilder();

            var booksProfits = context
                .Categories
                .Select(c => new
                {
                    categoryName = c.Name,
                    totalProfit = c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price)
                })
                .OrderByDescending(c=> c.totalProfit)
                .ThenBy(c => c.categoryName);

			foreach (var category in booksProfits)
			{
                sb.AppendLine($"{category.categoryName} ${category.totalProfit:F2}");
			}

            return sb.ToString().TrimEnd();
        }

        public static string GetMostRecentBooks(BookShopContext context)
		{
            StringBuilder sb = new StringBuilder();

            var recentBooks = context
                .Categories
                .Select(c => new
				{
                    categoryName = c.Name,
                    mostRecentBooks = c.CategoryBooks
                    .Select(cb => cb.Book)
                    .OrderByDescending(b => b.ReleaseDate)
                    .Take(3)
                    .Select(b => new
					{
                        title = b.Title,
                        releaseYear = b.ReleaseDate.Value.Year
                    }).ToArray()
				})
                .OrderBy(c => c.categoryName)
                .ToArray();

			foreach (var category in recentBooks)
			{
                sb.AppendLine($"--{category.categoryName}");

                foreach (var book in category.mostRecentBooks)
				{
                    sb.AppendLine($"{book.title} ({book.releaseYear})");

                }
                    
			}

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
		{
            var booksToIncrease = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToArray();

			foreach (var book in booksToIncrease)
			{
                book.Price += 5;
			}

            context.SaveChanges();

		}

        public static int RemoveBooks(BookShopContext context)
		{
            var booksToRemove = context
                .Books
                .Where(b => b.Copies < 4200)
                .ToArray();

            int count = booksToRemove.Count();

			foreach (var book in booksToRemove)
			{
                context.Books.Remove(book);
            }
            
            return count;
	}
    }
}
