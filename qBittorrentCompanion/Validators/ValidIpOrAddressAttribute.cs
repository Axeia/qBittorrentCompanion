using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace qBittorrentCompanion.Validators
{
    public class ValidIpOrAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                // Accept IP addresses
                if (IPAddress.TryParse(str, out _))
                    return ValidationResult.Success;

                // Accept hostnames/domains
                if (Uri.CheckHostName(str) != UriHostNameType.Unknown)
                    return ValidationResult.Success;
            }

            return new ValidationResult("Not a valid IP or domain name (e.g. 192.168.1.100 or example.com).");
        }
    }
}
