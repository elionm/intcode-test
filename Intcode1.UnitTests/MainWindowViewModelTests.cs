

using FluentAssertions;

namespace Intcode1.UnitTests
{
    public class MainWindowViewModelTests 
    {
        [Fact]
        public void Case1Test()
        {
            var vm= new MainWindowViewModel();
            vm.Program= "1,9,10,3,2,3,11,0,99,30,40,50";

            vm.Noun = "9";
            vm.Verb = "10";

            vm.CalculateCommand.CanExecute(null).Should().BeTrue();
            vm.CalculateCommand.Execute(null);
            vm.CalculateCommand.CanExecute(null).Should().BeFalse();
            vm.Result.Should().Be("3500");
        }

        [Fact]
        public void Case2Sync1Test()
        {
            var vm = new MainWindowViewModel();
            vm.Program = "1,9,10,3,2,3,11,0,99,30,40,50";
           
            vm.Result = "3500";
            vm.Sync1= true;

            vm.CalculateCommand.CanExecute(null).Should().BeTrue();
            vm.CalculateCommand.Execute(null);
            vm.CalculateCommand.CanExecute(null).Should().BeFalse();
            vm.Noun.Should().Be("9");
            vm.Verb.Should().Be("10");
        }

        // TODO: Pendiente
        //[Fact]
        //public void Case2Sync2Test()
        //{
        //    var vm = new MainWindowViewModel();
        //    vm.Program = "1,9,10,3,2,3,11,0,99,30,40,50";

        //    vm.Result = "3500";
        //    vm.Sync1 = false;
        //    vm.Sync2 = true;

        //    vm.CalculateCommand.CanExecute(null).Should().BeTrue();
        //    vm.CalculateCommand.Execute(null);
        //    vm.CalculateCommand.CanExecute(null).Should().BeFalse();
        //    vm.Noun.Should().Be("9");
        //    vm.Verb.Should().Be("10");
        //}

        [Fact]
        public void Case2Sync3Test()
        {
            var vm = new MainWindowViewModel();
            vm.Program = "1,9,10,3,2,3,11,0,99,30,40,50";

            vm.Result = "3500";
            vm.Sync1 = false;
            vm.Sync2 = false;
            vm.Sync3 = true;

            vm.CalculateCommand.CanExecute(null).Should().BeTrue();
            vm.CalculateCommand.Execute(null);
            vm.CalculateCommand.CanExecute(null).Should().BeFalse();
            vm.Noun.Should().Be("9");
            vm.Verb.Should().Be("10");
        }

        // Pendiente
        //[Fact]
        //public void Case2Sync4Test()
        //{
        //    var vm = new MainWindowViewModel();
        //    vm.Program = "1,9,10,3,2,3,11,0,99,30,40,50";

        //    vm.Result = "3500";
        //    vm.Sync1 = false;
        //    vm.Sync2 = false;
        //    vm.Sync3 = false;
        //    vm.Sync4 = true;

        //    vm.CalculateCommand.CanExecute(null).Should().BeTrue();
        //    vm.CalculateCommand.Execute(null);
        //    vm.CalculateCommand.CanExecute(null).Should().BeFalse();
        //    vm.Noun.Should().Be("9");
        //    vm.Verb.Should().Be("10");
        //}

    }
}