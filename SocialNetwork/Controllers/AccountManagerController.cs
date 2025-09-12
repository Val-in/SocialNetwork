using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.Models;
using SocialNetwork.BLL.Services;
using SocialNetwork.Extentions;
using SocialNetwork.ViewModels;
using System.Security.Claims;

namespace SocialNetwork.Controllers;


public class AccountManagerController(
    IMapper mapper,
    UserService userService,
    FriendService friendService,
    MessageService messageService)
    : Controller
{
    [HttpGet]
    [Route("Login")]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            // если пользователь уже авторизован, редирект на Home/Index
            return RedirectToAction("Index", "Home");
        }

        // иначе показываем страницу входа
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await userService.CheckPasswordAsync(model.Email, model.Password);

            if (result)
            {
                await userService.SignInAsync(model.Email, false);
                return RedirectToAction("MyPage", "AccountManager");
            }
            
            ModelState.AddModelError(nameof(model.Email), " ");
            ModelState.AddModelError(nameof(model.Password), "Неправильный логин и (или) пароль");

        }
        return View(model);
    }
    
    [Route("Logout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await userService.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [Route("MyPage")]
    [HttpGet]
    public async Task<IActionResult> MyPage()
    {
        var user = User;

        var result = await userService.GetUserAsync(user);

        var model = new UserViewModel(result);

        model.Friends = await friendService.GetFriendsByUser(model.User);

        return View("MyPage", model);
    }

    [Route("Edit")]
    [HttpGet]
    public IActionResult Edit()
    {
        var user = User;

        var result = userService.GetUserAsync(user);

        var editmodel = mapper.Map<UserEditViewModel>(result.Result);

        return View("EditUserProfile", editmodel);
    }

    [Authorize]
    [Route("Update")]
    [HttpPost]
    public async Task<IActionResult> Update(UserEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userService.GetUserByIdAsync(userId);

            user.Convert(model);

            var result = await userService.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("MyPage", "AccountManager");
            }
            else
            {
                ModelState.AddModelError("", "Ошибка, обновление не удалось");
                return RedirectToAction("Edit", "AccountManager");
            }
        }

        ModelState.AddModelError("", "Некорректные данные");
        return View("EditUserProfile", model);
    }

    [Route("UserList")]
    [HttpGet]
    public async Task<IActionResult> UserList(string search)
    {
        var model = await CreateSearch(search);
        return View("UserList", model);
    }

    [Route("AddFriend")]
    [HttpPost]
    public async Task<IActionResult> AddFriend(string id)
    {
        await friendService.AddFriend(User, id);

        return RedirectToAction("MyPage", "AccountManager");
    }

    [Route("DeleteFriend")]
    [HttpPost]
    public async Task<IActionResult> DeleteFriend(string id)
    {
        await friendService.DeleteFriend(User, id);

        return RedirectToAction("MyPage", "AccountManager");
    }

    private async Task<SearchViewModel> CreateSearch(string search)
    {
        var currentUser = User;
        var currentUserEntity = await userService.GetUserAsync(currentUser);

        // Получаем пользователей, подходящих под поиск
        var list = await userService.GetUsersForSearch(search, currentUserEntity.Id);

        // Получаем друзей текущего пользователя
        var friends = await friendService.GetFriendsByUser(currentUserEntity);

        var data = list.Select(x =>
        {
            var t = mapper.Map<UserWithFriendExt>(x);

            // Проверка дружбы
            t.IsFriendWithCurrent = friends.Any(y => y.Id == x.Id || x.Id == currentUserEntity.Id);

            return t;
        }).ToList();

        var model = new SearchViewModel()
        {
            UserList = data
        };

        return model;
    }

    [Route("Chat")]
    [HttpPost]
    public async Task<IActionResult> Chat(string id)
    {
        var model = await GenerateChat(id);
        return View("Chat", model);
    }

    private async Task<ChatViewModel> GenerateChat(string id)
    {
        var result = await messageService.GetMessagesAsync(User, id);

        var model = new ChatViewModel
        {
            You = result.Item2,
            ToWhom = result.Item3,
            History = result.Item1.OrderBy(x => x.Id).ToList()
        };

        return model;
    }

    [Route("Chat")]
    [HttpGet]
    public async Task<IActionResult> Chat()
    {
        var id = Request.Query["id"];
        var model = await GenerateChat(id);
        return View("Chat", model);
    }

    [Route("NewMessage")]
    [HttpPost]
    public async Task<IActionResult> NewMessage(string id, ChatViewModel chat)
    {
        await messageService.NewMessageAsync(User, chat.NewMessage.Text, id);

        // Генерируем обновленный чат (BLL-модели)
        var model = await GenerateChat(id);
        return View("Chat", model);
    }
}