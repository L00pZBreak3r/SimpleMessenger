using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleMessengerModelLibrary.Models
{
  public class User : IEquatable<User>
  {
    public long Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;

    [InverseProperty("Sender")]
    public ICollection<TextMessage> SenderTextMessages { get; set; } = new List<TextMessage>();

    [InverseProperty("Receiver")]
    public ICollection<TextMessage> ReceiverTextMessages { get; set; } = new List<TextMessage>();

    [NotMapped]
    public TextMessage[] TextMessages => SenderTextMessages.Union(ReceiverTextMessages).OrderBy(a => a.TimeStamp).ToArray();

    public bool Equals(User? other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return (Id > 0L) ? (Id == other!.Id) : Name.Equals(other!.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals(obj as User);
    }

    public override int GetHashCode()
    {
      return (Id > 0L) ? Id.GetHashCode() : Name.GetHashCode();
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
