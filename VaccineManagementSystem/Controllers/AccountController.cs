using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaccineManagementSystem.Models;
using DNTCaptcha.Core.Providers;
using DNTCaptcha.Core;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;


namespace VaccineManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        public readonly IDNTCaptchaValidatorService _validatorService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //DinkToPdf.HtmlToPdfDocument a = new DinkToPdf.HtmlToPdfDocument();
        public AccountController(IDNTCaptchaValidatorService validatorService, IHttpContextAccessor httpContextAccessor )
        {
            _validatorService = validatorService;
            _httpContextAccessor = httpContextAccessor;

        }
        
        
        //public AccountController()
        //{

        //}
        public IActionResult Index()
        {
          
            ViewData["Layout"] = "_Layout";
            return View();
        }


        VaccineBMContext dc = new VaccineBMContext();


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha",
            CaptchaGeneratorLanguage = Language.English,
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult Register(UserTbl r, string Password)
        {
            r.Password = Encoding.UTF8.GetBytes(Password);

            if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
            {
                this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
            }
            dc.UserTbls.Add(r);
            int i = dc.SaveChanges();
            if (i > 0)
            {
                ViewData["v"] = "User Registered Successfully";
            }
            else
            {
                ViewData["v"] = "Failed to Registered";
            }
            return View();
        }

        [HttpGet]
        public IActionResult Default()
        {
            var res = dc.UserTbls.ToList();

            return View(res);
        }









        [HttpGet]
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please Enter Valid Captcha",
        CaptchaGeneratorLanguage = Language.English,
        CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult Login( string t2, string t1)
        {
            if (ModelState.IsValid)
            {

                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
                    return View();
                }


                var res = dc.UserTbls.FirstOrDefault(t => t.UserName == t2 && t.Password == Encoding.UTF8.GetBytes(t1));

                if (res != null)
                {

                    HttpContext.Session.SetString("UserSession", t1);
                    return RedirectToAction("display");
                }
                else
                {
                    ViewData["v"] = "Invalid User..!!!!";
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

            HttpContext.Session.Remove("UserSession");
            return RedirectToAction("Login");
        }







        [HttpGet]
        public IActionResult display()
        {
            return View();
        }
        [HttpPost]
        public IActionResult display(string a)
        {
            return View();
        }
        







         [HttpGet]
    
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
     

        public IActionResult ResetPassword(string t, string newPassword, string conPassword, string oldPassword, UserTbl u)
        {
            

            byte[] p = Encoding.UTF8.GetBytes(oldPassword);

            var res = (from r in dc.UserTbls
                       where r.UserName == t && r.Password == p
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
                }else
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







        public string d()
        {
            try
            {
                throw new DivideByZeroException();

            }
            catch (Exception ex)
            {
                Log.Error("Error Occured: " + ex.Message + " : " + DateTime.Now);
            }
            return null;
        }






       




        [HttpGet]
        public IActionResult Search()
        {
            ViewBag.VaccineNames = dc.CommonVaccinetbls.Select(s=>s.VaccineName).ToList();
            ViewBag.States = dc.CommonVaccinetbls.Select(s => s.State).ToList();
            ViewBag.cities = dc.CommonVaccinetbls.Select(s => s.City).ToList();
            return View();
        }


        [HttpGet]

        public IActionResult DisplayAvailability()
        {
            


            string vc = Request.Query["vaccineName"].ToString();

            string s = Request.Query["state"].ToString();

            string c = Request.Query["city"].ToString();

            HttpContext.Session.SetString("state", s);
            HttpContext.Session.SetString("city", c);

            var res = from t in dc.CommonVaccinetbls
                      where t.City == c && t.VaccineName == vc  & t.State == s
                      select t;


            return View(res.ToList());

        }




        [HttpGet]
        public IActionResult BookSlotForFamilyMember()
        {

            var s = HttpContext.Session.GetString("UserSession");


            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            var familyMembers = (from t in dc.FamilyMembersTbls
                                 where t.UserId == x
                                 select t).ToList();

            ViewBag.FamilyMembers = new SelectList(familyMembers, "MId", "MName");

            return View();
        }

        [HttpPost]
        public IActionResult BookSlotForFamilyMember(VaccineSlotsTbl a)
        {
            var s = HttpContext.Session.GetString("UserSession");
            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            a.State = HttpContext.Session.GetString("state");
            a.City = HttpContext.Session.GetString("city");

            a.BookedByUserId = x;
            if (x != 0)
            {
                a.IsBooked = true;
            }
            dc.VaccineSlotsTbls.Add(a);
            int i = dc.SaveChanges();
            if (i > 0)
            {
                ViewData["v"] = "Slot Booked Successfully";
            }
            else
            {
                ViewData["v"] = "Failed to Book Slot";
            }

            return View();
        }



     



        [HttpPost]
        public IActionResult DeleteS()
        {
            var s = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(s))
            {
                ViewData["v"] = "User session is not available.";
                return View();
            }

            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            var res = (from t in dc.VaccineSlotsTbls
                       where t.BookedByUserId == x
                       select t).FirstOrDefault();

            if (res != null)
            {
                dc.VaccineSlotsTbls.Remove(res);
                int i = dc.SaveChanges();
                if (i > 0)
                {
                    ViewData["v"] = "Deleted Successfully";
                }
                else
                {
                    ViewData["v"] = "Failed to delete slot.";
                }
            }
            else
            {
                ViewData["v"] = "No slot found to delete.";
            }

            return View();
        }







 


        [HttpGet]
        public IActionResult ViewSlots()
        {

            var res = from t in dc.VaccineSlotsTbls
                      select t;
            return View(res.ToList());
        }

        [HttpPost]
        public IActionResult ViewSlots(string a)
        {
            return View();
        }







        [HttpGet]
        public IActionResult BookSlot()
        {

            var s = HttpContext.Session.GetString("UserSession");


            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            var familyMembers = (from t in dc.FamilyMembersTbls
                                 where t.UserId == x
                                 select t).ToList();

            ViewBag.FamilyMembers = new SelectList(familyMembers, "MId", "MName");

            return View();
        }

        [HttpPost]
        public IActionResult BookSlot(VaccineSlotsTbl a)
        {
            var s = HttpContext.Session.GetString("UserSession");
            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            a.State = HttpContext.Session.GetString("state");
            a.City = HttpContext.Session.GetString("city");

            a.BookedByUserId = x;
            if (x != 0)
            {
                a.IsBooked = true;
            }
            dc.VaccineSlotsTbls.Add(a);
            int i = dc.SaveChanges();
            if (i > 0)
            {
                ViewData["v"] = "Slot Booked Successfully";
            }
            else
            {
                ViewData["v"] = "Failed to Book Slot";
            }

            return View();
        }


        [HttpGet]
        public ViewResult AddFamilyMember()
        {
         
            return View();
        }

        [HttpPost]
        public ViewResult AddFamilyMember(FamilyMembersTbl r)
        {

            var s = HttpContext.Session.GetString("UserSession");
            int x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            var res = (from t in dc.UserTbls
                       where t.UserId == x
                       select t).ToList();



            if (res != null)
            {
                r.UserId = x;
                dc.FamilyMembersTbls.Add(r);
                int i = dc.SaveChanges();

                if (i > 0)
                {
                    ViewData["v"] = "Added member successfully";
                }
                else
                {
                    ViewData["v"] = "Failed to add member";
                }
            }
            else
            {
                ViewData["v"] = "No user found to assign UserId";
            }


            return View();
        }








        [HttpGet]
        public IActionResult SubmitFeedback()
        {

        return View();
        }
        [HttpPost]

        public async Task<ActionResult> SubmitFeedback(FeedbackTbl f)
        {
            // string res = null;

            var httpClient = new HttpClient();

        
            var response = await httpClient.PostAsJsonAsync("https://localhost:7054/api/User/fb", f);

            string responseContent = await response.Content.ReadAsStringAsync();

            
              

                ViewData["msg"] = "Data Submitted";
         

            return View();
        }


        [HttpGet]
        public IActionResult RescheduleSlot()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RescheduleSlot(VaccineSlotsTbl a)
        {
            var s = HttpContext.Session.GetString("UserSession");

            var x = (from u in dc.UserTbls where u.UserName == s select u.UserId).FirstOrDefault();

            var res = (from t in dc.VaccineSlotsTbls
                       where t.BookedByUserId == x
                       select t).FirstOrDefault();


            if (res != null)
            {
                res.SlotTime = a.SlotTime;
                res.YearDate = a.YearDate;
                dc.VaccineSlotsTbls.Update(res);
                int i = dc.SaveChanges();
                if (i > 0)
                {
                    ViewData["v"] = "Reschedule Successfully";
                }
                else
                {
                    ViewData["v"] = "Failed to Reschedule";
                }
            }

            return View();
        }


        //[HttpGet]
        //public async Task<IActionResult> DownloadCertificate()
        //{
         
        //    var vaccineHistoryList = dc.VaccineHistories
        //         .Select(v => new
        //         {
        //             v.UserId,
        //             v.FamilyMemberId,
        //             v.VaccineSlotId,
        //             v.VaccineDate
        //         }).ToList();

        //    ViewBag.VaccineHistoryList = vaccineHistoryList;
        //    return View();
        //}
        //[HttpPost]
        //public async Task<IActionResult> DownloadCertificate(int id)
        //{
        //    var vaccinationHistory = await dc.VaccineHistories
        //       .FindAsync(id);

        //    if (vaccinationHistory == null)
        //    {
        //        return NotFound();
        //    }

        //    // Create a certificate content
        //    var certificateContent = $"Vaccination Certificate\n\n" +
        //                              $"User ID: {vaccinationHistory.UserId}\n" +
        //                              $"Family Member ID: {vaccinationHistory.FamilyMemberId?.ToString() ?? "N/A"}\n" +
        //                              $"Vaccine Slot ID: {vaccinationHistory.VaccineSlotId}\n" +
        //                              $"Vaccine Date: {vaccinationHistory.VaccineDate.ToString()}\n";

        //    // Convert the content to bytes
        //    var bytes = System.Text.Encoding.UTF8.GetBytes(certificateContent);


        //    // Return the file as a downloadable file
        //    return File(bytes, "application/octet-stream", "VaccinationCertificate.pdf");
        //}


        //[HttpPost]
        //public IActionResult VaccinationHistory(VaccineHistory r)
        //{
         
        //  // Assuming you have a DbContext named 'context' and a User table
        //  var user = dc.VaccineHistories.FirstOrDefault(u => u.User == r.User);

        //    if (user != null)
        //    {
        //        return RedirectToAction("DisplayAvailability");
        //    }
        //    else
        //    {
        //        ViewBag.Response = "User not found.";
        //    }
        //    return View();

        //}

    }
}
