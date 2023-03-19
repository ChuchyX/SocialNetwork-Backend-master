using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork
{
    public class ReturnUser
    {
        public string username { get; set; }
        public string email { get; set; }
        public int edad { get; set; }
        public string sexo { get; set; }

        public FileContentResult image { get; set; }

    }
}
