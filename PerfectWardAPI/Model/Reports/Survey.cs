﻿using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class Survey
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
