namespace TaskManager.Application.Validators
{
    public static class ValidationHelpers
    {
        private static readonly HashSet<string> BlacklistedPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common passwords
            "password", "123456", "password123", "admin", "qwerty", "letmein",
            "welcome", "monkey", "1234567890", "password1", "123456789", "12345678",
            "12345", "1234", "111111", "1234567", "dragon", "123123", "baseball",
            "abc123", "football", "master", "sunshine", "iloveyou", "trustno1",
            "jordan23", "harley", "ranger", "woofwoof", "zxcvbnm", "asdfgh",
            "hunter", "buster", "soccer", "hockey", "killer", "george", "sexy",
            "andrew", "charlie", "superman", "asshole", "fuckyou", "dallas",
            "jessica", "panties", "pepper", "1111", "austin", "william", "daniel",
            "golfer", "summer", "heather", "hammer", "yankees", "joshua", "maggie",
            "biteme", "enter", "ashley", "thunder", "cowboy", "silver", "richard",
            "fucker", "orange", "merlin", "michelle", "corvette", "bigdog", "cheese",
            "matthew", "121212", "patrick", "martin", "freedom", "ginger", "blowjob",
            "nicole", "sparky", "yellow", "camaro", "secret", "dick", "falcon",
            "taylor", "birdman", "donald", "murphy", "mexico", "anthony", "ferrari",
            "bulldog", "toyota", "jordan", "purple", "banana", "peter", "nathan",
            "starwars", "5150", "willie", "love", "diablo", "celtic", "dublin",
            "williams", "chris", "john", "steel", "apple", "black", "jessica", "madrid",
            
            // Sequential patterns
            "abcdef", "fedcba", "abcdefg", "gfedcba", "qwerty", "ytrewq",
            "asdfgh", "hgfdsa", "zxcvbn", "nbvcxz", "098765", "567890",
            
            // Keyboard patterns
            "qwertyuiop", "asdfghjkl", "zxcvbnm", "1qaz2wsx", "qazwsx",
            "qweasd", "asdqwe", "zaqwsx", "xswzaq", "cdefgh", "vfrcde",
            
            // Company/Brand names
            "microsoft", "google", "facebook", "amazon", "netflix", "spotify",
            "instagram", "youtube", "twitter", "linkedin", "snapchat", "tiktok",
            
            // Years that are commonly used
            "2023", "2024", "2025", "1990", "1991", "1992", "1993", "1994",
            "1995", "1996", "1997", "1998", "1999", "2000", "2001", "2002"
        };

        public static bool BeAStrongPassword(string password)
        {
            if (BlacklistedPasswords.Contains(password))
                return false;

            return true;
        }
    }
}
