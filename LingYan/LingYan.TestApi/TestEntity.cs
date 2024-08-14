using System.ComponentModel.DataAnnotations.Schema;

namespace LingYan.TestApi
{
    [Table("Test")]
    public class TestEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ModKey { get; set; }
        public DateTime TimeKey { get; set; }
    }
}
