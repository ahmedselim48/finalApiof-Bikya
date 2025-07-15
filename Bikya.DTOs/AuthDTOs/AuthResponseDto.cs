using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.AuthDTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public List<string>? Roles { get; set; }
        public DateTime? Expiration { get; set; }
    }
}
