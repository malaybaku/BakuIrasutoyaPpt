using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace Baku.IrasutoyaPpt
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected void SetValue<T>(ref T target, T value, [CallerMemberName]string pname="")
            where T : IEquatable<T>
        {
            if (!target.Equals(value))
            {
                target = value;
                RaisePropertyChanged(pname);
            }
        }

        protected void RaisePropertyChanged([CallerMemberName]string pname = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pname));

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ActionCommand : ICommand
    {
        public ActionCommand(Action act)
        {
            _act = act;
        }
        private readonly Action _act;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _act();
    }

    internal static class DispatcherHolder
    {
        /// <summary>
        /// ペーンのDispatcherを登録して使う
        /// </summary>
        internal static Dispatcher UIDispatcher { get; set; }
    }

}
