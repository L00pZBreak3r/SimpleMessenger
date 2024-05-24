using System;
using System.Linq;
using Dapper;
using Npgsql;

using SimpleMessengerModelLibrary.Models;

namespace SimpleMessengerDataLibrary.Database
{
  public class SimpleMessengerDBContext : IDisposable
  {
    public readonly string DatabaseName;
    public readonly string DatabaseConnectionString;

    public readonly NpgsqlConnection ConnectionInstance;
    
    public User[] TableUsers { get; private set; }
    public TextMessage[] TableTextMessages { get; private set; }

    public SimpleMessengerDBContext(string aDatabaseName)
    {
      DatabaseName = aDatabaseName;
      DatabaseConnectionString = "Host=localhost;Database=" + DatabaseName + ";Username=simplemessenger_user;Password=SimpleMessenger_pw";
      ConnectionInstance = new NpgsqlConnection(DatabaseConnectionString);
      ConnectionInstance.Open();

      TableUsers = ConnectionInstance.Query<User>("SELECT * FROM Users").ToArray();
      TableTextMessages = ConnectionInstance.Query<TextMessage>("SELECT * FROM TextMessages").ToArray();
      UpdateTextMessages();
    }

    private void UpdateTextMessages()
    {
        foreach (var aTextMessage in TableTextMessages)
        {
            aTextMessage.Sender = TableUsers.First(a => a.Id == aTextMessage.SenderId);
            aTextMessage.Receiver = TableUsers.First(a => a.Id == aTextMessage.ReceiverId);
        }
    }

    public void AddTextMessage(TextMessage aTextMessage)
    {
        const string sqlCommand = "INSERT INTO TextMessages (Number, TimeStamp, Text, SenderId, ReceiverId) VALUES (@Number, @TimeStamp, @Text, @SenderId, @ReceiverId)";
        ConnectionInstance.Execute(sqlCommand, aTextMessage);
        TableTextMessages = ConnectionInstance.Query<TextMessage>("SELECT * FROM TextMessages").ToArray();
        UpdateTextMessages();
    }

    public void RemoveTextMessage(TextMessage aTextMessage)
    {
        const string sqlCommand = "DELETE FROM TextMessages WHERE Id = @Id";
        ConnectionInstance.Execute(sqlCommand, aTextMessage);
        TableTextMessages = ConnectionInstance.Query<TextMessage>("SELECT * FROM TextMessages").ToArray();
        UpdateTextMessages();
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          ConnectionInstance.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
