using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class DetailedReportResponse : IDeserializedCallback
    {
        [DataMember(Name = "report")]
        public Report Report { get; set; }

        public void OnDeserialized()
        {
            Report.OnDeserialized();
        }
    }
}