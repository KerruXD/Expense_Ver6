using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public List<Category> GetCategories(int userId)
        {
            return _categoryRepository.ViewCategories(userId);
        }

        public void AddCategory(Category category, int UserId)
        {
            category.UserId = UserId;
            _categoryRepository.AddCategory(category, UserId);
        }

        public void UpdateCategory(Category category)
        {
            _categoryRepository.UpdateCategory(category);
        }

        public void DeleteCategory(Category category)
        {
            _categoryRepository.DeleteCategory(category);
        }
    }
}