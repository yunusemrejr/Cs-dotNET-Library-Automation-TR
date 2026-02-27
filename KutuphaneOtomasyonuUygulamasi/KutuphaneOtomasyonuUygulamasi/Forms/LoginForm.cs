using System;
using System.Drawing;
using System.Windows.Forms;
using KutuphaneOtomasyonuUygulamasi.Business;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.Forms
{
    public partial class LoginForm : Form
    {
        private readonly LibraryService _libraryService;

        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _libraryService = new LibraryService();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            this.Text = "Kütüphane Yönetim Sistemi - Giriş";
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);

            // Title Label
            var titleLabel = new Label
            {
                Text = "KÜTÜPHANE YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(460, 40),
                Location = new Point(20, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitleLabel = new Label
            {
                Text = "v2.0 - Modern & Secure",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(460, 25),
                Location = new Point(20, 70),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username Panel
            var usernameLabel = new Label
            {
                Text = "Kullanıcı Adı:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(50, 120)
            };

            var txtUsername = new TextBox
            {
                Name = "txtUsername",
                Font = new Font("Segoe UI", 12),
                Size = new Size(380, 30),
                Location = new Point(50, 145),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password Panel
            var passwordLabel = new Label
            {
                Text = "Şifre:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(50, 190)
            };

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Font = new Font("Segoe UI", 12),
                Size = new Size(380, 30),
                Location = new Point(50, 215),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                UseSystemPasswordChar = true
            };

            // Login Button
            var btnLogin = new Button
            {
                Text = "GİRİŞ YAP",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 45),
                Location = new Point(50, 270),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => PerformLogin(txtUsername.Text, txtPassword.Text);

            // Enter key support
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    PerformLogin(txtUsername.Text, txtPassword.Text);
                    e.Handled = true;
                }
            };

            // Add controls
            this.Controls.AddRange(new Control[] {
                titleLabel, subtitleLabel, usernameLabel, txtUsername,
                passwordLabel, txtPassword, btnLogin
            });
        }

        private void PerformLogin(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Lütfen kullanıcı adı ve şifre giriniz.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = _libraryService.Login(username, password);

                if (user != null)
                {
                    LoggedInUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Giriş Başarısız",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Login error", ex);
                MessageBox.Show($"Giriş sırasında hata oluştu: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
