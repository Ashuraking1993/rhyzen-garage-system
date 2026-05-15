using Car_Rental.Data;
using Car_Rental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ControlPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ControlPanelController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.CarImages)
                .ToListAsync();

            return View(cars);
        }
        [HttpPost]
        public async Task<IActionResult> UploadImages(int carId, List<IFormFile> images)
        {
            foreach (var file in images)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var folder = Path.Combine(_env.WebRootPath, "images/cars");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _context.CarImages.Add(new CarImage
                {
                    CarId = carId,
                    ImagePath = "/images/cars/" + fileName,
                    IsMain = false
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFeatures(int carId, string features, string interior)
        {
            var car = await _context.Cars.FindAsync(carId);

            if (car != null)
            {
                car.Features = features;
                car.InteriorDetails = interior;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId);

            if (image == null)
                return NotFound();

            var carId = image.CarId; // store first (avoids null warning)

            // Remove existing main image
            var existingMain = await _context.CarImages
                .Where(i => i.CarId == carId && i.IsMain)
                .ToListAsync();

            foreach (var img in existingMain)
            {
                img.IsMain = false;
            }

            // Set new main
            image.IsMain = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId);
            if (image == null) return RedirectToAction("Index");

            var path = Path.Combine(_env.WebRootPath, image.ImagePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _context.CarImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }


}
