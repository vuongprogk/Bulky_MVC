using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
<<<<<<< HEAD
using System.Collections.Generic;
=======
>>>>>>> 8c8009025763747871efff86c56554ec7e7a5629

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> objectProductsList = _unitOfWork.ProductRepository.GetAll().ToList();
<<<<<<< HEAD
            IEnumerable<SelectListItem> categoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem()
            {
                Text = u.Name,
                Value = u.ID.ToString(),
            });
=======
>>>>>>> 8c8009025763747871efff86c56554ec7e7a5629
         
            return View(objectProductsList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> listCategories = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem()
            {
                Text = u.Name,
                Value = u.ID.ToString(),
            });
            ViewBag.ListCategories = listCategories;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Product is created successfully";
                return RedirectToAction("Index", "Product");
            }
            return View(obj);
        }

        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            Product Product = _unitOfWork.ProductRepository.GetFirstOrDefault(Product =>
                Product.Id == Id
            );
            if (Product == null)
            {
                return NotFound();
            }
            return View(Product);
        }

        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Product is updated successfully";
                return RedirectToAction("Index", "Product");
            }
            return View(obj);
        }

        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            Product Product = _unitOfWork.ProductRepository.GetFirstOrDefault(Product =>
                Product.Id == Id
            );
            if (Product == null)
            {
                return NotFound();
            }
            return View(Product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? Id)
        {
            Product Product = _unitOfWork.ProductRepository.GetFirstOrDefault(Product =>
                Product.Id == Id
            );
            if (Product == null)
            {
                return NotFound();
            }
            _unitOfWork.ProductRepository.Delete(Product);
            _unitOfWork.Save();
            TempData["success"] = "Product is deleted successfully";
            return RedirectToAction("Index");

        }
    }
}
