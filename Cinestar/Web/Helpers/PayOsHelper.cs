using System.Text;
using System.Text.RegularExpressions;

namespace Web.Helpers
{
    /// <summary>
    /// Helper class cho PayOS
    /// </summary>
    public static class PayOsHelper
    {
        /// <summary>
        /// Gi?i h?n t?i ?a ký t? cho description theo PayOS API
      /// </summary>
        public const int MAX_DESCRIPTION_LENGTH = 25;

   /// <summary>
        /// Gi?i h?n t?i ?a ký t? cho item name
    /// </summary>
  public const int MAX_ITEM_NAME_LENGTH = 50;

      /// <summary>
    /// Chu?n hóa description cho PayOS (không d?u, ng?n g?n)
    /// </summary>
  /// <param name="description">Mô t? g?c</param>
        /// <returns>Mô t? ?ã ???c chu?n hóa</returns>
        public static string NormalizeDescription(string description)
     {
     if (string.IsNullOrWhiteSpace(description))
 {
         return "Thanh toan";
            }

   // Lo?i b? d?u ti?ng Vi?t
    var normalized = RemoveVietnameseTones(description);

          // Lo?i b? ký t? ??c bi?t, ch? gi? ch?, s? và kho?ng tr?ng
      normalized = Regex.Replace(normalized, @"[^a-zA-Z0-9\s]", "");

    // Rút g?n kho?ng tr?ng
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

     // Gi?i h?n ?? dài
            if (normalized.Length > MAX_DESCRIPTION_LENGTH)
       {
          normalized = normalized.Substring(0, MAX_DESCRIPTION_LENGTH).Trim();
}

    return string.IsNullOrWhiteSpace(normalized) ? "Thanh toan" : normalized;
    }

    /// <summary>
        /// Chu?n hóa tên item
        /// </summary>
        /// <param name="itemName">Tên item g?c</param>
 /// <returns>Tên item ?ã ???c chu?n hóa</returns>
     public static string NormalizeItemName(string itemName)
        {
         if (string.IsNullOrWhiteSpace(itemName))
   {
             return "San pham";
      }

            var normalized = RemoveVietnameseTones(itemName);
 normalized = Regex.Replace(normalized, @"[^a-zA-Z0-9\s\-]", "");
          normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            if (normalized.Length > MAX_ITEM_NAME_LENGTH)
      {
          normalized = normalized.Substring(0, MAX_ITEM_NAME_LENGTH).Trim();
         }

            return string.IsNullOrWhiteSpace(normalized) ? "San pham" : normalized;
        }

        /// <summary>
        /// Lo?i b? d?u ti?ng Vi?t
 /// </summary>
        /// <param name="text">Text có d?u</param>
        /// <returns>Text không d?u</returns>
        public static string RemoveVietnameseTones(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
             return text;

            var vietnameseSigns = new string[]
    {
        "aAeEoOuUiIdDyY",
      "áà??ãâ???????????",
          "ÁÀ??ÃÂ???????????",
        "éè???ê?????",
      "ÉÈ???Ê?????",
              "óò??õô???????????",
             "ÓÒ??ÕÔ???????????",
     "úù?????????",
     "ÚÙ?????????",
             "íì???",
        "ÍÌ???",
       "?",
   "?",
     "ý????",
                "Ý????"
  };

        for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
       {
           text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
 }
       }

            return text;
        }

        /// <summary>
        /// Validate phone number
     /// </summary>
        /// <param name="phone">S? ?i?n tho?i</param>
        /// <returns>S? ?i?n tho?i h?p l?</returns>
        public static string ValidatePhoneNumber(string phone)
      {
            if (string.IsNullOrWhiteSpace(phone))
   {
                return "0000000000";
            }

        // Lo?i b? t?t c? ký t? không ph?i s?
            var digits = Regex.Replace(phone, @"\D", "");

       // N?u b?t ??u b?ng +84, lo?i b? và thay b?ng 0
            if (digits.StartsWith("84"))
            {
     digits = "0" + digits.Substring(2);
    }

        // ??m b?o có 10 ch? s?
        if (digits.Length < 10)
            {
     return "0000000000";
            }

        return digits.Substring(0, 10);
        }

     /// <summary>
        /// Validate email
  /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Email h?p l?</returns>
        public static string ValidateEmail(string email)
        {
     if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
   return "customer@example.com";
  }

            return email.Trim().ToLower();
        }

     /// <summary>
     /// T?o description ng?n g?n t? thông tin vé phim
        /// </summary>
        /// <param name="movieTitle">Tên phim</param>
/// <param name="seatCount">S? gh?</param>
     /// <returns>Description chu?n</returns>
    public static string CreateTicketDescription(string movieTitle, int seatCount = 1)
        {
     var description = seatCount > 1 
    ? $"Ve phim x{seatCount}" 
       : "Ve xem phim";

         return NormalizeDescription(description);
        }

        /// <summary>
 /// Format amount to VND string
        /// </summary>
        /// <param name="amount">S? ti?n</param>
        /// <returns>Chu?i ??nh d?ng ti?n VND</returns>
        public static string FormatAmount(int amount)
        {
            return $"{amount:N0} VND";
      }

        /// <summary>
        /// Validate amount (PayOS yêu c?u amount > 0)
        /// </summary>
      /// <param name="amount">S? ti?n</param>
        /// <returns>S? ti?n h?p l?</returns>
        public static int ValidateAmount(decimal amount)
        {
            var intAmount = (int)amount;
    return intAmount > 0 ? intAmount : 1000; // Minimum 1,000 VND
 }
    }
}
