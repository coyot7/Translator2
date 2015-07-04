using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Translator2
{
    public class Znaki
    {
        public Regex Regex { get; set; }
        public string Name { get; set; }
    }
    public class Wyniki
    {
        public Znaki ElementType { get; set; }
        public object Data { get; set; }
        public int Position { get; set; }
    }
    public class Analiza
    {
        private readonly List<Znaki> _elementsList;
        private readonly List<Wyniki> _resultsList;
        public string Input { get; set; }
        public int Pos { get; set; }

        public Analiza()
        {
            _elementsList = new List<Znaki>();
            _resultsList = new List<Wyniki>();
            Pos = 0;
        }

        public List<Znaki> GetElements
        {
            get { return _elementsList; }
        }

        public void AddElement(Znaki element)
        {
            _elementsList.Add(element);
        }

        public List<Wyniki> Results
        {
            get { return _resultsList; }
        }

        public Wyniki GetNextResult()
        {
            while (true)
            {
                var zmiana = false;
                Wyniki res = null;
                foreach (var element in _elementsList)
                {
                    var match = element.Regex.Match(Input);
                    if (match.Success)
                    {
                        res = new Wyniki() { Data = match.Value, ElementType = element, Position = Pos };
                        Input = element.Regex.Split(Input).Last();
                        zmiana = true;
                        Pos += match.Length;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(Input))
                    return null;
                if (zmiana == false) throw new Exception("Blad skladni na pozycji: " + Pos + ": " + Input);
                if (res.ElementType.Name == "whiteSpace") continue;
                return res;
            }
        }
    }
    public class Wyrazenie
    {
        private readonly Analiza _lexer;
        private Znaki _expectedElement;
        private int _bracket = 0;
        private int _id = 0;

        public Wyrazenie(Analiza analizer)
        {
            _lexer = analizer;
            CurrentElement();
        }

        public void Start()
        {
            W();
        }

        public void W()
        {
            S();
            X();
        }

        private void S()
        {

            C();
            Y();
        }

        private void Y()
        {
            while (true)
            {
                if (_expectedElement.Name == "multiplication" ||
                    _expectedElement.Name == "division")
                {
                    CheckCurrent(_expectedElement);
                    C();
                    continue;
                }
                break;
            }
        }

        private void C()
        {
            if (_expectedElement.Name == "integer" ||
                _expectedElement.Name == "float" ||
                _expectedElement.Name == "variable")
                CheckCurrent(_expectedElement);
            else if (_expectedElement.Name == "lp") //lewy nawias
            {
                _bracket++;
                CheckCurrent(_expectedElement);
                W();
                _bracket--;
                CheckCurrent(_lexer.GetElements.First(x => x.Name == "rp"));  //prawy nawias              
            }
            else
            {
                string blad ="";
                if (_expectedElement.Name == "multiplication")
                    blad = "mnozenia";
                else if (_expectedElement.Name == "division")
                    blad = "dzielenia";
                else if (_expectedElement.Name == "addidtion")
                    blad = "dodawania";
                else if (_expectedElement.Name == "substraction")
                    blad = "odejmowania";
                else if (_expectedElement.Name == "lp")
                    blad = "lewego nawiasu";
                else if (_expectedElement.Name == "rp")
                    blad = "prawego nawiasu";
                
                throw new Exception("Blad " + blad);
            }

        }

        private void X() 
        {
            while (true)
            {
                if (_expectedElement.Name != "addidtion" &&
                    _expectedElement.Name != "substraction") return;
                CheckCurrent(_expectedElement);
                S();
            }
        }

        private void CheckCurrent(Znaki type)
        {
            if (_expectedElement == type)
                CurrentElement();
            else
                throw new Exception("Blad " + type.Name);
        }

        private void CurrentElement()
        {
            var tmp = _lexer.GetNextResult();
            if (tmp == null && _bracket > 0)
                throw new Exception("Brak prawego nawiasu");
            if (tmp == null)
                throw new Exception("Operacja zakonczona sukcesem.");
            if (_bracket == 0 && tmp.ElementType.Name == "rp")
                throw new Exception("Brak lewego nawiasu");

            _expectedElement = tmp.ElementType;
            _id++;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(@"C:\Projekty\Translator2\Translator2\bin\Debug\Dane.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            var allines = sb.ToString();
            #region Elements
            var leks = new Analiza();
            leks.AddElement(new Znaki() { Name = "whiteSpace", Regex = new Regex(@"^\s+") });
            leks.AddElement(new Znaki() { Name = "addidtion", Regex = new Regex(@"^\+") });
            leks.AddElement(new Znaki() { Name = "substraction", Regex = new Regex(@"^\-") });
            leks.AddElement(new Znaki() { Name = "multiplication", Regex = new Regex(@"^\*") });
            leks.AddElement(new Znaki() { Name = "division", Regex = new Regex(@"^\\") });
            leks.AddElement(new Znaki() { Name = "lp", Regex = new Regex(@"^\(") });
            leks.AddElement(new Znaki() { Name = "rp", Regex = new Regex(@"^\)") });
            leks.AddElement(new Znaki() { Name = "equal", Regex = new Regex(@"^\=") });
            leks.AddElement(new Znaki() { Name = "variable", Regex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*") });
            leks.AddElement(new Znaki() { Name = "float", Regex = new Regex(@"^\d+\.\d+") });
            leks.AddElement(new Znaki() { Name = "integer", Regex = new Regex(@"^\d+") });
            #endregion
            Console.WriteLine(allines);

            leks.Input = allines;

            try
            {
                var algorithm = new Wyrazenie(leks);
                algorithm.Start();
                Console.WriteLine("Operacja zakonczona sukcesem.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
