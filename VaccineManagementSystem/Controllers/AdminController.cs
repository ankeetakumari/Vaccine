using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Text;
using VaccineManagementSystem.Models;
using Microsoft.AspNetCore.Http;

namespace VaccineManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        public readonly IDNTCaptchaValidatorService _validatorService;
        public AdminController(IDNTCaptchaValidatorService validatorService)
        {
            _validatorService = validatorService;
        }
        //public AccountController()
        //{

        //}

        public IActionResult Index()
        {
            return View();
        }

        VaccineBMContext dc = new VaccineBMContext();


        [HttpGet]
        public ViewResult RegisterAdmin()
        {
            return View();
        }


        [HttpPost]
        public IActionResult RegisterAdmin(Admintbl r, string Password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    r.Password = Encoding.UTF8.GetBytes(Password);

                    dc.Admintbls.Add(r);
                    int i = dc.SaveChanges();
                    if (i > 0)
                    {
                        ViewData["v"] = "User Registered Successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["v"] = "An error occurred while registering the user";
            }
            return View();
        }



        [HttpGet]
        public ViewResult AdminLogin()
        {
            return View();
        }
        [HttpPost]
       
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha",
            CaptchaGeneratorLanguage = Language.English,
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult AdminLogin(string t2, string t1)
        {

            if (ModelState.IsValid)
            {

                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
                    return View();
                }

                var res = dc.Admintbls.FirstOrDefault(t => t.AdminName == t1 && t.Password == Encoding.UTF8.GetBytes(t2));

                if (res != null)
                {

                    HttpContext.Session.SetString("AdminSession", t1);
                    return RedirectToAction("AdminDashboard");

                }
                else
                {
                    ViewData["a"] = "Invalid User..!!!!";
                    return View();
                }
            }
            else
            {
                ViewData["v"] = "Invalid user";
                return View();
            }
        }


        public IActionResult Logout()
        {

            HttpContext.Session.Remove("AdminSession");
            return RedirectToAction("AdminLogin");
        }




        [HttpGet]

        public IActionResult ResetPasswordAdmin() 
        {
            return View();
        }
        
        [HttpPost]

        public IActionResult ResetPasswordAdmin(string t, string newPassword, string conPassword, string oldPassword, Admintbl u)
        {


            byte[] p = Encoding.UTF8.GetBytes(oldPassword);

            var res = (from r in dc.Admintbls
                       where r.AdminName == t && r.Password == p
                       select r).FirstOrDefault();

            if (res != null)
            {
                if (newPassword == conPassword)
                {
                    res.Password = Encoding.UTF8.GetBytes(newPassword);

                    int i = dc.SaveChanges();

                    if (i > 0)
                    {
                        ViewData["x"] = "Reset successfully";
                    }
                    else
                    {
                        ViewData["x"] = "Failed to reset password";
                    }
                }
                else
                {
                    ViewData["x"] = "New Password and Confirm Password Mismatch";
                }

            }
            else
            {
                ViewData["x"] = "User not found";
            }

            return View();
        }



        [HttpGet]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminDashboard(string a)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search()
        {
            ViewBag.UserNames = dc.UserTbls.Select(s => s.UserName).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Search(UserTbl r)
        {
            var results = dc.UserTbls.AsQueryable();

            if (!string.IsNullOrEmpty(r.UserName))
            {
                results = results.Where(v => v.UserName == r.UserName);


                return RedirectToAction("DisplayUserAvailability");
            }
            else
            {
                ViewData["v"] = " User Not Found";
            }


            return View();
        }


        

        [HttpGet]

        public IActionResult DisplayUserAvailability(string c)
        {


            string vc = Request.Query["UserName"].ToString();




            var res = from t in dc.UserTbls.AsQueryable()
                      where t.UserName == c
                      select t;


            return View(res.ToList());
        }


        [HttpGet]
        public IActionResult AddVaccine()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddVaccine(CommonVaccinetbl r)
        {

            dc.CommonVaccinetbls.Add(r);
            int i = dc.SaveChanges();
            if (i > 0)
            {
                ViewData["v"] = "Vaccine added Successfully";
            }
            else
            {
                ViewData["v"] = "Failed to add";
            }
            return View();
        }



        [HttpGet]
        public IActionResult DeleteVaccine(int id)
        {
            var vaccine = dc.CommonVaccinetbls.Find(id);
            if (vaccine == null)
            {
                return NotFound();
            }
            return View(vaccine);
        }

        [HttpPost, ActionName("DeleteVaccine")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVaccineConfirmed(int id)
        {
            var vaccine = dc.CommonVaccinetbls.Find(id);
            if (vaccine != null)
            {
                dc.CommonVaccinetbls.Remove(vaccine);
                try
                {
                    int result = dc.SaveChanges();
                    if (result > 0)
                    {
                        /*  TempData["SuccessMessage"] = "Vaccine deleted successfully!";*/
                        ViewData["Message"] = "Vaccine Deleted Successfully";
                        return RedirectToAction("ViewVaccine");
                    }
                    else
                    {
                        ViewData["Message"] = "Failed to delete vaccine.";
                    }
                }
                catch (Exception ex)
                {

                    ViewData["Message"] = $"An error occurred while deleting the vaccine: {ex.Message}";
                }
            }
            else
            {
                ViewData["Message"] = "Vaccine not found.";
            }
            return View(vaccine);
        }



        [HttpGet]
        public IActionResult UpdateVaccine(int id)
        {
            var vaccine = dc.CommonVaccinetbls.Find(id);
            if (vaccine == null)
            {
                return NotFound();
            }
            return View(vaccine);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateVaccine(CommonVaccinetbl vaccine)
        {
            if (ModelState.IsValid)
            {
                dc.CommonVaccinetbls.Update(vaccine);
                try
                {
                    int result = dc.SaveChanges();
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Vaccine details updated successfully!";
                        return RedirectToAction("ViewVaccine"); // Ensure this action returns a list of vaccines
                    }
                    else
                    {
                        ViewData["Message"] = "Failed to update vaccine.";
                    }
                }
                catch (Exception ex)
                {
                    ViewData["Message"] = $"An error occurred while updating the vaccine: {ex.Message}";
                }
            }
            else
            {
                ViewData["Message"] = "Model state is invalid.";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ViewData["Message"] += $" {error.ErrorMessage}";
                }
            }
            return View(vaccine);
        }


        [HttpGet]
        public IActionResult ViewVaccine()
        {

            var res = from t in dc.CommonVaccinetbls
                      select t;
            return View(res.ToList());
        }

        [HttpPost]
        public IActionResult ViewVaccine(CommonVaccinetbl t)
        {
            return View(t);
        }




    }
}
