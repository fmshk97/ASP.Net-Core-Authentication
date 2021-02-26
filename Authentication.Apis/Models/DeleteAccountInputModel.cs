using System.ComponentModel.DataAnnotations;

namespace Authentication.Apis.Models
{
    public class DeleteAccountInputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
