using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorAuthor.Data.Entities
{
    public class BookCycle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
