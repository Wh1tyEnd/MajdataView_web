#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace JsonFormat
{
    public class SongDetail
    {

        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Designer { get; set; }
        public string? Description { get; set; }
        public string[]? Levels { get; set; } = new string[6];
        public SongDetail(int _id, string _title, string _artist, string _designer, IEnumerable<string> _levels, string _description = "")
        {
            Id = _id;
            Title = _title;
            Artist = _artist;
            Designer = _designer;
            Description = _description;
            Levels = _levels.ToArray();
        }
        public SongDetail() { }
        public static SongDetail LoadFromMaidata(string path)
        {
            var maidata = System.IO.File.ReadAllLines(path);
            var detail = new SongDetail();
            var levels = new string[7];
            for (int i = 0; i < maidata.Length; i++)
            {
                if (maidata[i].StartsWith("&title="))
                    detail.Title = GetValue(maidata[i]);
                else if (maidata[i].StartsWith("&artist="))
                    detail.Artist = GetValue(maidata[i]);
                else if (maidata[i].StartsWith("&des="))
                    detail.Designer = GetValue(maidata[i]);
                else if (maidata[i].StartsWith("&freemsg="))
                    detail.Description = GetValue(maidata[i]);
                else if (maidata[i].StartsWith("&lv_") || maidata[i].StartsWith("&inote_"))
                {
                    for (int j = 1; j < 8 && i < maidata.Length; j++)
                    {
                        if (maidata[i].StartsWith("&lv_" + j + "="))
                            levels[j - 1] = GetValue(maidata[i]);
                    }
                }
            }
            detail.Levels = levels;
            return detail;
        }
        static private string GetValue(string varline)
        {
            return varline.Split('=')[1];
        }
    }

}
