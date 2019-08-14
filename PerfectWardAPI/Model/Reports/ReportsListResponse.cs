using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class ReportsListResponse : IDeserializedCallback
    {
        [DataMember(Name = "reports")]
        public List<Report> Reports { get; set; }

        [DataMember(Name = "meta")]
        public Meta Meta { get; set; }

        public void OnDeserialized()
        {
            foreach(var r in Reports)
            {
                r.OnDeserialized();
            }
        }
    }
}
