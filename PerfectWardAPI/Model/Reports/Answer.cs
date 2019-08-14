using System.Runtime.Serialization;

namespace PerfectWardAPI.Model.Reports
{
    [DataContract]
    public class Answer : IDeserializedCallback
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "score")]
        public float Score { get; set; }

        [DataMember(Name = "it_scores")]
        public bool ItScores { get; set; }

        [DataMember(Name = "is_multiple")]
        public bool IsMultiple { get; set; }

        [DataMember(Name = "answer_text")]
        public string AnswerText { get; set; }

        [DataMember(Name = "answer_choice_id")]
        public int? AnswerChoiceId { get; set; }

        [DataMember(Name = "question_id")]
        public int QuestionId { get; set; }

        [DataMember(Name = "question_text")]
        public string QuestionText { get; set; }

        [DataMember(Name = "category_name")]
        public string CategoryName { get; set; }

        [DataMember(Name = "note")]
        public string Note { get; set; }

        [DataMember(Name = "sub_answers")]
        public Answer[] SubAnswers { get; set; }

        public void OnDeserialized()
        {
            if (SubAnswers == null)
            {
                SubAnswers = new Answer[0];
            }
        }
    }
}
