using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using auctions.Models;
using Microsoft.AspNetCore.Identity;


namespace auctions.Controllers
{
    public class AuctionController: Controller
    {
        private AuctionContext _context;
        public AuctionController(AuctionContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "Home");
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            
            ViewBag.Errors = new List<string>();
            return View("Create");
        }

        [HttpPost]
        [Route("create")]
        public IActionResult HandleCreate(AuctionViewModel model)
        {
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "Home");
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Errors = new List<string>();
            if(ModelState.IsValid)
            {
                System.Console.WriteLine("LLLLLLLLLLLLLOOOOOGGGGGGGG");
                if(model.EndDate > DateTime.Now)
                {
                    System.Console.WriteLine("CCCHHHHHHEEECKKKKKIIINNNNGG");
                    ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
                    Auction newAuction = new Auction
                    {
                        ProductName = model.ProductName,
                        Description = model.Description,
                        StartingBid = model.StartingBid,
                        HighestBid = model.StartingBid,
                        EndDate = model.EndDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UserId = ViewBag.UserId,
                    };
                    _context.Add(newAuction);
                    _context.SaveChanges();
                    return RedirectToAction("Show");
                }
                ViewBag.Errors.Add("Select an new date");
                return View("Create");
            }
            return View("Create");
        }

        
        [HttpPost]
        [Route("bids")]
        public IActionResult Buy(int AuctionId, double makebid)
        {
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "User");
            }
            System.Console.WriteLine("Did it work????????????????????");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Errors = new List<string>();

            User getUser = _context.Users.SingleOrDefault(user => user.UserId == (int)loggedInt);
            Auction makeAction = _context.Auctions.Include(a=> a.User)
                                                  .SingleOrDefault(a => a.AuctionId == AuctionId);
            ViewBag.User = getUser;
            ViewBag.Auction = makeAction;
           
            Bid highestbid = _context.Bids.Where(b=> b.AuctionId == AuctionId)
                                          .Include(b => b.User)
                                          .OrderByDescending(b=> b.Amount)
                                          .FirstOrDefault();
            ViewBag.Winner = highestbid;
            TimeSpan span = ViewBag.Auction.EndDate.Subtract(DateTime.Now);

            String maketime = new DateTime(span.Ticks).ToString("dd");

            ViewBag.Time = maketime;
    
            if ((double)makeAction.HighestBid >= (double)makebid)
            {

                ViewBag.Errors.Add("");
                return View("Details");
            }
            else if (makebid > getUser.Wallet)
            {
                ViewBag.Errors.Add("");
                return View("Details");
            }
            else {
                Bid newBid = new Bid {
                    Amount = (double)makebid,
                    UserId = (int)loggedInt,
                    AuctionId = AuctionId
                };
                _context.Add(newBid);
                makeAction.HighestBid = (double)makebid;
                _context.SaveChanges();
                return RedirectToAction("Show");
            }
        }
        [HttpGet]
        [Route("auction/{AuctionId}")]
        public IActionResult Details(int AuctionId)
        {
            System.Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "User");
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Errors = new List<string>();

            User getUser = _context.Users.SingleOrDefault(user => user.UserId == (int)loggedInt);


            Auction makeAction = _context.Auctions.Include(a => a.User)
                                                   .SingleOrDefault(a => a.AuctionId == AuctionId);
            ViewBag.Auction = makeAction;
            TimeSpan span = ViewBag.Auction.EndDate.Subtract(DateTime.Now);

            String maketime = new DateTime(span.Ticks).ToString("dd");
            ViewBag.Time = maketime;
            ViewBag.User = getUser;

            Bid highestbid = _context.Bids.Where(b=> b.AuctionId == AuctionId)
                                              .Include(b => b.User)
                                              .OrderByDescending(b => b.Amount)
                                              .FirstOrDefault();
            ViewBag.Winner = highestbid;
            return View();

        }

        [HttpPost]
        [Route("delete")]
        public IActionResult DeleteAuction(int AuctionId)
        {
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "User");
            }
            Auction removeAuction = _context.Auctions.SingleOrDefault(auction => auction.AuctionId == AuctionId);
            List<Bid> Bids = _context.Bids.Where(bid => bid.AuctionId == AuctionId).ToList();
            foreach (var bid in Bids)
            {
                _context.Remove(bid);
            }
            _context.Remove(removeAuction);
            _context.SaveChanges();
            return RedirectToAction("Show");
        }
        
        [HttpGet]
        [Route("auctions")]
        //begin
        public IActionResult Show()
        {
            int? loggedInt = HttpContext.Session.GetInt32("UserId");
            if (loggedInt == null)  
            {
                return RedirectToAction("Register", "Home");
            }
            User getUser = _context.Users.SingleOrDefault(user => user.UserId == (int)loggedInt);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.auction = _context.Auctions.Where(auction => auction.EndDate > DateTime.Now)
                                                .Include(auction => auction.User)
                                                .ToList()
                                                .OrderBy(auction => auction.EndDate);
            ViewBag.User = getUser;

            return View();
        }
        
    }
}