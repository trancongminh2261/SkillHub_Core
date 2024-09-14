using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models
{
    public class StatisticalModel
    {
    }

    public class OverviewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string SubValue { get; set; }
        [JsonIgnore]
        public double? PreValue { get; set; }
        [JsonIgnore]
        public string Roles { get; set; }
        [JsonIgnore]
        public int Groups { get; set; }
        public int Type { get; set; }
    }
    public class ListOverviewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<OverviewModel> OverviewModel { get; set; }
    }
    public class StatisticalYear
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double PreValue { get; set; }
    }
    public class StatisticialCustomerInMonth
    {
        public string Name { get; set; }
        public double ValueInMonth { get; set; }
        public double ValuePreInMonth { get; set; }
        public string Note { get; set; }
    }
    public class StatisticialAllCustomer
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class StatisticialTestAppointmentModel
    {
        public string Month { get; set; }
        public double Value { get; set; }
    }
}