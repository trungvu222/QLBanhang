﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLBanhang.Models;
using System.Net;
using System.Net.Mail;

namespace QLBanhang.Controllers
{
    public class GiohangController : Controller
    {
        private qlbanhangEntities db = new qlbanhangEntities();
        // GET: Giohang
        public ActionResult Index()
        {
            List<CartItem> giohang = Session["giohang"] as List<CartItem>;
            return View(giohang);
        }
        //Khai báo phương thức thêm sản phẩm vào giỏ hàng
        public RedirectToRouteResult AddToCart (string MaSP)
        {
            if(Session["giohang"] == null)
            {
                Session["giohang"] = new List<CartItem>();
            }
            List<CartItem> giohang = Session["giohang"] as List<CartItem>;
            //Kiểm tra sản phẩm khách đang chọn có trong giỏ hàng chưa
            if(giohang .FirstOrDefault(m => m.MaSP == MaSP)==null)//Chưa có trong giỏ hàng
            {
                SanPham sp = db.SanPhams.Find(MaSP);
                CartItem newItem = new CartItem();
                newItem.MaSP = MaSP;
                newItem.TenSP = sp.TenSP;
                newItem.SoLuong = 1;
                newItem.DonGia = Convert.ToDouble(sp.Dongia);
                giohang.Add(newItem);
            }
            else//sản phẩm đã có trong giỏ hàng ==> Tăng số lượng lên 1
            {
                CartItem cardItem = giohang.FirstOrDefault(m => m.MaSP == MaSP);
                cardItem.SoLuong++;
            }
            Session["giohang"] = giohang;
            return RedirectToAction("Index");
        }
        public RedirectToRouteResult Update (string MaSP, int txtSoluong)
        {
            //Tìm CartItem muốn xóa
            List<CartItem> giohang = Session["giohang"] as List<CartItem>;
            CartItem item = giohang.FirstOrDefault(m => m.MaSP == MaSP);
            if(item!=null)
            {
                item.SoLuong = txtSoluong;
                Session["giohang"] = giohang;
            }
            return RedirectToAction("Index");
        }
        public RedirectToRouteResult DelCartItem (string MaSP)
        {
            //Tìm CartItem muốn xóa
            List<CartItem> giohang = Session["giohang"] as List<CartItem>;
            CartItem item = giohang.FirstOrDefault(m => m.MaSP == MaSP);
            if (item != null)
            {
                giohang.Remove(item);
                Session["giohang"] = giohang;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Order (string Email, string Phone)
        {
            List<CartItem> giohang = Session["giohang"] as List<CartItem>;
            string sMsg = "<html><body><table border='1'><caption>Thông tin đặt hàng</caption><tr><th>STT</th><th>Tên hàng</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr>";
            int i = 0;
            double tongtien = 0;
            foreach (CartItem item in giohang)
            {
                i++;
                sMsg += "<tr>";
                sMsg += "<td>" + i.ToString() + "</td>";
                sMsg += "<td>" + item.TenSP + "</td>";
                sMsg += "<td>" + item.SoLuong.ToString() + "</td>";
                sMsg += "<td>" + item.DonGia.ToString() + "</td>";
                sMsg += "<td>" + String.Format("{0:#,###}", item.SoLuong*item.DonGia) + "</td>";
                sMsg += "<tr>";
                tongtien += item.SoLuong * item.DonGia;
            }
            //Gửi email cho khách hàng
            sMsg += "<tr><th colspan='5'>Tổng cộng: "
                + String.Format("{0:#,### vnđ}", tongtien) + "</th></tr></table>";
            MailMessage mail = new MailMessage("vuthanhtrung385@gmail.com", Email, "Thông tin đơn hàng", sMsg);
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("vuthanhtrung385@gmail.com", "yubhtslmbasvzeqa");
            mail.IsBodyHtml = true;
            client.Send(mail);
            return RedirectToAction("Index", "Home");
        }
    }
}