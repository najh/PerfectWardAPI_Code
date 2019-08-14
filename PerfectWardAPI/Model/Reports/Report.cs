using PerfectWardAPI.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    public class ReportEqualityComparer : IEqualityComparer<Report>
    {
        public bool Equals(Report x, Report y) => x.Id == y.Id;
        public int GetHashCode(Report obj) => obj.Id;
    }

    [DataContract]
    public class Report : IDeserializedCallback
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "started_at")]
        private long StartedAt { get; set; }

        [DataMember(Name = "ended_at")]
        private long EndedAt { get; set; }

        [DataMember(Name = "inspection_type")]
        public InspectionType InspectionType { get; set; }

        [DataMember(Name = "area")]
        public Area Area { get; set; }

        [DataMember(Name = "inspector")]
        public Inspector Inspector { get; set; }

        [DataMember(Name = "survey")]
        public Survey Survey { get; set; }

        [DataMember(Name = "final_reflections")]
        public FinalReflection[] FinalReflections { get; set; }

        [DataMember(Name = "answers")]
        public Answer[] Answers { get; set; }

        [IgnoreDataMember]
        public DateTime StartedAtDate { get => StartedAt.ToDateTime(); set => StartedAt = value.ToTimeStamp(); }

        [IgnoreDataMember]
        public DateTime EndedAtDate { get => EndedAt.ToDateTime(); set => EndedAt = value.ToTimeStamp(); }

        public void OnDeserialized()
        {
            if(FinalReflections == null)
            {
                FinalReflections = new FinalReflection[0];
            }
            if (Answers == null)
            {
                Answers = new Answer[0];
            }
            foreach(var a in Answers)
            {
                a.OnDeserialized();
            }
        }
    }
}
