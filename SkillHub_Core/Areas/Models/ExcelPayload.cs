using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Models
{
    public class ExcelPayload<T>
    {
        public List<T> items { get; set; }
        public string templateName { get; set; }
        public string folderToSave { get; set; }
        //public Dictionary<ExcelIndex, string> keyValues { get; set; }
        public int fromRow { get; set; } = 3;
        public int fromCol { get; set; } = 1;

    }
    // Sử dụng để gán drop down vào file Excel
    public class ListDropDown
    {
        public string columnName { get; set; }
        public List<DropDown> dataDropDown { get; set; }
    }
    public class DropDown
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}