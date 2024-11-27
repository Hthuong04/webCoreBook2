﻿using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;

namespace webCore.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Product_admin> _productCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);

            _productCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _categoryCollection = mongoDatabase.GetCollection<Category>("Category");
        }
        public async Task<Product_admin> GetProductByIdAsync(string id)
        {
            return await _productCollection.Find(p => p.Id == id && !p.Deleted).FirstOrDefaultAsync();
        }
        // Lấy danh sách sản phẩm
        public async Task<Dictionary<string, List<Product_admin>>> GetProductsGroupedByFeaturedAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);
            var products = await _productCollection.Find(filter).ToListAsync();

            var groupedProducts = products
                .GroupBy(p => p.Featured)
                .ToDictionary(
                    g => GetFeaturedStatusName(g.Key),
                    g => g.ToList()
                );

            return groupedProducts;
        }

        private string GetFeaturedStatusName(FeaturedStatus status)
        {
            switch (status)
            {
                case FeaturedStatus.Highlighted: return "Nổi bật";
                case FeaturedStatus.New: return "Mới";
                case FeaturedStatus.Suggested: return "Gợi ý";
                default: return "Không nổi bật";
            }
        }



        // Lưu người dùng
        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Lấy thông tin người dùng theo email (Bất đồng bộ)
        public async Task<User> GetAccountByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        // Lấy thông tin người dùng theo Username
        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            var user = await _userCollection.Find(u => u.Name == userName).FirstOrDefaultAsync();
            return user;
        }

        // Cập nhật thông tin người dùng
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                // Tạo filter để tìm người dùng cần cập nhật theo Id (để đảm bảo cập nhật đúng người dùng)
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

                // Tạo update với các trường cần thay đổi
                var update = Builders<User>.Update
                    .Set(u => u.Name, user.Name)          // Cập nhật tên người dùng
                    .Set(u => u.Phone, user.Phone)        // Cập nhật số điện thoại
                    .Set(u => u.Gender, user.Gender)      // Cập nhật giới tính
                    .Set(u => u.Birthday, user.Birthday)  // Cập nhật ngày sinh
                    .Set(u => u.Address, user.Address)    // Cập nhật địa chỉ
                    .Set(u => u.Password, user.Password)  // Cập nhật mật khẩu
                    .Set(u => u.ProfileImage, user.ProfileImage);  // Cập nhật ảnh đại diện

                // Thực hiện cập nhật
                var result = await _userCollection.UpdateOneAsync(filter, update);

                // Kiểm tra kết quả
                if (result.MatchedCount == 0)
                {
                    Console.WriteLine("Không tìm thấy người dùng để cập nhật.");
                    return false;
                }

                if (result.ModifiedCount > 0)
                {
                    return true;  // Cập nhật thành công
                }
                else
                {
                    Console.WriteLine("Không có thay đổi nào được thực hiện.");
                    return false;  // Không có thay đổi
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật người dùng: {ex.Message}");
                return false;
            }
        }
        // Xóa người dùng (thay đổi trạng thái thay vì xóa cứng)
        public async Task DeleteUserAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Deleted, true);

            await _userCollection.UpdateOneAsync(filter, update);
        }
        // Lấy danh mục gốc (không có ParentId)
        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, null);
            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy danh mục con theo ParentId
        public async Task<List<Category>> GetSubCategoriesByParentIdAsync(string parentId)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, parentId);
            return await _categoryCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false);
            return await _categoryCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Product_admin>> GetProductsByCategoryPositionAsync(int position)
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Position, position) &
                         Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);

            return await _productCollection.Find(filter).ToListAsync();
        }
        public IMongoCollection<Product_admin> GetProductsCollection()
        {
            return _mongoDatabase.GetCollection<Product_admin>("Product");
        }
        
        // Lấy tất cả sản phẩm
        public async Task<List<Product_admin>> GetProductsAsync()
        {
            return await _productCollection.Find(product => true).ToListAsync();
        }
    }
}
