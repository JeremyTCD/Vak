﻿using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels
{
    public class SignUpViewModel
    {
        [Required]
        [ValidateEmailAddress(nameof(StringOptions.Email_Invalid), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateMinLength(8, nameof(StringOptions.Password_TooShort), typeof(StringOptions))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [ValidateMatches(nameof(Password), nameof(StringOptions.ConfirmPassword_DoesNotMatchPassword), typeof(StringOptions))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}