using System.Collections.Generic;
using financiera.Models;
using Financiera.Models;

namespace Financiera.Data
{
    public static class InMemoryDatabase
    {
        public static List<Associated> Associateds { get; set; } = new List<Associated>();
        public static List<Movement> Movements { get; set; } = new List<Movement>();
    }
}

