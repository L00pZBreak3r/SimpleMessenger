using System;
using Dapper;

using SimpleMessengerDataLibrary.Database;
using SimpleMessengerModelLibrary.Models;

namespace SimpleMessengerDataLibrary
{

  public class SimpleMessengerDBConfiguration : IDisposable
  {
    private const string DATABASE_NAME_DEFAULT = "SimpleMessenger_db";
    private const int USER_COUNT_DEFAULT = 3;

    private bool disposedValue;
    public readonly SimpleMessengerDBContext SimpleMessengerDB;

    public SimpleMessengerDBConfiguration(string? aDatabaseName = null)
    {
      if (string.IsNullOrWhiteSpace(aDatabaseName))
        aDatabaseName = DATABASE_NAME_DEFAULT;

      SimpleMessengerDB = new SimpleMessengerDBContext(aDatabaseName!);

    }

    /// <summary>
    /// Fills table Users with fake test data.
    /// </summary>
    /// <returns></returns>
    public void AddRandomUsers()
    {
        if (SimpleMessengerDB.TableUsers.Length > 0)
            return;

        const int aUserCount = USER_COUNT_DEFAULT;

        const string sqlCommand = "INSERT INTO Users (Name) VALUES (@Name)";
        
        for (int i = 0; i < aUserCount; i++)
        {
          var aUser = new User
          {
            Name = $"User_{i + 1}"
          };
          SimpleMessengerDB.ConnectionInstance.Execute(sqlCommand, aUser);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          SimpleMessengerDB.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SimpleMessengerDBConfiguration()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
