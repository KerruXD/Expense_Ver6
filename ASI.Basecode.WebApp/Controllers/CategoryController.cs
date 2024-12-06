using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;

        public CategoryController(ICategoryService categoryService, IExpenseService expenseService, IUserService userService)
        {
            _categoryService = categoryService;
            _expenseService = expenseService;
            _userService = userService;
        }

        //public IActionResult Index()
        //{
        //    var categories = _categoryService.GetCategories();
        //    return View(categories);
        //}

        public IActionResult Index(int? categoryFilter)
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Email: " + Email);
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            var categories = _categoryService.GetCategories(currentUser.Id);

            // Pass categories to the ViewBag for the dropdown
            ViewBag.Categories = categories;

            // Filter categories if a filter is applied
            if (categoryFilter.HasValue && categoryFilter.Value > 0)
            {
                categories = categories.Where(c => c.CategoryID == categoryFilter.Value).ToList();
            }

            return View(categories);
        }

        public IActionResult View(int id)
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Email: " + Email);
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            var expenses = _expenseService.GetExpenses()
                .Where(e => e.CategoryID == id)
                .ToList();

            ViewBag.CategoryName = _categoryService.GetCategories(currentUser.Id)
                .FirstOrDefault(c => c.CategoryID == id)?.CategoryName;

            return View(expenses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Email: " + Email);
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            if (ModelState.IsValid)
            {
                _categoryService.AddCategory(category ,currentUser.Id);
                return RedirectToAction("Index", "Home");
            }
            //return View(category);
            return PartialView("Create", category);
        }

        [HttpGet]
        public IActionResult Edit(int id)

        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            var category = _categoryService.GetCategories(currentUser.Id).FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            TempData["PreviousPage"] = Request.Headers["Referer"].ToString();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.UpdateCategory(category);

                var previousPage = TempData["PreviousPage"]?.ToString();
                if (!string.IsNullOrEmpty(previousPage))
                {
                    return Redirect(previousPage); // Redirect to the page the user came from (Category Management)
                }

                //return RedirectToAction("Index","Home");
            }
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }

            var category = _categoryService.GetCategories(currentUser.Id).FirstOrDefault(c => c.CategoryID == id);
            if (category != null)
            {
                _categoryService.DeleteCategory(category);
            }
            return RedirectToAction("Index","Home");
        }
    }
}