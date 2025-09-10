using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;

namespace MajorAuthor.Controllers
{
    public class MyWorksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;
        private readonly IAuthorService _authorService;
        private readonly IBookInvitationService _bookInvitationService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MyWorksController(
            IBookService bookService,
            IWorkService<Poem> poemService,
            IWorkService<Blog> blogService,
            IAuthorService authorService,
            IBookInvitationService bookInvitationService,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _bookService = bookService;
            _poemService = poemService;
            _blogService = blogService;
            _authorService = authorService;
            _bookInvitationService = bookInvitationService;
            _emailSender = emailSender;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            var viewModel = new MyWorksViewModel();

            if (author == null)
            {
                viewModel.IsUserAuthor = false;
                viewModel.HasWorks = false;
            }
            else
            {
                viewModel.IsUserAuthor = true;

                var books = await _bookService.GetAllByAuthorIdAsync(author.Id);
                var poems = await _poemService.GetAllByAuthorIdAsync(author.Id);
                var blogs = await _blogService.GetAllByAuthorIdAsync(author.Id);

                viewModel.Works = new List<MyWorksViewModel.WorkDisplayModel>();
                viewModel.Works.AddRange(books.Select(b => new MyWorksViewModel.WorkDisplayModel { Id = b.Id, Title = b.Title, Type = "Книга"}));
                viewModel.Works.AddRange(poems.Select(p => new MyWorksViewModel.WorkDisplayModel { Id = p.Id, Title = p.Title, Type = "Стих" }));
                viewModel.Works.AddRange(blogs.Select(b => new MyWorksViewModel.WorkDisplayModel { Id = b.Id, Title = b.Title, Type = "Блог" }));

                viewModel.HasWorks = viewModel.Works.Any();
            }

            return View(viewModel);
        }

        public IActionResult AddWork()
        {
            return View();
        }

        public IActionResult CreateBook()
        {
            return View(new CreateBookViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(CreateBookViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var author = await _authorService.GetAuthorByUserIdAsync(userId);

                if (author != null)
                {
                    string uniqueFileName = null;
                    if (model.CoverImage != null)
                    {
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImage.FileName);
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.CoverImage.CopyToAsync(fileStream);
                        }
                    }

                    var newBook = new Book
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CoverImageUrl = uniqueFileName != null ? "/images/books/" + uniqueFileName : null,
                        IsAdultContent = model.IsAdultContent,
                        PublicationDate = System.DateTime.UtcNow,
                        LastUpdateTime = System.DateTime.UtcNow
                    };

                    await _bookService.AddAsync(newBook);
                    await _bookService.LinkBookToAuthorAsync(newBook.Id, author.Id);

                    bool invitationSent = false;
                    if (!string.IsNullOrEmpty(model.CoAuthorEmail))
                    {
                        string token = Guid.NewGuid().ToString();

                        var invitation = new BookInvitation
                        {
                            BookId = newBook.Id,
                            InviteeEmail = model.CoAuthorEmail,
                            InvitationToken = token,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _bookInvitationService.AddInvitationAsync(invitation);

                        var invitationLink = Url.Action("AcceptInvitation", "MyWorks", new { token }, Request.Scheme);

                        var subject = $"Приглашение стать соавтором книги «{model.Title}»";
                        var message = $"<p>Здравствуйте!</p><p>Автор {author.PenName} приглашает вас стать соавтором книги «{model.Title}».</p><p>Чтобы принять приглашение, перейдите по следующей ссылке: <a href='{invitationLink}'>{invitationLink}</a></p>";

                        try
                        {
                            await _emailSender.SendEmailAsync(model.CoAuthorEmail, subject, message);
                            invitationSent = true;
                        }
                        catch
                        {
                            invitationSent = false;
                        }
                    }
                    ViewBag.InvitationSent = invitationSent;
                    return View("BookCreated", newBook);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            var invitation = await _bookInvitationService.GetInvitationByTokenAsync(token);

            if (invitation == null || invitation.IsAccepted)
            {
                ViewBag.Message = "Ссылка-приглашение недействительна или уже использована.";
                return View("InvitationResult");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.Message = "Пожалуйста, войдите в систему, чтобы принять приглашение.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("AcceptInvitation", "MyWorks", new { token }) });
            }

            var invitedUser = await _userManager.GetUserAsync(User);
            if (invitedUser.Email != invitation.InviteeEmail)
            {
                ViewBag.Message = "Эта ссылка предназначена для другого пользователя.";
                return View("InvitationResult");
            }

            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null)
            {
                return RedirectToAction("RegisterAuthorPrompt", "Author", new { returnUrl = Url.Action("AcceptInvitation", "MyWorks", new { token }) });
            }

            invitation.IsAccepted = true;
            await _bookInvitationService.UpdateInvitationAsync(invitation);

            ViewBag.Message = "Поздравляем! Вы были добавлены в качестве соавтора.";
            return View("InvitationResult");
        }

        // Другие действия...
        public async Task<IActionResult> CreatePoem()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePoem(CreatePoemModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var author = await _authorService.GetAuthorByUserIdAsync(userId);

                if (author != null)
                {
                    var newPoem = new Poem
                    {
                        Title = model.Title,
                        Content = model.Content,
                        AuthorId = author.Id,
                        Status = "draft",
                        PublicationDate = System.DateTime.UtcNow
                    };

                    await _poemService.AddAsync(newPoem);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> CreateBlog()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(CreateBlogModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var author = await _authorService.GetAuthorByUserIdAsync(userId);

                if (author != null)
                {
                    var newBlog = new Blog
                    {
                        Title = model.Title,
                        Content = model.Content,
                        AuthorId = author.Id,
                        PublicationDate = System.DateTime.UtcNow
                    };

                    if (model.Photo != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "blogs");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(fileStream);
                        }
                        newBlog.ImageUrl = "/images/blogs/" + uniqueFileName;
                    }

                    await _blogService.AddAsync(newBlog);
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAuthor([FromBody] AuthorRegistrationModel model)
        {
            if (string.IsNullOrEmpty(model.PenName))
            {
                return Json(new { success = false, message = "Пожалуйста, введите псевдоним или имя." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Ошибка: пользователь не аутентифицирован." });
            }

            await _authorService.RegisterAuthorAsync(userId, model.PenName);
            return Json(new { success = true, message = "Поздравляем, вы стали автором! Ждём от вас новых интересных произведений." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWork([FromQuery] int id, [FromQuery] string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var author = await _authorService.GetAuthorByUserIdAsync(userId);

            if (author == null)
            {
                return Forbid();
            }

            // Используем оператор switch для выбора нужного сервиса
            switch (type)
            {
                case "Книга":
                    var book = await _bookService.GetByIdAsync(id);
                    if (book == null) return NotFound();
                    if (!(await _bookService.IsAuthorOfBookAsync(id, author.Id))) return Forbid();

                    // Удаление обложки книги
                    if (!string.IsNullOrEmpty(book.CoverImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    await _bookService.DeleteAsync(book);
                    return Ok(new { message = $"Книга \"{book.Title}\" успешно удалена." });

                case "Стих":
                    var poem = await _poemService.GetByIdAsync(id);
                    if (poem == null) return NotFound();
                    if (poem.AuthorId != author.Id) return Forbid();

                    await _poemService.DeleteAsync(poem);
                    return Ok(new { message = $"Стихотворение \"{poem.Title}\" успешно удалено." });

                case "Блог":
                    var blog = await _blogService.GetByIdAsync(id);
                    if (blog == null) return NotFound();
                    if (blog.AuthorId != author.Id) return Forbid();

                    // Удаление изображения блога
                    if (!string.IsNullOrEmpty(blog.ImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    await _blogService.DeleteAsync(blog);
                    return Ok(new { message = $"Блог \"{blog.Title}\" успешно удален." });

                default:
                    return BadRequest(new { message = "Неизвестный тип произведения." });
            }
        }
    }
}
