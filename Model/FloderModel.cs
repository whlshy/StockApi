using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace I3S_API.Model
{
    public class AddFolder
    {
        [DisplayName("資料夾名稱名稱")]
        [StringLength(200, ErrorMessage = "長度不能超過{1}")]
        public string? cname { get; set; }

        [DisplayName("資料夾描述")]
        [StringLength(4000, ErrorMessage = "長度不能超過{1}")]
        public string? des { get; set; }
    }

    public class EditFolder
    {
        [DisplayName("資料夾名稱名稱")]
        [StringLength(200, ErrorMessage = "長度不能超過{1}")]
        public string? cname { get; set; }

        [DisplayName("資料夾描述")]
        [StringLength(4000, ErrorMessage = "長度不能超過{1}")]
        public string? des { get; set; }
    }

    public class SortFolder
    {
        public int seq { get; set; }

    }
}
