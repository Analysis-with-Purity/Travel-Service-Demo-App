using System.Security.Cryptography;
using System.Text;

namespace TravelPackageService.Helper;

 
public static class HelperHasherPassword
{
    
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            string hashedString = ConvertToHexString(hashBytes);

            return hashedString;
        }
        ;
    }
    private static string ConvertToHexString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
