//using Microsoft.AspNetCore.Mvc;
//using VaccineManagementSystem.Controllers;
//using VaccineManagementSystem.Models;

//namespace TestProject1
//{
//    public class UnitTest1
//    {
//        AccountController controller = new AccountController();

//        [Fact]

      

//        public void Login_ValidCredentials_RedirectsTodisplay()
//        {
//            // Arrange
//            string username = "Ankeeta Kumari";
//            string password = "Ankeeta30";
//            //UserTbl u = new UserTbl();
//            // Act
//            var result = controller.Login(username, password) as RedirectToActionResult;


//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("display", result.ActionName);
//        }

//        [Fact]
//        public void Login_InvalidCredentials_ReturnsViewWithErrorMessage()
//        {
//            // Arrange
//            string username = "user";
//            string password = "wrongpassword";

//            // Act
//            var result = controller.Login(username, password) as ViewResult;

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("Invalid User..!!!!", result.ViewData["v"]);
//        }
//    }
//}