using Fruitables.Data;
using Fruitables.Models;
using Fruitables.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var vm = new HomeVM
            {
                Categories = await _context.Categories.ToListAsync(),
                Products = await _context.Products.Include(p => p.Category).ToListAsync()
            };

            return View(vm);
        }
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
        public async Task<IActionResult> Search(string? searchText, int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p => p.Name.Contains(searchText));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var products = await query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Description,
                Image = p.Image,
                Category = p.Category.Name
            }).ToListAsync();

            return Json(products);
        }
        public async Task<IActionResult> Sort(string sort)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;

                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
            }

            var products = await query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Description,
                p.Image,
                Category = p.Category.Name
            }).ToListAsync();

            return Json(products);
        }
        public async Task<IActionResult> FilterByPrice(string maxPrice)
        {
            decimal price = decimal.Parse(
                maxPrice,
                System.Globalization.CultureInfo.InvariantCulture
            );

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Price <= price)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    p.Image,
                    Category = p.Category.Name
                })
                .ToListAsync();

            return Json(products);
        }



    }
}
