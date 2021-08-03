using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace XinHuaDictionarySpider
{
    public class DictionaryContext : DbContext
    {
        public DbSet<Character> Characters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=Dic.db");
    }

    public class Character
    {
        /// <summary>
        /// 字符ID，这里使用 Char 的 Int 值
        /// </summary>
        [Key]
        public int CharId { get; set; }

        /// <summary>
        /// 汉字
        /// </summary>
        public string Char { get; set; } // 反正 Sqlite 没有 Char 类型

        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 部首
        /// </summary>
        public string Radical { get; set; }

        /// <summary>
        /// 笔画数
        /// </summary>
        public int StrokeNum { get; set; }

        /// <summary>
        /// 解释
        /// </summary>
        public string Definition { get; set; }
    }
}
