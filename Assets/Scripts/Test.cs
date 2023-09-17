using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private string HashPassword(string password)
    {
        // Erstelle eine Instanz des SHA-256-Hashalgorithmus
        using (SHA256 sha256 = SHA256.Create())
        {
            // Konvertiere das Passwort in Bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Berechne den Hashwert des Passworts
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Konvertiere den Hashwert in eine hexadezimale Zeichenfolge
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashedPassword[..26] + "aA1!";
        }
    }
}
