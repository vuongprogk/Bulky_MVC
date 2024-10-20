using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objectProductsList = _unitOfWork
                .ProductRepository.GetAll(includeProperties: "Category")
                .ToList();
            return View(objectProductsList);
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> listCategories = _unitOfWork
                .CategoryRepository.GetAll()
                .Select(u => new SelectListItem() { Text = u.Name, Value = u.ID.ToString() });
            ProductVM productVM = new ProductVM()
            {
                ListCategory = listCategories,
                Product = new Product(),
            };
            if (id == null || id == 0)
            {
                // Create
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.ProductRepository.GetFirstOrDefault(u =>
                    u.Id == id
                );
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    if (!String.IsNullOrEmpty(productVM.Product.ImageURL))
                    {
                        var oldImagePath = Path.Combine(
                            wwwRootPath,
                            productVM.Product.ImageURL.TrimStart('\\')
                        );
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (
                        var fileStream = new FileStream(
                            Path.Combine(productPath, fileName),
                            FileMode.Create
                        )
                    )
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageURL = @"\images\product\" + fileName;
                }
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(productVM.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product is created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.ListCategory = _unitOfWork
                    .CategoryRepository.GetAll()
                    .Select(u => new SelectListItem() { Text = u.Name, Value = u.ID.ToString() });
                return View(productVM);
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objectProductsList = _unitOfWork
                .ProductRepository.GetAll(includeProperties: "Category")
                .ToList();
            return Json(new { data = objectProductsList });
        }



        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Product productToBeDeleted = _unitOfWork.ProductRepository.GetFirstOrDefault(product => product.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Product to be deleted not found" });
            }
            var oldImagePath = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            productToBeDeleted.ImageURL.TrimStart('\\')
                        );
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.ProductRepository.Delete(productToBeDeleted);
            _unitOfWork.Save();


            return Json(new { succes = true, message = "Delete successful" });
        }
        #endregion
    }
}
