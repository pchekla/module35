using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Module35.Models;

public class FriendRelation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [Required]
    public string FriendId { get; set; } = string.Empty;
    
    [ForeignKey("FriendId")]
    public User? Friend { get; set; }
    
    public DateTime RequestDate { get; set; } = DateTime.Now;
    
    public bool IsAccepted { get; set; } = false;
    
    public DateTime? AcceptedDate { get; set; }
} 