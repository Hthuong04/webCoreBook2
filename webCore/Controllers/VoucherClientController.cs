﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class VoucherClientController : Controller
    {
        private readonly VoucherClientService _voucherService;

        // Constructor để inject VoucherClientService
        public VoucherClientController(VoucherClientService voucherService)
        {
            _voucherService = voucherService;
        }

        // Phương thức hiển thị danh sách các voucher
        public async Task<IActionResult> VoucherClient()
        {

            var userId = HttpContext.Session.GetString("UserToken");
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            // Kiểm tra nếu chưa đăng nhập, điều hướng về trang đăng nhập
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            // lấy tất cả voucher có trạng thái 'isActive' là true
            var vouchers = await _voucherService.GetActiveVouchersAsync();

            // Trả về view với danh sách voucher
            return View(vouchers);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string code)
        {
            // Validate user is logged in
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            // Try to apply the voucher
            bool isApplied = await _voucherService.ApplyVoucherAsync(code);

            if (isApplied)
            {
                // Get the voucher to retrieve discount value
                var voucher = await _voucherService.GetVoucherByCodeAsync(code);

                // Save voucher details in session
                HttpContext.Session.SetString("SelectedVoucher", voucher.DiscountValue.ToString());

                return Json(new { success = true, discountValue = voucher.DiscountValue });
            }
            else
            {
                return Json(new { success = false, message = "Invalid or expired voucher" });
            }
        }

    }
}
