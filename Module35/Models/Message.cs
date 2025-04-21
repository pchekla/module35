using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Module35.Models;

public class Message
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Text { get; set; } = string.Empty;
    
    public DateTime SentDate { get; set; } = DateTime.Now;

    [Required]
    public string SenderId { get; set; } = string.Empty;
    
    [ForeignKey("SenderId")]
    public User? Sender { get; set; }

    [Required]
    public string RecipientId { get; set; } = string.Empty;
    
    [ForeignKey("RecipientId")]
    public User? Recipient { get; set; }
} 