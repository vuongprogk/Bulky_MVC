using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> objectCompanysList = _unitOfWork
                .CompanyRepository.GetAll()
                .ToList();
            return View(objectCompanysList);
        }

        public IActionResult Upsert(int? id)
        {
            
            if (id == null || id == 0)
            {
                // Create
                Company companyObj = new Company();
                return View(companyObj);
            }
            else
            {
                Company companyObj = _unitOfWork.CompanyRepository.GetFirstOrDefault(u => u.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company CompanyObj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
               
                if (CompanyObj.Id == 0)
                {
                    _unitOfWork.CompanyRepository.Add(CompanyObj);
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(CompanyObj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company is created successfully";
                return RedirectToAction("Index", "Company");
            }
            else
            {
                return View(CompanyObj);
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objectCompanysList = _unitOfWork
                .CompanyRepository.GetAll()
                .ToList();
            return Json(new { data = objectCompanysList });
        }



        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company CompanyToBeDeleted = _unitOfWork.CompanyRepository.GetFirstOrDefault(Company => Company.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Company to be deleted not found" });
            }
            
            _unitOfWork.CompanyRepository.Delete(CompanyToBeDeleted);
            _unitOfWork.Save();


            return Json(new { succes = true, message = "Delete successful" });
        }
        #endregion
    }
}
