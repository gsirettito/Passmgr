using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace SiretT.Controls {
    [DefaultEvent("ValueChanged")]
    [Localizability(LocalizationCategory.Text)]
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(FrameworkElement))]
    public class NumericBox : TextBox {
        private int caret;

        private List<Key> keys = new List<Key>() {
            Key.C, Key.X, Key.V, Key.Tab,
            Key.Home, Key.End, Key.Enter, Key.Return,
            Key.Delete, Key.Decimal, Key.Left,
            Key.Up, Key.Down,
            Key.Right, Key.NumPad0, Key.NumPad1,
            Key.NumPad2, Key.NumPad3, Key.NumPad4,
            Key.NumPad5, Key.NumPad6, Key.NumPad7,
            Key.NumPad8, Key.NumPad9, Key.OemMinus,
            Key.D0, Key.D1, Key.D2, Key.D3, Key.D4,
            Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
            Key.Back, Key.Subtract, Key.OemPeriod };

        public NumericBox()
            : base() {
            Text = "0";
            this.AddCharacterCommand = new DelegateCommand(ExecuteAddCharacter);
            this.DeleteCharacterCommand = new DelegateCommand(ExecuteDeleteCharacter);
        }

        public class ValueEventArgs : EventArgs {
            public ValueEventArgs(object value) {
                this.Value = value;
            }

            public object Value { get; set; }
        }

        public event EventHandler<EventArgs> ValueChanged;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(NumericBox), new PropertyMetadata(0d, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(NumericBox), new PropertyMetadata(0d, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(double), typeof(NumericBox), new PropertyMetadata(0d, OnValueChanged));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(NumericBox), new PropertyMetadata(null, OnValueChanged));

        static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var self = (obj as NumericBox);
            if (e.Property == ValueProperty) {
                self.Text = e.NewValue.ToString();
                if (self.ValueChanged != null)
                    self.ValueChanged(self, new ValueEventArgs(e.NewValue));
            }
        }

        #region Command

        public interface IDelegateCommand : ICommand {
            void RaiseCanExecuteChanged();
        }

        public class DelegateCommand : IDelegateCommand {
            Action<object> execute;
            Func<object, bool> canExecute;
            // Event required by ICommand
            public event EventHandler CanExecuteChanged;
            // Two constructors
            public DelegateCommand(Action<object> execute, Func<object, bool> canExecute) {
                this.execute = execute;
                this.canExecute = canExecute;
            }
            public DelegateCommand(Action<object> execute) {
                this.execute = execute;
                this.canExecute = this.AlwaysCanExecute;
            }
            // Methods required by ICommand
            public void Execute(object param) {
                execute(param);
            }
            public bool CanExecute(object param) {
                return canExecute(param);
            }
            // Method required by IDelegateCommand
            public void RaiseCanExecuteChanged() {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
            // Default CanExecute method
            bool AlwaysCanExecute(object param) {
                return true;
            }
        }

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IDelegateCommand AddCharacterCommand { protected set; get; }

        public IDelegateCommand DeleteCharacterCommand { protected set; get; }

        void ExecuteAddCharacter(object param) {
            if (param is NumericBox) {
                (param as NumericBox).Cressing();
                (param as NumericBox).Text = (param as NumericBox).Value.ToString();
            }
        }

        void ExecuteDeleteCharacter(object param) {
            if (param is NumericBox) {
                (param as NumericBox).Decressing();
                (param as NumericBox).Text = (param as NumericBox).Value.ToString();
            }
        }

        bool CanExecuteDeleteCharacter(object param) {
            if (param is NumericBox) {
                NumericBox numBox = (param as NumericBox);
                double _double;
                if (double.TryParse(numBox.Text, out _double)) {
                    int _int = _double >= int.MaxValue ? int.MaxValue : (int)_double;
                    byte _byte = _int >= byte.MaxValue ? byte.MaxValue : (byte)_int;

                    switch (NumType) {
                        case NumericType.ByteType: return _byte >= byte.MinValue;
                        case NumericType.DoubleType: return _double >= double.MinValue;
                        case NumericType.IntType: return _int >= int.MinValue;
                    }
                }
            }
            return false;
        }

        #endregion

        public double Value {
            set {
                switch (NumType) {
                    case NumericType.IntType:
                        int _int;
                        if (!int.TryParse(value.ToString().Replace('.', ','), out _int))
                            _int = Convert.ToInt32(Value);
                        SetValue(ValueProperty, (double)_int);
                        break;
                    case NumericType.DoubleType: SetValue(ValueProperty, Convert.ToDouble(value)); break;
                    case NumericType.ByteType:
                        byte _byte;
                        if (!byte.TryParse(value.ToString().Replace('.', ','), out _byte))
                            _byte = Convert.ToByte(Value);
                        SetValue(ValueProperty, (double)_byte);
                        break;
                }
            }
            get { return (double)GetValue(ValueProperty); }
        }

        public double Maximum {
            set {
                switch (NumType) {
                    case NumericType.IntType:
                        int _int;
                        if (!int.TryParse(value.ToString().Replace('.', ','), out _int))
                            _int = Convert.ToInt32(Value);
                        if (_int < Minimum) return;
                        SetValue(MaximumProperty, (double)_int);
                        break;
                    case NumericType.DoubleType:
                        if (value < Minimum) return;
                        SetValue(MaximumProperty, Convert.ToDouble(value)); break;
                    case NumericType.ByteType:
                        byte _byte;
                        if (!byte.TryParse(value.ToString().Replace('.', ','), out _byte))
                            _byte = Convert.ToByte(Value);
                        if (_byte < Minimum) return;
                        SetValue(MaximumProperty, (double)_byte);
                        break;
                }
            }
            get { return (double)GetValue(MaximumProperty); }
        }

        public double Minimum {
            set {
                switch (NumType) {
                    case NumericType.IntType:
                        int _int;
                        if (!int.TryParse(value.ToString().Replace('.', ','), out _int))
                            _int = Convert.ToInt32(Value);
                        if (_int > Maximum) return;
                        SetValue(MinimumProperty, (double)_int);
                        break;
                    case NumericType.DoubleType: 
                        if (value > Maximum) return;
                        SetValue(MinimumProperty, Convert.ToDouble(value)); break;
                    case NumericType.ByteType:
                        byte _byte;
                        if (!byte.TryParse(value.ToString().Replace('.', ','), out _byte))
                            _byte = Convert.ToByte(Value);
                        if (_byte > Maximum) return;
                        SetValue(MinimumProperty, (double)_byte);
                        break;
                }
            }
            get { return (double)GetValue(MinimumProperty); }
        }

        [Browsable(false)]
        public virtual string Text { get { return base.Text; } set { base.Text = value; } }

        protected override void OnTextChanged(TextChangedEventArgs e) {
            var caret = this.CaretIndex > 0 ? CaretIndex - 1 : 0;
            var l = Text.Length;
            if (NumType == NumericType.ByteType) {
                byte value = 0;
                if (Text.Contains(",")) Text = Text.Substring(0, Text.IndexOf(','));
                if (string.IsNullOrEmpty(Text) || Convert.ToInt32(Text) == 0) { Text = "0"; CaretIndex = 1; }
                if (!byte.TryParse(Text, out value))
                    Text = Value.ToString();
                else Value = value;
                if (l > Value.ToString().Length)
                    CaretIndex = caret;
            }
            base.OnTextChanged(e);
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e) {
            if (keys.Contains(e.Key)) {
                double clipb;
                if (e.Key == Key.V && (Keyboard.Modifiers != ModifierKeys.Control
                    || !double.TryParse(Clipboard.GetText(), out clipb)
                    || !double.TryParse(Text.Insert(CaretIndex, Clipboard.GetText()), out clipb))) e.Handled = true;
                if (e.Key == Key.C && Keyboard.Modifiers != ModifierKeys.Control) e.Handled = true;
                if (e.Key == Key.X && Keyboard.Modifiers != ModifierKeys.Control) e.Handled = true;
                if (e.Key == Key.OemMinus || e.Key == Key.Subtract) {// menos
                    if (NumType == NumericType.ByteType) e.Handled = true;
                    if (Text.Contains("-")) e.Handled = true;
                    if (Text.Length != 0 && CaretIndex > 0) e.Handled = true;
                } else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal) {// punto
                    if (NumType == NumericType.ByteType) e.Handled = true;
                    if (Text.Length > 0 && Text[0] == '-' && CaretIndex == 0) e.Handled = true;
                    if (Text.Contains(".")) e.Handled = true;
                } else if (e.Key == Key.Up) {
                    Cressing();
                } else if (e.Key == Key.Down)
                    Decressing();
            } else e.Handled = true;
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0)
                Cressing();
            else
                Decressing();
            base.Text = Value.ToString();
            SelectAll();
            base.OnMouseWheel(e);
        }

        public int Direction {
            private set;
            get;
        }

        public void Cressing() {
            Direction = 1;
            switch (NumType) {
                case NumericType.IntType:
                    int _int;
                    if (!int.TryParse(Value.ToString(), out _int))
                        _int = Convert.ToInt32(Value);
                    if (_int < ((Maximum != 0) ? Math.Min(Maximum, int.MaxValue) : int.MaxValue))
                        Value = _int + (int)Step > Maximum ? Maximum : _int + (int)Step;
                    else Value = ((Maximum != 0) ? Math.Min(Maximum, int.MaxValue) : int.MaxValue);
                    break;
                case NumericType.DoubleType:
                    double dvalue = Convert.ToDouble(Value);
                    if (dvalue < ((Maximum != 0) ? Math.Min(Maximum, double.MaxValue) : double.MaxValue))
                        Value = dvalue + Step > Maximum ? Maximum : dvalue + Step;
                    else Value = ((Maximum != 0) ? Math.Min(Maximum, double.MaxValue) : double.MaxValue);
                    break;
                case NumericType.ByteType:
                    byte _byte;
                    if (!byte.TryParse(Value.ToString(), out _byte))
                        _byte = Convert.ToByte(Value);
                    if (_byte < ((Maximum != 0) ? Math.Min(Maximum, byte.MaxValue) : byte.MaxValue))
                        Value = _byte + (byte)Step;
                    else Value = ((Maximum != 0) ? Math.Min(Maximum, byte.MaxValue) : byte.MaxValue);
                    break;
            }
        }

        public void Decressing() {
            Direction = -1;
            switch (NumType) {
                case NumericType.IntType:
                    int _int;
                    if (!int.TryParse(Value.ToString(), out _int))
                        _int = Convert.ToInt32(Value);
                    if (_int > Minimum)
                        Value = _int - (int)Step < Minimum ? Minimum : _int - (int)Step;
                    else Value = Minimum;
                    break;
                case NumericType.DoubleType:
                    double dvalue = Convert.ToDouble(Value);
                    if (dvalue > Minimum)
                        Value = dvalue - Step < Minimum ? Minimum : dvalue - Step;
                    else Value = Minimum;
                    break;
                case NumericType.ByteType:
                    byte _byte;
                    if (!byte.TryParse(Value.ToString(), out _byte))
                        _byte = Convert.ToByte(Value);
                    if (_byte > Minimum)
                        Value = _byte - (byte)Step < Minimum ? Minimum : _byte - (byte)Step;
                    else Value = Minimum;
                    break;
            }
        }

        public NumericType NumType { get; set; }

        public double Step { get; set; } = 1;

        public enum NumericType {
            DoubleType,
            IntType,
            ByteType
        }
    }
}
