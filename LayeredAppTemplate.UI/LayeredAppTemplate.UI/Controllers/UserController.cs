using LayeredAppTemplate.Application.DTOs.User;
using LayeredAppTemplate.Application.Interfaces;
using LayeredAppTemplate.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LayeredAppTemplate.UI.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Kullanıcıları Listele
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            var userViewModels = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            }).ToList();

            return View(userViewModels);
        }

        // Yeni Kullanıcı Ekle
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userDto = new UserDto
                {
                    FullName = model.FullName,
                    Email = model.Email
                };
                await _userService.CreateAsync(userDto);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // Kullanıcıyı Düzenle
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userDto = new UserDto
                {
                    Id = model.Id,
                    FullName = model.FullName,
                    Email = model.Email
                };
                var result = await _userService.UpdateAsync(userDto);
                if (result)
                {
                    return RedirectToAction("Index");
                }

                return NotFound();
            }

            return View(model);
        }

        // Kullanıcıyı Sil
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            return result ? RedirectToAction("Index") : NotFound();
        }
    }
}
