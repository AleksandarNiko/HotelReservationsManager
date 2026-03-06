
namespace HotelReservationsManager.Common
{
    public class EntityValidationsConstants
    {
        public class User
        {
            public const int UsernameMinLength = 3;
            public const int UsernameMaxLength = 30;

            public const int PasswordMinLength = 6;
            public const int PasswordMaxLength = 100;

            public const int NameMinLength = 2;
            public const int NameMaxLength = 50;

            public const int EGNLength = 10;

            public const int PhoneNumberLength = 10;
            
            public const int EmailMinLength = 10;
            public const int EmailMaxLength = 255;

        }
    }
}
