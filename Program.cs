using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Data.OleDb;
using FileSignatures;

namespace HFSQL
{
  class Program
  {
    static string NULL = "null";
    static void Main(string[] args)
    {
      if (args.Length > 2)
      {
        Console.WriteLine("Error: Too many arguments");
        Environment.Exit(-1);
      }
      string dbName = args.ElementAtOrDefault(0);
      string tableName = args.ElementAtOrDefault(1);

      if (dbName == null || tableName == null)
      {
        Console.WriteLine("Error: Please supply DB name and table name as arguments");
        Environment.Exit(-1);
      }

      string connectionString = $"Provider=PCSOFT.HFSQL; Data Source=localhost:4900; User ID=admin; Initial Catalog={dbName}; Extended Properties=\"Language=ISO-8859-2;\"";
      OleDbConnection connect = new OleDbConnection(connectionString);

      connect.Open();
      Console.WriteLine("In progress...");

      OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", connect);
      OleDbDataReader reader = cmd.ExecuteReader();

      string path = Assembly.GetExecutingAssembly().Location;
      string rootDirectory = Path.GetDirectoryName(path);

      string filesDirectory = rootDirectory + @"\files\" + tableName;
      Directory.CreateDirectory(filesDirectory);

      string query = $"CREATE TABLE \"{tableName}\" (";

      string[] columns = new string[reader.FieldCount];
      for (int i = 0; i < reader.FieldCount; i++)
      {
        columns[i] = "\"" + reader.GetName(i) + "\"";
      }
      query += String.Join(",", columns) + ") AS SELECT * FROM (VALUES \n";

      List<string> values = new List<string>();
      while (reader.Read())
      {
        string[] fields = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
          Type fieldType = reader.GetFieldType(i);
          if (fieldType.IsArray)
          {
            using (Stream stream = reader.GetStream(i))
            {
              FileFormatInspector inspector = new FileFormatInspector();
              FileFormat format = inspector.DetermineFileFormat(stream);
              if (format == null)
              {
                fields[i] = NULL;
                continue;
              }

              string fileName = Regex.Replace(Guid.NewGuid().ToString(), "-", "") + "." + format.Extension;
              string filePath = filesDirectory + "/" + fileName;
              using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
              {
                stream.CopyTo(fileStream);
              }
              fields[i] = $"'{fileName}'";
            }
          }
          else
          {
            string row = Regex.Replace(reader.GetValue(i).ToString().Trim(), "'", @"\'");
            if (
                (fieldType == typeof(String) && row.Length == 0) ||
                (fieldType == typeof(DateTime) && row == "01.01.1999 00:00:00")
            )
            {
              fields[i] = NULL;
              continue;
            }
            if (fieldType == typeof(Boolean))
            {
              row = row.ToLower();
            }
            fields[i] = fieldType == typeof(String) || fieldType == typeof(DateTime) ? $"E'{row}'" : row;
          }
        }
        values.Add("(" + string.Join(",", fields) + ")");
      }
      query += String.Join(",", values) + ") AS t";

      string scriptsDirectory = rootDirectory + @"\scripts";
      Directory.CreateDirectory(scriptsDirectory);

      string scriptPath = scriptsDirectory + @"\" + tableName + ".sql";
      using (FileStream fileStream = new FileStream(scriptPath, FileMode.Create, FileAccess.Write))
      {
        byte[] charBuffer = Encoding.UTF8.GetBytes(query);
        fileStream.Write(charBuffer, 0, charBuffer.Length);
      }
      connect.Close();
      connect.Dispose();
      Exit(tableName);
    }
    static void Exit(string tableName)
    {
      Console.WriteLine($"Done for: {tableName}");
      System.Environment.Exit(-1);
    }
  }
}
