using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class BoardsResponse
    {
        public IEnumerable<Board> Values { get; set; }
    }

    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
