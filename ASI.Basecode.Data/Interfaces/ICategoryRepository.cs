using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ICategoryRepository
    {
        List<Category> ViewCategories(int UserId); // Ensure this return type is List<Category>
        void AddCategory(Category category, int UserId);
        void UpdateCategory(Category category);
        void DeleteCategory(Category category);
    }
}