using Fruitables.Areas.Admin.ViewModels.ProductVM;
using Fruitables.Data;
using Fruitables.Helpers;
using Fruitables.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1, int take = 5)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Skip((page - 1) * take)
                .Take(take)
                .Select(p => new GetAllProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.Image,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            int count = await _context.Products.CountAsync();
            int pageCount = (int)Math.Ceiling((decimal)count / take);

            Paginate<GetAllProductVM> paginate = new(products, page, pageCount);
            return View(paginate);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateProductVM vm = new()
            {
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM request)
        {
            request.Categories = await _context.Categories.ToListAsync();
            request.Tags = await _context.Tags.ToListAsync();

            if (!ModelState.IsValid)
                return View(request);

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Kateqoriya tapılmadı");
                return View(request);
            }

            if (request.Image == null || !request.Image.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", "Sekil formatı duzgun deyil");
                return View(request);
            }

            if (request.Image.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Image", "Sekil maksimum 2MB ola bilər");
                return View(request);
            }

            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
            string filePath = Path.Combine(uploadPath, fileName);

            using (FileStream fs = new(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(fs);
            }

            Product product = new()
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Image = fileName,
                CategoryId = request.CategoryId,
                ProductTags = request.TagIds.Select(tagId => new ProductTag
                {
                    TagId = tagId
                }).ToList()
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            DetailProductVM vm = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Image = product.Image,
                CategoryName = product.Category.Name,
                Tags = product.ProductTags
                    .Select(pt => pt.Tag.Name)
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            UpdateProductVM vm = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImage = product.Image,
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM request)
        {
            if (id != request.Id) return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                request.Categories = await _context.Categories.ToListAsync();
                request.Tags = await _context.Tags.ToListAsync();
                request.ExistingImage = product.Image;
                return View(request);
            }

            // Image update
            if (request.Image != null)
            {
                if (!request.Image.ContentType.Contains("image/") ||
                    request.Image.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "Sekil duzgun deyil");
                    request.Categories = await _context.Categories.ToListAsync();
                    request.Tags = await _context.Tags.ToListAsync();
                    return View(request);
                }

                string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                if (!string.IsNullOrEmpty(product.Image))
                {
                    string oldPath = Path.Combine(uploadPath, product.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
                string newPath = Path.Combine(uploadPath, fileName);

                using FileStream fs = new(newPath, FileMode.Create);
                await request.Image.CopyToAsync(fs);

                product.Image = fileName;
            }
            product.ProductTags.Clear();
            foreach (int tagId in request.TagIds)
            {
                product.ProductTags.Add(new ProductTag
                {
                    TagId = tagId
                });
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.Image))
            {
                string path = Path.Combine(_env.WebRootPath, "uploads", product.Image);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}