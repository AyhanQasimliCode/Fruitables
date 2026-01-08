using Fruitables.Areas.Admin.ViewModels.TagVM;
using Fruitables.Data;
using Fruitables.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags
                .Select(t => new GetAllTagVM
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();

            return View(tags);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM request)
        {
            if (!ModelState.IsValid)
                return View(request);

            bool isExist = await _context.Tags.AnyAsync(t =>
                t.Name.Trim().ToLower() == request.Name.Trim().ToLower());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda tag movcuddur!");
                return View(request);
            }

            Tag tag = new()
            {
                Name = request.Name.Trim()
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();

            DetailTagVM vm = new()
            {
                Id = tag.Id,
                Name = tag.Name
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();

            UpdateTagVM vm = new()
            {
                Id = tag.Id,
                Name = tag.Name
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateTagVM request)
        {
            if (id != request.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(request);

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();

            bool isExist = await _context.Tags.AnyAsync(t =>
                t.Name.Trim().ToLower() == request.Name.Trim().ToLower()
                && t.Id != id);

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda tag movcuddur!");
                return View(request);
            }

            tag.Name = request.Name.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
