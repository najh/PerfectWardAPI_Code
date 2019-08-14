using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class Meta
    {
        [DataMember(Name = "current_page")]
        public int CurrentPage { get; set; }

        [DataMember(Name = "total_pages")]
        public int TotalPages { get; set; }

        [DataMember(Name = "total_count")]
        public int TotalCount { get; set; }
    }
}
