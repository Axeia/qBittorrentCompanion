using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.Validators
{
    public class ValidIpAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                if (IPAddress.TryParse(str, out _))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Not a valid IP, it should look something like 192.168.1.100.");
        }
    }
}
