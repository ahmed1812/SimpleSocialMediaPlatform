using Microsoft.AspNetCore.Mvc;

namespace SimpleSocialMediaPlatform.Models
{
    public interface DesignPatterns
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int? id);
        IActionResult Create();
        Task<IActionResult> Edit(int? id);
        Task<IActionResult> Delete(int? id); // Ensure async method signature
        Task<IActionResult> DeleteConfirmed(int id);

    }
}
