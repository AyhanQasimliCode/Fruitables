using Fruitables.Areas.Admin.ViewModels.ProductVM;
using Fruitables.Data;
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

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category) 
                .Select(p => new GetAllProductVM
                {
                    Id = p.Id,
                    Image = p.Image,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return View(products); 
        }
        public async Task<IActionResult> Create()
        {
            var vm = new CreateProductVM
            {
                Categories = await _context.Categories.ToListAsync()
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM request)
        {
            request.Categories = await _context.Categories.ToListAsync();

            if (!ModelState.IsValid)
                return View(request);

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            {
                ModelState.AddModelError("CategoryId", " kateqoriya tapilmadı");
                return View(request);
            }

            if (request.Image == null || !request.Image.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", " format duzgun deyil");
                return View(request);
            }

            if (request.Image.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Image", " maksimum 2MB ola bilər");
                return View(request);
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
            string path = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            Product product = new()
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Image = fileName,
                CategoryId = request.CategoryId
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var vm = new UpdateProductVM
            {
                Id = product.Id, 
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ExistingImage = product.Image,
                CategoryId = product.CategoryId,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM request)
        {
            if (id != request.Id) return BadRequest();

            var product = await _context.Products.FindAsync(request.Id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                request.Categories = await _context.Categories.ToListAsync();
                request.ExistingImage = product.Image; 
                return View(request);
            }

            if (request.Image != null)
            {
                if (!request.Image.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("Image", " format duzgun deyil");
                    request.Categories = await _context.Categories.ToListAsync();
                    request.ExistingImage = product.Image;
                    return View(request);
                }

                if (request.Image.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", " maksimum 2MB ola bilər");
                    request.Categories = await _context.Categories.ToListAsync();
                    request.ExistingImage = product.Image;
                    return View(request);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(product.Image))
                {
                    string oldPath = Path.Combine(uploadsFolder, product.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
                string newPath = Path.Combine(uploadsFolder, fileName);
                using (var fs = new FileStream(newPath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(fs);
                }

                product.Image = fileName;
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new DetailProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Image = product.Image, 
                CategoryName = product.Category?.Name
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.Image))
            {
                string path = Path.Combine(_env.WebRootPath, "uploads", product.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}