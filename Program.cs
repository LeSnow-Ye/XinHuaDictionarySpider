using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace XinHuaDictionarySpider
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime beginTime = DateTime.Now;

            // 单单添加 System.Text.Encoding.CodePages 仍会报错，需要用此方法来注册一下 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding= Encoding.GetEncoding("GB2312");

            // 获取所有拼音索引链接
            var pinyinPages = new List<string>();
            var pinyinDoc = web.Load("http://xh.5156edu.com/pinyi.html");
            var pinyinNodes = pinyinDoc.DocumentNode.SelectNodes("//a[@class='fontbox']"); // HAP 是用 XPath 来选择的
            foreach (var item in pinyinNodes)
            {
                pinyinPages.Add("http://xh.5156edu.com/" + item.GetAttributeValue("href", string.Empty));
            }

            // 获得所有汉字链接
            var charPages = new List<string>();
            foreach (var pinyinPage in pinyinPages)
            {
                var pinyinPageDoc = web.Load(pinyinPage);
                var charNodes = pinyinPageDoc.DocumentNode.SelectNodes(@"//a[@class='fontbox']");
                foreach (var item in charNodes)
                {
                    Console.WriteLine("准备链接中: " + item.InnerText + " http://xh.5156edu.com/" + item.GetAttributeValue("href", string.Empty));
                    charPages.Add("http://xh.5156edu.com/" + item.GetAttributeValue("href", string.Empty));
                }
            }

            // 汉字页处理及向数据库中添加汉字
            using (var db = new DictionaryContext())
            {
                db.Database.EnsureCreated(); // 确保数据库文件是存在的

                foreach (var item in charPages)
                {
                    var charDoc = web.Load(item);

                    var character = charDoc.DocumentNode.SelectSingleNode("//td[@class='font_22']").InnerText; // 汉字

                    if (db.Characters.Find((int)character[0]) == null) // 防止多音字重复添加，进行消重
                    {
                        Console.WriteLine($"[{db.Characters.Count() + 1}] Adding '{(int)character[0]} {character}' ...");

                        int strokeNum; // 笔画数
                        if (!Int32.TryParse(charDoc.DocumentNode.SelectSingleNode("//tr[@bgcolor='#E7ECF8'][1]/td[4]").InnerText, out strokeNum))
                        {
                            // 后经测试发现部分页面规格与大部分汉字有所不同，因此若索引有误则设置为 -1
                            strokeNum = -1;
                        }

                        var definitionHtml = charDoc.DocumentNode.SelectSingleNode("//td[@class='font_18']").InnerHtml; // 解释部分的 html
                        db.Add(new Character()
                        {
                            Char = character,
                            CharId = (int)character[0],
                            PinYin = charDoc.DocumentNode.SelectSingleNode("//td[@class='font_14']").InnerText,
                            StrokeNum = strokeNum,
                            Radical = charDoc.DocumentNode.SelectSingleNode("//tr[@bgcolor='#E7ECF8'][2]/td[2]").InnerText,
                            // 用正则表达式消去所有尖括号及其中的内容，并除掉 Non-breaking space
                            Definition = Regex.Replace(definitionHtml, "\\<.*?>", string.Empty).Replace("&nbsp;", string.Empty),
                    });

                        // 记得 SaveChanges
                        db.SaveChanges();
                    }
                }
            }

            Console.WriteLine($"Done!\nTime cost: {DateTime.Now - beginTime}");
        }
    }
}
