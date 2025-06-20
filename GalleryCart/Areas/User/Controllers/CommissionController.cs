using GalleryCart.DataAccess;
using GalleryCart.DataAccess.Repository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace GalleryCart.Areas.User.Controllers
{
    [Area("User")]
    public class CommissionController : Controller
    {
        private readonly CommissionRepository _commissionRepository;
        public CommissionController(CommissionRepository commissionRepository)
        {
            _commissionRepository = commissionRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(Commission model)
        {
            if (ModelState.IsValid)
            {
                model.CommissionId = Guid.NewGuid();
                model.CreatedDate = DateTime.Now;
                model.Status = "Pending";
                await _commissionRepository.AddAsync(model);
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "There were errors in your submission. Please correct them and try again.";
            }
            return View(model);
        }

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }
    }
}
