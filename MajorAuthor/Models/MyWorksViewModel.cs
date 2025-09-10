using System.Collections.Generic;

namespace MajorAuthor.Models
{
    public class MyWorksViewModel
    {
        public bool IsUserAuthor { get; set; }
        public bool HasWorks { get; set; }
        public List<WorkDisplayModel> Works { get; set; }

        public class WorkDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}
