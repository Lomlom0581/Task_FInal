using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

namespace Task_Final
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Состояния разрешения ввода запятой
        /// </summary>
        enum CommaState
        {
            /// <summary>
            /// Нет ни одной цифры
            /// </summary>
            NoDigit,

            /// <summary>
            /// Запятая уже есть
            /// </summary>
            CommaExists,

            /// <summary>
            /// Ввод запятой разрешён
            /// </summary>
            CommaEnable
        }

        /// <summary>
        /// Состояние разрешения ввода запятой
        /// </summary>
        CommaState commaState = CommaState.NoDigit;

        /// <summary>
        /// Позиция бинарного оператора. -1 нет бинарного оператора.
        /// </summary>
        int posOperator = -1;

        /// <summary>
        /// Добаавляет к тексту ввода цифру
        /// </summary>
        /// <param name="sender">Кнопка цифры</param>
        /// <param name="e">Событие</param>
        private void AddDigit(object sender, RoutedEventArgs e)
        {
            txt.Text += (sender as Button).Content;
            if (commaState == CommaState.NoDigit)
                commaState = CommaState.CommaEnable;
        }

        /// <summary>
        /// Завершение работы с калькулятором
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculatorExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Очищает текстовое поле и сбрасывает все настройки в начало
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Text(object sender, RoutedEventArgs e)
        {
            txt.Text = "";
            commaState = CommaState.NoDigit;
            posOperator = -1;
        }

        /// <summary>
        /// Обработка нажатия кнопки ','. Если ввод разрешён, то вводится запятая в текст.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommaPressed(object sender, RoutedEventArgs e)
        {
            if (commaState == CommaState.CommaEnable)
            {
                txt.Text += ',';
                commaState = CommaState.CommaExists;
            }
        }

        /// <summary>
        /// Удаляет последний введённый символ, если он есть. Изменяет состояния.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSymbol(object sender, RoutedEventArgs e)
        {
            if (txt.Text == "")
                return; //Ничего не введено
            char LastSymbol = txt.Text[txt.Text.Length - 1];//Последний введённый символ
            txt.Text = txt.Text.Substring(0, txt.Text.Length - 1); //Убрать последний символ
            if (LastSymbol == ',')
                commaState = CommaState.CommaEnable; //Можно опять вводить запятую
            else if (char.IsDigit(LastSymbol) && (txt.Text == "" || (!char.IsDigit(txt.Text[txt.Text.Length - 1]) && txt.Text[txt.Text.Length - 1] != ',')))
            {//Если удалена 1-я цифра числа, то запятая запрещена
                commaState = CommaState.NoDigit;
            }
        }

        /// <summary>
        /// Поменять знак числа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SigneChange(object sender, RoutedEventArgs e)
        {
            int pos; //Позиция изменения знака
            for (pos = txt.Text.Length - 1; pos >= 0 && (txt.Text[pos] == ',' || char.IsDigit(txt.Text[pos])); pos--) ; //Пропуск всех цифр и запятых
            if (pos == txt.Text.Length - 1)
                return; //Число не вводится, поэтому знак вводить не надо
            if (pos < 0 || txt.Text[pos] != '-')
                txt.Text = txt.Text.Insert(pos + 1, "-"); //Поменяли знак на минус
            else
                txt.Text = txt.Text.Remove(pos, 1); //Удалить унарный минус
        }

        //static private NumberStyles style = NumberStyles.Number;

        /// <summary>
        /// Формат числа в российской кодировке, то есть с запятой.
        /// </summary>
        static private NumberFormatInfo nfi = new CultureInfo("ru-Ru", false).NumberFormat;

        /// <summary>
        /// Функция считывает число из конца текста. Позицию pos устанавливает на начало числа.
        /// Если в конце текста нет числа то функция устанавливает позицию в -1 и возвращает 0.
        /// </summary>
        /// <param name="pos">Позиция начала числа</param>
        /// <returns>Считанное число</returns>
        private double GetNumber(out int pos)
        {
            for (pos = txt.Text.Length - 1; pos >= 0 && (txt.Text[pos] == ',' || txt.Text[pos] == '-' || char.IsDigit(txt.Text[pos])); pos--) ;
            if (pos == txt.Text.Length - 1)
            {//Нет числа
                pos = -1;
                return 0;
            }
            pos++; //Чтобы указыаал на начало числа
            return Convert.ToDouble(txt.Text.Substring(pos), nfi);
        }

        /// <summary>
        /// Выполнение бинарного оператора, если он ещё не выполнен. Если не введено 2-е число, то
        /// функция отменяет оператор.
        /// </summary>
        private void ExecuteBinaryOperation()
        {
            commaState = CommaState.NoDigit;
            if (posOperator < 0)
                return; //Нет введённого бинарного оператора
            if (posOperator == txt.Text.Length - 2)
            {//За бинарным оператором не следует число. Отменить оператор.
                txt.Text = txt.Text.Substring(0, posOperator - 1);
                posOperator = -1;
                return;
            }
            int pos; //Позиция числа
            double value2 = GetNumber(out pos); //2-e число
            char oper = txt.Text[posOperator]; //Символ оператора
            txt.Text = txt.Text.Substring(0, posOperator - 1);
            double value1 = GetNumber(out pos); //1-e число, а также результат выполения операции
            posOperator = -1; //Оператор будет выполнен
            switch (oper) //Выполнить операцию
            {
                case '%': //Вычисляет процент
                    value1 *= value2 / 100;
                    break;
                case '+': //Сложение чисел
                    value1 += value2;
                    break;
                case '-': //Вычитание чисел
                    value1 -= value2;
                    break;
                case '*': //Умножение числе
                    value1 *= value2;
                    break;
                case '/': /* Деление чисел. При делении на 0 функция выводит сообщение на экран об ошибке
                            и очищает текст */
                    if (value2 == 0)
                    {
                        MessageBox.Show("Деление на 0 невозможно.");
                        posOperator = -1;
                        txt.Text = "";
                        return;
                    }
                    value1 /= value2;
                    break;
            }
            txt.Text = value1.ToString();
        }

        /// <summary>
        /// Рассчитать обратную величину
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneByX(object sender, RoutedEventArgs e)
        {
            double value; //Введённое число
            int pos; //Позиция начала числа в тексте
            ExecuteBinaryOperation(); //Выполнить бинарную операцию, если она введена
            value = GetNumber(out pos);
            if (pos < 0)
                return;
            if (value == 0)
            {
                MessageBox.Show("Попытка деления на 0");
                return;
            }
            value = 1 / value;
            txt.Text = txt.Text.Substring(0, pos) + value.ToString();
        }

        /// <summary>
        /// Возведение в квадрат числа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Square(object sender, RoutedEventArgs e)
        {
            double value; //Введённое число
            int pos; //Позиция начала числа в тексте
            ExecuteBinaryOperation(); //Выполнить бинарную операцию, если она введена
            value = GetNumber(out pos);
            if (pos < 0)
                return;
            value *= value;
            txt.Text = txt.Text.Substring(0, pos) + value.ToString();
        }

        /// <summary>
        /// Извлечение квадратного корня
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SquareRoot(object sender, RoutedEventArgs e)
        {
            double value; //Введённое число
            int pos; //Позиция начала числа в тексте
            ExecuteBinaryOperation(); //Выполнить бинарную операцию, если она введена
            value = GetNumber(out pos);
            if (pos < 0)
                return;
            if (value < 0)
            {
                MessageBox.Show("Попытка извлечения квадратного корня из отрицательного числа");
                return;
            }
            value = Math.Sqrt(value);
            txt.Text = txt.Text.Substring(0, pos) + value.ToString();
        }

        /// <summary>
        /// Вставка бинарного оператора в текст с пробелом перед и после оператора.
        /// Функция также устанавливает позицию оператора для использования при исполнении операции в дальнейшем.
        /// </summary>
        /// <param name="sender">Кнопка, из которой функция берёт оператор</param>
        /// <param name="e"></param>
        private void BinaryOperator(object sender, RoutedEventArgs e)
        {
            ExecuteBinaryOperation(); //Выполнить бинарную операцию, если она введена ранее
            txt.Text += ' '; //Добавить разделитель
            posOperator = txt.Text.Length; //Позиция оператора в тексте
            txt.Text += (sender as Button).Content;
            txt.Text += ' '; //Добавить разделитель
        }

        /// <summary>
        /// Операция "=". Выполняется бинарная операция, если она введена
        /// </summary>
        /// <param name="sender">Кнопка, из которой функция берёт оператор</param>
        /// <param name="e"></param>
        private void Equ(object sender, RoutedEventArgs e)
        {
            ExecuteBinaryOperation(); //Выполнить бинарную операцию, если она введена ранее
        }
    }
}
