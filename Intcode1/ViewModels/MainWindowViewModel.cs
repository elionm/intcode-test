using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Intcode1
{
    public class MainWindowViewModel: ViewModelBase
    {
        private string _fullFileName = string.Empty;
        private string _program = string.Empty;
        private string _noun = string.Empty;
        private string _verb = string.Empty;
        private string _result = string.Empty;
        private string _output = string.Empty;

        private bool _sync1 = false;
        private bool _sync2 = false;
        private bool _sync3 = false;
        private bool _sync4 = false;

        int MaxValue = 1000;  

        public string FullFileName
        {
            set
            {
                _fullFileName = value;
                OnPropertyChanged();
            }
            get { return _fullFileName; }
        }

        public string Program
        {
            set
            {
                _program = value;
                OnPropertyChanged();
            }
            get { return _program; }
        }

        public string Noun
        {
            set
            {
                _noun = value;
                OnPropertyChanged();
            }
            get { return _noun; }
        }

        public string Verb
        {
            set
            {
                _verb = value;
                OnPropertyChanged();
            }
            get { return _verb; }
        }

        public string Result
        {
            set
            {
                _result = value;
                OnPropertyChanged();
            }
            get { return _result; }
        }

        public bool Sync1
        {
            set
            {
                _sync1 = value;
                OnPropertyChanged();
            }
            get { return _sync1; }
        }

        public bool Sync2
        {
            set
            {
                _sync2 = value;
                OnPropertyChanged();
            }
            get { return _sync2; }
        }

        public bool Sync3
        {
            set
            {
                _sync3 = value;
                OnPropertyChanged();
            }
            get { return _sync3; }
        }
        public bool Sync4
        {
            set
            {
                _sync4 = value;
                OnPropertyChanged();
            }
            get { return _sync4; }
        }

        public string Output
        {
            set
            {
                _output = value;
                OnPropertyChanged();
            }
            get { return _output; }
        }
        public AsyncDelegateCommand CalculateCommand { get; set; }
        public DelegateCommand LoadFileCommand { get; set; }



        public MainWindowViewModel()
        {
            Sync1 = true;
            Sync2 = false;

            CalculateCommand = new AsyncDelegateCommand(Calculate,(_)=> CanExecuteCase1() || CanExecuteCase2() );
            LoadFileCommand = new DelegateCommand(LoadFile);

            this.PropertyChanged += MyViewModel_PropertyChanged;
        }

        private void LoadFile(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt"; 
            dialog.Filter = "Opcode files (.txt)|*.txt"; 
            dialog.InitialDirectory=Environment.CurrentDirectory;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                FullFileName = dialog.FileName;
                Program = File.ReadAllText(FullFileName);
            }
        }

        void MyViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            switch (e?.PropertyName)
            {
                case nameof(Program):
                case nameof(Noun):
                case nameof(Verb):
                case nameof(Result):
                case nameof(Sync1):
                case nameof(Sync2):
                case nameof(Sync3):
                case nameof(Sync4):
                    CalculateCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        public override void Dispose()
        {
            this.PropertyChanged -= MyViewModel_PropertyChanged;
        }

        //****************************************************
        bool CanExecuteCase1()
        {
            if (Program.Trim().Length == 0) return false;
            if (Result.Trim().Length > 0) return false;
            if (!int.TryParse(Noun.Trim(), out _)) return false;
            if (!int.TryParse(Verb.Trim(), out _)) return false;
            return true;
        }

        bool CanExecuteCase2()
        {
            if (Program.Trim().Length == 0) return false;
            if (Noun.Trim().Length > 0) return false;
            if (Verb.Trim().Length > 0) return false;
            if (!int.TryParse(Result.Trim(), out _)) return false;
            return Sync1 || Sync2 || Sync3 || Sync4;
        }

        private async Task Calculate(object? args)
        {
            if(CanExecuteCase1()) ExecuteCase1();
            if (CanExecuteCase2()) await ExecuteCase2();
        }

        private void ExecuteCase1()
        {
            if (int.TryParse(Noun.Trim(), out int noun) &&
                            int.TryParse(Verb.Trim(), out int verb))
            {
                var res = Execute(noun, verb, Program.Trim());
                if (res.IsOk)
                {
                    Result = res.Result.ToString();
                    WriteLine("Executed Ok.");
                }
                else if (res.IsUnknownOpcode)
                {
                    WriteLine("Error: Unknown Opcode.");
                }
                else if (res.IsInvalidMemoryIndex)
                {
                    WriteLine("Error: Invalid Memory Index.");
                }
                return;
            }
        }

        private async Task ExecuteCase2()
        {
            if (int.TryParse(Result.Trim(), out int expected))
            {
                if (Sync1) DoAction(Search, MaxValue, expected, Program);
                if (Sync2) await DoActionAsync(SearchAsync, MaxValue, expected, Program);
                if (Sync3) DoAction(SearchParallel, MaxValue, expected, Program);
                if (Sync4) await DoActionAsync(SearchParallelAsync, MaxValue, expected, Program);
            }
        }

        private void WriteLine(string s)
        {
            Output += s + Environment.NewLine;
        }
        //
        void DoAction(Func<int, int, string, TResult?> func, int max, int expected, string program)
        {
            var funcname = func.Method.Name;
            string h1 = funcname;
            WriteLine($"**** {h1} ****");
            var bm = new Benchmark(h1);
            bm.Start();
            var res = func(max, expected,program);   
            bm.Stop();
            WriteLine($"Elapsed time: {bm}");
            WriteLine($"Solution: {res?.ToString()}");
            if(res!=null)
            {
                Noun = res.Noun.ToString();
                Verb= res.Verb.ToString();
            }
        }

        async Task DoActionAsync(Func<int, int, string, Task<TResult?>> func, 
                                 int max, int expected, string program)
        {
            var funcname = func.Method.Name;
            string h1 = funcname;
            WriteLine($"**** {h1} ****");
            var bm = new Benchmark(h1);
            bm.Start();
            var res = await func(max, expected, program); 
            bm.Stop();
            WriteLine($"Elapsed time: {bm}");
            WriteLine($"Solution: {res?.ToString()}");
            if (res != null)
            {
                Noun = res.Noun.ToString();
                Verb = res.Verb.ToString();
            }
        }
        //
        private static TExecutionResult Execute(int noun, int verb, string initialState)
        {
            var intcode = CreateIntCode.FromString(initialState);
            intcode.Set(1, noun);
            intcode.Set(2, verb);
            var exitCode = intcode.Execute();
            if (exitCode == 0)
            {
                var result = intcode.Get(0);
                return new TExecutionResult(0, result);
            }
            return new TExecutionResult(exitCode, 0);
        }

        // Test2
        private static int Execute(int noun, int verb, Intcode initialState)
        {
            var intcode = CreateIntCode.FromIntcode(initialState);

            intcode.Set(1, noun);
            intcode.Set(2, verb);

            var exitCode = intcode.Execute();
            if (exitCode != 0) return -1;
            var result = intcode.Get(0);
            return result;
        }

        private async static Task<int> ExecuteAsync(int noun, int verb, Intcode initialState)
        {
            return await Task.Run(() =>
            {
                var intcode = CreateIntCode.FromIntcode(initialState);

                intcode.Set(1, noun);
                intcode.Set(2, verb);

                var exitCode = intcode.Execute();
                if (exitCode != 0) return -1;
                var result = intcode.Get(0);
                return result;
            });
        }

        //Search
        private static TResult? Search(int max, int result, string program)
        {
            var originalIntcode = CreateIntCode.FromString(program);

            for (int s = 0; s < max; s++)
            {
                for (int v = 0; v < max; v++)
                {
                    var res = Execute(s, v, originalIntcode); 
                    if (res == result)
                    {
                        TResult output = new TResult(s, v);
                        return output;
                    }
                }
            }
            return null;
        }

        private async static Task<TResult?> SearchAsync(int max, int result, string program)
        {
            return await Task.Run(() =>
            {
                return Search(max, result, program);
            });
        }

        private async static Task<TResult?> SearchAsync2(int max, int result, string program)
        {
            var originalIntcode = CreateIntCode.FromString(program);

            for (int s = 0; s < max; s++)
            {
                for (int v = 0; v < max; v++)
                {
                    var res = await ExecuteAsync(s, v, originalIntcode); 
                    if (res == result)
                    {
                        TResult output = new TResult(s, v);
                        return output;
                    }
                }
            }
            return null;
        }
        // Search Parallel
        private static TResult? SearchParallel(int max, int result, string program)
        {
            var originalIntcode = CreateIntCode.FromString(program);

            TResult? output = null;

            //for (int s = 0; s < max; s++)
            Parallel.For(0, max, (s, Sstate) =>
            {
                if (output != null) Sstate.Stop();
                //for (int v = 0; v < max; v++)
                Parallel.For(0, max, (v, Vstate) =>
                {
                    if (output != null) Vstate.Stop();
                    var res = Execute(s, v, originalIntcode); 
                    if (res == result)
                    {
                        output = new TResult(s, v);
                        Sstate.Stop();
                        Vstate.Stop();
                    }
                });
            });
            return output;
        }

        private async static Task<TResult?> SearchParallelAsync(int max, int result, string program)
        {
            return await Task.Run(() =>
            {
                return SearchParallel(max, result, program);

            });
        }
        //--
        // TODO: 
        private static async Task<TResult?> SearchParallelAsync2(int max, int result, string program)
        {  
            Intcode originalIntcode = CreateIntCode.FromString(program);
            TResult? output = null;

            List<Task<TResult?>> tasks = new();

            for (int s = 0; s < max; s++)
            {
                //if (s == 69)
                {
                    var x = s;
                    tasks.Add(Task.Run(() => InnerLoop(max, result, x, originalIntcode)));
                }
                //tasks.Add(Task.Run(() => InnerLoop(max, result, s, originalIntcode)));
            }

            //var results= await Task.WhenAll(tasks);

            while (tasks.Count > 0)
            {
                var justCompletedTask = await Task.WhenAny(tasks);//will not throw
                tasks.Remove(justCompletedTask);
                try
                {
                    var res = await justCompletedTask;
                    if (res != null)
                    {
                        return res;
                        //results.Add(result);
                    }
                }
                catch (Exception)
                {
                    //deal with it
                }
            }

            //foreach (var item in results)
            //{
            //    if (item != null) 
            //        return item;
            //}

            return output;
        }

        public static TResult? InnerLoop(int max, int result, int noun, Intcode originalIntcode)
        {
            int res = 0;
            for (int v = 0; v < max; v++)
            {
                res = Execute(noun, v, originalIntcode); 
                if (res == result)
                {
                    var output = new TResult(noun, v);
                    return output;
                }
            }
            return null;
        }

        //--
        public class TResult
        {
            public readonly int Noun;
            public readonly int Verb;

            public TResult(int noun, int verb)
            {
                Noun = noun;
                Verb = verb;
            }

            public override string ToString()
            {
                return $"Sustantivo={Noun}; Verbo={Verb}";
            }
        }
    }
}
