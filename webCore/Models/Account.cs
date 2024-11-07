﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class Account
    {
        // Sử dụng Guid thay vì ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.String)]  // Đảm bảo kiểu dữ liệu là String (UUID)
        public Guid Id { get; set; } = Guid.NewGuid();  // Khởi tạo giá trị UUID mới cho Id

        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        // Trường này chỉ dùng trong đăng ký, không lưu vào MongoDB
        [BsonIgnore]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; } = GenerateRandomString(20);

        public int Status { get; set; }

        public bool Deleted { get; set; }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
