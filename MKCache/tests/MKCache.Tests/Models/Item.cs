using System;
using System.Collections.Generic;
using System.Text;

namespace MKCache.Tests
{
    public class Item
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public Item()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = Guid.NewGuid().ToString("n");
            this.Count = DateTime.UtcNow.Second;
        }
    }
}
