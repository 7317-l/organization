using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartySchoolApi.Models.Entities;

/// <summary>
/// 党员考试记录表
/// </summary>
[Table("member_test_records")]
public class MemberTestRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [ForeignKey(nameof(MemberId))]
    public PartyMember? Member { get; set; }

    [Column("test_id")]
    public int TestId { get; set; }

    [ForeignKey(nameof(TestId))]
    public ExamTest? Test { get; set; }

    /// <summary>答案记录，JSON格式，如{"1":"0","2":"[0,1]"}</summary>
    [Column("answers", TypeName = "json")]
    public string Answers { get; set; } = "{}";

    [Column("score")]
    public int Score { get; set; }

    [Column("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
}
