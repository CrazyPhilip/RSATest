using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RSATest
{
    public class MainViewModel : BaseViewModel
    {
        private int randomCount;   //comment

        public int RandomCount
        {
            get { return randomCount; }
            set { SetProperty(ref randomCount, value); }
        }

        private string randomNumber1;   //comment

        public string RandomNumber1
        {
            get { return randomNumber1; }
            set { SetProperty(ref randomNumber1, value); }
        }

        private string randomNumber2;   //comment

        public string RandomNumber2
        {
            get { return randomNumber2; }
            set { SetProperty(ref randomNumber2, value); }
        }

        private string primeNumP;   //comment

        public string PrimeNumP
        {
            get { return primeNumP; }
            set { SetProperty(ref primeNumP, value); }
        }

        private string primeNumQ;   //comment

        public string PrimeNumQ
        {
            get { return primeNumQ; }
            set { SetProperty(ref primeNumQ, value); }
        }

        private string numberN;   //comment

        public string NumberN
        {
            get { return numberN; }
            set { SetProperty(ref numberN, value); }
        }

        private string numberPHI;   //comment

        public string NumberPHI
        {
            get { return numberPHI; }
            set { SetProperty(ref numberPHI, value); }
        }

        private string numberE;   //comment

        public string NumberE
        {
            get { return numberE; }
            set { SetProperty(ref numberE, value); }
        }

        private string numberD;   //comment

        public string NumberD
        {
            get { return numberD; }
            set { SetProperty(ref numberD, value); }
        }

        private string clearText;   //comment

        public string ClearText
        {
            get { return clearText; }
            set { SetProperty(ref clearText, value); }
        }

        private string cipherText;   //comment

        public string CipherText
        {
            get { return cipherText; }
            set { SetProperty(ref cipherText, value); }
        }

        public MainViewModel()
        {
            RandomCount = 100;
        }
    }

    public class BaseViewModel : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyName);

            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;

            if (changed == null)
                return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
