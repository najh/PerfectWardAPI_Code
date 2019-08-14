using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class FinalReflection
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "note")]
        public string Note { get; set; }
    }
}
