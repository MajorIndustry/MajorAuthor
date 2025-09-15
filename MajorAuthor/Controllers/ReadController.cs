using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;

// Принцип единственной ответственности (SRP).
// Контроллер отвечает только за обработку HTTP-запросов и координацию.
// Вся бизнес-логика перенесена в WorkFacade.
// Принцип зависимости от абстракций (DIP).
// Контроллер зависит от интерфейса IWorkFacade, а не от его конкретной реализации.
namespace MajorAuthor.Controllers
{
    public class ReadController : Controller
    {
        private readonly IWorkFacade _workFacade;
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;

        public ReadController(IWorkFacade workFacade, IWorkService<Poem> poemService,
            IWorkService<Blog> blogService)
        {
            _workFacade = workFacade;
            _poemService = poemService;
            _blogService = blogService;
        }


        [HttpGet]
        public async Task<IActionResult> ReadBlog(int id)
        {
            var blog = await _blogService.GetByIdWithCommentsAndLikesAsync(id);
            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> ReadPoem(int id)
        {
            var poem = await _poemService.GetByIdWithCommentsAndLikesAsync(id);
            return View(poem);
        }
        [HttpPost]
        public async Task<IActionResult> LikeWork(int id, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _workFacade.ToggleLikeAsync(id, type, userId);

            return Json(new { success = result.success, likesCount = result.newLikesCount });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int id, string type, string commentText)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(commentText)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _workFacade.AddCommentAsync(id, type, userId, commentText);

            if (comment == null)
            {
                return BadRequest();
            }

            // Я предполагаю, что у вас есть частичное представление для комментариев.
            return Json(new { success = true, comment = comment });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentId, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _workFacade.DeleteCommentAsync(commentId, type, userId);

            if (success)
            {
                return Ok(new { message = $" успешно удален." });
            }
            return BadRequest(new { message = "Неизвестный тип произведения." });
        }
    }
}
