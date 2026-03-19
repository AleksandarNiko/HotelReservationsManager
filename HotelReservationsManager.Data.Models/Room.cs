using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HotelReservationsManager.Data.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 10)]
        public int Capacity { get; set; }

        [Required]
        public RoomType Type { get; set; }

        public bool IsFree { get; set; } = true;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BedPriceAdult { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BedPriceYoung { get; set; }

        public int Number { get; set; }
    }

    public enum RoomType
    {
        TwoSingleBeds,
        Apartment,
        DoubleBed,
        Penthouse,
        Maisonette
    }
}
