using System.ComponentModel.DataAnnotations;

namespace Authentication.Apis.Models
{
    public class LoginInputModel
    {
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
