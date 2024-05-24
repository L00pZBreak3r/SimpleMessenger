using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleMessengerModelLibrary.Models
{
  public class TextMessage : IEquatable<TextMessage>
  {
    public long Id { get; set; }
    [Required]
    public int Number { get; set; }
    [Required]
    public DateTime TimeStamp { get; set; }
    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public long SenderId { get; set; }

    [InverseProperty("SenderTextMessages")]
    public User? Sender { get; set; }

    [Required]
    public long ReceiverId { get; set; }

    [InverseProperty("ReceiverTextMessages")]
    public User? Receiver { get; set; }

    public bool Equals(TextMessage? other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return (Id > 0L) ? (Id == other!.Id) : Number == other!.Number && Text.Equals(other!.Text) &&
            SenderId == other!.SenderId &&
            ReceiverId == other!.ReceiverId &&
            TimeStamp == other!.TimeStamp;
    }

    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals(obj as TextMessage);
    }

    public override int GetHashCode()
    {
      return (Id > 0L) ? Id.GetHashCode() : ((((((((Number * 397) ^ Text.GetHashCode()) * 397) ^ SenderId.GetHashCode()) * 397) ^ ReceiverId.GetHashCode()) * 397) ^ TimeStamp.GetHashCode());
    }

    public override string ToString()
    {
      return Text;
    }
  }
}
