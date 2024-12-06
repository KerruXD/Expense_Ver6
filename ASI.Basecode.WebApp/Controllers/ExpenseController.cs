using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseRepository expenseRepository, 
            ICategoryService categoryService, 
            IUserService userService,
            IExpenseService expenseService)
        {
            _expenseRepository = expenseRepository;
            _categoryService = categoryService;
            _userService = userService;
            _expenseService = expenseService;
        }

        public IActionResult Index(string startDate, string endDate, int? category)
        {
            var expenses = _expenseRepository.ViewExpenses();
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                expenses = expenses.Where(e => e.Date >= start).ToList();
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                expenses = expenses.Where(e => e.Date <= end).ToList();
            }

            if (category.HasValue)
            {
                expenses = expenses.Where(e => e.CategoryID == category.Value).ToList();
            }

            ViewBag.Categories = _categoryService.GetCategories(currentUser.Id); 
            return View("Index", expenses);
        }

        public IActionResult Create()
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Email: " + Email);
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            ViewBag.Categories = _categoryService.GetCategories(currentUser.Id); // Ensure categories are set
            return View();
        }

        [HttpPost]
        public IActionResult Create(Expense expense)
        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Email: " + Email);
            User currentUser = null;

            if (Email != null) {
                currentUser = _userService.GetUserByEmail(Email);
            }

            if (ModelState.IsValid)
            {
                _expenseService.AddExpense(expense ,currentUser);


                return RedirectToAction("Index","Home");
            }
            
            ViewBag.Categories = _categoryService.GetCategories(currentUser.Id);
            return PartialView("Create", expense);
        }

        public IActionResult Edit(int id)

        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _categoryService.GetCategories(currentUser.Id); // Load categories for the dropdown
            return View(expense);
        }

        
        [HttpPost]
        public IActionResult Edit(Expense expense)

        {
            var Email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User currentUser = null;

            if (Email != null)
            {
                currentUser = _userService.GetUserByEmail(Email);
            }
            if (ModelState.IsValid)
            {
                _expenseRepository.UpdateExpense(expense);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = _categoryService.GetCategories(currentUser.Id); // Reload categories in case of validation error
            return View(expense); // Return the view with the invalid model to display errors
        }


        public IActionResult Delete(int id)
        {
            var expense = _expenseRepository.ViewExpenses().FirstOrDefault(e => e.ExpenseID == id);
            if (expense != null)
            {
                _expenseRepository.DeleteExpense(expense);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}