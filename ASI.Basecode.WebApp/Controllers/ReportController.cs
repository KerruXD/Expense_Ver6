using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryService _categoryService;
        public ReportController(IExpenseRepository expenseRepository, ICategoryService categoryService)
        {
            _expenseRepository = expenseRepository;
            _categoryService = categoryService;
        }



        public IActionResult ExpenseSummary()
        {
            // Get all categories and expenses from the repositories
            var categories = _categoryService.GetCategories(); // Use CategoryService for categories
            var expenses = _expenseRepository.ViewExpenses(); // Fetch expenses



            // If there are no categories or expenses, you might want to return an empty model
            if (categories == null || expenses == null)
            {
                categories = new List<Category>();
                expenses = new List<Expense>();
            }

            // Prepare the view model to pass to the view
            var model = new ExpenseSummaryViewModel
            {
                Categories = categories,
                Expenses = expenses
            };

            return View(model); // Pass the model to the view
        }

        // Action to generate a report


        // Action to display Expense Trends
        //public IActionResult ExpenseTrends()
        //{
        //    // Fetch expenses
        //    var expenses = _expenseRepository.ViewExpenses();
        //    if (expenses == null)
        //    {
        //        expenses = new List<Expense>(); // Initialize as empty list if null
        //    }

        //    // Fetch categories using the CategoryService
        //    var categories = _categoryService.GetCategories();
        //    if (categories == null || !categories.Any())
        //    {
        //        categories = new List<Category>();  // Initialize categories if null or empty
        //    }

        //    // Prepare the trend data
        //    var trendData = categories.Select(category => new ExpenseTrendData
        //    {
        //        CategoryName = category.CategoryName,
        //        MonthlyAmounts = expenses
        //            .Where(e => e.CategoryID == category.CategoryID)
        //            .GroupBy(e => new { e.Date.Year, e.Date.Month })
        //            .OrderBy(g => g.Key.Month)
        //            .Select(g => g.Sum(e => e.Amount))
        //            .ToList()
        //    }).ToList();

        //    // Get distinct months
        //    var months = trendData.SelectMany(td => td.MonthlyAmounts.Select(m => m.ToString("MMMM yyyy"))).Distinct().ToList();

        //    // Create ViewModel
        //    var viewModel = new ExpenseTrendViewModel
        //    {
        //        Categories = categories.Select(c => c.CategoryName).ToList(),
        //        TrendData = trendData,
        //        Months = months
        //    };

        //    return View(viewModel);
        //}

        public IActionResult ExpenseTrends()
        {
            var model = new ExpenseTrendViewModel
            {
                Months = GetMonths(), // List of months
                TrendData = GetSpendingOverTime(), // List of TrendData
                CategoryTrends = GetCategoryTrends(), // List of CategoryData
                TopExpenses = GetTopExpenses() // List of TopExpenseData
            };

            return View(model);
        }


        public IActionResult GenerateReport(string startDate = null, string endDate = null, int? categoryId = null)
        {
            var categories = _categoryService.GetCategories() ?? new List<Category>();
            var expenses = _expenseRepository.ViewExpenses();

            if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParse(startDate, out var parsedStartDate))
            {
                expenses = expenses.Where(e => e.Date >= parsedStartDate).ToList();
            }

            if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParse(endDate, out var parsedEndDate))
            {
                expenses = expenses.Where(e => e.Date <= parsedEndDate).ToList();
            }

            if (categoryId.HasValue)
            {
                expenses = expenses.Where(e => e.CategoryID == categoryId).ToList();
            }

            var model = new GenerateReportViewModel
            {
                Categories = categories,
                Expenses = expenses,
                StartDate = startDate,
                EndDate = endDate,
                SelectedCategoryId = categoryId,
                TotalAmount = expenses.Sum(e => e.Amount)
            };

            return View(model);
        }
        // Helper method: Get a list of distinct months
        private List<string> GetMonths()
        {
            var expenses = _expenseRepository.ViewExpenses() ?? new List<Expense>();

            return expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => $"{g.Key.Month:D2}/{g.Key.Year}") // Format: "MM/YYYY"
                .ToList();
        }

        // Helper method: Get spending over time for each category
        private List<TrendData> GetSpendingOverTime()
        {
            var expenses = _expenseRepository.ViewExpenses() ?? new List<Expense>();
            var categories = _categoryService.GetCategories() ?? new List<Category>();

            return categories.Select(category => new TrendData
            {
                CategoryName = category.CategoryName,
                MonthlyAmounts = expenses
                    .Where(e => e.CategoryID == category.CategoryID)
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => g.Sum(e => e.Amount))
                    .ToList()
            }).ToList();
        }

        // Helper method: Get total spending per category
        private List<CategoryData> GetCategoryTrends()
        {
            var expenses = _expenseRepository.ViewExpenses() ?? new List<Expense>();
            var categories = _categoryService.GetCategories() ?? new List<Category>();

            return categories.Select(category => new CategoryData
            {
                CategoryName = category.CategoryName,
                TotalAmount = expenses
                    .Where(e => e.CategoryID == category.CategoryID)
                    .Sum(e => e.Amount)
            }).ToList();
        }

        // Helper method: Get top expenses by amount
        private List<TopExpenseData> GetTopExpenses()
        {
            var expenses = _expenseRepository.ViewExpenses() ?? new List<Expense>();

            return expenses
                .OrderByDescending(e => e.Amount)
                .Take(5) // Adjust this to return the top N expenses
                .Select(e => new TopExpenseData
                {
                    Title = e.Title,
                    Amount = e.Amount
                })
                .ToList();
        }
    }
}