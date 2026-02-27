using System;
using System.Drawing;
using System.Windows.Forms;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Business;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.Forms
{
    public partial class MainForm : Form
    {
        private readonly LibraryService _libraryService;
        private User _currentUser;
        private Panel _contentPanel;
        private Label _welcomeLabel;

        public MainForm()
        {
            InitializeComponent();
            _libraryService = new LibraryService();
            
            // Show login first
            ShowLoginDialog();
        }

        private void ShowLoginDialog()
        {
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _currentUser = loginForm.LoggedInUser;
                    UpdateWelcomeMessage();
                    LoadDashboard();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Kütüphane Yönetim Sistemi v2.0";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(236, 240, 241);

            // Top Panel (Header)
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            var titleLabel = new Label
            {
                Text = "📚 KÜTÜPHANE YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            _welcomeLabel = new Label
            {
                Text = "Hoş Geldiniz",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(1100, 30)
            };

            topPanel.Controls.AddRange(new Control[] { titleLabel, _welcomeLabel });

            // Left Panel (Menu)
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            int menuY = 20;
            var menuButtons = new[]
            {
                CreateMenuButton("📊 Ana Sayfa", menuY, (s, e) => LoadDashboard()),
                CreateMenuButton("📖 Kitap Yönetimi", menuY += 60, (s, e) => LoadBookManagement()),
                CreateMenuButton("👥 Üye Yönetimi", menuY += 60, (s, e) => LoadMemberManagement()),
                CreateMenuButton("📤 Ödünç İşlemleri", menuY += 60, (s, e) => LoadBorrowManagement()),
                CreateMenuButton("🔍 Kitap Ara", menuY += 60, (s, e) => LoadBookSearch()),
                CreateMenuButton("📈 Raporlar", menuY += 60, (s, e) => LoadReports()),
                CreateMenuButton("⚙️ Ayarlar", menuY += 60, (s, e) => LoadSettings()),
                CreateMenuButton("🚪 Çıkış", menuY += 80, (s, e) => Logout())
            };

            leftPanel.Controls.AddRange(menuButtons);

            // Content Panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(20)
            };

            // Status Strip
            var statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(52, 73, 94)
            };

            var statusLabel = new ToolStripStatusLabel
            {
                Text = $"Bağlantı: Aktif | Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}",
                ForeColor = Color.White
            };

            statusStrip.Items.Add(statusLabel);

            // Add all panels
            this.Controls.AddRange(new Control[] { _contentPanel, leftPanel, topPanel, statusStrip });
        }

        private Button CreateMenuButton(string text, int y, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(230, 50),
                Location = new Point(10, y),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            button.Click += clickHandler;

            return button;
        }

        private void UpdateWelcomeMessage()
        {
            if (_currentUser != null)
            {
                _welcomeLabel.Text = $"Hoş Geldiniz, {_currentUser.FullName ?? _currentUser.Username}";
            }
        }

        private void LoadDashboard()
        {
            _contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = "📊 KONTROL PANELİ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            _contentPanel.Controls.Add(titleLabel);

            try
            {
                var stats = _libraryService.GetDashboardStatistics();

                int cardX = 20;
                int cardY = 80;

                CreateStatCard("Toplam Kitap", stats.GetValueOrDefault("TotalBooks", 0).ToString(), 
                    Color.FromArgb(52, 152, 219), cardX, cardY);
                CreateStatCard("Mevcut Kitap", stats.GetValueOrDefault("AvailableBooks", 0).ToString(), 
                    Color.FromArgb(46, 204, 113), cardX + 280, cardY);
                CreateStatCard("Ödünç Kitap", stats.GetValueOrDefault("BorrowedBooks", 0).ToString(), 
                    Color.FromArgb(241, 196, 15), cardX + 560, cardY);
                CreateStatCard("Geciken Kitap", stats.GetValueOrDefault("OverdueBooks", 0).ToString(), 
                    Color.FromArgb(231, 76, 60), cardX + 840, cardY);

                CreateStatCard("Toplam Üye", stats.GetValueOrDefault("TotalMembers", 0).ToString(), 
                    Color.FromArgb(155, 89, 182), cardX, cardY + 180);
                CreateStatCard("Aktif Üye", stats.GetValueOrDefault("ActiveMembers", 0).ToString(), 
                    Color.FromArgb(26, 188, 156), cardX + 280, cardY + 180);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading dashboard", ex);
                MessageBox.Show("İstatistikler yüklenirken hata oluştu.", "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateStatCard(string title, string value, Color color, int x, int y)
        {
            var panel = new Panel
            {
                Size = new Size(250, 150),
                Location = new Point(x, y),
                BackColor = color
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(230, 30),
                Location = new Point(10, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(230, 80),
                Location = new Point(10, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.AddRange(new Control[] { titleLabel, valueLabel });
            _contentPanel.Controls.Add(panel);
        }

        private void LoadBookManagement()
        {
            _contentPanel.Controls.Clear();
            var bookForm = new BookManagementControl(_libraryService);
            bookForm.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(bookForm);
        }

        private void LoadMemberManagement()
        {
            _contentPanel.Controls.Clear();
            var memberForm = new MemberManagementControl(_libraryService);
            memberForm.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(memberForm);
        }

        private void LoadBorrowManagement()
        {
            _contentPanel.Controls.Clear();
            var borrowForm = new BorrowManagementControl(_libraryService);
            borrowForm.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(borrowForm);
        }

        private void LoadBookSearch()
        {
            _contentPanel.Controls.Clear();
            var searchControl = new BookSearchControl(_libraryService);
            searchControl.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(searchControl);
        }

        private void LoadReports()
        {
            MessageBox.Show("Raporlar modülü yakında eklenecek!", "Bilgi", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSettings()
        {
            MessageBox.Show("Ayarlar modülü yakında eklenecek!", "Bilgi", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Logout()
        {
            var result = MessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", "Çıkış",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
