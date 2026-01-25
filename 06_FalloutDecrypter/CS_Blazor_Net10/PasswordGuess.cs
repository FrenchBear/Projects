// Fallout Decrypter
// Store one password attempt and number of placed letters

namespace FalloutDecrypter;

public class PasswordGuess
{
    public string Guess { get; set; } = "";
    public int Placed { get; set; }
    public string Status { get; set; } = "";
}
