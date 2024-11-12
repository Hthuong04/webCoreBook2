﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace webCore.Models
{
    [Table("Category")]
    public class Category_admin
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string title;
        [Required]
        [MaxLength(100)]
        public string Title
        {
            get => title;
            set
            {
                title = value;
                slug = GenerateSlug(value);
            }
        }

        [MaxLength(250)]
        public string Description { get; set; }

        // ID của danh mục cha
        [MaxLength(100)]
        public string ParentId { get; set; } // Đổi tên từ Parent_id thành ParentId
                                            // Thuộc tính mới để lưu tên danh mục cha

        [MaxLength(50)]
        public string Status { get; set; }

        public int Position { get; set; }

        public bool Deleted { get; set; } = false;

        private string slug;
        [MaxLength(100)]
        public string Slug
        {
            get => slug;
            private set => slug = value;
        }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Phương thức tạo slug
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Chuyển sang chữ thường
            string slug = title.ToLowerInvariant();

            // Xóa dấu
            slug = RemoveDiacritics(slug);

            // Thay thế các ký tự không hợp lệ bằng dấu gạch nối
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);

            // Thay thế nhiều dấu gạch nối hoặc khoảng trắng bằng một dấu gạch nối
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

            return slug;
        }

        // Phương thức xóa dấu
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}