using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RSATest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = mainViewModel;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewRandomCountBtn_Click(object sender, RoutedEventArgs e)
        {
            string number1 = "";
            string number2 = "";
            Random random = new Random();

            if (mainViewModel.RandomCount > 0)
            {
                for (int i = 0; i < mainViewModel.RandomCount; i++)
                {
                    number1 += random.Next(0, 10);
                    number2 += random.Next(0, 10);
                }

                mainViewModel.RandomNumber1 = number1;
                mainViewModel.RandomNumber2 = number2;
            }
        }

        /// <summary>
        /// 生成密钥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProducePrivateKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.PrimeNumP = ProducePrimeNumber(mainViewModel.RandomNumber1);
            mainViewModel.PrimeNumQ = ProducePrimeNumber(mainViewModel.RandomNumber2);

            BigInteger p = BigInteger.Parse(mainViewModel.PrimeNumP);
            BigInteger q = BigInteger.Parse(mainViewModel.PrimeNumQ);

            mainViewModel.NumberN = GetN(p, q);
            mainViewModel.NumberPHI = GetPHI(p, q);

            BigInteger _e = BigInteger.Zero;
            BigInteger d = BigInteger.Zero;

            exgcd(p, q, ref _e, ref d);

            mainViewModel.NumberE = _e.ToString();
            mainViewModel.NumberD = d.ToString();
        }

        /// <summary>
        /// 加密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.CipherText = RSAEncrypt(mainViewModel.ClearText, mainViewModel.NumberE);
        }

        /// <summary>
        /// 解密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.ClearText = RSADecrypt(mainViewModel.CipherText, mainViewModel.NumberD);
        }

        /// <summary>
        /// 清空明文、密文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton2_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.ClearText = "";
            mainViewModel.CipherText = "";
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.PrimeNumP = "";
            mainViewModel.PrimeNumQ = "";
            mainViewModel.NumberN = "";
            mainViewModel.NumberPHI = "";
            mainViewModel.NumberE = "";
            mainViewModel.NumberD = "";
        }

        /// <summary>
        /// 生成素数
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string ProducePrimeNumber(string number)
        {
            string primeNumber = "";
            BigInteger bigInteger = BigInteger.Parse(number);

            while (!IsProbablePrime(bigInteger))
            {
                bigInteger += 1;
            }

            primeNumber = bigInteger.ToString();
            return primeNumber;
        }

        /// <summary>
        /// rabin-miller素性检验算法
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool IsProbablePrime(BigInteger source)
        {
            int certainty = 2;
            if (source == 2 || source == 3)
                return true;
            if (source < 2 || source % 2 == 0)
                return false;

            BigInteger d = source - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[source.ToByteArray().LongLength];
            BigInteger a;

            for (int i = 0; i < certainty; i++)
            {
                do
                {
                    rng.GetBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= source - 2);

                BigInteger x = BigInteger.ModPow(a, d, source);
                if (x == 1 || x == source - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, source);
                    if (x == 1)
                        return false;

                    if (x == source - 1)
                        break;
                }

                if (x != source - 1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 求N
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetN(string p, string q)
        {
            try
            {
                BigInteger num1 = BigInteger.Parse(p);
                BigInteger num2 = BigInteger.Parse(q);

                return (num1 * num2).ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 求N
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetN(BigInteger p, BigInteger q)
        {
            try
            {
                return (p * q).ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 求PHI
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetPHI(string p, string q)
        {
            try
            {
                BigInteger num1 = BigInteger.Parse(p) - 1;
                BigInteger num2 = BigInteger.Parse(q) - 1;

                return (num1 * num2).ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 求PHI
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetPHI(BigInteger p, BigInteger q)
        {
            try
            {
                return ((p - 1) * (q - 1)).ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 扩展欧几里得算法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public BigInteger exgcd(BigInteger a, BigInteger b, ref BigInteger x, ref BigInteger y)
        {
            if (b == 0)
            {//推理1，终止条件
                x = 1;
                y = 0;
                return a;
            }

            BigInteger r = exgcd(b, a % b, ref x, ref y);

            //先得到更底层的x2,y2,再根据计算好的x2,y2计算x1，y1。
            //推理2，递推关系

            BigInteger t = y;
            y = x - (a / b) * y;
            x = t;
            return r;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public string RSAEncrypt(string content, string publicKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                byte[] cipherbytes;

                byte[] bytes = Encoding.Default.GetBytes(publicKey);
                string baseKey = Convert.ToBase64String(bytes);
                string xml = "<RSAKeyValue><Modulus>" + baseKey + "</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

                rsa.FromXmlString(xml);
                cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);
                return Convert.ToBase64String(cipherbytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="s"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public string RSADecrypt(string s, string privateKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                byte[] cipherbytes;

                byte[] bytes = Encoding.Default.GetBytes(privateKey);
                string baseKey = Convert.ToBase64String(bytes);
                string xml = "<RSAKeyValue><Modulus>" + baseKey + "</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

                rsa.FromXmlString(xml);
                cipherbytes = rsa.Decrypt(Convert.FromBase64String(s), false);
                return Encoding.UTF8.GetString(cipherbytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

    }
}
