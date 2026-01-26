using System.Collections.Generic;
using System.Linq;

namespace MajorAuthor.Models.ViewModels
{
    // =========================================================================
    // Вспомогательные ViewModels для результатов
    // =========================================================================

    public class SearchResultPoemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string ContentSnippet { get; set; }
        public int ViewsCount { get; set; } // 💡 Добавлено для сортировки
        public int LikesCount { get; set; } // 💡 Добавлено для сортировки
    }
    public class SearchResultWorkViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string ContentSnippet { get; set; }
        public string Type { get; set; } // "Poem", "Book", "Blog" - для маршрутизации
        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }
        public string TargetAction { get; set; } // ReadPoem, ReadBook, ReadBlog
    }
    public class SearchResultBookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsAdultContent { get; set; }
        public string AuthorName { get; set; }
        public int ViewsCount { get; set; } // 💡 Добавлено для сортировки
        public int LikesCount { get; set; } // 💡 Добавлено для сортировки
    }

    public class SearchResultBlogViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string ContentSnippet { get; set; }
        public int ViewsCount { get; set; } // 💡 Добавлено для сортировки
        public int LikesCount { get; set; } // 💡 Добавлено для сортировки
    }

    public class SearchResultAuthorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string PenName { get; set; }
        public string FullName { get; set; }
        public int SubscribersCount { get; set; } // 💡 Добавлено для сортировки
    }

    // =========================================================================
    // Основная Search ViewModel
    // =========================================================================

    public class SearchViewModel
    {
        public string SearchQuery { get; set; }
        public string ActiveTab { get; set; } = "all";

        // 💡 Добавлено: Сортировка для авторов
        public string AuthorSortBy { get; set; } = "name";

        // 💡 Добавлено: Сортировка для произведений (используется на вкладке 'all' и в отдельных вкладках)
        public string WorkSortBy { get; set; } = "title";

        // 💡 Новый список для всех произведений (используется только на вкладке "Все")
        public List<SearchResultWorkViewModel> AllWorks { get; set; } = new List<SearchResultWorkViewModel>();

        public List<SearchResultPoemViewModel> Poems { get; set; } = new List<SearchResultPoemViewModel>();
        public List<SearchResultBookViewModel> Books { get; set; } = new List<SearchResultBookViewModel>();
        public List<SearchResultBlogViewModel> Blogs { get; set; } = new List<SearchResultBlogViewModel>();
        public List<SearchResultAuthorViewModel> Authors { get; set; } = new List<SearchResultAuthorViewModel>();

        public bool HasResults => AllWorks.Any() || Authors.Any(); // Изменена логика проверки

        public int WorksCount => Poems.Count + Books.Count + Blogs.Count; // Для вкладки "Все"
        public int AllCount => WorksCount + Authors.Count;
    }

}