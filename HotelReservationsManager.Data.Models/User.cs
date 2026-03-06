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
        [MaxLength()]
        public  string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public  string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; } = null!;

        [Required] 
        public string EGN { get; set; } = null!;

        [Required] 
        public string PhoneNumber { get; set; } = null!;

        [Required] 
        public string Email { get; set; } = null!;

        [Required]
        public  DateTime HireDate { get; set; }

        [Required]
        public  bool IsActive { get; set; }

        public DateTime?  LeavingDate { get; set; }
    }
}
