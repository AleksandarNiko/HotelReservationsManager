using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.ViewModels
{
    public class ReservationCreateViewModel
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public List<int> SelectedClientIds { get; set; } = new List<int>();

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DateAfter(nameof(CheckInDate), ErrorMessage = "Датата на освобождаване трябва да е след датата на настаняване.")]
        public DateTime CheckOutDate { get; set; }

        public bool HasBreakfast { get; set; }
        public bool IsAllInclusive { get; set; }
    }

    public class DateAfterAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;
        public DateAfterAttribute(string startDatePropertyName) => _startDatePropertyName = startDatePropertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            var startDate = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDate = (DateTime)value;

            return endDate > startDate ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
    }
}
