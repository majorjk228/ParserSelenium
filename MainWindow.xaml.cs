using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
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

namespace ParserSelenium
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> _dict;
        public MainWindow()
        {
            InitializeComponent();

            _dict = new Dictionary<string, string>();
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DateFrom.SelectedDate == null || DateTo.SelectedDate == null || string.IsNullOrEmpty(ArticleTb.Text))
            {
                MessageBox.Show("Не выбрана дата начала или окончания или номер статьи!");
                return;
            }

            var dateFrom = (DateTime)DateFrom.SelectedDate;
            var dateTo = (DateTime)DateTo.SelectedDate;
            var article = ArticleTb.Text;

            IWebDriver driver = new ChromeDriver(); //создаем хром бразуер
            driver.Url = @"https://bsr.sudrf.ru/bigs/portal.html";

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    driver.FindElement(By.XPath("(//input[@class='calendar-input date-filter-from manual-date-element'])[1]"));
                    await Task.Delay(1000); //ждем секунду
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(1000); //ждем секунду если не может найти
                }
            }

            driver.FindElement(By.XPath("(//input[@class='calendar-input date-filter-from manual-date-element'])[1]")).SendKeys(dateFrom.ToString("dd.MM.yyyy"));//Можно взять вместо XPath By.CssSelector(искать по классу нейму .(Див назваине класса)

            await Task.Delay(500); //Делаем паузы, имитурем человека

            driver.FindElement(By.XPath("(//input[@class='calendar-input date-filter-to manual-date-element'])[1]")).SendKeys(dateTo.ToString("dd.MM.yyyy"));

            await Task.Delay(500);

            driver.FindElement(By.XPath("//input[@placeholder='Введите статью или категорию дела']")).SendKeys(article);

            await Task.Delay(500);

            driver.FindElement(By.XPath("//input[@id='searchFormButton']")).Click();

            await Task.Delay(2000);

            driver.FindElement(By.XPath("//a[contains(.,'Уголовное дело')]")).Click();

            await Task.Delay(1000);

            var oldNumber = string.Empty;
            
            var counter = 0;
            
            while (true)
            {
                await Task.Delay(2500);

                counter++;

                if (counter == 6) //Антикапча
                {
                    break;
                }        

                try
                {
                    driver.FindElement(By.XPath("//label[contains(.,'Дело')]")).Click();
                }
                catch (Exception)
                {                
                }

                await Task.Delay(2500);

                var number = driver.FindElement(By.XPath("(//span[@data-pos='0'])[1]")).GetAttribute("textContent");//забираем текст который лежит в контейнере

                var region = driver.FindElement(By.XPath("(//span[@data-pos='0'])[6]")).GetAttribute("textContent");//забираем текст который лежит в контейнере

                if (number == oldNumber) // если значение намбера пустое, то остановим цикл(проходим до конца, чтоб цикл не уходил в бесконечность
                {
                    break;
                }
                
                _dict[number] = region; //ключ номер дела, значение словаря регион 

                await Task.Delay(2500);

                driver.FindElement(By.XPath("(//span[contains(@title,'Вперед')])[3]")).Click();

                await Task.Delay(2500);
            }

            driver.Quit();

            var result = string.Empty;
            foreach (var pair in _dict)
            { 
                result += $"{pair.Key}={pair.Value}\r\n"; //в переменную запишутся все строки
            }

            MessageBox.Show(result);
        }
    }
}
