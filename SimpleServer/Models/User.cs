using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleServer.Models
{
    [Table("users", Schema = "public")]   // map to public.users
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
