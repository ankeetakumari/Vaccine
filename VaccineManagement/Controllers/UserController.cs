using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VaccineManagement.Models;

namespace VaccineManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        VaccineBMContext dc = new VaccineBMContext();
        [HttpPost]
        [Route("fb")]
        public string Feedback(FeedbackTbl f) 
        {
            
        dc.FeedbackTbls.Add(f);
            dc.SaveChanges();
           return "Your Feedback has been submitted";
        }
    }
}
