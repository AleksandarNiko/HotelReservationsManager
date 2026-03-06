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
        public  int Id { get; set; }

        [Required]
        [MaxLength(EntityValidationsConstants.User.UsernameMaxLength)]
        public  string Username { get; set; } = null!;

        [Required]
        [MaxLength(EntityValidationsConstants.User.PasswordMaxLength)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string MiddleName { get; set; } = null!;

        [Required]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(EntityValidationsConstants.User.EGNLength)]
        public string EGN { get; set; } = null!;

        [Required,Phone]
        [StringLength(EntityValidationsConstants.User.PhoneNumberLength)]
        public string PhoneNumber { get; set; } = null!;

        [Required,EmailAddress]
        [MaxLength(EntityValidationsConstants.User.EmailMaxLength)]
        public string Email { get; set; } = null!;

        [Required]
        public  DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime?  LeavingDate { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
