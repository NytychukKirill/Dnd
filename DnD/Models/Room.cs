using System.Security.Cryptography;

namespace DnD.Models;

public class Room
{
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    
    public Guid Id { get; set; } =  Guid.NewGuid();
    public Guid DmToken { get; set; } = Guid.NewGuid();
    
    public string Code { get; set; } = GenerateCode();
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Character> Characters { get; set; } = [];
    
    public static string GenerateCode(int length = 6) => 
        RandomNumberGenerator.GetString(Alphabet, length);
}