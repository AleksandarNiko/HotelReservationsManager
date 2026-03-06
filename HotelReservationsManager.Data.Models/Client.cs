using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using HotelReservationsManager.Common;

namespace HotelReservationsManager.Data.Models
{
    public class Client
    {
        [Key]
        public  int Id { get; set; }

        [Required]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(EntityValidationsConstants.User.NameMaxLength)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(EntityValidationsConstants.User.PhoneNumberLength)]
        public string PhoneNumber { get; set; } = null!;

        [Required,EmailAddress]
        [MaxLength(EntityValidationsConstants.User.EmailMaxLength)]
        public string Email { get; set; } = null!;

        public  bool IsAdult { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
