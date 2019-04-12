using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using QRCoder;

namespace qrcode
{
    public partial class MainWindow : Window
    {
        private Bitmap bimg = null; //保存生成的二维码，方便后面保存
        private string logoImagepath = string.Empty; //存储Logo的路径
        private string number;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            DateTime dt = DateTime.Now;
            txtYear.Text = dt.ToString("yy");
            txtWeek.Text = getWeek().ToString();
        }

        public int getWeek()
        {
            var dt = DateTime.Now;
            int firstWeekend = Convert.ToInt32(DateTime.Parse(dt.Year + "-1-1").DayOfWeek);
            int weekDay = firstWeekend == 0 ? 1 : (7 - firstWeekend + 1);
            int currentDay = dt.DayOfYear;
            int current_week = Convert.ToInt32(Math.Ceiling((currentDay - weekDay) / 7.0)) + 1;
            return current_week;
        }

        public Bitmap CreateQRCode(string content)
        {
            int selectedServer = server.SelectedIndex;
            string url = "https://www.mx-ai.com/?number=";
            System.Drawing.Brush brush = System.Drawing.Brushes.Black;
            if (selectedServer == 1)
            {
                url = "https://mx.test.mx-ai.com/?number=";
                brush = System.Drawing.Brushes.Red;
            }
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url + content, QRCodeGenerator.ECCLevel.H);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap bitmapLogo = null;
                if (!logoImagepath.Equals(string.Empty))
                {
                    bitmapLogo = new Bitmap(logoImagepath);
                }
                int logoSize = Convert.ToInt32(txtLogoSize.Text);
                if (logoSize > 30) logoSize = 30;
                if (logoSize < 10) logoSize = 15;

                Bitmap qrcode = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, bitmapLogo, logoSize, 6, false);
                Bitmap paper = new Bitmap(qrcode.Width, qrcode.Height + 80);
                Graphics g = Graphics.FromImage(paper);
                g.DrawImage(qrcode, new PointF(0, 0));
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                String txt = "设备ID：" + number;
                Font font = new Font("Microsoft Yahei", 40);
                g.DrawString(txt, font, brush, new PointF(qrcode.Width / 2, qrcode.Height), format);
                return paper;
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("超出当前二维码版本的容量上限，请选择更高的二维码版本！", "系统提示");
                return new Bitmap(100, 100);
            }
            catch (Exception)
            {
                MessageBox.Show("生成二维码出错！", "系统提示");
                return new Bitmap(100, 100);
            }
        }

        public bool checkParam()
        {
            if (txtYear.Text.Trim().Length <= 0)
            {
                MessageBox.Show("生产年份不能为空！", "系统提示");
                txtYear.Focus();
                return false;
            }
            if (txtWeek.Text.Trim().Length <= 0)
            {
                MessageBox.Show("制造周数不能为空！", "系统提示");
                txtWeek.Focus();
                return false;
            }
            if (txtTezheng.Text.Trim().Length <= 0)
            {
                MessageBox.Show("特征码不能为空！", "系统提示");
                txtTezheng.Focus();
                return false;
            }
            if (txtIssue.Text.Trim().Length <= 0)
            {
                MessageBox.Show("生产批号不能为空！", "系统提示");
                txtIssue.Focus();
                return false;
            }
            if (txtBegin.Text.Trim().Length <= 0)
            {
                MessageBox.Show("开始编号不能为空！", "系统提示");
                txtBegin.Focus();
                return false;
            }
            if (txtEnd.Text.Trim().Length <= 0)
            {
                MessageBox.Show("结束编号不能为空！", "系统提示");
                txtEnd.Focus();
                return false;
            }
            return true;
        }

        public void ShowQRCode()
        {
            if (!checkParam())
            {
                return;
            }
            string num = txtBegin.Text;
            num = num.PadLeft(3, '0');
            string issue = txtIssue.Text;
            issue = issue.PadLeft(2, '0');
            string week = txtWeek.Text;
            week = week.PadLeft(2, '0');
            number = txtYear.Text + week + issue + num + txtTezheng.Text;
            bimg = CreateQRCode(number);
            imgQRcode.Source = BitmapToBitmapImage(bimg);
            ResetImageStrethch(imgQRcode, bimg);
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bImage = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            bImage.BeginInit();
            bImage.StreamSource = new MemoryStream(ms.ToArray());
            bImage.EndInit();
            return bImage;
        }

        private void ResetImageStrethch(System.Windows.Controls.Image img, Bitmap bImg)
        {
            if (bImg.Width <= img.Width)
            {
                img.Stretch = Stretch.None;
            }
            else
            {
                img.Stretch = Stretch.Fill;
            }
        }

        public void SaveQRCode(string path)
        {
            if (!checkParam())
            {
                return;
            }
            string issue = txtIssue.Text;
            issue = issue.PadLeft(2, '0');
            string week = txtWeek.Text;
            week = week.PadLeft(2, '0');
            string tezheng = txtTezheng.Text;
            string year = txtYear.Text;

            int begin = Convert.ToInt32(txtBegin.Text);
            int end = Convert.ToInt32(txtEnd.Text);

            for (int i = begin; i <= end; i++)
            {
                string num = i.ToString().PadLeft(3, '0');
                number = year + week + issue + num + tezheng;
                Bitmap img = CreateQRCode(number);
                if (img != null)
                {
                    img.Save(path + "\\" + number + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    img.Dispose();
                }
            }

        }

        private void btnCreateQRCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowQRCode();
            }
            catch (Exception)
            {
                MessageBox.Show("生成二维码出错！", "系统提示");
                return;
            }
        }

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog path = new System.Windows.Forms.FolderBrowserDialog();
            path.RootFolder = Environment.SpecialFolder.Desktop;

            if (path.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = path.SelectedPath;
                try
                {
                    SaveQRCode(filePath);
                    MessageBox.Show("保存成功！", "系统提示");
                }
                catch (Exception)
                {
                    MessageBox.Show("保存二维码出错！", "系统提示");
                    return;
                }
            }
        }

        private void btnAddLogoClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "图片文件|*.jpg;*.png;*.gif|All files(*.*)|*.*";
            if (openDialog.ShowDialog() == true)
            {
                logoImagepath = openDialog.FileName;
                Bitmap bImg = new Bitmap(logoImagepath);
                imgLogo.Source = new BitmapImage(new Uri(openDialog.FileName));
                ResetImageStrethch(imgLogo, bImg);
            }
        }

    }
}
