using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> objectCatagoriesList = _unitOfWork.CategoryRepository.GetAll().ToList();
            return View(objectCatagoriesList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Catagory is created successfully";
                return RedirectToAction("Index", "Catagory");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category catagory = _unitOfWork.CategoryRepository.GetFirstOrDefault(catagory =>
                catagory.ID == id
            );
            if (catagory == null)
            {
                return NotFound();
            }
            return View(catagory);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Catagory is updated successfully";
                return RedirectToAction("Index", "Catagory");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category catagory = _unitOfWork.CategoryRepository.GetFirstOrDefault(catagory =>
                catagory.ID == id
            );
            if (catagory == null)
            {
                return NotFound();
            }
            return View(catagory);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category catagory = _unitOfWork.CategoryRepository.GetFirstOrDefault(catagory =>
                catagory.ID == id
            );
            if (catagory == null)
            {
                return NotFound();
            }
            _unitOfWork.CategoryRepository.Delete(catagory);
            _unitOfWork.Save();
            TempData["success"] = "Catagory is deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
