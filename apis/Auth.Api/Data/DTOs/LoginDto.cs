using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Data.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Имя пользователя обязательно для заполнения.")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Пароль обязателен для заполнения.")]
        public string Password { get; set; } = string.Empty;
    }
}