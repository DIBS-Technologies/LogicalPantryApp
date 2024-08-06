namespace LogicalPantry.Web.Helper
{

    public class ViewConstants
    {
        public const string GETALLUSER = "GetAllUser";
        public const string ADDUSER = "AddUser";
        public const string LOGINVIEW = "loginView";
        public const string INDEX = "Index";
        public const string HOME = "Home";
        public const string AUTH = "Auth";
        public const string USER = "User";
        public const string Registration = "Registration";
        public const string TimeSlotSignUp = "TimeSlotSignup";
        public const string TimeSlot = "TimeSlot";
        public const string Calandar = "Calandar";
        public const string UserCalandar = "UserCalendar";




    }

    public enum UserRoles
        {
        Admin = 1,
        Manager,
        User
        }
    public class UserSystemRoles
        {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Manager = "Manager";
        public const string AdminOrUserOrManager = Admin + "," + User + "," + Manager;
        public const string UserOrManager = User + "," + Manager;
        public const string AdminOrManager = Admin + "," + Manager;
        public const string AdminOrUser = Admin + "," + User;

        }

    }

