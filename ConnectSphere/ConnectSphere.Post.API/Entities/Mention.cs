using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectSphere.Post.API.Entities;

public class Mention
{
    [Key]
    public int MentionId { get; set; }

    public int PostId { get; set; }

    [ForeignKey("PostId")]
    public Post Post { get; set; }

    public int UserId { get; set; } 

    public DateTime MentionedAt { get; set; } = DateTime.UtcNow;
}
