using InternLog.Data;
using InternLog.Models;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace InternLog.Services;

public class AuthService
{
    public bool Register(
        string firstName,
        string lastName,
        string email,
        string password)
    {
        using var db = new AppDbContext();

        // Provjera postoji li već korisnik s tim emailom
        bool emailExists = db.Users.Any(u => u.Email == email);

        if (emailExists)
        {
            return false;
        }

        // Generiranje lozinke
        string passwordHash = HashPassword(password);

        // Kreiranje korisnika
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash
        };

        // Spremanje u bazu
        db.Users.Add(user);
        db.SaveChanges();

        return true;
    }


    public User? Login(string email, string password)
    {
        using var db = new AppDbContext();

        var user = db.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            return null;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }


    private string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }




    private bool VerifyPassword(string password, string storedPassword)
    {
        string[] parts = storedPassword.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedHash = Convert.FromBase64String(parts[1]);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(
            hash,
            storedHash);
    }

}