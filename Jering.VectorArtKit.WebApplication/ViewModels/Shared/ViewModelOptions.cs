using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Shared
{
    public class ViewModelOptions
    {
        public string Password_TooShort { get; set; } = "Password must be at least 8 characters.";
        public string Password_NonAlphaNumericRequired { get; set; } = "Password must have at least 1 non-alphanumeric character.";
        public string Password_DigitRequired { get; set; } = "Password must have at least 1 digit.";
        public string Password_LowercaseRequired { get; set; } = "Password must have at least 1 lowercase character.";
        public string Password_UppercaseRequired { get; set; } = "Password must have at least 1 uppercase character.";
        public string Password_Invalid { get; set; } = "Password is invalid.";

        public string Email_Invalid { get; set; } = "Email address is invalid.";
    }
}
