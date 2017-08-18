using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using auctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace auctions.Controllers
{
    public class HomeController: Controller
    {
        private AuctionContext _context;
        public HomeController(AuctionContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Register()
        {
            ViewBag.Errors = new List<string>();
            return View("Register");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult HandleRegister(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                User NewUser = new User
                {
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                double newWallet = 1000.00;
                NewUser.Wallet = newWallet;
                NewUser.Password = Hasher.HashPassword(NewUser, model.Password);
                
                _context.Add(NewUser);
                _context.SaveChanges();
                User justEnteredPerson = _context.Users.SingleOrDefault(user => user.UserName == model.UserName);
                HttpContext.Session.SetString("UserName", justEnteredPerson.FirstName);
                HttpContext.Session.SetInt32("UserId", justEnteredPerson.UserId);

                return RedirectToAction("Create", "Auction");
            }
            System.Console.WriteLine("Not Valid!");
            ViewBag.Errors = new List<string>();
            return View("Register");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(string UserName, string PasswordToCheck)
        {
            System.Console.WriteLine("IIIIIFFFFFFF");
            User logged = _context.Users.SingleOrDefault(user => user.UserName == UserName);
            if (logged != null && PasswordToCheck != null)
            {
                System.Console.WriteLine("TTHHHHHHHHEEEEEENNNNN");
                var Hasher = new PasswordHasher<User>();
                if (0 != Hasher.VerifyHashedPassword(logged, logged.Password, PasswordToCheck))
                {
                    HttpContext.Session.SetString("UserName", logged.FirstName);
                    HttpContext.Session.SetInt32("UserId", logged.UserId);
                    return RedirectToAction("Create", "Auction"); 
                }
            }
        
            ViewBag.Errors = new List<string>();
            ViewBag.Errors.Add("Invalid Login Credentials.");
            return View("Register");
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Register");
        }

    }
}
