namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_WriteLog
    {
        public int Id { get; set; }
        public string Note { get; set; }
    }
}