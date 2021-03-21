using QRCoder;
using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Passmgr {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public MainWindow() {
            InitializeComponent();

            Loaded += (s, e) => { generate_Click(s, e); };
        }

        private string Generate(int lenght,
            bool mayuscule = true, bool minuscule = true, bool digits = true,
            bool special = true, string specials = "~!@#$%^&*+-/.,\\{}[]();:|?<>=\"`") {
            string randomstr = "";
            int minValue = 0x21;
            int maxValue = 0x7e;
            int opc = 0;
            opc = mayuscule ? opc + 1 : opc;
            opc = minuscule ? opc + 1 : opc;
            opc = digits ? opc + 1 : opc;
            opc = special ? opc + 1 : opc;
            if (opc == 0) throw new ArgumentException();
            int[] p = { 0, 0, 0, 0 };
            char _char;
            using (var rng = new RNGCryptoServiceProvider()) {
                byte[] barray = new byte[1];
                while (randomstr.Length < lenght) {
                    rng.GetNonZeroBytes(barray);
                    _char = (char)(barray[0] % maxValue + minValue);

                    bool isUpper = char.IsUpper(_char);
                    bool isLower = char.IsLower(_char);
                    bool isDigit = char.IsNumber(_char);
                    bool isChars = specials.Contains(_char);
                    if ((isUpper && !mayuscule) ||
                     (isLower && !minuscule) ||
                     (isDigit && !digits) ||
                     (isChars && !special)) continue;
                    int indx = isUpper ? 0 : isLower ? 1 : isDigit ? 2 : isChars ? 3 : -1;
                    if (indx < 0) continue;
                    if (opc == lenght - randomstr.Length && p[indx] > 0) continue;
                    p[indx]++;

                    if (p[indx] == 1) opc--;

                    randomstr += _char;
                }
            }
            return randomstr;
        }

        private void generate_Click(object sender, RoutedEventArgs e) {
            //gen_btn.IsEnabled = false;
            copy_btn.IsEnabled = false;
            double rand = new Random(DateTime.Now.Millisecond * DateTime.Now.Second).NextDouble();
            DispatcherTimer time = new DispatcherTimer();
            time.Interval = TimeSpan.FromMilliseconds(30);
            time.Start();
            var startedTime = DateTime.Now;
            time.Tick += (s, e1) => {
                var password = text.Text = Generate((int)lenghtBox.Value,
                    (bool)mayuscule.IsChecked,
                    (bool)minuscule.IsChecked,
                    (bool)digits.IsChecked,
                    (bool)special.IsChecked,
                    specials.Text);
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(password, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                qrImage.Source = BitmapConverter(qrCodeImage);
                if (DateTime.Now - startedTime >= TimeSpan.FromSeconds(rand)) {
                    time.Stop();
                    //gen_btn.IsEnabled = true;
                    copy_btn.IsEnabled = true;
                }
            };
        }

        private void copy_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(text.Text);
        }

        private BitmapSource BitmapConverter(Bitmap source) {
            try {
                IntPtr hBitmap = source.GetHbitmap();
                try {
                    return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
                } finally {
                    DeleteObject(hBitmap);
                }
            } catch {
                return new BitmapImage();
            }

        }
    }
}
