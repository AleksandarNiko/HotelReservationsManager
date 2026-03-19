using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using HotelReservationsManager.Common;

namespace HotelReservationsManager.Data.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(EntityValidationsConstants.User.UsernameMinLength)]
        [MaxLength(EntityValidationsConstants.User.UsernameMaxLength)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(EntityValidationsConstants.User.PasswordMinLength)]
        [MaxLength(EntityValidationsConstants.User.PasswordMaxLength)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [MinLength(EntityValidationsConstants.User.NameMinLength)]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MinLength(EntityValidationsConstants.User.NameMinLength)]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string MiddleName { get; set; } = null!;

        [Required]
        [MinLength(EntityValidationsConstants.User.NameMinLength)]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(EntityValidationsConstants.User.EGNLength, MinimumLength = EntityValidationsConstants.User.EGNLength)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "ЕГН-то трябва да съдържа точно 10 цифри.")]
        public string EGN { get; set; } = null!;

        [Required]
        [StringLength(EntityValidationsConstants.User.PhoneNumberLength)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Телефонният номер трябва да съдържа само цифри.")]
        public string PhoneNumber { get; set; } = null!;

        [Required, EmailAddress]
        [MaxLength(EntityValidationsConstants.User.EmailMaxLength)]
        public string Email { get; set; } = null!;

        [Required]
        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public DateTime? LeavingDate { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
