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
        private BigInteger[] _cipheredText;

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

            BigInteger _e = GetE(mainViewModel.NumberPHI);
            //BigInteger d = (1 / _e) % BigInteger.Parse(mainViewModel.NumberPHI);
            BigInteger d = BigInteger.Zero;
            BigInteger y = BigInteger.Zero;

            exgcd(_e, BigInteger.Parse(mainViewModel.NumberPHI), out d, out y);

            _e = _e < 0 ? _e + BigInteger.Parse(mainViewModel.NumberPHI) : _e;
            d = d < 0 ? d + BigInteger.Parse(mainViewModel.NumberPHI) : d;

            mainViewModel.NumberE = _e.ToString();
            mainViewModel.NumberD = d.ToString();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            
        }

        /// <summary>
        /// 加密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            //string n = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberN));
            //string ee = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberE));
            //
            //string xml = "<RSAKeyValue><Modulus>" + n + "</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            //
            //mainViewModel.CipherText = RSAEncrypt(mainViewModel.ClearText, xml);
            mainViewModel.CipherText = Encrypt(mainViewModel.ClearText);
        }

        /// <summary>
        /// 解密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            byte[] bytes = Encoding.Default.GetBytes(mainViewModel.NumberD);
            string baseKey = Convert.ToBase64String(bytes);

            string p = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.PrimeNumP));
            string q = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.PrimeNumQ));
            string n = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberN));
            string phi = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberPHI));
            string ee = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberE));
            string d = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberD));
            BigInteger _dp = BigInteger.Parse(mainViewModel.NumberD) % (BigInteger.Parse(mainViewModel.PrimeNumP) - 1);
            BigInteger _dq = BigInteger.Parse(mainViewModel.NumberD) % (BigInteger.Parse(mainViewModel.PrimeNumQ) - 1);
            string dp = Convert.ToBase64String(Encoding.Default.GetBytes(_dp.ToString()));
            string dq = Convert.ToBase64String(Encoding.Default.GetBytes(_dq.ToString()));

            string xml = "<RSAKeyValue><Modulus>" + n + "</Modulus><Exponent>AQAB</Exponent><P>" +
                p + "</P><Q>" + q + "</Q><DP>" + dp + "</DP><DQ>" + dq + "</DQ><InverseQ>" + phi + "</InverseQ><D>" + d + "</D></RSAKeyValue>";

            mainViewModel.ClearText = RSADecrypt(mainViewModel.CipherText, xml);
            */
            mainViewModel.ClearText = Decrypt(mainViewModel.ClearText);
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
        /// 生成e
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private BigInteger GetE(string p)
        {
            BigInteger phi = BigInteger.Parse(p);

            BigInteger e = phi;
            BigInteger d = phi;

            while (e > 2)
            {
                e -= 1;
                if (gcd(phi, e) == 1)
                {
                    break;
                }
            }

            return e;        
        }

        private BigInteger GetD(string e, string phi)
        {
            return 0;
        }

        /// <summary>
        /// 欧几里得算法，非递归
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private BigInteger gcd(BigInteger a, BigInteger b)
        {
            while (b !=0 )
            {
                BigInteger t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        /// <summary>
        /// 扩展欧几里得算法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public BigInteger exgcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            if (b == 0)
            {//推理1，终止条件
                x = 1;
                y = 0;
                return a;
            }

            BigInteger r = exgcd(b, a % b, out x, out y);

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

                rsa.FromXmlString(publicKey);
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

                rsa.FromXmlString(privateKey);
                cipherbytes = rsa.Decrypt(Convert.FromBase64String(s), false);
                return Encoding.UTF8.GetString(cipherbytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        #region
        private byte[] StringToByteArray(string data)
        {
            var length = data.Length;
            var results = new byte[length];
            char temp;
            byte t;

            for (int i = 0; i < length; ++i)
            {
                temp = data[i];
                t = (byte)temp;

                // If current character is numeric digit.
                if (Char.IsDigit(temp)) { t = (byte)(t - 48); }

                // If current character is blank space.
                else if (temp == ' ') { t = (byte)199; }

                // If current character is new line.
                else if (temp == '\n') { t = (byte)200; }

                // If current character is carriage return.
                else if (t == 13) { t = (byte)201; }

                else { t = (byte)(t - 55); }

                results[i] = t;
            }

            return results;
        }

        private string ByteArrayToString(byte[] data)
        {
            int length = data.Length;
            var stringBuilder = new StringBuilder();
            byte temp;

            for (int i = 0; i < length; ++i)
            {
                temp = data[i];

                // If current byte represents numeric digit.
                if (temp >= 0 && temp <= 9) { temp = (byte)(temp + 48); }

                // If current byte represents white space.
                else if (temp == 199) { temp = (byte)32; }

                // If current byte represents new line.
                else if (temp == 200) { temp = (byte)10; }

                // If current byte represents carriage return.
                else if (temp == 201) { temp = (byte)13; }

                else { temp = (byte)(temp + 55); }

                stringBuilder.Append((char)temp);
            }

            return stringBuilder.ToString();
        }

        private BigInteger[] CipheredText(byte[] data, BigInteger e, BigInteger n)
        {
            int length = data.Length;
            BigInteger temp;
            var results = new BigInteger[length];

            for (int i = 0; i < length; ++i)
            {
                temp = data[i];

                // If whitespace, newline or carriage return.
                if (temp >= 199 && temp <= 201)
                {
                    results[i] = temp;
                    continue;
                }


                temp = quick(temp, e, n);
                results[i] = temp;
            }

            return results;
        }

        // Convert ciphered text into plain text.
        private byte[] DecipheredText(BigInteger[] data, BigInteger d, BigInteger n)
        {
            int length = data.Length;
            BigInteger temp = 0;
            var results = new byte[length];
            BigInteger bigInt;
            string stringRepresentation;

            for (int i = 0; i < data.Length; ++i)
            {
                temp = data[i];

                // If whitespace, newline or carriage return.
                if (temp >= 199 && temp <= 201)
                {
                    results[i] = (byte)temp;
                    continue;
                }

                bigInt = temp;
                bigInt = BigInteger.ModPow(bigInt, d, n);
                stringRepresentation = bigInt.ToString();
                temp = Int32.Parse(stringRepresentation);
                results[i] = (byte)temp;
            }

            return results;
        }

        BigInteger quick(BigInteger a, BigInteger b, BigInteger c)
        {
            BigInteger A = 1;   //结果的保存，就是An，初始化一下        
            BigInteger T = a % c;     //首先计算T0的值，用于Tn的递推          
            while (b != 0)
            {
                //这个if是判断目前最右边的一位bn是不是1，如果是1，那么Kn=Tn直接用Tn递推，具体看上面原理，如果bn=0,那么Kn=1,考虑到An-1是小于c的，所以 An=（An-1）%c =An-1 就是说可以不用计算了 因为相当于直接 A=A        
                if ((b & 1) == 1)
                {
                    A = (A * T) % c;
                }
                b >>= 1;       //二进制位移，相当于从右到左读取位b0 b1 b2 b3 b4等等              
                T = (T * T) % c;   //更新T，如果下一位是1就可以用这个算A，具体的可以看上面原理的递推关系       
            }
            return A;
        }

        public string Encrypt(string message)
        {
            BigInteger _e = BigInteger.Parse(mainViewModel.NumberE);
            BigInteger _n = BigInteger.Parse(mainViewModel.NumberN);
            byte[] arr = StringToByteArray(message);
            _cipheredText = CipheredText(arr, _e, _n);
            var stringBuilder = new StringBuilder();
            int length = _cipheredText.Length;

            for (int j = 0; j < length; ++j)
            {
                stringBuilder.Append(_cipheredText[j]);
            }

            return stringBuilder.ToString();
        }

        public string Decrypt(string message)
        {
            BigInteger _d = BigInteger.Parse(mainViewModel.NumberD);
            BigInteger _n = BigInteger.Parse(mainViewModel.NumberN);

            char[] temp = message.ToCharArray();
            byte[] plainText = DecipheredText(_cipheredText, _d, _n);
            string decipheredText = ByteArrayToString(plainText);
            return decipheredText;
        }

        #endregion
        /*
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
        private BigInteger[] _cipheredText;
        private BigInteger random1, random2, _p, _q, _phi, _n, _e, _d;

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

                random1 = BigInteger.Parse(number1);
                random2 = BigInteger.Parse(number2);
            }
        }

        /// <summary>
        /// 生成密钥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProducePrivateKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            _p = ProducePrimeNumber(random1);
            _q = ProducePrimeNumber(random2);

            _n = _p * _q;       //求n

            _phi = (_p - 1) * (_q - 1);     //求phi

            _e = GetE();        //求e

            _d = GetD(_phi, _e);        //求d

            mainViewModel.PrimeNumP = _p.ToString();
            mainViewModel.PrimeNumQ = _q.ToString();
            mainViewModel.NumberN = _n.ToString();
            mainViewModel.NumberPHI = _phi.ToString();
            mainViewModel.NumberE = _e.ToString();
            mainViewModel.NumberD = _d.ToString();
        }

        /// <summary>
        /// 加密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.CipherText = Encrypt(mainViewModel.ClearText);
        }

        /// <summary>
        /// 解密按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            
            //byte[] bytes = Encoding.Default.GetBytes(mainViewModel.NumberD);
            //string baseKey = Convert.ToBase64String(bytes);
            //
            //string p = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.PrimeNumP));
            //string q = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.PrimeNumQ));
            //string n = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberN));
            //string phi = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberPHI));
            //string ee = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberE));
            //string d = Convert.ToBase64String(Encoding.Default.GetBytes(mainViewModel.NumberD));
            //BigInteger _dp = BigInteger.Parse(mainViewModel.NumberD) % (BigInteger.Parse(mainViewModel.PrimeNumP) - 1);
            //BigInteger _dq = BigInteger.Parse(mainViewModel.NumberD) % (BigInteger.Parse(mainViewModel.PrimeNumQ) - 1);
            //string dp = Convert.ToBase64String(Encoding.Default.GetBytes(_dp.ToString()));
            //string dq = Convert.ToBase64String(Encoding.Default.GetBytes(_dq.ToString()));
            //
            //string xml = "<RSAKeyValue><Modulus>" + n + "</Modulus><Exponent>"+ee+"</Exponent><P>" +
            //    p + "</P><Q>" + q + "</Q><DP>" + dp + "</DP><DQ>" + dq + "</DQ><InverseQ>" + phi + "</InverseQ><D>" + d + "</D></RSAKeyValue>";
            //    
            //mainViewModel.ClearText = RSADecrypt(mainViewModel.CipherText, xml);
            

        mainViewModel.ClearText = Decrypt(mainViewModel.CipherText);
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
    private BigInteger ProducePrimeNumber(BigInteger number)
    {
        while (!IsProbablePrime(number))
        {
            number += 1;
        }

        return number;
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
    /// 生成e
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private BigInteger GetE()
    {
        BigInteger result = -1;

        for (BigInteger i = 2; i < _phi; ++i)
        {
            if (IsCoPrime(i, _phi))
            {
                result = i;
                break;
            }
        }

        return result;
    }

    private bool IsCoPrime(BigInteger number1, BigInteger number2)
    {
        return (GCD(number1, number2) == 1);
    }

    private BigInteger GCD(BigInteger number1, BigInteger number2)
    {
        if (number1 == 0)
        {
            return number2;
        }

        return GCD(number2 % number1, number1);
    }

    private BigInteger GetD(BigInteger phi, BigInteger e)
    {
        BigInteger result = 1;

        while ((e * result - 1) % phi != 0)
        {
            result++;
        }

        //_d = result;
        return result;
    }

    /// <summary>
    /// 欧几里得算法，非递归
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private BigInteger gcd(BigInteger a, BigInteger b)
    {
        while (b != 0)
        {
            BigInteger t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    /// <summary>
    /// 扩展欧几里得算法
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BigInteger exgcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
    {
        if (b == 0)
        {//推理1，终止条件
            x = 1;
            y = 0;
            return a;
        }

        BigInteger r = exgcd(b, a % b, out x, out y);

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

            rsa.FromXmlString(publicKey);
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

            rsa.FromXmlString(privateKey);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(s), false);
            return Encoding.UTF8.GetString(cipherbytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "";
        }
    }

    private byte[] StringToByteArray(string data)
    {
        var length = data.Length;
        var results = new byte[length];
        char temp;
        byte t;

        for (int i = 0; i < length; ++i)
        {
            temp = data[i];
            t = (byte)temp;

            // If current character is numeric digit.
            if (Char.IsDigit(temp)) { t = (byte)(t - 48); }

            // If current character is blank space.
            else if (temp == ' ') { t = (byte)199; }

            // If current character is new line.
            else if (temp == '\n') { t = (byte)200; }

            // If current character is carriage return.
            else if (t == 13) { t = (byte)201; }

            else { t = (byte)(t - 55); }

            results[i] = t;
        }

        return results;
    }

    private string ByteArrayToString(byte[] data)
    {
        int length = data.Length;
        var stringBuilder = new StringBuilder();
        byte temp;

        for (int i = 0; i < length; ++i)
        {
            temp = data[i];

            // If current byte represents numeric digit.
            if (temp >= 0 && temp <= 9) { temp = (byte)(temp + 48); }

            // If current byte represents white space.
            else if (temp == 199) { temp = (byte)32; }

            // If current byte represents new line.
            else if (temp == 200) { temp = (byte)10; }

            // If current byte represents carriage return.
            else if (temp == 201) { temp = (byte)13; }

            else { temp = (byte)(temp + 55); }

            stringBuilder.Append((char)temp);
        }

        return stringBuilder.ToString();
    }

    private BigInteger[] CipheredText(byte[] data, BigInteger e, BigInteger n)
    {
        int length = data.Length;
        BigInteger temp;
        var results = new BigInteger[length];

        for (int i = 0; i < length; ++i)
        {
            temp = data[i];

            // If whitespace, newline or carriage return.
            if (temp >= 199 && temp <= 201)
            {
                results[i] = temp;
                continue;
            }


            temp = quick(temp, e, n);
            results[i] = temp;
        }

        return results;
    }

    // Convert ciphered text into plain text.
    private byte[] DecipheredText(BigInteger[] data, BigInteger d, BigInteger n)
    {
        int length = data.Length;
        BigInteger temp = 0;
        var results = new byte[length];
        BigInteger bigInt;
        string stringRepresentation;

        for (int i = 0; i < data.Length; ++i)
        {
            temp = data[i];

            // If whitespace, newline or carriage return.
            if (temp >= 199 && temp <= 201)
            {
                results[i] = (byte)temp;
                continue;
            }

            bigInt = temp;
            bigInt = BigInteger.ModPow(bigInt, d, n);
            stringRepresentation = bigInt.ToString();
            temp = Int32.Parse(stringRepresentation);
            results[i] = (byte)temp;
        }

        return results;
    }

    BigInteger quick(BigInteger a, BigInteger b, BigInteger c)
    {
        BigInteger A = 1;   //结果的保存，就是An，初始化一下        
        BigInteger T = a % c;     //首先计算T0的值，用于Tn的递推          
        while (b != 0)
        {
            //这个if是判断目前最右边的一位bn是不是1，如果是1，那么Kn=Tn直接用Tn递推，具体看上面原理，如果bn=0,那么Kn=1,考虑到An-1是小于c的，所以 An=（An-1）%c =An-1 就是说可以不用计算了 因为相当于直接 A=A        
            if ((b & 1) == 1)
            {
                A = (A * T) % c;
            }
            b >>= 1;       //二进制位移，相当于从右到左读取位b0 b1 b2 b3 b4等等              
            T = (T * T) % c;   //更新T，如果下一位是1就可以用这个算A，具体的可以看上面原理的递推关系       
        }
        return A;
    }

    public string Encrypt(string message)
    {
        byte[] arr = StringToByteArray(message);
        _cipheredText = CipheredText(arr, _e, _n);
        var stringBuilder = new StringBuilder();
        int length = _cipheredText.Length;

        for (int j = 0; j < length; ++j)
        {
            stringBuilder.Append(_cipheredText[j]);
        }

        return stringBuilder.ToString();
    }

    public string Decrypt(string message)
    {
        char[] temp = message.ToCharArray();
        byte[] plainText = DecipheredText(_cipheredText, _d, _n);
        string decipheredText = ByteArrayToString(plainText);
        return decipheredText;
    }

}
}

         */
    }
}
